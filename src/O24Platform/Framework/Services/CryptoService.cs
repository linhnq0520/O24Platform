using System.Security.Cryptography;
using System.Text;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The crypto service class
/// </summary>
/// <seealso cref="ICryptoService"/>
public class CryptoService : ICryptoService
{
    /// <summary>
    /// The master key
    /// </summary>
    private readonly string _masterKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoService"/> class
    /// </summary>
    /// <param name="masterKey">The master key</param>
    /// <exception cref="ArgumentException">Master key must be at least 32 characters long.</exception>
    public CryptoService(string masterKey)
    {
        if (string.IsNullOrEmpty(masterKey) || masterKey.Length < 32)
        {
            throw new ArgumentException("Master key must be at least 32 characters long.");
        }

        _masterKey = masterKey;
    }

    /// <summary>
    /// Encrypts the plain text
    /// </summary>
    /// <param name="plainText">The plain text</param>
    /// <returns>The string</returns>
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_masterKey);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts the encrypted text
    /// </summary>
    /// <param name="encryptedText">The encrypted text</param>
    /// <returns>The string</returns>
    public string Decrypt(string encryptedText)
    {
        var fullCipher = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_masterKey);

        var iv = new byte[aes.BlockSize / 8];
        var cipherBytes = new byte[fullCipher.Length - iv.Length];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
