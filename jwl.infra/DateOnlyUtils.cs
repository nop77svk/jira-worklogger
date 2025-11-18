namespace jwl.Infra;

public static class DateOnlyUtils
{
    public static (DateTime, DateTime) DateOnlyRangeToDateTimeRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentOutOfRangeException(nameof(to), to, $"{nameof(to)} cannot be less than {nameof(from)}");
        }

        DateTime minDt = from.ToDateTime(TimeOnly.MinValue);
        DateTime supDt = to.ToDateTime(TimeOnly.MinValue).AddDays(1);

        return (minDt, supDt);
    }

    public static int NumberOfDaysInRange(DateOnly from, DateOnly to)
    {
        (DateTime minDt, DateTime supDt) = DateOnlyRangeToDateTimeRange(from, to);
        return (int)(supDt - minDt).TotalDays;
    }

    public static int NumberOfDaysTo(this DateOnly from, DateOnly to)
    {
        return NumberOfDaysInRange(from, to) - 1;
    }

    public static int NumberOfDaysSince(this DateOnly to, DateOnly from)
    {
        return NumberOfDaysInRange(from, to) - 1;
    }
}
