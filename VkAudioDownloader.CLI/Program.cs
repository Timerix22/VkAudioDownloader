using System;
using System.Linq;
using System.Net.Http;
using DTLib.Dtsod;
using DTLib.Filesystem;
using VkAudioDownloader;
using DTLib.Logging.New;
using VkAudioDownloader.VkM3U8;


if(!File.Exists("config.dtsod"))
    File.Copy("config.dtsod.default","config.dtsod");

var logger = new CompositeLogger(new DefaultLogFormat(true), 
    new ConsoleLogger(), 
            new FileLogger("logs", "VkAudioDownloaer"));
var client = new VkClient(
    VkClientConfig.FromDtsod(new DtsodV23(File.ReadAllText("config.dtsod"))), 
        logger);
logger.Log("main", LogSeverity.Info, "initializing api...");
logger.DebugLogEnabled = true;
client.Connect();
var audio = client.FindAudio("гражданская оборона", 1).First();
Console.WriteLine($"{audio.Title} -- {audio.Artist} [{TimeSpan.FromSeconds(audio.Duration)}]");
var Http = new HttpClient();
var m3u8 = await Http.GetStringAsync(audio.Url);
Console.WriteLine("downloaded m3u8 playlist:\n" + m3u8);
var parser = new M3U8Parser();
var HLSPlaylist = parser.Parse(audio.Url, m3u8);
Console.WriteLine(HLSPlaylist);
