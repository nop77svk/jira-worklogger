namespace jwl.infra;

public static class MultiTask
{
    public static async Task WhenAll(IEnumerable<Task> tasks, Action<MultiTaskProgress, Task?> reportProgress, CancellationToken? cancellationToken = null)
    {
        Task[] tasksFetched = tasks.ToArray();

        MultiTaskProgress progress = new MultiTaskProgress()
        {
            State = MultiTaskProgressState.Starting,
            Total = tasksFetched.Length,
            Finished = 0,
            Faulted = 0,
            Cancelled = 0,
            Unknown = 0
        };
        reportProgress(progress, null);

        try
        {
            for (int i = 0; i < tasksFetched.Length; i++)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                progress.State = MultiTaskProgressState.InProgress;

                Task taskFinished = await Task.WhenAny(tasksFetched);

                if (taskFinished.IsCompletedSuccessfully)
                    progress.Finished++;
                else if (taskFinished.IsCanceled)
                    progress.Cancelled++;
                else if (taskFinished.IsFaulted)
                    progress.Faulted++;
                else if (taskFinished.IsCompleted)
                    progress.Unknown++;

                reportProgress(progress, taskFinished);
            }
        }
        catch (TaskCanceledException)
        {
            progress.State = MultiTaskProgressState.Cancelled;
            reportProgress(progress, null);
            throw;
        }
        catch (Exception)
        {
            progress.State = MultiTaskProgressState.Error;
            reportProgress(progress, null);
            throw;
        }

        progress.State = MultiTaskProgressState.Finished;
        reportProgress(progress, null);
    }
}
