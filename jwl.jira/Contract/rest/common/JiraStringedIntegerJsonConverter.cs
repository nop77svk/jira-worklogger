namespace jwl.Jira.Contract.Rest.Common;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JiraStringedIntegerJsonConverter
    : JsonConverter<JiraStringedInteger>
{
    public override JiraStringedInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JiraStringedInteger.Parse(reader.GetString() ?? string.Empty); // 2do! how to properly handle the null?

    public override void Write(Utf8JsonWriter writer, JiraStringedInteger value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
