using System.Security.Cryptography;
using System.Text;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Services.Security;

/// <summary>
/// The encryption service class
/// </summary>
/// <seealso cref="IEncryptionService"/>
public class EncryptionService(SecuritySettings securitySettings) : IEncryptionService
{
    /// <summary>
    /// The security settings
    /// </summary>
    private readonly SecuritySettings _securitySettings = securitySettings;

    /// <summary>
    /// Create salt key
    /// </summary>
    /// <param name="size">Key size</param>
    /// <returns>Salt key</returns>
    public virtual string CreateSaltKey(int size)
    {
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        byte[] array = new byte[size];
        randomNumberGenerator.GetBytes(array);
        return Convert.ToBase64String(array);
    }

    /// <summary>
    /// Create a password hash
    /// </summary>
    /// <param name="password">Password</param>
    /// <param name="saltkey">Salk key</param>
    /// <param name="passwordFormat">Password format (hash algorithm)</param>
    /// <returns>Password hash</returns>
    public virtual string CreatePasswordHash(string password, string saltkey, string passwordFormat)
    {
        return HashHelper.CreateHash(Encoding.UTF8.GetBytes(password + saltkey), passwordFormat);
    }

    /// <summary>
    /// Encrypt text
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <param name="encryptionPrivateKey">Encryption private key</param>
    /// <param name="encryptType"></param>
    /// <returns>Encrypted text</returns>
    public virtual string EncryptText(
        string plainText,
        string encryptionPrivateKey = "",
        string encryptType = "3DES"
    )
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }
        if (string.IsNullOrEmpty(encryptionPrivateKey))
        {
            encryptionPrivateKey = _securitySettings.EncryptionKey;
        }
        return CommonHelper.EncryptText(plainText, encryptionPrivateKey, encryptType);
    }

    /// <summary>
    /// Decrypt text
    /// </summary>
    /// <param name="cipherText">Text to decrypt</param>
    /// <param name="encryptionPrivateKey">Encryption private key</param>
    /// <param name="encryptType"></param>
    /// <returns>Decrypted text</returns>
    public virtual string DecryptText(
        string cipherText,
        string encryptionPrivateKey = "",
        string encryptType = "3DES"
    )
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }
        if (string.IsNullOrEmpty(encryptionPrivateKey))
        {
            encryptionPrivateKey = _securitySettings.EncryptionKey;
        }
        return CommonHelper.DecryptText(cipherText, encryptionPrivateKey, encryptType);
    }
}
