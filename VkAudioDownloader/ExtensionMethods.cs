using System.Runtime.CompilerServices;
using VkNet.Model;

namespace VkAudioDownloader;

public static class ExtensionMethods
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AudioToString(this Audio a)
        => $"\"{a.Title}\" -- {a.Artist} ({TimeSpan.FromSeconds(a.Duration)})";
}