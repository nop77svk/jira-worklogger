namespace jwl.jira.api.rest.response;
using System.Text.Json.Serialization;
using jwl.jira.api.rest.common;

public class TempoWorklog
{
    [JsonPropertyName("originId")]
    public int? Id { get; init; }
    public string? Started { get; init; }
    public int? TimeSpentSeconds { get; init; }
    public int? BillableSeconds { get; init; }
    [JsonPropertyName("worker")]
    public string? WorkerKey { get; init; }
    public string? Created { get; init; }
    public Dictionary<string, TempoWorklogAttribute>? Attributes { get; init; }
    public string? Comment { get; init; }
    [JsonPropertyName("updater")]
    public string? UpdaterKey { get; init; }
    public string? Updated { get; init; }
}
