namespace jwl.core;

public class AppConfig
{
    public jwl.jira.ServerConfig? ServerConfig { get; init; }
    public jwl.core.UserConfig? UserConfig { get; init; }
    public jwl.inputs.CsvFormatConfig? CsvFormatConfig { get; init; }
}
