namespace VkAudioDownloader.VkM3U8;

public readonly record struct HLSFragment
(
    string Name,
    string Url,
    float Duration,
    bool Encrypted,
    string? EncryptionKeyUrl
);