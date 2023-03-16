namespace jwl.core.api.rest.response;
using System.Text.Json.Serialization;

public struct TempoWorklogAttributeDefinition
{
    public int Id;
    public string Key;
    public string Name;
    public common.TempoWorklogAttributeTypeDefinition Type;
    [JsonPropertyName("required")]
    public bool IsRequired;
    public int Sequence;
    public common.TempoWorklogAttributeStaticListValue[]? StaticListValues;
}
