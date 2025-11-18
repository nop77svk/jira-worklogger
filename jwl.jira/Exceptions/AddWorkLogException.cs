namespace jwl.Jira.Exceptions;

internal class AddWorkLogException
    : JiraIssueSpecificException
{
    public DateTime Moment { get; }
    public int TimeSpentSeconds { get; }
    public string? Activity { get; init; }
    public string? Comment { get; init; }

    public AddWorkLogException(string issueKey, DateTime moment, int timeSpentSeconds)
        : base(issueKey, $"Error adding {timeSpentSeconds} seconds on issue {issueKey} at {moment}")
    {
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    public AddWorkLogException(string issueKey, DateTime moment, int timeSpentSeconds, Exception innerException)
        : base(issueKey, $"Error adding {timeSpentSeconds} seconds on issue {issueKey} at {moment}", innerException)
    {
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }
}
