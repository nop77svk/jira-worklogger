namespace jwl.Jira.Contract.Rest.Common;

using System.Globalization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(TempoDateJsonConverter))]
public struct TempoDate
{
    public const string TempoDateFormat = @"yyyy-MM-dd";

    public TempoDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static TempoDate Parse(string input)
    {
        return new TempoDate(DateOnly.ParseExact(input, TempoDateFormat, CultureInfo.InvariantCulture));
    }

    public override string ToString() => Value.ToString(TempoDateFormat);
}
