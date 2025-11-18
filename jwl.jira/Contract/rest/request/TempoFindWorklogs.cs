namespace jwl.Jira.Contract.Rest.Request;

using System.Text.Json.Serialization;

public class TempoFindWorklogs
{
    public Common.TempoDate From { get; }
    public Common.TempoDate To { get; }

    [JsonPropertyName("taskKey")]
    public string[]? IssueKey { get; init; }

    [JsonPropertyName("worker")]
    public string[]? UserKey { get; init; }

    public TempoFindWorklogs(DateOnly from, DateOnly to)
    {
        From = new Common.TempoDate(from);
        To = new Common.TempoDate(to);
    }
}