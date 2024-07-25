﻿namespace jwl.jira.Exceptions;

using System;

public class JiraClientException
    : ApplicationException
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
