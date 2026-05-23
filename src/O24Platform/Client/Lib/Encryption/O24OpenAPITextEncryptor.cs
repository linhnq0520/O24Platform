using System.Security.Cryptography;
using System.Text;

namespace O24OpenAPI.Client.Lib.Encryption;

/// <summary>
/// The 24 open api text encryptor class
/// </summary>
public class O24OpenAPITextEncryptor
{
    /// <summary>
    /// The encyption key
    /// </summary>
    private static readonly string EncyptionKey = "9ECC075ABB0744A4A2DB5D6C58474C6F";

    /// <summary>
    /// Encrypts the string using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="plainText">The plain text</param>
    /// <returns>The string</returns>
    private static string EncryptString(string key, string plainText)
    {
        byte[] iV = new byte[16];
        byte[] inArray;
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iV;
            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new();
            using CryptoStream stream = new(memoryStream, transform, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(stream))
            {
                streamWriter.Write(plainText);
            }
            inArray = memoryStream.ToArray();
        }
        return Convert.ToBase64String(inArray);
    }

    /// <summary>
    /// Decrypts the string using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="plainText">The plain text</param>
    /// <returns>The string</returns>
    private static string DecryptString(string key, string plainText)
    {
        byte[] iV = new byte[16];
        byte[] buffer = Convert.FromBase64String(plainText);
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iV;
        ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream stream = new(buffer);
        using CryptoStream stream2 = new(stream, transform, CryptoStreamMode.Read);
        using StreamReader streamReader = new(stream2);
        return streamReader.ReadToEnd();
    }

    /// <summary>
    /// Aeses the encrypt string using the specified plain text
    /// </summary>
    /// <param name="plainText">The plain text</param>
    /// <returns>The string</returns>
    public static string AESEncryptString(string plainText)
    {
        return EncryptString(EncyptionKey, plainText);
    }

    /// <summary>
    /// Aeses the decrypt string using the specified plain text
    /// </summary>
    /// <param name="plainText">The plain text</param>
    /// <returns>The string</returns>
    public static string AESDecryptString(string plainText)
    {
        return DecryptString(EncyptionKey, plainText);
    }

    /// <summary>
    /// Computes the sha 512 hash using the specified raw data
    /// </summary>
    /// <param name="rawData">The raw data</param>
    /// <returns>The string</returns>
    public static string ComputeSha512Hash(string rawData)
    {
        using SHA512 sHA = SHA512.Create();
        byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        StringBuilder stringBuilder = new();
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(array[i].ToString("x2"));
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Computes the sha 256 hash using the specified raw data
    /// </summary>
    /// <param name="rawData">The raw data</param>
    /// <returns>The string</returns>
    public static string ComputeSha256Hash(string rawData)
    {
        using SHA256 sHA = SHA256.Create();
        byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        StringBuilder stringBuilder = new();
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(array[i].ToString("x2"));
        }
        return stringBuilder.ToString();
    }
}
