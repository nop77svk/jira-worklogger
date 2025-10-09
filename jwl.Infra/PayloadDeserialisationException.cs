namespace Jwl.Infra;

public class PayloadDeserialisationException : Exception
{
    public PayloadDeserialisationException()
    {
    }

    public PayloadDeserialisationException(string message)
        : base(message)
    {
    }

    public PayloadDeserialisationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
