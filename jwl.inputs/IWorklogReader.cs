namespace jwl.inputs;
using jwl.jira;

public interface IWorklogReader : IDisposable
{
    IEnumerable<JiraWorklog> AsEnumerable();
}
