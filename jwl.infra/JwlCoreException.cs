namespace jwl.Infra;

using System;

public class JwlCoreException
    : Exception
{
    public JwlCoreException()
    {
    }

    public JwlCoreException(string message)
        : base(message)
    {
    }

    public JwlCoreException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
