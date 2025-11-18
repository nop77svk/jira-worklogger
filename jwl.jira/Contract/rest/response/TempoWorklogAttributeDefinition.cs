namespace jwl.Jira.Contract.Rest.Response;

using System.Text.Json.Serialization;

using jwl.Jira.Contract.Rest.Common;

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
    public Common.TempoWorklogAttributeTypeDefinition Type { get; }

    [JsonPropertyName("required")]
    public bool IsRequired { get; }

    public int Sequence { get; }
    public Common.TempoWorklogAttributeStaticListValue[] StaticListValues { get; }
}
