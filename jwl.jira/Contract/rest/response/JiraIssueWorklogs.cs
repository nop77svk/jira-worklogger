<<<<<<<< HEAD:jwl.jira/Contract/rest/response/JiraGetIssueWorklogsResponse.cs
namespace jwl.Jira.Contract.Rest.Response;
========
﻿namespace jwl.Jira.Contract.Rest.Response;
>>>>>>>> origin/main:jwl.jira/Contract/rest/response/JiraIssueWorklogs.cs

public class JiraGetIssueWorklogsResponse
{
    public JiraGetIssueWorklogsResponse(int startAt, int maxResults, int total, JiraIssueWorklogsWorklog[] worklogs)
    {
        StartAt = startAt;
        MaxResults = maxResults;
        Total = total;
        Worklogs = worklogs;
    }

    public int StartAt { get; }
    public int MaxResults { get; }
    public int Total { get; }
    public JiraIssueWorklogsWorklog[] Worklogs { get; }
}