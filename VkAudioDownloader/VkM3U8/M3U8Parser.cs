using System;
using System.Collections.Generic;

namespace VkAudioDownloader.VkM3U8;

public class M3U8Parser
{
    private string _m3u8="";
    private int _pos;
    private List<HLSFragment> _fragments = new();
    private int _playlistDuration = 0;
    private HLSFragment _currentFragment = default;
    
    // parses m3u8 playlist and resets state
    public HLSPlaylist Parse(Uri m3u8Url, string m3u8Content)
    {
        _m3u8 = m3u8Content;
        var line = NextLine();
        while (!line.IsEmpty)
        {
            if (line.Contains('#'))
                ParseHashTag(line);
            else
            {
                _currentFragment.Name = line.ToString();
                _fragments.Add(_currentFragment);
                _currentFragment = default;
            }
                
            line = NextLine();
        }

        string urlStr = m3u8Url.ToString();
        var rezult = new HLSPlaylist(
            _fragments.ToArray(),
            _playlistDuration,
            urlStr.Remove(urlStr.LastIndexOf('/')+1));
        Clear();
        return rezult;
    }
    
    ReadOnlySpan<char> NextLine()
    {
        int pos = _pos;
        int index = _m3u8.IndexOf('\n', pos);
        if (index == -1)
            index = _m3u8.Length - _pos;
        if (index == 0)
            return ReadOnlySpan<char>.Empty;
        _pos = index+1;
        if (_m3u8[index - 1] == '\r')
            index--; // skip /r
        var line = _m3u8.AsSpan(pos, index - pos);
        return line;
    }

    private void ParseHashTag(ReadOnlySpan<char> line)
    {
        if(line.StartsWith("EXT-X-TARGETDURATION:"))
            _playlistDuration=Int32.Parse(line.After(':'));
        else if (line.StartsWith("#EXT-X-KEY:METHOD="))
        {
            var method = line.After("#EXT-X-KEY:METHOD=");
            
            if (method.ToString() == "NONE")
            {
                _currentFragment.Encrypted = false;
                return;
            }

            var alg = method.Before(',');
            if (alg.ToString() != "AES-128")
                throw new Exception($"unknown encryption algorythm: {method}");
            
            var keyUrl=method.After("URI=\"").Before('\"');
            if (!keyUrl.StartsWith("http"))
                throw new Exception($"key uri is not url: {keyUrl}");
            
            // AES-128 which AudioDecryptor can decrypt
            _currentFragment.Encrypted = true;
            _currentFragment.EncryptionKeyUrl = keyUrl.ToString();
        }
    }

    private void Clear()
    {
        _m3u8 = "";
        _pos=0;
        _fragments.Clear();
        _currentFragment = default;
        _playlistDuration = 0;
    }
}