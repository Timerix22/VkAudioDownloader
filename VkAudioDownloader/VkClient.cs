using System;
using System.IO;
using System.Net.Http;
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
using ILogger = DTLib.Logging.New.ILogger;

namespace VkAudioDownloader;



public class VkClient : IDisposable
{
    public VkApi Api;
    public VkClientConfig Config;
    private ILogger _logger;

    public VkClient(VkClientConfig conf, ILogger logger)
    {
        Config = conf;
        _logger = logger;
        var services = new ServiceCollection()
            .Add(new LoggerService<VkApi>(logger))
            .AddAudioBypass();
        Api = new VkApi(services);
    }

    public void Connect()
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
        Api.Authorize(authParams);
    }

    public VkCollection<Audio> FindAudio(string query, int maxRezults=10) =>
        Api.Audio.Search(new AudioSearchParams()
        {
            Query = query,
            Count = maxRezults,
        });

    public Stream DownloadAudio(Audio audio)
    {
        HttpClient http = new HttpClient();
        var stream = http.GetStreamAsync(audio.Url).GetAwaiter().GetResult();
        return stream;
    }
    public void DownloadAudio(Audio audio, TimeSpan lengthLimit)
    {
        
    }

    private bool Disposed = false;
    public void Dispose()
    {
        if (Disposed) return;
        Api.Dispose();
        Disposed = true;
    }
}