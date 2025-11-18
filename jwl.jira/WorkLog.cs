#pragma warning disable SA1313

namespace jwl.Jira;

public record WorkLog(long Id, long IssueId, string? AuthorAccountId, string? AuthorKey, string? AuthorName, DateTime Created, DateTime Started, int TimeSpentSeconds, string? Activity, string Comment)
{
}
