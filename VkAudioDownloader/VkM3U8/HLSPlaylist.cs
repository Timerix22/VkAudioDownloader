namespace VkAudioDownloader.VkM3U8;

public class HLSPlaylist
{

    public HLSFragment[] Fragments { get; internal set; } 

    /// content duration in seconds
    public int Duration { get; internal set; }
    
    /// url before index.m3u8
    public string BaseUrl { get; internal set; }
    
    internal HLSPlaylist(HLSFragment[] fragments, int duration, string baseUrl)
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