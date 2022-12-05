global using System.Threading.Tasks;
using System.Net.Http;
using DTLib.Filesystem;
using Stream = System.IO.Stream;


namespace VkAudioDownloader.VkM3U8;

public class HttpHelper : HttpClient
{
    private AudioAesDecryptor Decryptor = new();

    public static async Task WriteStreamAsync(Stream stream, string localFilePath, bool disposeStream=true)
    {
        await using var file = File.OpenWrite(localFilePath);
        await stream.CopyToAsync(file);
        if(disposeStream)
            await stream.DisposeAsync();
    }
    public async Task DownloadAsync(string url, string localFilePath) => 
        await WriteStreamAsync(await GetStreamAsync(url), localFilePath);

    public async Task<Stream> GetStreamAsync(HLSFragment fragment)
    {
        var fragmentStream = await GetStreamAsync(fragment.Url);
        if (!fragment.Encrypted)
            return fragmentStream;
        string key = await GetStringAsync(fragment.EncryptionKeyUrl);
        return Decryptor.DecryptStream(fragmentStream, key);
    }

    public async Task DownloadAsync(HLSFragment fragment, string localDir) => 
        await WriteStreamAsync(await GetStreamAsync(fragment), Path.Concat(localDir, fragment.Name));

    public async Task DownloadAsync(HLSPlaylist playlist, string localDir)
    {
        foreach (var fragment in playlist.Fragments)
        {
            //TODO log file download progress
            await DownloadAsync(fragment, localDir);
            // playlist.CreateFragmentList();
        }
    }
}