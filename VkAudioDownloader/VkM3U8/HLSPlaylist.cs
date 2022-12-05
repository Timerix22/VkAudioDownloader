namespace VkAudioDownloader.VkM3U8;

public class HLSPlaylist
{
    public HLSFragment[] Fragments { get; } 

    /// content duration in seconds
    public float Duration { get; }
    
    /// url before index.m3u8
    public string BaseUrl { get; }
    
    internal HLSPlaylist(HLSFragment[] fragments, float duration, string baseUrl)
    {
        Fragments = fragments;
        Duration = duration;
        BaseUrl = baseUrl;
    }

    public override string ToString() =>
        $"BaseUrl: {BaseUrl}\n" +
        $"Duration: {Duration}\n" +
        $"Fragments: HLSFragment[{Fragments.Length}]";
}