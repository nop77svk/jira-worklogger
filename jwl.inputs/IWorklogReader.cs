namespace jwl.inputs;
using jwl.jira;

public interface IWorklogReader : IDisposable
{
    IEnumerable<InputWorkLog> Read(Action<InputWorkLog>? validateResult = null);
}
