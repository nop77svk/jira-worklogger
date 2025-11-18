namespace Jwl.Console;

using CommandLine;

using Jwl.Core;

internal static class Program
{
    internal static async Task Main(string[] args)
    {
        await Parser.Default
            .ParseArguments<FillCLI>(args)
            .WithParsedAsync<FillCLI>(cli => MainWithOptions(cli.ToAppConfig(), cli.InputFiles));
    }

    internal static async Task MainWithOptions(AppConfig cli, IEnumerable<string> inputFiles)
    {
        using ICoreProcessFeedback feedback = new ScrollingConsoleProcessFeedback(JwlCoreProcess.TotalProcessSteps);
        using ICoreProcessInteraction interaction = new ConsoleProcessInteraction();
        AppConfig config = AppConfigFactory.ReadConfig()
            .OverrideWith(cli);

        using JwlCoreProcess engine = new JwlCoreProcess(config, interaction)
        {
            Feedback = feedback
        };

        if (inputFiles.Any())
        {
            await engine.PreProcess();
        }

        await engine.Process(inputFiles);
    }
}
