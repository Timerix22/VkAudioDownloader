using System;
using System.IO;
using System.Security.Cryptography;

namespace VkAudioDownloader.VkM3U8;

public class AudioDecryptor : IDisposable
{
    private Aes Aes;
    private ICryptoTransform Decryptor;

    AudioDecryptor()
    {
        Aes=Aes.Create();
        Aes.KeySize = 128;
        Aes.Mode = CipherMode.CBC;
        Aes.IV = new byte[4] { 0, 0, 0, 0 };
        Aes.Padding = PaddingMode.Zeros;
        Decryptor = Aes.CreateDecryptor();
    }

    public Stream Decrypt(Stream fragment) 
        => new CryptoStream(fragment, Decryptor, CryptoStreamMode.Read);
    
    private bool _disposed;
    public void Dispose()
    {
        if(_disposed) return;
        Aes.Dispose();
        Decryptor.Dispose();
        _disposed = true;
    }

    ~AudioDecryptor() => Dispose();
}