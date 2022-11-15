using DTLib.Dtsod;
using VkAudioDownloader;

var client = new VkClient(VkClientConfig.FromDtsod(new DtsodV23(File.ReadAllText("config.dtsod"))));
client.Connect();
Console.WriteLine(client.Api.Token);
var audios = client.FindAudio("моя оборона");
foreach (var a in audios)
{
    Console.WriteLine(a.Title);
}
