namespace jwl.infra;

public struct MultiTaskProgress
{
    public MultiTaskProgressState State;
    public int Total;
    public int Succeeded;
    public float SucceededPct => Total > 0 ? Succeeded / Total : float.NaN;
    public int Faulted;
    public float FaultedPct => Total > 0 ? Faulted / Total : float.NaN;
    public int Cancelled;
    public float CancelledPct => Total > 0 ? Cancelled / Total : float.NaN;
    public int Unknown;
    public float UnknownPct => Total > 0 ? Unknown / Total : float.NaN;
    public int ErredSoFar => Faulted + Cancelled + Unknown;
    public float ErredSoFarPct => Total > 0 ? ErredSoFarPct / Total : float.NaN;
    public int DoneSoFar => Succeeded + ErredSoFar;
    public float DoneSoFarPct => Total > 0 ? DoneSoFar / Total : float.NaN;
}
