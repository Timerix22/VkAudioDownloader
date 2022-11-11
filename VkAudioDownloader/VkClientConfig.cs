global using DTLib.Dtsod;

namespace VkAudioDownloader;

public class VkClientConfig
{
    /// account password
    public string Password;
    /// account login (email/phone number)
    public string Login;
    /// vk app id from https://vk.com/apps?act=manage
    public ulong AppId;

    
    public static VkClientConfig FromDtsod(DtsodV23 dtsod) =>
        new VkClientConfig
        {
            Password = dtsod["password"],
            Login = dtsod["login"],
            AppId = dtsod["app_id"]
        };

    public DtsodV23 ToDtsod() =>
        new DtsodV23
        {
            { "password", Password },
            { "login", Login },
            { "app_id", AppId }
        };

}