using System.IO;
using System.Text;

namespace VkAudioDownloader.VkM3U8;

public class HLSPlaylist
{

    public HLSFragment[] Fragments { get; internal set; } 

    /// content duration in seconds
    public float Duration { get; internal set; }
    
    /// url before index.m3u8
    public string BaseUrl { get; internal set; }
    
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

    public void CreateFragmentListFile(string path)
    {
        using var playlistFile = File.Open(path, FileMode.Create);
        foreach (var fragment in Fragments)
        {
            playlistFile.Write(Encoding.ASCII.GetBytes("file '"));
            playlistFile.Write(Encoding.ASCII.GetBytes(fragment.Name));
            playlistFile.WriteByte((byte)'\'');
            playlistFile.WriteByte((byte)'\n');
        }
    }
}