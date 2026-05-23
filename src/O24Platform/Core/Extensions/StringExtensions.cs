using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using O24OpenAPI.Core.Enums;

namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The string extensions class
/// </summary>
public static class StringExtensions
{
    public static bool HasValue(this string value) =>
        !string.IsNullOrWhiteSpace(value) && value != "null";

    public static bool NullOrEmpty(this string value) => string.IsNullOrEmpty(value);

    public static bool NullOrWhiteSpace(this string value) => !value.HasValue();

    public static T ToEnum<T>(this string value, T defaultValue = default)
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (Enum.TryParse(value, true, out T result))
        {
            return result;
        }

        return defaultValue;
    }

    public static string Coalesce(this string str, params string[] values)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        return str;
    }

    public static string? GetValueFromJson(this string json, string key)
    {
        try
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
            {
                return null;
            }

            ReadOnlySpan<byte> jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(jsonBytes);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == key)
                {
                    reader.Read();
                    return reader.TokenType switch
                    {
                        JsonTokenType.String => reader.GetString(),
                        JsonTokenType.Number => reader.GetDouble().ToString(),
                        JsonTokenType.True => "true",
                        JsonTokenType.False => "false",
                        JsonTokenType.Null => null,
                        _ => reader.GetString(),
                    };
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public static string Hash(
        this string input,
        HashAlgorithmType algorithm = HashAlgorithmType.SHA256,
        bool useBase64 = true
    )
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException(nameof(input));
        }

        using HashAlgorithm hashAlgorithm = algorithm switch
        {
            HashAlgorithmType.MD5 => MD5.Create(),
            HashAlgorithmType.SHA1 => SHA1.Create(),
            HashAlgorithmType.SHA256 => SHA256.Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), "Invalid algorithm."),
        };

        byte[] hashBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        return useBase64
            ? Convert.ToBase64String(hashBytes)
            : BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static DateTime ToDateTime(this string dateTimeString, string format = "o")
    {
        if (string.IsNullOrEmpty(dateTimeString))
        {
            throw new ArgumentNullException(nameof(dateTimeString));
        }

        return DateTime.ParseExact(dateTimeString, format, null);
    }

    public static string ToString(this DateTime dateTime, string format = "o")
    {
        return dateTime.ToString(format);
    }

    public static string DecimalToString(this decimal value, string format = "o") =>
        value.ToString(format);

    public static decimal StringToDecimal(string value) =>
        decimal.TryParse(value, out decimal v) ? v : 0;

    #region Equals Methods

    public static bool EqualsOrdinal(this string strA, string strB)
    {
        if (ReferenceEquals(strA, strB))
        {
            return true;
        }

        if (strA == null || strB == null)
        {
            return false;
        }

        return string.Equals(strA, strB, StringComparison.Ordinal);
    }

    public static bool EqualsOrdinalIgnoreCase(this string? strA, string? strB)
    {
        if (ReferenceEquals(strA, strB))
        {
            return true;
        }

        if (strA == null || strB == null)
        {
            return false;
        }

        return string.Equals(strA, strB, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EqualsCurrentCulture(this string strA, string strB)
    {
        if (ReferenceEquals(strA, strB))
        {
            return true;
        }

        if (strA == null || strB == null)
        {
            return false;
        }

        return string.Equals(strA, strB, StringComparison.CurrentCulture);
    }

    public static bool EqualsCurrentCultureIgnoreCase(this string strA, string strB)
    {
        if (ReferenceEquals(strA, strB))
        {
            return true;
        }

        if (strA == null || strB == null)
        {
            return false;
        }

        return string.Equals(strA, strB, StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool EqualsInvariantCulture(this string strA, string strB)
    {
        if (ReferenceEquals(strA, strB))
        {
            return true;
        }

        if (strA == null || strB == null)
        {
            return false;
        }

        return string.Equals(strA, strB, StringComparison.InvariantCulture);
    }

    public static bool EqualsInvariantCultureIgnoreCase(this string strA, string strB)
    {
        if (ReferenceEquals(strA, strB))
        {
            return true;
        }

        if (strA == null || strB == null)
        {
            return false;
        }

        return string.Equals(strA, strB, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region StartsWith Methods

    public static bool StartsWithOrdinal(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.StartsWith(value, StringComparison.Ordinal);
    }

    public static bool StartsWithOrdinalIgnoreCase(this string? str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.StartsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool StartsWithCurrentCulture(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.StartsWith(value, StringComparison.CurrentCulture);
    }

    public static bool StartsWithCurrentCultureIgnoreCase(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.StartsWith(value, StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool StartsWithInvariantCulture(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.StartsWith(value, StringComparison.InvariantCulture);
    }

    public static bool StartsWithInvariantCultureIgnoreCase(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region EndsWith Methods

    public static bool EndsWithOrdinal(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.EndsWith(value, StringComparison.Ordinal);
    }

    public static bool EndsWithOrdinalIgnoreCase(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EndsWithCurrentCulture(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.EndsWith(value, StringComparison.CurrentCulture);
    }

    public static bool EndsWithCurrentCultureIgnoreCase(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.EndsWith(value, StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool EndsWithInvariantCulture(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.EndsWith(value, StringComparison.InvariantCulture);
    }

    public static bool EndsWithInvariantCultureIgnoreCase(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        if (value.Length > str.Length)
        {
            return false;
        }

        return str.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region Contains Methods

    public static bool ContainsOrdinal(this string? str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        return str.Contains(value);
    }

    public static bool ContainsOrdinalIgnoreCase(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        return str.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsCurrentCulture(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        return str.Contains(value, StringComparison.CurrentCulture);
    }

    public static bool ContainsCurrentCultureIgnoreCase(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        return str.Contains(value, StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool ContainsInvariantCulture(this string str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        return str.Contains(value, StringComparison.InvariantCulture);
    }

    public static bool ContainsInvariantCultureIgnoreCase(this string? str, string value)
    {
        if (str == null)
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }

        return str.Contains(value, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion
}
