using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using VkNet.Model.Attachments;
using VkNet.AudioBypassService.Extensions;
using DTLib.Logging.DependencyInjection;
using DTLib.Logging.New;
using DTLib.Filesystem;
using VkAudioDownloader.VkM3U8;

namespace VkAudioDownloader;



public class VkClient : IDisposable
{
    public VkApi Api;
    public VkClientConfig Config;
    private  DTLib.Logging.New.ILogger _logger;
    private HttpHelper _http;
    private FFMPegHelper _ffmpeg;
    
    public VkClient(VkClientConfig conf,  DTLib.Logging.New.ILogger logger)
    {
        Config = conf;
        _logger = logger;
        _http = new HttpHelper();
        _ffmpeg = new FFMPegHelper(logger,conf.FFMPegDir);
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
            Settings = Settings.Audio
        };
        if (Config.Token is not null)
        {
            _logger.Log(nameof(VkClient),LogSeverity.Info,"authorizing by token");
            authParams.AccessToken = Config.Token;
        }
        else
        {
            _logger.Log(nameof(VkClient),LogSeverity.Info,"authorizing by login and password");
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
                _logger.LogException(nameof(VkClient),aex);
            }
        }
    }

    public VkCollection<Audio> FindAudio(string query, int maxRezults=10) =>
        Api.Audio.Search(new AudioSearchParams()
        {
            Query = query,
            Count = maxRezults,
        });
    
    
    public Task<string> DownloadAudioAsync(Audio audio, string localDir) => 
        DownloadAudioAsync(audio, localDir,TimeSpan.FromHours(1));

    public async Task<string> DownloadAudioAsync(Audio audio, string localDir, TimeSpan durationLimit)
    {
        if (!audio.Url.ToString().StartsWith("http"))
            throw new Exception($"incorrect audio url: {audio.Url}");

        string outFile = Path.Concat(localDir, DTLib.Filesystem.Path.CorrectString($"{audio.Artist}-{audio.Title}.opus"));
        string fragmentDir = $"{outFile}_{DateTime.Now.Ticks}";
        if(File.Exists(outFile))
            _logger.LogWarn(nameof(VkClient), $"file {outFile} already exists");
        
        string m3u8 = await _http.GetStringAsync(audio.Url);
        var parser = new M3U8Parser();
        var hls = parser.Parse(audio.Url, m3u8);
        if (hls.Duration > durationLimit.TotalSeconds)
            throw new Exception($"duration limit <{durationLimit}> exceeded by track <{audio}> - <{hls.Duration}>");
        
        await _http.DownloadAsync(hls, fragmentDir);
        string[] opusFragments = await _ffmpeg.ToOpus(fragmentDir);
        string listFile = _ffmpeg.CreateFragmentList(fragmentDir, opusFragments);
        await _ffmpeg.Concat(outFile, listFile);
        // Directory.Delete(fragmentDir);
        
        return outFile;
    }

    private bool _disposed = false;
    public void Dispose()
    {
        if (_disposed) return;
        Api.Dispose();
        _http.Dispose();
        _disposed = true;
    }
}