namespace jwl.Jira.Contract.Rest.Response;

using System.Text.Json.Serialization;

using jwl.Jira.Contract.Rest.Common;

public class TempoWorklog
{
    [JsonPropertyName("originId")]
    public long? Id { get; init; }

    [JsonPropertyName("originTaskId")]
    public long? IssueId { get; init; }

    public Common.TempoTimeStamp? Started { get; init; }
    public int? TimeSpentSeconds { get; init; }
    public int? BillableSeconds { get; init; }

    [JsonPropertyName("worker")]
    public string? WorkerKey { get; init; }

    [JsonPropertyName("dateCreated")]
    public Common.TempoTimeStamp? Created { get; init; }

    public Dictionary<string, TempoWorklogAttribute>? Attributes { get; init; }
    public string? Comment { get; init; }

    [JsonPropertyName("updater")]
    public string? UpdaterKey { get; init; }

    [JsonPropertyName("dateUpdated")]
    public Common.TempoTimeStamp? Updated { get; init; }
}
