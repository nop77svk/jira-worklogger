namespace jwl.Jira.api.rest.common;

using System.Globalization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JiraStringedIntegerJsonConverter))]
public struct JiraStringedInteger
{
    private const NumberStyles AllowedNumberStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.Integer;

    public JiraStringedInteger(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public static JiraStringedInteger Parse(string input)
    {
        return new JiraStringedInteger(long.Parse(input, AllowedNumberStyle, CultureInfo.InvariantCulture));
    }

    public override string ToString() => Value.ToString();
}
