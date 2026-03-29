#pragma warning disable S1075
namespace jwl.Core;

using jwl.Inputs;
using jwl.Jira;
using jwl.Jira.Flavours;

using Microsoft.Extensions.Configuration;

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
        const int RootAppConfigId = -1;

        Action<BinderOptions> binderOptions = opt =>
        {
            opt.BindNonPublicProperties = false;
            opt.ErrorOnUnknownConfiguration = true;
        };

        var config = Enum.GetValues<JiraServerFlavour>()
            .Select(e => new KeyValuePair<int, string>(
                (int)e,
                string.Format(FlavourConfigFileNameTemplate, e.ToString().ToLowerInvariant()))
            )
            .Prepend(new KeyValuePair<int, string>(RootAppConfigId, ConfigFileName))
            .Select(cfg => new KeyValuePair<int, IConfiguration>(
                cfg.Key,
                new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfg.Value), optional: true)
                    .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), cfg.Value), optional: true)
                    .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), cfg.Value), optional: true)
                    .AddJsonFile(Path.Combine(Path.GetFullPath("."), cfg.Value), optional: true)
                    .Build()
            ))
            .Select(cfg => new KeyValuePair<int, object?>(
                cfg.Key,
                cfg.Key switch
                {
                    -1 => cfg.Value.Get<AppConfig>(binderOptions),
                    (int)JiraServerFlavour.Vanilla => cfg.Value.Get<FlavourVanillaJiraOptions>(binderOptions) ?? new FlavourVanillaJiraOptions(),
                    (int)JiraServerFlavour.TempoTimeSheets => cfg.Value.Get<FlavourTempoTimesheetsOptions>(binderOptions) ?? new FlavourTempoTimesheetsOptions(),
                    (int)JiraServerFlavour.ICTime => cfg.Value.Get<FlavourICTimeOptions>(binderOptions) ?? new FlavourICTimeOptions(),
                    _ => throw new JwlConfigurationException($"Unrecognized server flavour ID {cfg.Key}")
                }
            ))
            .ToDictionary(cfg => cfg.Key, cfg => cfg.Value);

        // 2do! automap the defaults
        AppConfig result = (AppConfig?)config[RootAppConfigId] ?? CreateWithDefaults();
        result.JiraServer!.VanillaJiraFlavourOptions = (FlavourVanillaJiraOptions?)config[(int)JiraServerFlavour.Vanilla];
        result.JiraServer!.FlavourOptions = (IFlavourOptions?)config[(int)result.JiraServer.FlavourId];

        return result;
    }
}
