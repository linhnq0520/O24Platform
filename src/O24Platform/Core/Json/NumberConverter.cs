using System.Text.Json;
using System.Text.Json.Serialization;

namespace O24OpenAPI.Core.Json;

/// <summary>
/// The number converter class
/// </summary>
/// <seealso cref="JsonConverter{int}"/>
public class NumberConverter : JsonConverter<int>
{
    /// <summary>
    /// Reads the reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The options</param>
    /// <exception cref="JsonException"></exception>
    /// <returns>The int</returns>
    public override int Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (int.TryParse(reader.GetString(), out int result))
            {
                return result;
            }
        }
        else
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return Convert.ToInt32(reader.GetDecimal());
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0;
            }
        }
        throw new JsonException();
    }

    /// <summary>
    /// Writes the writer
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    /// <param name="options">The options</param>
    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
