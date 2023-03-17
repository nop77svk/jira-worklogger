namespace jwl.infra;

public struct MultiTaskProgress
{
    public MultiTaskProgressState State;
    public int Total;
    public int Finished;
    public int Faulted;
    public int Cancelled;
    public int Unknown;
}
