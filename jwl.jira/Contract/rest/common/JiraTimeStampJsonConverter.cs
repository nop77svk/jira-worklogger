namespace jwl.Jira.Contract.Rest.Common;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JiraTimeStampJsonConverter
    : JsonConverter<JiraTimeStamp>
{
    public override JiraTimeStamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JiraTimeStamp.Parse(reader.GetString() ?? string.Empty); // 2do! how to properly handle the null?

    public override void Write(Utf8JsonWriter writer, JiraTimeStamp value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
