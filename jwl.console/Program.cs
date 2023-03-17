namespace jwl.console;
using jwl.core;

internal class Program
{
    internal static async Task Main(string[] args)
    {
        using ICoreProcessFeedback feedback = new ConsoleProcessFeedback(JwlCoreProcess.TotalProcessSteps);
        using ICoreProcessInteraction interaction = new ConsoleProcessInteraction();
        using JwlCoreProcess engine = new JwlCoreProcess(feedback, interaction);
        await engine.Execute(@"d:\x.csv");
    }
}
