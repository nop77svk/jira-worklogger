namespace jwl.Jira.api.rest.request;
using System.Text.Json.Serialization;

public class TempoFindWorklogs
{
    public common.TempoDate From { get; }
    public common.TempoDate To { get; }

    [JsonPropertyName("taskKey")]
    public string[]? IssueKey { get; init; }
    [JsonPropertyName("worker")]
    public string[]? UserKey { get; init; }

    public TempoFindWorklogs(DateOnly from, DateOnly to)
    {
        From = new common.TempoDate(from);
        To = new common.TempoDate(to);
    }
}
