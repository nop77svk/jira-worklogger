namespace jwl.Jira.Contract.Rest.Common;

using System.Globalization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JiraTimeStampJsonConverter))]
public struct JiraTimeStamp
{
    public const string JiraTimeStampFormat = @"yyyy-MM-dd""T""HH:mm:ss.fffzzzz";

    public JiraTimeStamp(DateTime value)
    {
        Value = value;
    }

    public DateTime Value { get; }

    public static JiraTimeStamp Parse(string input)
    {
        return new JiraTimeStamp(DateTime.ParseExact(input, JiraTimeStampFormat, CultureInfo.InvariantCulture));
    }

    public override string ToString() => Value.ToString(JiraTimeStampFormat);
}
