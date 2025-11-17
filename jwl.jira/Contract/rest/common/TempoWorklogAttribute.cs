namespace jwl.Jira.Contract.Rest.Common;

public class TempoWorklogAttribute
{
    public int? WorkAttributeId { get; init; }
    public string? Key { get; init; }
    public string? Name { get; init; }
    public TempoWorklogAttributeTypeIdentifier? Type { get; init; }
    public string? Value { get; init; }
}
