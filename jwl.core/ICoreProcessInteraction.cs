namespace Jwl.Core;

public interface ICoreProcessInteraction
    : IDisposable
{
    (string, string) AskForCredentials(string? userName);
}
