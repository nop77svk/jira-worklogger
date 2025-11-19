namespace jwl.Core;

using jwl.Infra;

public class JwlConfigurationException : JwlCoreException
{
    public JwlConfigurationException()
    {
    }

    public JwlConfigurationException(string message)
        : base(message)
    {
    }

    public JwlConfigurationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
