namespace jwl.Jira.Contract.Rest.Common;

using System.Text.Json.Serialization;

public class TempoWorklogAttributeStaticListValue
{
    public int? Id { get; init; }
    public string? Name { get; init; }
    public string? Value { get; init; }

    [JsonPropertyName("removed")]
    public bool? IsRemoved { get; init; }

    public int? Sequence { get; init; }
    public int? WorkAttributeId { get; init; }
}
