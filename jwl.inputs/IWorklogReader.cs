namespace jwl.inputs;
using jwl.core;

public interface IWorklogReader : IDisposable
{
    IEnumerable<JiraWorklog> AsEnumerable(Action<JiraWorklog>? assertCorrectResult = null);
}
