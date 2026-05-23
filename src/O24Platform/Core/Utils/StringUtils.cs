using System.Globalization;
using System.Text;
using Microsoft.VisualBasic;

namespace O24OpenAPI.Core.Utils;

/// <summary>
/// The string utils class
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// Coalesces the values
    /// </summary>
    /// <param name="values">The values</param>
    /// <returns>The string</returns>
    public static string? Coalesce(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        return null;
    }

    /// <summary>
    /// Applies the mask using the specified input
    /// </summary>
    /// <param name="input">The input</param>
    /// <param name="mask">The mask</param>
    /// <returns>The string</returns>
    public static string? ApplyMask(object input, string mask)
    {
        CultureInfo culture = new("en-GB");

        if (input == null || string.IsNullOrEmpty(mask))
        {
            return input?.ToString() ?? string.Empty;
        }

        if (input is string strInput)
        {
            string[] formats =
            [
                "M/d/yyyy h:m:s tt",
                "MM/dd/yyyy hh:mm:ss tt",
                "yyyy-MM-dd HH:mm:ss",
            ];
            if (
                DateTime.TryParseExact(
                    strInput,
                    formats,
                    culture,
                    DateTimeStyles.None,
                    out DateTime parsedDate
                )
            )
            {
                return parsedDate.ToString(mask, culture);
            }
        }

        if (input is int || input is long || input is decimal || input is double || input is float)
        {
            var inputStr = input.ToString();
            if (mask.Contains('#'))
            {
                StringBuilder formatted = new();
                int inputIndex = 0;

                foreach (char c in mask)
                {
                    if (c == '#')
                    {
                        if (inputIndex < inputStr?.Length)
                        {
                            formatted.Append(inputStr[inputIndex++]);
                        }
                    }
                    else
                    {
                        formatted.Append(c);
                    }
                }
                return formatted.ToString();
            }
            return inputStr;
        }

        return input.ToString();
    }

    /// <summary>
    /// Converts the date to long string format using the specified dt
    /// </summary>
    /// <param name="dt">The dt</param>
    /// <param name="fmt">The fmt</param>
    /// <returns>The string</returns>
    public static string ConvertDateToLongStringFormat(DateTime dt, string fmt = "")
    {
        if (string.IsNullOrEmpty(fmt))
        {
            fmt = "dd/MM/yyyy HH:mm:ss";
        }
        var culture = new CultureInfo("en-GB");
        return dt.ToString(fmt, culture);
    }

    /// <summary>
    /// Formats the date time using the specified expression
    /// </summary>
    /// <param name="expression">The expression</param>
    /// <param name="namedFormat">The named format</param>
    /// <exception cref="ArgumentException">Invalid date format </exception>
    /// <returns>The string</returns>
    public static string FormatDateTime(
        DateTime expression,
        DateFormat namedFormat = DateFormat.GeneralDate
    )
    {
        try
        {
            string format = namedFormat switch
            {
                DateFormat.LongDate => "D",
                DateFormat.ShortDate => "d",
                DateFormat.LongTime => "T",
                DateFormat.ShortTime => "HH:mm",
                DateFormat.GeneralDate => (expression.TimeOfDay.Ticks != expression.Ticks)
                    ? ((expression.TimeOfDay.Ticks != 0L) ? "G" : "d")
                    : "T",
                _ => throw new ArgumentException("Invalid date format", nameof(namedFormat)),
            };
            return expression.ToString(format, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "";
        }
    }
}
