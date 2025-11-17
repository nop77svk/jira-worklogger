namespace jwl.Infra;
using System.Globalization;

public static class InexactDecimal
{
    public static double Parse(string numberStr)
    {
        double result;

        NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        if (!double.TryParse(numberStr, numberStyles, CultureInfo.InvariantCulture, out result))
        {
            if (!double.TryParse(numberStr.Replace('.', ','), numberStyles, CultureInfo.InvariantCulture, out result))
            {
                if (!double.TryParse(numberStr.Replace(',', '.'), numberStyles, CultureInfo.InvariantCulture, out result))
                {
                    throw new FormatException($"Cannot parse a decimal from string \"{numberStr}\"");
                }
            }
        }

        return result;
    }

    public static bool TryParse(string numberStr, out double result)
    {
        try
        {
            result = Parse(numberStr);
            return true;
        }
        catch (FormatException)
        {
            result = double.NaN;
            return false;
        }
    }
}
