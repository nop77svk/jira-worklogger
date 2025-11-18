namespace jwl.Jira.Contract.Rest.Common;

using System.Globalization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(TempoTimeStampJsonConverter))]
public struct TempoTimeStamp
{
    public const string TempoTimeStampFormat = @"yyyy-MM-dd HH:mm:ss.fff";

    public TempoTimeStamp(DateTime value)
    {
        Value = value;
    }

    public DateTime Value { get; }

    public static TempoTimeStamp Parse(string input)
    {
        return new TempoTimeStamp(DateTime.ParseExact(input, TempoTimeStampFormat, CultureInfo.InvariantCulture));
    }

    public override string ToString() => Value.ToString(TempoTimeStampFormat);
}
