namespace jwl.jira.api.rest.common;
using System.Text.Json.Serialization;

public struct TempoWorklogAttributeTypeDefinition
{
    public string Name;
    public TempoWorklogAttributeTypeIdentifier Value;
    [JsonPropertyName("systemType")]
    public bool IsSystemType;
}
