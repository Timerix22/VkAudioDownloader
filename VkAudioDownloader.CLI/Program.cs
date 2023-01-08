using System;
using System.Linq;
using DTLib.Dtsod;
using System.IO;
using DTLib.Extensions;
using VkAudioDownloader;
using DTLib.Logging.New;
using VkAudioDownloader.CLI;

Console.InputEncoding = StringConverter.UTF8;
Console.OutputEncoding = StringConverter.UTF8;

LaunchArgumentParser argParser = new LaunchArgumentParser();

if(!File.Exists("config.dtsod"))
{
    File.Copy("config.dtsod.default", "config.dtsod");
    throw new Exception("No config detected, default created. Edit it!");
}

var config = VkClientConfig.FromDtsod(new DtsodV23(File.ReadAllText("config.dtsod")));

var logger = new CompositeLogger(new DefaultLogFormat(true), 
    new ConsoleLogger(), 
            new FileLogger("logs", "VkAudioDownloaer"),
            new FileLogger("logs", "VkAudioDownloaer_debug") { DebugLogEnabled = true});
var mainLoggerContext = new ContextLogger(logger, "Main");
mainLoggerContext.LogDebug("DEBUG LOG ENABLED");

try
{
#if DEBUG
    // checking correctness of my aes-128 decryptor on current platform
    VkAudioDownloader.Helpers.AudioAesDecryptor.TestAes();
#endif

    mainLoggerContext.LogInfo("initializing api...");
    var client = new VkClient(config, logger);
    await client.ConnectAsync();
    
    argParser.Add(new LaunchArgument(new []{"s", "search"}, "search audio on vk.com", SearchAudio, "query"));
    argParser.ParseAndHandle(args);
    
    void SearchAudio(string query)
    {
        var audios = client.FindAudio(query).ToArray();
        for (var i = 0; i < audios.Length; i++)
        {
            var a = audios[i];
            Console.WriteLine($"[{i}] {a.AudioToString()}");
        }
        Console.Write("choose audio: ");
        int ain = Convert.ToInt32(Console.ReadLine());
        var audio = audios[ain];
        Console.WriteLine($"selected {audio.AudioToString()}");
    
        string downloadedFile = client.DownloadAudioAsync(audio, "downloads").GetAwaiter().GetResult();
        mainLoggerContext.LogInfo($"audio {audio.AudioToString()} downloaded to {downloadedFile}");
    }
}
catch (LaunchArgumentParser.ExitAfterHelpException)
{
    
}
catch (Exception ex)
{
    mainLoggerContext.LogError(ex);
}
Console.ResetColor();
