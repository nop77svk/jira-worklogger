namespace jwl.core;
using AutoMapper;

public class AppConfig
{
    public jwl.jira.ServerConfig? JiraServer { get; init; }
    public jwl.core.UserConfig? User { get; init; }
    public jwl.inputs.CsvFormatConfig? CsvOptions { get; init; }

    private static MapperConfiguration overridingMapperConfiguration = new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<AppConfig, AppConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<jira.ServerConfig, jira.ServerConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<inputs.CsvFormatConfig, inputs.CsvFormatConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
        cfg.CreateMap<core.UserConfig, core.UserConfig>()
            .ForAllMembers(m => m.Condition((src, dest, member) => member != null));
    });
    private static IMapper overridingMapper = overridingMapperConfiguration.CreateMapper();

    public AppConfig OverrideWith(AppConfig? other)
    {
        if (other == null)
            return this;

        return overridingMapper.Map<AppConfig, AppConfig>(other, this);
    }
}
