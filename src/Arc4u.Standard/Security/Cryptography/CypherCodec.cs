using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Arc4u.Security.Cryptography;

/// <summary>
/// The Rijndael class is used to encode and decode a string with the Aes API.
/// </summary>
public static class CypherCodec
{
    private static readonly Encoding _cypherEncoding = Encoding.UTF8;

    /// <summary>
    /// Decodes the cypher string.
    /// </summary>
    /// <param name="cypherText">The cypher PWD.</param>
    /// <param name="rgbKey">The RGB key.</param>
    /// <param name="rgbIV">The RGB IV.</param>
    /// <returns>the clear text.</returns>
    public static string DecodeCypherString(string cypherText, byte[] rgbKey, byte[] rgbIV)
    {
        using var aes = Aes.Create();
        using var decryptor = aes.CreateDecryptor(rgbKey, rgbIV);
        using var csDecrypt = new CryptoStream(new MemoryStream(Convert.FromBase64String(cypherText)), decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(csDecrypt, _cypherEncoding);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Encodes the clear text.
    /// </summary>
    /// <param name="clearText">The clear text.</param>
    /// <param name="rgbKey">The RGB key.</param>
    /// <param name="rgbIV">The RGB IV.</param>
    /// <returns>The cyper text.</returns>
    public static string EncodeClearText(string clearText, byte[] rgbKey, byte[] rgbIV)
    {
        using var aes = Aes.Create();
        using var encryptor = aes.CreateEncryptor(rgbKey, rgbIV);
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        //Convert the data to a byte array.
        var toEncrypt = _cypherEncoding.GetBytes(clearText);
        //Write all data to the crypto stream and flush it.
        csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
        csEncrypt.FlushFinalBlock();
        // do not allocate an extra buffer: use the memory stream buffer directly to get the encrypted data
        return Convert.ToBase64String(msEncrypt.GetBuffer(), 0, (int)msEncrypt.Length);
    }

    /// <summary>
    /// Generates the key and IV which are used by the rijndael algorithm.
    /// </summary>
    /// <param name="IV">The IV.</param>
    /// <returns>the key</returns>
    public static string GenerateKeyAndIV(out string IV)
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        IV = Convert.ToBase64String(aes.IV);
        return Convert.ToBase64String(aes.Key);
    }
}
