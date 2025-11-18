namespace jwl.jira.Exceptions;

using jwl.Jira.Exceptions;

public class JiraGetUserInfoException : JiraClientException
{
    public string UserName { get; }

    public JiraGetUserInfoException(string userName)
        : base(FormatMessage(userName, null))
    {
        UserName = userName;
    }

    public JiraGetUserInfoException(string userName, string message)
        : base(FormatMessage(userName, message))
    {
        UserName = userName;
    }

    public JiraGetUserInfoException(string userName, Exception inner)
        : base(FormatMessage(userName, null), inner)
    {
        UserName = userName;
    }

    private static string FormatMessage(string userName, string? optionalMessage)
        => DefaultMessageFormatter($"Error while retrieving user {userName} info", optionalMessage);
}