using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;
using System.Text.Json;

namespace O24OpenAPI.Framework.Utils;

public static class SignatureUtils
{
    private const int TimestampValiditySeconds = 300;
    private const int NonceCacheExpirationMinutes = 6;
    private const string NonceCachePrefix = "nonce:";
    public const string SignatureKeyTemplate = "{0}|{1}";
    public const string SignatureDataTemplate = "{0}_{1}_{2}";

    public static bool VerifySignature(
        object requestObject,
        string timestamp,
        string signatureHex,
        string nonceHex,
        string publicKeyHex,
        IMemoryCache memoryCache,
        out string errorDetails
    )
    {
        try
        {
            if (!IsNonceValidAndStore(nonceHex, memoryCache, out errorDetails))
            {
                Console.WriteLine($"Nonce check failed: {errorDetails}");
                return false;
            }

            string normalizedJsonString = NormalizeJson(requestObject);
            Console.WriteLine($"Normalized JSON: {normalizedJsonString}");

            string dataToVerify = normalizedJsonString + timestamp + nonceHex;
            Console.WriteLine($"Data to Verify: {dataToVerify}");

            bool isValid = VerifySignatureInternal(
                dataToVerify,
                signatureHex,
                publicKeyHex,
                out errorDetails
            );
            if (!isValid)
            {
                Console.WriteLine($"Internal signature verification failed: {errorDetails}");
            }
            return isValid;
        }
        catch (Exception ex)
        {
            errorDetails = $"Unexpected error during signature verification: {ex.Message}";
            Console.WriteLine($"{errorDetails}\nStackTrace: {ex.StackTrace}");
            return false;
        }
    }

    private static string NormalizeJson(object requestObject)
    {
        if (requestObject is string json)
        {
            return json;
        }
        JsonSerializerOptions options = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false,
        };

        string tempJson = JsonSerializer.Serialize(requestObject, options);

        var sortedDictionary = JsonSerializer.Deserialize<SortedDictionary<string, JsonElement>>(
            tempJson
        );
        if (sortedDictionary == null)
        {
            if (requestObject == null)
            {
                return "{}";
            }

            throw new ArgumentException(
                "requestObject cannot be deserialized into a key-value structure for sorting."
            );
        }

        string normalizedJson = JsonSerializer.Serialize(sortedDictionary, options);

        return normalizedJson;
    }

    private static bool IsTimestampValid(string timestamp, out string errorDetails)
    {
        errorDetails = string.Empty;
        if (!long.TryParse(timestamp, out long unixTimestamp))
        {
            errorDetails = "Invalid timestamp format (not a number).";
            return false;
        }

        try
        {
            DateTimeOffset timestampDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            var now = DateTimeOffset.UtcNow;
            var diff = Math.Abs((now - timestampDateTime).TotalSeconds);

            if (diff > TimestampValiditySeconds)
            {
                errorDetails =
                    $"Timestamp is outside the allowed window ({TimestampValiditySeconds}s). Difference: {diff:F0}s.";
                return false;
            }
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            errorDetails = "Timestamp value is out of the valid range for DateTimeOffset.";
            return false;
        }
    }

    private static bool IsNonceValidAndStore(
        string nonceHex,
        IMemoryCache memoryCache,
        out string errorDetails
    )
    {
        errorDetails = string.Empty;
        if (string.IsNullOrWhiteSpace(nonceHex) || nonceHex.Length != 32)
        {
            errorDetails = "Invalid nonce format or length.";
            return false;
        }

        var cacheKey = NonceCachePrefix + nonceHex;

        if (memoryCache.TryGetValue(cacheKey, out _))
        {
            errorDetails = "Nonce has already been used (replay attack detected).";
            return false;
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
            TimeSpan.FromMinutes(NonceCacheExpirationMinutes)
        );

        memoryCache.Set(cacheKey, true, cacheEntryOptions);

        return true;
    }

    private static bool VerifySignatureInternal(
        string data,
        string signatureHex,
        string publicKeyHex,
        out string errorDetails
    )
    {
        errorDetails = string.Empty;
        try
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = SHA256.HashData(dataBytes);
            Console.WriteLine(
                $"Calculated Hash (Hex): {Convert.ToHexString(hash).ToLowerInvariant()}"
            );

            if (signatureHex.Length != 128)
            {
                errorDetails =
                    $"Invalid signature length. Expected 128 hex characters, got {signatureHex.Length}.";
                return false;
            }
            byte[] signatureBytes = Convert.FromHexString(signatureHex);

            var r = new BigInteger(1, signatureBytes.Take(32).ToArray());
            var s = new BigInteger(1, signatureBytes.Skip(32).Take(32).ToArray());

            byte[] publicKeyBytes = Convert.FromHexString(publicKeyHex);
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

            Org.BouncyCastle.Math.EC.ECPoint point;
            try
            {
                point = curve.Curve.DecodePoint(publicKeyBytes);
                if (point == null || point.IsInfinity)
                {
                    errorDetails = "Failed to decode public key point or point is at infinity.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorDetails = $"Error decoding public key point: {ex.Message}";
                return false;
            }

            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

            var signer = new ECDsaSigner();
            signer.Init(false, publicKeyParams);

            bool isValid = signer.VerifySignature(hash, r, s);
            if (!isValid)
            {
                errorDetails =
                    "ECDSA signature verification failed. The signature does not match the data and public key.";
            }
            return isValid;
        }
        catch (FormatException ex)
        {
            errorDetails = $"Hex string format error during verification: {ex.Message}";
            Console.WriteLine(errorDetails);
            return false;
        }
        catch (Exception ex)
        {
            errorDetails = $"Internal error during signature verification: {ex.Message}";
            Console.WriteLine($"{errorDetails}\nStackTrace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Verify signature for given data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="timestamp"></param>
    /// <param name="nounce"></param>
    /// <param name="privateKey"></param>
    /// <param name="requestSignature"></param>
    /// <returns></returns>
    public static bool VerifySignature(
        this object data,
        string timestamp,
        string nounce,
        string privateKey,
        string requestSignature
    )
    {
        string normalizedJsonString = NormalizeJson(data);

        string rawSignatureData = string.Format(
            SignatureDataTemplate,
            normalizedJsonString,
            timestamp,
            nounce
        );
        rawSignatureData = string.Format(SignatureKeyTemplate, privateKey, rawSignatureData);

        string signature = rawSignatureData.HashSignature(HashAlgorithmOption.SHA512);
        return signature.Equals(requestSignature);
    }
}
