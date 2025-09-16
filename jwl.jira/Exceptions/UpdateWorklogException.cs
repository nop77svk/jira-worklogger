namespace Jwl.Jira.Exceptions;

public class UpdateWorklogException
    : JiraIssueSpecificException
{
    public long WorklogId { get; }
    public DateTime Moment { get; }
    public int TimeSpentSeconds { get; }
    public string? Activity { get; init; }
    public string? Comment { get; init; }

    public UpdateWorklogException(string issueKey, long worklogId, DateTime moment, int timeSpentSeconds)
        : base(issueKey, $"Error updating worklog ID {worklogId} with {timeSpentSeconds} seconds on issue {issueKey} at {moment}")
    {
        WorklogId = worklogId;
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    public UpdateWorklogException(string issueKey, long worklogId, DateTime moment, int timeSpentSeconds, Exception innerException)
        : base(issueKey, $"Error updating worklog ID {worklogId} with {timeSpentSeconds} seconds on issue {issueKey} at {moment}", innerException)
    {
        WorklogId = worklogId;
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }
}
