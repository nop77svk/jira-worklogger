namespace Jwl.Jira.Exceptions;

using System;

public class JiraClientException
    : Exception
{
    public JiraClientException()
    {
    }

    public JiraClientException(string? message)
        : base(message)
    {
    }

    public JiraClientException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
