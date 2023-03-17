namespace jwl.jira.api.rest.common;
using System.Text.Json.Serialization;

public struct TempoWorklogAttributeStaticListValue
{
    public int Id;
    public string Name;
    public string Value;
    [JsonPropertyName("removed")]
    public bool IsRemoved;
    public int Sequence;
    public int WorkAttributeId;
}
