namespace jwl.core;

public interface ICoreProcessInteraction
    : IDisposable
{
    (string, string) AskForCredentials(string? userName);
}
