namespace Jwl.Jira.api.rest.response;
using Jwl.Jira.api.rest.common;

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

    public common.JiraStringedInteger Id { get; }
    public common.JiraStringedInteger IssueId { get; }
    public common.JiraUserInfo Author { get; }
    public common.JiraUserInfo UpdateAuthor { get; }
    public string Comment { get; }
    public common.JiraTimeStamp Created { get; }
    public common.JiraTimeStamp Updated { get; }
    public common.JiraTimeStamp Started { get; }
    public string TimeSpent { get; }
    public int TimeSpentSeconds { get; }
}
