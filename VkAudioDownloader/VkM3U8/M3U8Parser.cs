namespace VkAudioDownloader.VkM3U8;

public class M3U8Parser
{
    #nullable disable
    private string _m3u8;
    private int _pos;
    private List<HLSFragment> _fragments = new();
    private string _baseUrl;
    private float _playlistDuration;
    private string _fragmentName;
    private float _fragmentDuration;
    private string _fragmentEncryptionKeyUrl;
    private bool _fragmentEncrypted;
    
    // parses m3u8 playlist and resets state
    public HLSPlaylist Parse(Uri m3u8Url, string m3u8Content)
    {
        _m3u8 = m3u8Content;
        var urlStr = m3u8Url.ToString();
        _baseUrl = urlStr.Remove(urlStr.LastIndexOf('/') + 1);
        
        var line = NextLine();
        while (!line.IsEmpty)
        {
            if (line.Contains('#'))
                ParseHashTag(line);
            else
            {
                _fragmentName = line.ToString();
                _fragments.Add(new HLSFragment(
                    _fragmentName, 
                    _baseUrl+_fragmentName,
                    _fragmentDuration,
                    _fragmentEncrypted, 
                    _fragmentEncryptionKeyUrl));
                _playlistDuration += _fragmentDuration;
                // m3u8 format uses hashtags to replace some properties, so there is no need to reset them after every fragment name
                // _fragmentName = null;
                // _fragmentDuration = 0;
                // _fragmentEncrypted = false;
                // _fragmentEncryptionKeyUrl = null;
            }
                
            line = NextLine();
        }

        var rezult = new HLSPlaylist(
            _fragments.ToArray(),
            _playlistDuration,
            _baseUrl);
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
        if(line.StartsWith("#EXTINF:"))
        {
            var duration = line.After(':').Before(',');
            _fragmentDuration = duration.ToFloat();
        }
        else if (line.StartsWith("#EXT-X-KEY:METHOD="))
        {
            var method = line.After("#EXT-X-KEY:METHOD=");
            
            if (method.ToString() == "NONE")
            {
                _fragmentEncrypted = false;
                return;
            }

            var alg = method.Before(',');
            if (alg.ToString() != "AES-128")
                throw new Exception($"unknown encryption algorythm: {method}");
            
            var keyUrl=method.After("URI=\"").Before('\"');
            if (!keyUrl.StartsWith("http"))
                throw new Exception($"key uri is not url: {keyUrl}");
            
            // AES-128 which AudioAesDecryptor can decrypt
            _fragmentEncrypted = true;
            _fragmentEncryptionKeyUrl = keyUrl.ToString();
        }
    }

    private void Clear()
    {
        _baseUrl = null;
        _m3u8 = null;
        _pos=0;
        _playlistDuration = 0;
        _fragments.Clear();
        _fragmentName = null;
        _fragmentEncrypted = false;
        _fragmentEncryptionKeyUrl = null;
    }
}
