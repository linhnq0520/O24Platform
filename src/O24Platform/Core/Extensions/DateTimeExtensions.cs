namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The date time extensions class
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Returns the date time offset using the specified unix time milliseconds
    /// </summary>
    /// <param name="unixTimeMilliseconds">The unix time milliseconds</param>
    /// <returns>The date time offset</returns>
    public static DateTimeOffset ToDateTimeOffset(this long unixTimeMilliseconds)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds);
        return dateTimeOffset;
    }

    /// <summary>
    /// Convert DateTime to Unix timestamp in milliseconds
    /// </summary>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        // Đảm bảo dùng UTC
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    public static long? ToUnixTimeMilliseconds(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return null;

        var dt = dateTime.Value;

        if (dt.Kind == DateTimeKind.Unspecified)
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        return new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeMilliseconds();
    }
}
