using System.Security.Cryptography;
using System.Text;

namespace O24OpenAPI.Framework.Utils;

public enum HashAlgorithmOption
{
    MD5,
    SHA1,
    SHA256,
    SHA384,
    SHA512,
}

public static class HashUtil
{
    public static string HashSignature(
        this string input,
        HashAlgorithmOption algorithm = HashAlgorithmOption.SHA256,
        Encoding? encoding = null
    )
    {
        encoding ??= Encoding.UTF8;
        byte[] inputBytes = encoding.GetBytes(input);
        byte[] hashBytes;

        using var hashAlg = CreateAlgorithm(algorithm);
        hashBytes = hashAlg.ComputeHash(inputBytes);

        // Convert to hex string
        return ConvertToHex(hashBytes);
    }

    private static HashAlgorithm CreateAlgorithm(HashAlgorithmOption algorithm)
    {
        return algorithm switch
        {
            HashAlgorithmOption.MD5 => MD5.Create(),
            HashAlgorithmOption.SHA1 => SHA1.Create(),
            HashAlgorithmOption.SHA256 => SHA256.Create(),
            HashAlgorithmOption.SHA384 => SHA384.Create(),
            HashAlgorithmOption.SHA512 => SHA512.Create(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(algorithm),
                "Unsupported hash algorithm"
            ),
        };
    }

    private static string ConvertToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    public static string ComputeSha256Hash(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        StringBuilder builder = new();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
