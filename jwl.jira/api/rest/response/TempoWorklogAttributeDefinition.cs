namespace jwl.Jira.api.rest.response;
using System.Text.Json.Serialization;
using jwl.Jira.api.rest.common;

public class TempoWorklogAttributeDefinition
{
    public TempoWorklogAttributeDefinition(int id, string key, string name, TempoWorklogAttributeTypeDefinition type, bool isRequired, int sequence, TempoWorklogAttributeStaticListValue[] staticListValues)
    {
        Id = id;
        Key = key;
        Name = name;
        Type = type;
        IsRequired = isRequired;
        Sequence = sequence;
        StaticListValues = staticListValues;
    }

    public int Id { get; }
    public string Key { get; }
    public string Name { get; }
    public common.TempoWorklogAttributeTypeDefinition Type { get; }
    [JsonPropertyName("required")]
    public bool IsRequired { get; }
    public int Sequence { get; }
    public common.TempoWorklogAttributeStaticListValue[] StaticListValues { get; }
}
