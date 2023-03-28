namespace jwl.core;

public class AppConfig
{
    public jwl.jira.ServerConfig? JiraServer { get; init; }
    public jwl.core.UserConfig? User { get; init; }
    public jwl.inputs.CsvFormatConfig? CsvOptions { get; init; }
}
