using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Validators;

/// <summary>
/// The validator extensions class
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Ises the decimal using the specified rule builder
    /// </summary>
    /// <typeparam name="TModel">The model</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <param name="maxValue">The max value</param>
    /// <returns>A rule builder options of t model and decimal</returns>
    public static IRuleBuilderOptions<TModel, decimal> IsDecimal<TModel>(
        this IRuleBuilder<TModel, decimal> ruleBuilder,
        decimal maxValue
    )
    {
        return ruleBuilder.SetValidator(
            (IPropertyValidator<TModel, decimal>)
                (object)new DecimalPropertyValidator<TModel, decimal>(maxValue)
        );
    }

    /// <summary>
    /// Validates the data using the specified json data
    /// </summary>
    /// <param name="jsonData">The json data</param>
    /// <param name="specifications">The specifications</param>
    /// <returns>The string</returns>
    public static string ValidateData(string jsonData, List<FieldDefinitionModel> specifications)
    {
        Dictionary<string, JsonElement> dictionary = JsonSerializer.Deserialize<
            Dictionary<string, JsonElement>
        >(jsonData);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (FieldDefinitionModel specification in specifications)
        {
            if (!dictionary.ContainsKey(specification.FieldName) && specification.IsRequired)
            {
                StringBuilder stringBuilder2 = stringBuilder;
                StringBuilder stringBuilder3 = stringBuilder2;
                StringBuilder.AppendInterpolatedStringHandler handler =
                    new StringBuilder.AppendInterpolatedStringHandler(31, 1, stringBuilder2);
                handler.AppendLiteral("[");
                handler.AppendFormatted(specification.FieldName);
                handler.AppendLiteral("] is required but not provided");
                stringBuilder3.AppendLine(ref handler);
            }
            else
            {
                if (!dictionary.ContainsKey(specification.FieldName))
                {
                    continue;
                }
                JsonElement jsonElement = dictionary[specification.FieldName];
                if (!Enum.IsDefined(typeof(DataTypeEnum), specification.DataType))
                {
                    StringBuilder stringBuilder2 = stringBuilder;
                    StringBuilder stringBuilder4 = stringBuilder2;
                    StringBuilder.AppendInterpolatedStringHandler handler =
                        new StringBuilder.AppendInterpolatedStringHandler(27, 1, stringBuilder2);
                    handler.AppendLiteral("[");
                    handler.AppendFormatted(specification.DataType);
                    handler.AppendLiteral("] is not a valid DataType ");
                    stringBuilder4.AppendLine(ref handler);
                }
                switch (specification.DataType)
                {
                    case DataTypeEnum.Number:
                        if (jsonElement.ValueKind != JsonValueKind.Number)
                        {
                            StringBuilder stringBuilder2 = stringBuilder;
                            StringBuilder stringBuilder7 = stringBuilder2;
                            StringBuilder.AppendInterpolatedStringHandler handler =
                                new StringBuilder.AppendInterpolatedStringHandler(
                                    25,
                                    1,
                                    stringBuilder2
                                );
                            handler.AppendLiteral("[");
                            handler.AppendFormatted(specification.FieldName);
                            handler.AppendLiteral("] must be of type Number");
                            stringBuilder7.AppendLine(ref handler);
                        }
                        break;
                    case DataTypeEnum.String:
                        if (jsonElement.ValueKind != JsonValueKind.String)
                        {
                            StringBuilder stringBuilder2 = stringBuilder;
                            StringBuilder stringBuilder6 = stringBuilder2;
                            StringBuilder.AppendInterpolatedStringHandler handler =
                                new StringBuilder.AppendInterpolatedStringHandler(
                                    25,
                                    1,
                                    stringBuilder2
                                );
                            handler.AppendLiteral("[");
                            handler.AppendFormatted(specification.FieldName);
                            handler.AppendLiteral("] must be of type String");
                            stringBuilder6.AppendLine(ref handler);
                        }
                        break;
                    case DataTypeEnum.Boolean:
                        if (
                            jsonElement.ValueKind != JsonValueKind.True
                            && jsonElement.ValueKind != JsonValueKind.False
                        )
                        {
                            StringBuilder stringBuilder2 = stringBuilder;
                            StringBuilder stringBuilder5 = stringBuilder2;
                            StringBuilder.AppendInterpolatedStringHandler handler =
                                new StringBuilder.AppendInterpolatedStringHandler(
                                    26,
                                    1,
                                    stringBuilder2
                                );
                            handler.AppendLiteral("[");
                            handler.AppendFormatted(specification.FieldName);
                            handler.AppendLiteral("] must be of type Boolean");
                            stringBuilder5.AppendLine(ref handler);
                        }
                        break;
                }
                if (specification.ValueRange == null)
                {
                    continue;
                }
                if (
                    !specification.ValueRange.Nullable
                    && jsonElement.ValueKind == JsonValueKind.Null
                )
                {
                    StringBuilder stringBuilder2 = stringBuilder;
                    StringBuilder stringBuilder8 = stringBuilder2;
                    StringBuilder.AppendInterpolatedStringHandler handler =
                        new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
                    handler.AppendLiteral("[");
                    handler.AppendFormatted(specification.FieldName);
                    handler.AppendLiteral("] cannot be null");
                    stringBuilder8.AppendLine(ref handler);
                }
                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    string @string = jsonElement.GetString();
                    if (specification.ValueRange.LengthRange.Count == 2)
                    {
                        int num = specification.ValueRange.LengthRange[0];
                        int num2 = specification.ValueRange.LengthRange[1];
                        if (@string.Length < num || @string.Length > num2)
                        {
                            StringBuilder stringBuilder2 = stringBuilder;
                            StringBuilder stringBuilder9 = stringBuilder2;
                            StringBuilder.AppendInterpolatedStringHandler handler =
                                new StringBuilder.AppendInterpolatedStringHandler(
                                    31,
                                    3,
                                    stringBuilder2
                                );
                            handler.AppendLiteral("[");
                            handler.AppendFormatted(specification.FieldName);
                            handler.AppendLiteral("] length must be between ");
                            handler.AppendFormatted(num);
                            handler.AppendLiteral(" and ");
                            handler.AppendFormatted(num2);
                            stringBuilder9.AppendLine(ref handler);
                        }
                    }
                }
                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    decimal @decimal = jsonElement.GetDecimal();
                    if (specification.ValueRange.DecimalRange.Count == 2)
                    {
                        decimal num3 = specification.ValueRange.DecimalRange[0];
                        decimal num4 = specification.ValueRange.DecimalRange[1];
                        if (@decimal < num3 || @decimal > num4)
                        {
                            StringBuilder stringBuilder2 = stringBuilder;
                            StringBuilder stringBuilder10 = stringBuilder2;
                            StringBuilder.AppendInterpolatedStringHandler handler =
                                new StringBuilder.AppendInterpolatedStringHandler(
                                    24,
                                    3,
                                    stringBuilder2
                                );
                            handler.AppendLiteral("[");
                            handler.AppendFormatted(specification.FieldName);
                            handler.AppendLiteral("] must be between ");
                            handler.AppendFormatted(num3);
                            handler.AppendLiteral(" and ");
                            handler.AppendFormatted(num4);
                            stringBuilder10.AppendLine(ref handler);
                        }
                    }
                }
                if (specification.ValueRange.EnumList.Count > 0)
                {
                    bool flag = false;
                    foreach (object @enum in specification.ValueRange.EnumList)
                    {
                        if (jsonElement.ToString() == @enum.ToString())
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        string value = string.Join(", ", specification.ValueRange.EnumList);
                        StringBuilder stringBuilder2 = stringBuilder;
                        StringBuilder stringBuilder11 = stringBuilder2;
                        StringBuilder.AppendInterpolatedStringHandler handler =
                            new StringBuilder.AppendInterpolatedStringHandler(
                                48,
                                2,
                                stringBuilder2
                            );
                        handler.AppendLiteral("[");
                        handler.AppendFormatted(specification.FieldName);
                        handler.AppendLiteral("] must be one of the specified enum values : [");
                        handler.AppendFormatted(value);
                        handler.AppendLiteral("]");
                        stringBuilder11.AppendLine(ref handler);
                    }
                }
                if (
                    !string.IsNullOrEmpty(specification.ValueRange.Regex)
                    && jsonElement.ValueKind == JsonValueKind.String
                    && !Regex.IsMatch(jsonElement.GetString(), specification.ValueRange.Regex)
                )
                {
                    StringBuilder stringBuilder2 = stringBuilder;
                    StringBuilder stringBuilder12 = stringBuilder2;
                    StringBuilder.AppendInterpolatedStringHandler handler =
                        new StringBuilder.AppendInterpolatedStringHandler(45, 1, stringBuilder2);
                    handler.AppendLiteral("[");
                    handler.AppendFormatted(specification.FieldName);
                    handler.AppendLiteral("] does not match the specified regex pattern");
                    stringBuilder12.AppendLine(ref handler);
                }
            }
        }
        return string.Join(
            Environment.NewLine,
            stringBuilder
                .ToString()
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        );
    }
}
