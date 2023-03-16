namespace jwl.core.api.rest.common;
using System.Text.Json.Serialization;

public struct TempoWorklogAttributeStaticListValue
{
    public int Id;
    public string Name;
    public TempoWorklogType Value;
    [JsonPropertyName("removed")]
    public bool IsRemoved;
    public int Sequence;
    public int WorkAttributeId;
}
