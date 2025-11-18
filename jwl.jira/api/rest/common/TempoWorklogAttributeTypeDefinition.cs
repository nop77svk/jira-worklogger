namespace jwl.Jira.api.rest.common;
using System.Text.Json.Serialization;

public class TempoWorklogAttributeTypeDefinition
{
    public string? Name { get; init; }
    public TempoWorklogAttributeTypeIdentifier? Value { get; init; }
    [JsonPropertyName("systemType")]
    public bool? IsSystemType { get; init; }
}
