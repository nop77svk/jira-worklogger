namespace jwl.console;
using jwl.core;
using NoP77svk.Console;

public class ConsoleProcessInteraction
    : ICoreProcessInteraction
{
    private bool _isDisposed;

    public ConsoleProcessInteraction()
    {
    }

    public (string, string) AskForCredentials(string? userName)
    {
        string userNameNN = userName ?? string.Empty;
        if (string.IsNullOrEmpty(userNameNN))
        {
            Console.Error.Write(@"Enter Jira user name: ");
            userNameNN = Console.ReadLine() ?? string.Empty;
        }

        Console.Error.Write($"Enter password for Jira user {userNameNN}: ");
        string userPasswordNN = SecretConsoleExt.ReadLineInSecret(_ => '*', true);

        return (userNameNN, userPasswordNN);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }
    }
}
