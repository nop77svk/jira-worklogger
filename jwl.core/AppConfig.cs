namespace jwl.core;
using AutoMapper;

public class AppConfig
{
    public bool? UseVerboseFeedback { get; init; }
    public jwl.jira.ServerConfig? JiraServer { get; init; }
    public jwl.core.UserConfig? User { get; init; }
    public jwl.inputs.CsvFormatConfig? CsvOptions { get; init; }

    private static Lazy<MapperConfiguration> overridingMapperConfiguration = new (() => new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<AppConfig, AppConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<jira.ServerConfig, jira.ServerConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<inputs.CsvFormatConfig, inputs.CsvFormatConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<core.UserConfig, core.UserConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));

        cfg.AddGlobalIgnore(nameof(AppConfig.JiraServer.FlavourOptions));
    }));

    private static Lazy<IMapper> overridingMapper = new (() => overridingMapperConfiguration.Value.CreateMapper());

    public AppConfig OverrideWith(AppConfig? other)
    {
        if (other == null)
            return this;

        AppConfig result = overridingMapper.Value.Map<AppConfig, AppConfig>(other, this);
        return result;
    }
}
