namespace jwl.jira.api.rest.request;
using System.Text.Json.Serialization;
using jwl.jira.api.rest.common;

public class TempoAddWorklogByIssueKey
{
    [JsonPropertyName("originTaskId")]
    public string? IssueKey { get; init; }
    public int? TimeSpentSeconds { get; init; }
    public int? BillableSeconds { get; init; }
    public string? Worker { get; init; }
    public string? Started { get; init; }
    public string? EndDate { get; init; }
    public bool? IncludeNonWorkingDays { get; init; }
    public Dictionary<string, TempoWorklogAttribute>? Attributes { get; init; }
}
