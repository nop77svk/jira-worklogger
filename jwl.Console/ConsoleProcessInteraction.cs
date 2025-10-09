namespace Jwl.Console;

using Jwl.Core;

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
            System.Console.Error.Write(@"Enter Jira user name: ");
            userNameNN = System.Console.ReadLine() ?? string.Empty;
        }

        System.Console.Error.Write($"Enter password for Jira user {userNameNN}: ");
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
                // note: no disposal as of now
            }

            _isDisposed = true;
        }
    }
}
