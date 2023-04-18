namespace jwl.core;
using jwl.inputs;
using jwl.jira;
using Microsoft.Extensions.Configuration;

public static class AppConfigFactory
{
    public const int DefaultMaxConnectionsPerServer = 4;

    private const string ConfigFileName = @"jwl.config";
    private const string DefaultSubFolder = @"jira-worklogger";

    public static AppConfig CreateWithDefaults()
    {
        return new AppConfig()
        {
            JiraServer = new ServerConfig()
            {
                BaseUrl = @"http://jira.my-domain.xyz",
                MaxConnectionsPerServer = DefaultMaxConnectionsPerServer,
                SkipSslCertificateCheck = false,
                UseProxy = false
            },
            CsvOptions = new CsvFormatConfig()
            {
                FieldDelimiter = ","
            },
            User = new UserConfig()
            {
                Name = string.Empty,
                Password = null
            }
        };
    }

    public static AppConfig ReadConfig()
    {
        AppConfig? result;

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Path.GetFullPath("."), ConfigFileName), optional: true)
            // 2do! custom command line config provider at the highest priority
            .Build();

        result = config.Get<AppConfig>(opt =>
        {
            opt.BindNonPublicProperties = false;
            opt.ErrorOnUnknownConfiguration = true;
        });

        return result ?? CreateWithDefaults();
    }
}
