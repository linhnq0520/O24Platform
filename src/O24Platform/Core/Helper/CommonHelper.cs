using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The common helper class
/// </summary>
public class CommonHelper
{
    public static IO24OpenAPIFileProvider? DefaultFileProvider { get; set; }

    /// <summary>
    /// Decrypts the text using the specified cipher text
    /// </summary>
    /// <param name="cipherText">The cipher text</param>
    /// <param name="encryptionPrivateKey">The encryption private key</param>
    /// <param name="encryptType">The encrypt type</param>
    /// <returns>The string</returns>
    public static string DecryptText(
        string cipherText,
        string encryptionPrivateKey,
        string encryptType = "3DES"
    )
    {
        if (encryptType == "AES")
        {
            byte[] numArray = new byte[16];
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionPrivateKey);
            aes.IV = numArray;
            return CommonHelper.DecryptTextFromMemory(
                Convert.FromBase64String(cipherText),
                aes.Key,
                aes.IV,
                encryptType
            );
        }
        else
        {
            using TripleDES tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.ASCII.GetBytes(encryptionPrivateKey.Substring(0, 16));
            tripleDes.IV = Encoding.ASCII.GetBytes(encryptionPrivateKey.Substring(8, 8));
            return CommonHelper.DecryptTextFromMemory(
                Convert.FromBase64String(cipherText),
                tripleDes.Key,
                tripleDes.IV,
                encryptType
            );
        }
    }

    /// <summary>
    /// Decrypts the text from memory using the specified data
    /// </summary>
    /// <param name="data">The data</param>
    /// <param name="key">The key</param>
    /// <param name="iv">The iv</param>
    /// <param name="encryptType">The encrypt type</param>
    /// <returns>The string</returns>
    private static string DecryptTextFromMemory(
        byte[] data,
        byte[] key,
        byte[] iv,
        string encryptType = "3DES"
    )
    {
        ICryptoTransform transform = !(encryptType == "AES")
            ? TripleDES.Create().CreateDecryptor(key, iv)
            : Aes.Create().CreateDecryptor(key, iv);
        using MemoryStream memoryStream = new(data);
        using CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Read);
        using StreamReader streamReader = new(
            cryptoStream,
            encryptType == "3DES" ? Encoding.Unicode : null
        );
        return streamReader.ReadToEnd();
    }

    /// <summary>
    /// Gets or sets the value of the default file provider
    /// </summary>
    /// <summary>
    /// Returns the value
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="destinationType">The destination type</param>
    /// <returns>The object</returns>
    public static object? To(object value, Type destinationType)
    {
        return To(value, destinationType, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns the value
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="destinationType">The destination type</param>
    /// <param name="culture">The culture</param>
    /// <returns>The object</returns>
    public static object? To(object value, Type destinationType, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }

        Type type = value.GetType();
        TypeConverter converter1 = TypeDescriptor.GetConverter(destinationType);
        if (converter1.CanConvertFrom(value.GetType()))
        {
            return converter1.ConvertFrom(null, culture, value);
        }

        TypeConverter converter2 = TypeDescriptor.GetConverter(type);
        if (converter2.CanConvertTo(destinationType))
        {
            return converter2.ConvertTo(null, culture, value, destinationType);
        }

        if (destinationType.IsEnum && value is int v)
        {
            return Enum.ToObject(destinationType, v);
        }

        return !destinationType.IsInstanceOfType(value)
            ? Convert.ChangeType(value, destinationType, culture)
            : value;
    }

    /// <summary>
    /// Returns the value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The</returns>
    public static T? To<T>(object value) => (T?)To(value, typeof(T));

    /// <summary>
    /// Converts the to unix timestamp using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The long</returns>
    public static long ConvertToUnixTimestamp(DateTime value)
    {
        return (long)
            (value - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public static long GetCurrentDateAsLongNumber()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Encrypts the text using the specified plain text
    /// </summary>
    /// <param name="plainText">The plain text</param>
    /// <param name="encryptionPrivateKey">The encryption private key</param>
    /// <param name="encryptType">The encrypt type</param>
    /// <returns>The string</returns>
    public static string EncryptText(
        string plainText,
        string encryptionPrivateKey,
        string encryptType = "3DES"
    )
    {
        if (encryptType == "AES")
        {
            byte[] iV = new byte[16];
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionPrivateKey);
            aes.IV = iV;
            byte[] inArray = EncryptTextToMemory(plainText, aes.Key, aes.IV, encryptType);
            return Convert.ToBase64String(inArray);
        }
        using TripleDES tripleDES = TripleDES.Create();
        tripleDES.Key = Encoding.ASCII.GetBytes(encryptionPrivateKey.Substring(0, 16));
        tripleDES.IV = Encoding.ASCII.GetBytes(encryptionPrivateKey.Substring(8, 8));
        byte[] inArray2 = EncryptTextToMemory(plainText, tripleDES.Key, tripleDES.IV, encryptType);
        return Convert.ToBase64String(inArray2);
    }

    /// <summary>
    /// Encrypts the text to memory using the specified data
    /// </summary>
    /// <param name="data">The data</param>
    /// <param name="key">The key</param>
    /// <param name="iv">The iv</param>
    /// <param name="encryptType">The encrypt type</param>
    /// <returns>The byte array</returns>
    private static byte[] EncryptTextToMemory(
        string data,
        byte[] key,
        byte[] iv,
        string encryptType = "3DES"
    )
    {
        ICryptoTransform transform = (
            (!(encryptType == "AES"))
                ? TripleDES.Create().CreateEncryptor(key, iv)
                : Aes.Create().CreateEncryptor(key, iv)
        );
        using MemoryStream memoryStream = new();
        using (CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Write))
        {
            if (encryptType == "AES")
            {
                using StreamWriter streamWriter = new(cryptoStream);
                streamWriter.Write(data);
            }
            else
            {
                byte[] bytes = Encoding.Unicode.GetBytes(data);
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.FlushFinalBlock();
            }
        }
        return memoryStream.ToArray();
    }
}
