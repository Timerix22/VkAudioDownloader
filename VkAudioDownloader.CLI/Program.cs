using System;
using System.Linq;
using DTLib.Dtsod;
using System.IO;
using VkAudioDownloader;
using DTLib.Logging.New;

if(!File.Exists("config.dtsod"))
{
    File.Copy("config.dtsod.default", "config.dtsod");
    throw new Exception("No config detected, default created. Edit it!");
}

var config = VkClientConfig.FromDtsod(new DtsodV23(File.ReadAllText("config.dtsod")));

var logger = new CompositeLogger(new DefaultLogFormat(true), 
    new ConsoleLogger(), 
            new FileLogger("logs", "VkAudioDownloaer"));
var _logger = new LoggerContext(logger, "main");
_logger.LogDebug("DEBUG LOG ENABLED");

try
{
#if DEBUG
    AudioAesDecryptor.TestAes();
#endif
    
    _logger.LogInfo("initializing api...");
    var client = new VkClient(config, logger);
    await client.ConnectAsync();
    
// getting audio from vk
    var audios = client.FindAudio("сталинский костюм").ToArray();

    for (var i = 0; i < audios.Length; i++)
    {
        var a = audios[i];
        Console.WriteLine($"[{i}] {a.AudioToString()}");
    }

    Console.Write("choose audio: ");
    int ain = Convert.ToInt32(Console.ReadLine());
    var audio = audios[ain];
    Console.WriteLine($"selected \"{audio.Title}\" -- {audio.Artist} [{TimeSpan.FromSeconds(audio.Duration)}]");
    // downloading parts
    string downloadedFile = await client.DownloadAudioAsync(audio, "downloads");
    _logger.LogInfo($"audio {audio.AudioToString()} downloaded to {downloadedFile}");
}
catch (Exception ex)
{
    _logger.LogException(ex);
}
