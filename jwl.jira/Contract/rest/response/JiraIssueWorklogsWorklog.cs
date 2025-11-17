namespace jwl.Jira.Contract.Rest.Response;
using jwl.Jira.Contract.Rest.Common;

public class JiraIssueWorklogsWorklog
{
    public JiraIssueWorklogsWorklog(JiraStringedInteger id, JiraStringedInteger issueId, JiraUserInfo author, JiraUserInfo updateAuthor, string comment, JiraTimeStamp created, JiraTimeStamp updated, JiraTimeStamp started, string timeSpent, int timeSpentSeconds)
    {
        Id = id;
        IssueId = issueId;
        Author = author;
        UpdateAuthor = updateAuthor;
        Comment = comment;
        Created = created;
        Updated = updated;
        Started = started;
        TimeSpent = timeSpent;
        TimeSpentSeconds = timeSpentSeconds;
    }

    public Common.JiraStringedInteger Id { get; }
    public Common.JiraStringedInteger IssueId { get; }
    public Common.JiraUserInfo Author { get; }
    public Common.JiraUserInfo UpdateAuthor { get; }
    public string Comment { get; }
    public Common.JiraTimeStamp Created { get; }
    public Common.JiraTimeStamp Updated { get; }
    public Common.JiraTimeStamp Started { get; }
    public string TimeSpent { get; }
    public int TimeSpentSeconds { get; }
}
