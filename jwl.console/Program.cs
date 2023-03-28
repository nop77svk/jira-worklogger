namespace jwl.console;
using jwl.core;

internal class Program
{
    internal static async Task Main(string[] args)
    {
        using ICoreProcessFeedback feedback = new ScrollingConsoleProcessFeedback(JwlCoreProcess.TotalProcessSteps);
        using ICoreProcessInteraction interaction = new ConsoleProcessInteraction();
        AppConfig config = AppConfigFactory.ReadConfig();
        using JwlCoreProcess engine = new JwlCoreProcess(config, interaction)
        {
            Feedback = feedback
        };

        await engine.PreProcess();
        await engine.Process(args);
    }
}
