namespace jwl.infra;

public struct MultiTaskProgress
{
    public MultiTaskProgressState State;
    public int Total;
    public int Succeeded;
    public float SucceededPct => Total > 0 ? (float)Succeeded / Total : float.NaN;
    public int Faulted;
    public float FaultedPct => Total > 0 ? (float)Faulted / Total : float.NaN;
    public int Cancelled;
    public float CancelledPct => Total > 0 ? (float)Cancelled / Total : float.NaN;
    public int Unknown;
    public float UnknownPct => Total > 0 ? (float)Unknown / Total : float.NaN;
    public int ErredSoFar => Faulted + Cancelled + Unknown;
    public float ErredSoFarPct => Total > 0 ? (float)ErredSoFar / Total : float.NaN;
    public int DoneSoFar => Succeeded + ErredSoFar;
    public float DoneSoFarPct => Total > 0 ? (float)DoneSoFar / Total : float.NaN;
}
