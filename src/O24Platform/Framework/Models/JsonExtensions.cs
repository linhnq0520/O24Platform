using System.Text.Json;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The pascal case naming policy class
/// </summary>
/// <seealso cref="JsonNamingPolicy"/>
public class PascalCaseNamingPolicy : JsonNamingPolicy
{
    /// <summary>
    /// Converts the name using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The string</returns>
    public override string ConvertName(string name)
    {
        // Chuyển từ camelCase sang PascalCase: viết hoa chữ cái đầu tiên
        if (string.IsNullOrEmpty(name) || char.IsUpper(name[0]))
        {
            return name;
        }

        return char.ToUpper(name[0]) + name.Substring(1);
    }
}
