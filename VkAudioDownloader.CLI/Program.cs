// See https://aka.ms/new-console-template for more information

using VkAudioDownloader;


var client = new VkClient(new VkClientConfig()
{
    AppId = 51473647,
    Login = "aaa",
    Password = "aaa"
});
client.Connect();
Console.WriteLine(client.Api.Token);
