namespace jwl.console;
using System.Net.Http.Headers;
using System.Text;
using jwl.core;
using jwl.infra;
using jwl.inputs;
using jwl.jira;
using NoP77svk.Console;
using NoP77svk.Linq;
using ShellProgressBar;

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
