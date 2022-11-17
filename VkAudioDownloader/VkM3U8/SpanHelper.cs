using System;

namespace VkAudioDownloader.VkM3U8;

internal static class SpanHelper
{
    public static ReadOnlySpan<char> After(this ReadOnlySpan<char> span, char c)
    {
        var index = span.IndexOf(c);
        if (index == -1)
            throw new Exception($"char <{c}> not found in span <{span}>");
        return span.Slice(index+1);
    }
    
    public static ReadOnlySpan<char> After(this ReadOnlySpan<char> span, ReadOnlySpan<char> s)
    {
        var index = span.IndexOf(s);
        if (index == -1)
            throw new Exception($"span <{s}> not found in span <{span}>");
        return span.Slice(index+s.Length);
    }
    
    
    public static ReadOnlySpan<char> Before(this ReadOnlySpan<char> span, char c)
    {
        var index = span.IndexOf(c);
        if (index == -1)
            throw new Exception($"char <{c}> not found in span <{span}>");
        return span.Slice(0,index);
    }
    
    public static ReadOnlySpan<char> Before(this ReadOnlySpan<char> span, ReadOnlySpan<char> s)
    {
        var index = span.IndexOf(s);
        if (index == -1)
            throw new Exception($"span <{s}> not found in span <{span}>");
        return span.Slice(0,index);
    }
}