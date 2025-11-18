namespace jwl.Core;

public class MultiTaskStats
{
    public int Total { get; private set; }
    public int Succeeded { get; private set; }
    public float SucceededPct => Total > 0 ? (float)Succeeded / Total : float.NaN;
    public int Faulted { get; private set; }
    public float FaultedPct => Total > 0 ? (float)Faulted / Total : float.NaN;
    public int Cancelled { get; private set; }
    public float CancelledPct => Total > 0 ? (float)Cancelled / Total : float.NaN;
    public int Unknown { get; private set; }
    public float UnknownPct => Total > 0 ? (float)Unknown / Total : float.NaN;

    public int ErredSoFar => Faulted + Cancelled + Unknown;
    public float ErredSoFarPct => Total > 0 ? (float)ErredSoFar / Total : float.NaN;
    public int DoneSoFar => Succeeded + ErredSoFar;
    public float DoneSoFarPct => Total > 0 ? (float)DoneSoFar / Total : float.NaN;

    public MultiTaskStats(int total)
    {
        Total = total;
        Succeeded = 0;
        Faulted = 0;
        Cancelled = 0;
        Unknown = 0;
    }

    private readonly object _locker = new object();

    public MultiTaskStats ApplyTaskStatus(TaskStatus? taskStatus)
    {
        if (taskStatus == null)
        {
            return this;
        }

        lock (_locker)
        {
            switch (taskStatus)
            {
                case TaskStatus.RanToCompletion:
                    Succeeded++;
                    break;

                case TaskStatus.Canceled:
                    Cancelled++;
                    break;

                case TaskStatus.Faulted:
                    Faulted++;
                    break;

                case TaskStatus.Created:
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingToRun:
                case TaskStatus.WaitingForChildrenToComplete:
                    break;

                default:
                    Unknown++;
                    break;
            }
        }

        return this;
    }
}
