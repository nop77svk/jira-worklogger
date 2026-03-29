namespace jwl.Jira.Exceptions;

public class JiraGetUserInfoException : JiraClientException
{
    public string UserName { get; }

    public JiraGetUserInfoException(string userName)
        : this(userName, (string?)null)
    {
    }

    public JiraGetUserInfoException(string userName, string? message)
        : base(FormatMessage(userName, message))
    {
        UserName = userName;
    }

    public JiraGetUserInfoException(string userName, Exception inner)
        : this(userName, (string?)null, inner)
    {
    }

    public JiraGetUserInfoException(string userName, string? message, Exception inner)
        : base(FormatMessage(userName, message), inner)
    {
        UserName = userName;
    }

    private static string FormatMessage(string userName, string? optionalMessage)
        => DefaultMessageFormatter($"Error while retrieving user {userName} info", optionalMessage);
}
