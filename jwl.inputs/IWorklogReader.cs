namespace jwl.Inputs;

public interface IWorklogReader : IDisposable
{
    IEnumerable<InputWorkLog> Read(Action<InputWorkLog>? validateResult = null);
}