using System.Security.Cryptography;

namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The hash helper class
/// </summary>
public class HashHelper
{
    /// <summary>
    /// /// Creates the hash using the specified data
    /// </summary>
    /// <param name="data">The data</param>
    /// <param name="hashAlgorithm">The hash algorithm</param>
    /// <param name="trimByteCount">The trim byte count</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException">Unrecognized hash name</exception>
    /// <returns>The string</returns>
    public static string CreateHash(byte[] data, string hashAlgorithm, int trimByteCount = 0)
    {
        var hashAlgorithm1 =
            (
                !string.IsNullOrEmpty(hashAlgorithm)
                    ? (HashAlgorithm?)CryptoConfig.CreateFromName(hashAlgorithm)
                    : throw new ArgumentNullException(nameof(hashAlgorithm))
            ) ?? throw new ArgumentException("Unrecognized hash name");
        if (trimByteCount <= 0 || data.Length <= trimByteCount)
        {
            return BitConverter
                .ToString(hashAlgorithm1.ComputeHash(data))
                .Replace("-", string.Empty);
        }

        byte[] numArray = new byte[trimByteCount];
        Array.Copy(data, numArray, trimByteCount);
        return BitConverter
            .ToString(hashAlgorithm1.ComputeHash(numArray))
            .Replace("-", string.Empty);
    }
}
