global using DTLib.Dtsod;

namespace VkAudioDownloader;

public class VkClientConfig
{
    /// vk app id from https://vk.com/apps?act=manage
    public ulong AppId;
    /// account password
    public string? Password;
    /// account login (email/phone number)
    public string? Login;
    /// can be used instead of login and password
    public string? Token;
    public static VkClientConfig FromDtsod(DtsodV23 dtsod)
    {
        var config = new VkClientConfig
        {
            AppId = dtsod["app_id"]
        };
        if (dtsod.TryGetValue("login", out var login))
            config.Login = login;
        if (dtsod.TryGetValue("password", out var password))
            config.Password = password;
        if (dtsod.TryGetValue("token", out var token))
            config.Token = token;
        return config;
    }

    public DtsodV23 ToDtsod() =>
        new DtsodV23
        {
            { "app_id", AppId },
            { "password", Password ?? null },
            { "login", Login ?? null },
            { "token", Token ?? null }
        };

}