namespace jwl.jira.api.rest.response;

public struct JiraIssueWorklogs
{
    public int StartAt;
    public int MaxResults;
    public int Total;
    public JiraIssueWorklogsWorklog[] Worklogs;
}