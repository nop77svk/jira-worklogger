#pragma warning disable SA1313
namespace jwl.jira;

public record WorkLog(long Id, long IssueId, string? AuthorName, string? AuthorKey, DateTime Created, DateTime Started, int TimeSpentSeconds, string? WorkLogType, string Comment)
{
}
