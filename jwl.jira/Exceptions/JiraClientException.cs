namespace jwl.Jira.Exceptions;

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

    protected static string DefaultMessageFormatter(string mandatoryMessage, string? optionalMessage)
        => mandatoryMessage
        + (!string.IsNullOrEmpty(optionalMessage) ? $"\n{optionalMessage}" : string.Empty);
}
