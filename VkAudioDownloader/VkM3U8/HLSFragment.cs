namespace VkAudioDownloader.VkM3U8;

public record struct HLSFragment
{
    public string Name;
    // public int Duration;
    public bool Encrypted;
    public string? EncryptionKeyUrl;
}