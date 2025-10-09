namespace Jwl.Infra;

using System.Globalization;

public static class HumanReadableTimeSpan
{
    public static readonly string[] AvailableTimeSpanTimeFormats = [
        @"hh\:mm\:ss",
        @"hh\:mm",
        @"mm",
        @"hh'h'mm",
        @"hh'h'mm'm'"
    ];

    public static TimeSpan Parse(string timeSpanStr)
        => Parse(timeSpanStr, AvailableTimeSpanTimeFormats);

    public static TimeSpan Parse(string timeSpanStr, string[] timespanTimeFormats)
        => TryParse(timeSpanStr, timespanTimeFormats, out TimeSpan parsedTimeSpan)
        ? parsedTimeSpan
        : throw new FormatException($"Invalid timespan string \"{timeSpanStr}\"");

    public static bool TryParse(string timeSpanStr, out TimeSpan conversionResult)
        => TryParse(timeSpanStr, AvailableTimeSpanTimeFormats, out conversionResult);

    public static bool TryParse(string timeSpanStr, string[] timespanTimeFormats, out TimeSpan conversionResult)
    {
        if (!TimeSpan.TryParseExact(timeSpanStr.Replace(" ", string.Empty), timespanTimeFormats, CultureInfo.InvariantCulture, out conversionResult))
        {
            if (!InexactDecimal.TryParse(timeSpanStr, out double timeSpanInNumberOfHours))
            {
                return false;
            }

            conversionResult = TimeSpan.FromHours(timeSpanInNumberOfHours);
        }

        return true;
    }
}
