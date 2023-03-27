namespace jwl.core;
using jwl.inputs;

public class Config
{
    public jwl.jira.ServerConfig ServerConfig { get; init; } = new jwl.jira.ServerConfig();
    public UserConfig UserConfig { get; init; } = new UserConfig();
    public CsvFormatConfig CsvFormatConfig { get; init; } = new CsvFormatConfig();
}
