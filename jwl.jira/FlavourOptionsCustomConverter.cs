namespace jwl.jira;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FlavourOptionsCustomConverter
    : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);

        var root = doc.RootElement;
        string? flavourString = root.GetProperty("JiraServer").GetProperty("Flavour").GetString();
        JiraServerFlavour flavour = ServerApiFactory.DecodeServerClass(flavourString) ?? JiraServerFlavour.Vanilla;

        return flavour switch
        {
            JiraServerFlavour.ICTime => JsonSerializer.Deserialize<ICTimeFlavourOptions>(root.GetRawText(), options),
            JiraServerFlavour.Vanilla or JiraServerFlavour.TempoTimeSheets => null,
            _ => throw new JsonException($"Unknown flavour: {flavourString}")
        };
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
