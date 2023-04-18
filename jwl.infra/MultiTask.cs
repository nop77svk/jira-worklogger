namespace jwl.infra;

public static class MultiTask
{
    public static async Task WhenAll(IEnumerable<Task> tasks, Action<MultiTaskProgressState, Task?> progressFeedback, CancellationToken? cancellationToken = null)
    {
        progressFeedback(MultiTaskProgressState.Starting, null);
        HashSet<Task> tasksToExecute = tasks.ToHashSet();

        try
        {
            while (tasksToExecute.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();

                Task taskFinished = await Task.WhenAny(tasksToExecute);
                progressFeedback(MultiTaskProgressState.InProgress, taskFinished);

                if (tasksToExecute.Contains(taskFinished))
                    tasksToExecute.Remove(taskFinished);
                else
                    throw new Exception("Task reported as finished... again!");
            }
        }
        catch (TaskCanceledException)
        {
            progressFeedback(MultiTaskProgressState.Cancelled, null);
            throw;
        }
        catch (Exception)
        {
            progressFeedback(MultiTaskProgressState.Error, null);
            throw;
        }

        progressFeedback(MultiTaskProgressState.Finished, null);
    }
}
