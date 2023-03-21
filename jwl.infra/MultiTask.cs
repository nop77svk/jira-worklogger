namespace jwl.infra;

public static class MultiTask
{
    public static async Task WhenAll(IEnumerable<Task> tasks, Action<MultiTaskProgress, Task?> reportProgress, CancellationToken? cancellationToken = null)
    {
        HashSet<Task> tasksToExecute = tasks.ToHashSet();

        MultiTaskProgress progress = new MultiTaskProgress()
        {
            State = MultiTaskProgressState.Starting,
            Total = tasksToExecute.Count,
            Finished = 0,
            Faulted = 0,
            Cancelled = 0,
            Unknown = 0
        };
        reportProgress(progress, null);

        try
        {
            while (tasksToExecute.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                progress.State = MultiTaskProgressState.InProgress;

                Task taskFinished = await Task.WhenAny(tasksToExecute);

                if (tasksToExecute.Contains(taskFinished))
                {
                    tasksToExecute.Remove(taskFinished);

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
                else
                {
                    Console.Error.WriteLine("ERROR: Task accidentally reported as finished!");
                }
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
