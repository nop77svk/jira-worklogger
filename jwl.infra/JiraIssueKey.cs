namespace jwl.Infra;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

public struct JiraIssueKey
{
    public string ProjectKey { get; private set; }
    public int IssueNumber { get; private set; }

    public class ProjectOnlyEqualityComparer : IEqualityComparer<JiraIssueKey>
    {
        public ProjectOnlyEqualityComparer()
        {
        }

        public bool Equals(JiraIssueKey x, JiraIssueKey y)
        {
            return x.ProjectKey.Equals(y.ProjectKey);
        }

        public int GetHashCode([DisallowNull] JiraIssueKey obj)
        {
            return obj.ProjectKey.GetHashCode();
        }
    }

    public JiraIssueKey(string project, int issueNumber)
    {
        ProjectKey = project;
        IssueNumber = issueNumber;
    }

    public JiraIssueKey(string issueKey)
    {
        (ProjectKey, IssueNumber) = SplitIssueKey(issueKey);
    }

    public override string ToString()
    {
        return ProjectKey + "-" + IssueNumber.ToString();
    }

    private static (string, int) SplitIssueKey(string issueKey)
    {
        (string, int) result;

        string[] issueKeySplit = issueKey.Split('-', 3, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (issueKeySplit.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(issueKey), issueKey, $"Invalid format of the issue key; must be <string>-<number>");

        result.Item1 = issueKeySplit[0];

        if (!int.TryParse(issueKeySplit[1], out result.Item2))
            throw new ArgumentOutOfRangeException(nameof(issueKey), issueKey, @"Issue number not an actual number");
        if (result.Item2 <= 0)
            throw new ArgumentOutOfRangeException(nameof(issueKey), issueKey, @"Issue number must be positive");

        return result;
    }
}
