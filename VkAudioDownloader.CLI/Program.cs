using System;
using System.Linq;
using DTLib.Dtsod;
using System.IO;
using VkAudioDownloader;
using DTLib.Logging.New;
using VkAudioDownloader.VkM3U8;


if(!File.Exists("config.dtsod"))
    File.Copy("config.dtsod.default","config.dtsod");

var logger = new CompositeLogger(new DefaultLogFormat(true), 
    new ConsoleLogger(), 
            new FileLogger("logs", "VkAudioDownloaer"));
#if DEBUG
logger.DebugLogEnabled = true;
#endif
AudioAesDecryptor.TestAes();

var client = new VkClient(
    VkClientConfig.FromDtsod(new DtsodV23(File.ReadAllText("config.dtsod"))), 
        logger);
logger.Log("main", LogSeverity.Debug, "initializing api...");
client.Connect();
// getting audio from vk
var http = new HttpHelper();
var audio = client.FindAudio("гражданская оборона", 1).First();
Console.WriteLine($"{audio.Title} -- {audio.Artist} [{TimeSpan.FromSeconds(audio.Duration)}]");
var m3u8 = await http.GetStringAsync(audio.Url);
Console.WriteLine("downloaded m3u8 playlist\n");
// parsing index.m3u8
var parser = new M3U8Parser();
var playlist = parser.Parse(audio.Url, m3u8);
Console.WriteLine(playlist);
// downloading parts
var frag = playlist.Fragments[3];
var kurl =frag.EncryptionKeyUrl ?? throw new NullReferenceException();
await http.DownloadAsync(kurl, "key.pub");
if(Directory.Exists("playlist"))
    Directory.Delete("playlist",true);
Directory.CreateDirectory("playlist");
await http.DownloadAsync(playlist, "playlist");

// var decryptor = new AudioAesDecryptor();
// string key = "cca42800074d7aeb";
// using var encryptedFile = File.Open("encrypted.ts", FileMode.Open, FileAccess.ReadWrite);
// using var cryptoStream = decryptor.DecryptStream(encryptedFile, key);
// using var decryptedFile = File.Open("out.ts", FileMode.Create);
// await cryptoStream.CopyToAsync(decryptedFile);
