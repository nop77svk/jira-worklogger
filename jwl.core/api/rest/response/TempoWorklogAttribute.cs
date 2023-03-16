namespace jwl.core.api.rest.response;

public struct TempoWorklogAttributeDefinition
{
    public int Id;
    public string Key;
    public string Name;
    public common.TempoWorklogAttributeTypeDefinition Type;
    public bool Required;
    public int Sequence;
    public common.TempoWorklogAttributeStaticListValue[] StaticListValues;
}
