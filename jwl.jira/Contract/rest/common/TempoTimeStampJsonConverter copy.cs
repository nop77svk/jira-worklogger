namespace jwl.Jira.Contract.Rest.Common;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TempoTimeStampJsonConverter
    : JsonConverter<TempoTimeStamp>
{
    public override TempoTimeStamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TempoTimeStamp.Parse(reader.GetString() ?? string.Empty); // 2do! how to properly handle the null?

    public override void Write(Utf8JsonWriter writer, TempoTimeStamp value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}