namespace jwl.Infra;

using System;

public static class DateTimeConversions
{
    public static long ToUnixTimeStamp(this DateTime dateTime)
        => new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
}