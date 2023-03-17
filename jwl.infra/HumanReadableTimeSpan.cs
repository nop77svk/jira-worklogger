namespace jwl.infra;
using System.Globalization;

public static class HumanReadableTimeSpan
{
    public static TimeSpan Parse(string timeSpanStr, string[] timespanTimeFormats)
    {
        TimeSpan result;

        if (!TimeSpan.TryParseExact(timeSpanStr.Replace(" ", null), timespanTimeFormats, CultureInfo.InvariantCulture, out result))
        {
            double timeSpanInNumberOfHours;
            NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
            if (!double.TryParse(timeSpanStr, numberStyles, CultureInfo.InvariantCulture, out timeSpanInNumberOfHours))
            {
                if (!double.TryParse(timeSpanStr.Replace('.', ','), numberStyles, CultureInfo.InvariantCulture, out timeSpanInNumberOfHours))
                {
                    if (!double.TryParse(timeSpanStr.Replace(',', '.'), numberStyles, CultureInfo.InvariantCulture, out timeSpanInNumberOfHours))
                    {
                        throw new FormatException($"Invalid timespan string \"{timeSpanStr}\"");
                    }
                }
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
