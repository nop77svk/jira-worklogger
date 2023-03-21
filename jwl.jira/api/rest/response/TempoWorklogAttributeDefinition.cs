namespace jwl.jira.api.rest.response;
using System.Text.Json.Serialization;

public class TempoWorklogAttributeDefinition
{
    public int? Id { get; init; }
    public string? Key { get; init; }
    public string? Name { get; init; }
    public common.TempoWorklogAttributeTypeDefinition? Type { get; init; }
    [JsonPropertyName("required")]
    public bool? IsRequired { get; init; }
    public int? Sequence { get; init; }
    public common.TempoWorklogAttributeStaticListValue[]? StaticListValues { get; init; }
}
