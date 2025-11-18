namespace jwl.Core;

using AutoMapper;

public class AppConfig
{
    public bool? UseVerboseFeedback { get; init; }
    public jwl.Jira.ServerConfig? JiraServer { get; init; }
    public jwl.Core.UserConfig? User { get; init; }
    public jwl.Inputs.CsvFormatConfig? CsvOptions { get; init; }

    private static Lazy<MapperConfiguration> overridingMapperConfiguration = new(() => new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<AppConfig, AppConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<Jira.ServerConfig, Jira.ServerConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<Inputs.CsvFormatConfig, Inputs.CsvFormatConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<Core.UserConfig, Core.UserConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));

        cfg.AddGlobalIgnore(nameof(AppConfig.JiraServer.FlavourOptions));
        cfg.AddGlobalIgnore(nameof(AppConfig.JiraServer.VanillaJiraFlavourOptions));
    }));

    private static Lazy<IMapper> overridingMapper = new(() => overridingMapperConfiguration.Value.CreateMapper());

    public AppConfig OverrideWith(AppConfig? other)
    {
        if (other == null)
        {
            return this;
        }

        AppConfig result = overridingMapper.Value.Map<AppConfig, AppConfig>(other, this);
        return result;
    }
}
