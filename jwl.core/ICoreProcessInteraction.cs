namespace jwl.core;

public interface ICoreProcessInteraction
    : IDisposable
{
    string AskForPassword(string userName);
}
