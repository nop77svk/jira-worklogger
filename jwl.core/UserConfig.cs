namespace jwl.core;
using CommandLine;

public class UserConfig
{
    [Option('u', "user", HelpText = "Jira user name")]
    public string? Name { get; init; }
    [Option("password", HelpText = "Jira user password")]
    public string? Password { get; init; }
}
