global using DTLib;
global using DTLib.Extensions;
using DTLib.Logging;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VkAudioDownloader;



public class VkClient : IDisposable
{
    public VkApi Api;
    public VkClientConfig Config;
    
    public VkClient(VkClientConfig conf)
    {
        Config = conf;
        var services = new ServiceCollection();
        //services.AddSingleton<LoggerService>();
        Api = new VkApi(services);
        
    }

    public void Connect()
    {
        Api.Authorize(new ApiAuthParams
        {
            ApplicationId = Config.AppId,
            Login = Config.Login,
            Password = Config.Password,
            Settings = Settings.Audio
        });
    }

    private bool Disposed = false;
    public void Dispose()
    {
        if (Disposed) return;
        Api.Dispose();
        Disposed = true;
    }
}