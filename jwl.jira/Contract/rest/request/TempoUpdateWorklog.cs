namespace jwl.Jira.Contract.Rest.Request;
using jwl.Jira.Contract.Rest.Common;

public class TempoUpdateWorklog
{
    public TempoDate? Started { get; init; }
    public TempoDate? EndDate { get; init; }
    public int? TimeSpentSeconds { get; init; }
    public int? BillableSeconds { get; init; }
    public bool? IncludeNonWorkingDays { get; init; }
    public int? RemainingEstimate { get; init; }
    public Dictionary<string, TempoWorklogAttribute>? Attributes { get; init; }
    public string? Comment { get; init; }
}
