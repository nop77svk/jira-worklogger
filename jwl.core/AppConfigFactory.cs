namespace jwl.core;
using jwl.inputs;
using jwl.jira;
using jwl.jira.Flavours;
using Microsoft.Extensions.Configuration;
using System.Xml.XPath;

public static class AppConfigFactory
{
    public const int DefaultMaxConnectionsPerServer = 4;

    private const string ConfigFileName = @"jwl.config";
    private const string FlavourConfigFileNameTemplate = @"jwl.{0}.config";

    public static AppConfig CreateWithDefaults()
    {
        return new AppConfig()
        {
            JiraServer = new ServerConfig()
            {
                Flavour = nameof(JiraServerFlavour.Vanilla),
                FlavourOptions = null,
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
            .Build();

        result = config.Get<AppConfig>(opt =>
        {
            opt.BindNonPublicProperties = false;
            opt.ErrorOnUnknownConfiguration = true;
        });

        if (result?.JiraServer != null)
        {
            JiraServerFlavour flavour = result.JiraServer.FlavourId;
            string flavourConfigFileName = string.Format(FlavourConfigFileNameTemplate, flavour.ToString().ToLower());
            IConfiguration flavourConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, flavourConfigFileName), optional: true)
                .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), flavourConfigFileName), optional: true)
                .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), flavourConfigFileName), optional: true)
                .AddJsonFile(Path.Combine(Path.GetFullPath("."), flavourConfigFileName), optional: true)
                .Build();

            result!.JiraServer.FlavourOptions = flavour switch
            {
                JiraServerFlavour.ICTime => flavourConfig.Get<ICTimeFlavourOptions>(opt =>
                {
                    opt.BindNonPublicProperties = false;
                    opt.ErrorOnUnknownConfiguration = true;
                }),
                JiraServerFlavour.Vanilla or JiraServerFlavour.TempoTimeSheets => null,
                _ => throw new ArgumentOutOfRangeException(nameof(result.JiraServer.Flavour))
            };
        }

        return result ?? CreateWithDefaults();
    }
}
