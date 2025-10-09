namespace Jwl.Jira.api.rest.response;
using System.Text.Json.Serialization;
using Jwl.Jira.api.rest.common;

public class TempoWorklog
{
    [JsonPropertyName("originId")]
    public long? Id { get; init; }
    [JsonPropertyName("originTaskId")]
    public long? IssueId { get; init; }
    public common.TempoTimeStamp? Started { get; init; }
    public int? TimeSpentSeconds { get; init; }
    public int? BillableSeconds { get; init; }
    [JsonPropertyName("worker")]
    public string? WorkerKey { get; init; }
    [JsonPropertyName("dateCreated")]
    public common.TempoTimeStamp? Created { get; init; }
    public Dictionary<string, TempoWorklogAttribute>? Attributes { get; init; }
    public string? Comment { get; init; }
    [JsonPropertyName("updater")]
    public string? UpdaterKey { get; init; }
    [JsonPropertyName("dateUpdated")]
    public common.TempoTimeStamp? Updated { get; init; }
}
