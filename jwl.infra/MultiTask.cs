namespace jwl.infra;

public static class MultiTask
{
    public static async Task WhenAll(IEnumerable<Task> tasks, Action<MultiTaskProgressState, Task?>? progressFeedback, CancellationToken? cancellationToken = null)
    {
        progressFeedback?.Invoke(MultiTaskProgressState.Starting, null);
        HashSet<Task> tasksToExecute = tasks.ToHashSet();

        List<Exception> errors = new List<Exception>();

        while (tasksToExecute.Any())
        {
            Task? taskFinished = null;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                taskFinished = await Task.WhenAny(tasksToExecute);

                if (tasksToExecute.Contains(taskFinished))
                {
                    tasksToExecute.Remove(taskFinished);
                    progressFeedback?.Invoke(MultiTaskProgressState.Finished, taskFinished);
                }
                else
                {
                    throw new InvalidOperationException("Task reported as finished... again!");
                }
            }
            catch (TaskCanceledException ex)
            {
                progressFeedback?.Invoke(MultiTaskProgressState.Cancelled, ex.Task);
                errors.Add(ex);
            }
            catch (Exception ex)
            {
                progressFeedback?.Invoke(MultiTaskProgressState.Error, taskFinished);
                errors.Add(ex);
            }
        }

        if (errors.Any())
        {
            if (errors.All(ex => ex is TaskCanceledException))
                throw new TaskCanceledException($"All {errors.Count} multitasks have been cancelled");
            else
                throw new AggregateException(errors);
        }

        progressFeedback?.Invoke(MultiTaskProgressState.Finished, null);
    }
}
