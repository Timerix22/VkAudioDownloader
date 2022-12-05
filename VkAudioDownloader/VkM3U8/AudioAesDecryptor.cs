using System;
using System.Security.Cryptography;
using DTLib.Filesystem;
using Stream = System.IO.Stream;

namespace VkAudioDownloader.VkM3U8;

public class AudioAesDecryptor : IDisposable
{
    private Aes aes;
    private byte[] _key;
    private byte[] _iv;

    public byte[] Key
    {
        get => _key;
        set
        {
            if (value.Length != 16) 
                throw new Exception($"key.Length!=16, key: [{value.Length}]{{{string.Join(",",value)}}}");
            _key = value;
            aes.Key = value;
        }
    }
    public byte[] IV
    {
        get => _iv;
        set
        {
            if (value.Length != 16) 
                throw new Exception($"iv.Length!=16, iv: [{value.Length}]{{{string.Join(",",value)}}}");
            _iv = value;
            aes.IV = value;
        }
    }
    
    public AudioAesDecryptor(byte[] key, byte[] iv)
    {
        aes = CreateAes();
        _iv = iv;
        _key = key;
        Key = key;
        IV = iv;
    }

    public AudioAesDecryptor() : this(GetZeroIV(), GetZeroIV())
    {}

    static byte[] GetZeroIV()
    {
        var iv= new byte[16];
        for (int i = 0; i < 16; i++) 
            iv[i] = 0;
        return iv;
    }

    // analog of xxd -p key.pub
    public static byte[] KeyToBytes(string key)
    {
        if (key.Length != 16)
            throw new Exception($"invalid key string: {key}");
        var bytes = new byte[16];
        for (int i = 0; i < 16; i++) 
            bytes[i] = (byte)key[i];
        return bytes;
    }

    public void ResetIV()
    {
        aes.IV = _iv;
    }
    
    public Stream DecryptStream(Stream fragment, string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new NullReferenceException("key is null");
        aes.Key = KeyToBytes(key);
        aes.IV = GetZeroIV();
        var decryptor = aes.CreateDecryptor();
		// aes.Dispose(); decryptor.Dispose();
        return new CryptoStream(fragment, decryptor, CryptoStreamMode.Read);
    }

    public static Aes CreateAes()
    {
        var aes=Aes.Create();
        aes.KeySize = 128;
        // aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    static byte HexToByte(char c)
        =>c switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '3' => 3,
            '4' => 4,
            '5' => 5,
            '6' => 6,
            '7' => 7,
            '8' => 8,
            '9' => 9,
            'A' or 'a' => 10,
            'B' or 'b' => 11,
            'C' or 'c' => 12,
            'D' or 'd' => 13,
            'E' or 'e' => 14,
            'F' or 'f' => 15,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        };
    
    static char HalfByteToHex(byte b)
        =>b switch
        {
            0 => '0',
            1 => '1',
            2 => '2',
            3 => '3',
            4 => '4',
            5 => '5',
            6 => '6',
            7 => '7',
            8 => '8',
            9 => '9',
            0xA => 'a',
            0xB => 'b',
            0xC => 'c',
            0xD => 'd',
            0xE => 'e',
            0xF => 'f',
            _ => throw new ArgumentOutOfRangeException(nameof(b), b, null)
        };

    static byte[] HexToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
            throw new Exception("argument length is not even");
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i++)
        {
            bytes[i / 2] = (byte)(HexToByte(hex[i]) * 16 + HexToByte(hex[++i]));
        }
        return bytes;
    }
    static string BytesToHex(byte[] bytes)
    {
        if (bytes.Length % 2 != 0)
            throw new Exception("argument length is not even");
        char[] hex = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            hex[i * 2] = HalfByteToHex((byte)(b / 16));
            hex[i * 2 +1] = HalfByteToHex((byte)(b % 16));
        }
        return new string(hex);
    }
    
    
    public static void TestAes()
    {
        const string PLAINTEXT =  "6a84867cd77e12ad07ea1be895c53fa3";
        byte[] PLAINTEXT_B = HexToBytes(PLAINTEXT);
        const string CIPHERTEXT = "732281c0a0aab8f7a54a0c67a0c45ecfcf52019292387d1b2c9d44c45d418a48";
        byte[] CIPHERTEXT_B = HexToBytes(CIPHERTEXT);
        var aes = new AudioAesDecryptor();
        var padding = PaddingMode.PKCS7;
        var enc = aes.aes.EncryptCbc(PLAINTEXT_B, aes._iv, padding);
        var encs = BytesToHex(enc);
        if (encs != CIPHERTEXT)
            throw new Exception("encryption went wrong");
        aes.ResetIV();
        var dec = aes.aes.DecryptCbc(enc, aes._iv, padding);
        var decs = BytesToHex(dec);
        if (decs != PLAINTEXT)
            throw new Exception("decryption went wrong");
    }
    

    private bool _disposed = false;
    public void Dispose()
    {
        if (_disposed) return;
        aes.Dispose();
    }

    ~AudioAesDecryptor() => Dispose();
}