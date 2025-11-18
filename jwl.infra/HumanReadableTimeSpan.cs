namespace jwl.Infra;

using System.Globalization;

public static class HumanReadableTimeSpan
{
    public static TimeSpan Parse(string timeSpanStr, string[] timespanTimeFormats)
    {
        TimeSpan result;

        if (!TimeSpan.TryParseExact(timeSpanStr.Replace(" ", null), timespanTimeFormats, CultureInfo.InvariantCulture, out result))
        {
            if (!InexactDecimal.TryParse(timeSpanStr, out double timeSpanInNumberOfHours))
            {
                throw new FormatException($"Invalid timespan string \"{timeSpanStr}\"");
            }

            result = TimeSpan.FromHours(timeSpanInNumberOfHours);
        }

        return result;
    }

    public static bool TryParse(string timeSpanStr, string[] timespanTimeFormats, out TimeSpan result)
    {
        try
        {
            result = Parse(timeSpanStr, timespanTimeFormats);
            return true;
        }
        catch (FormatException)
        {
            result = TimeSpan.Zero;
            return false;
        }
    }
}