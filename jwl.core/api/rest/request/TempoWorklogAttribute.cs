namespace jwl.core.api.rest.request;

public struct TempoWorklogAttribute
{
    public int WorkAttributeId;
    public string Key;
    public string Name;
    public TempoWorklogAttributeType Type;
    public TempoWorklogType Value;
}
