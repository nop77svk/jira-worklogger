namespace jwl.Jira.Exceptions;

public class JiraFindUserException : JiraClientException
{
    public string UserName { get; }

    public JiraFindUserException(string userName)
        : this(userName, (string?)null)
    {
    }

    public JiraFindUserException(string userName, string? message)
        : base(FormatMessage(userName, message))
    {
        UserName = userName;
    }

    public JiraFindUserException(string userName, Exception inner)
        : this(userName, (string?)null, inner)
    {
    }

    public JiraFindUserException(string userName, string? message, Exception inner)
        : base(FormatMessage(userName, message), inner)
    {
        UserName = userName;
    }

    private static string FormatMessage(string userName, string? optionalMessage)
        => DefaultMessageFormatter($"Error while retrieving user {userName} info", optionalMessage);
}
