global using System;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using DTLib.Filesystem;
global using DTLib.Extensions;
using DTLib.Logging;
using DTLib.Logging.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkAudioDownloader.Helpers;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;
using VkNet.AudioBypassService.Extensions;
using VkAudioDownloader.VkM3U8;

namespace VkAudioDownloader;

public class VkClient : IDisposable
{
    public VkApi Api;
    public VkClientConfig Config;
    private ContextLogger _logger;
    public HttpHelper Http;
    public FFMPegHelper Ffmpeg;
    
    public VkClient(VkClientConfig conf,  DTLib.Logging.ILogger logger)
    {
        Config = conf;
        _logger = new ContextLogger(nameof(VkClient), logger);
        Http = new HttpHelper();
        Ffmpeg = new FFMPegHelper(logger, conf.FfmpegDir);
        var services = new ServiceCollection()
            .Add(new LoggerService<VkApi>(logger))
            .AddAudioBypass();
        Api = new VkApi(services);
    }

    public async Task ConnectAsync(int attempts=5)
    {
        var authParams = new ApiAuthParams
        {
            ApplicationId = Config.AppId,
            Settings = Settings.Audio,
        };
        if (Config.Token is not null)
        {
            _logger.LogInfo("authorizing by token");
            authParams.AccessToken = Config.Token;
        }
        else
        {
            _logger.LogInfo("authorizing by login and password");
            authParams.Login = Config.Login;
            authParams.Password = Config.Password;
        }

        for (int authAttempt = 0; authAttempt < attempts; authAttempt++)
        {
            try
            {
                await Api.AuthorizeAsync(authParams);
                break;
            }
            catch (Exception aex)
            {
                _logger.LogError(aex);
            }
        }
    }

    public VkCollection<Audio> FindAudio(string query, int maxRezults=10) =>
        Api.Audio.Search(new AudioSearchParams()
        {
            Query = query,
            Count = maxRezults,
        });
    
    ///<returns>file name</returns>
    public Task<IOPath> DownloadAudioAsync(Audio audio, string localDir) => 
        DownloadAudioAsync(audio, localDir,TimeSpan.FromHours(1));

    ///<returns>file name</returns>
    public async Task<IOPath> DownloadAudioAsync(Audio audio, string localDir, TimeSpan durationLimit)
    {
        if (!audio.Url.ToString().StartsWith("http"))
            throw new Exception($"incorrect audio url: {audio.Url}");

        IOPath outFile = Path.Concat(localDir, DTLib.Filesystem.Path.ReplaceRestrictedChars($"{audio.Artist}-{audio.Title}.opus"));
        string fragmentDir = $"{outFile}_{DateTime.Now.Ticks}";
        if(File.Exists(outFile))
            _logger.LogWarn( $"file {outFile} already exists");
        
        string m3u8 = await Http.GetStringAsync(audio.Url);
        var parser = new M3U8Parser();
        var hls = parser.Parse(audio.Url, m3u8);
        if (hls.Duration > durationLimit.TotalSeconds)
            throw new Exception($"duration limit <{durationLimit}> exceeded by track <{audio}> - <{hls.Duration}>");
        
        await Http.DownloadAsync(hls, fragmentDir);
        var opusFragments = await Ffmpeg.ToOpus(fragmentDir);
        IOPath listFile = Ffmpeg.CreateFragmentList(fragmentDir, opusFragments);
        await Ffmpeg.Concat(outFile, listFile);
        Directory.Delete(fragmentDir);
        
        return outFile;
    }

    private bool _disposed = false;
    public void Dispose()
    {
        if (_disposed) return;
        Api.Dispose();
        Http.Dispose();
        _disposed = true;
    }
}
