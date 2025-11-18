namespace jwl.Core;

public interface ICoreProcessInteraction
    : IDisposable
{
    (string, string) AskForCredentials(string? userName);
}
