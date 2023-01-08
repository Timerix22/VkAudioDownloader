global using DTLib.Dtsod;

namespace VkAudioDownloader;

public class VkClientConfig
{
    /// directory where ffmpeg and ffprobe binaries are stored
    public string FfmpegDir;
    /// vk app id from https://vk.com/apps?act=manage
    public ulong AppId;
    /// account password
    public string? Password;
    /// account login (email/phone number)
    public string? Login;
    /// can be used instead of login and password
    public string? Token;


    public VkClientConfig(string ffmpegDir, ulong appId, string? token)
    {
        FfmpegDir = ffmpegDir;
        AppId = appId;
        Token = token;
    }

    public VkClientConfig(string ffmpegDir, ulong appId, string? password, string? login)
    {
        FfmpegDir = ffmpegDir;
        AppId = appId;
        Password = password;
        Login = login;
    }
    
    private VkClientConfig(string ffmpegDir, ulong appId)
    {
        FfmpegDir = ffmpegDir;
        AppId = appId;
    }
    
    /// <summary>
    /// {
    ///     ffmpeg_dir: "";
    ///     app_id: 0ul;
    ///     token: "";
    ///     #or
    ///     login: "";
    ///     password: "";
    /// };
    /// </summary>
    public static VkClientConfig FromDtsod(DtsodV23 dtsod)
    {
        var config = new VkClientConfig(dtsod["ffmpeg_dir"], dtsod["app_id"]);
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
            { "token", Token ?? null },
            { "ffmpeg_dir", FfmpegDir}
        };

}
