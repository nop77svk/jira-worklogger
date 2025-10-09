namespace Jwl.Core;

using Jwl.Inputs;
using Jwl.Jira;

public class AppConfig
{
    public bool? UseVerboseFeedback { get; init; }
    public ServerConfig? JiraServer { get; init; }
    public UserConfig? User { get; init; }
    public CsvFormatConfig? CsvOptions { get; init; }

    public AppConfig OverrideWith(AppConfig? other)
    {
        if (other == null)
        {
            return this;
        }

        AppConfig result = new AppConfig()
        {
            UseVerboseFeedback = other.UseVerboseFeedback ?? UseVerboseFeedback,
            JiraServer = JiraServer?.OverrideWith(other.JiraServer),
            User = User?.OverrideWith(other.User),
            CsvOptions = CsvOptions?.OverrideWith(other.CsvOptions)
        };

        return result;
    }
}
