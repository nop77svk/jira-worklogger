namespace jwl.Jira.api.rest.common;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TempoDateJsonConverter
    : JsonConverter<TempoDate>
{
    public override TempoDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TempoDate.Parse(reader.GetString() ?? string.Empty); // 2do! how to properly handle the null?

    public override void Write(Utf8JsonWriter writer, TempoDate value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
