namespace jwl.core;

public class Config
{
    public jwl.jira.ServerConfig ServerConfig { get; init; } = new jwl.jira.ServerConfig();
    public UserConfig UserConfig { get; init; } = new UserConfig();
}
