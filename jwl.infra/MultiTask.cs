namespace jwl.infra;

public static class MultiTask
{
    public enum ProgressState
    {
        Unknown,
        Starting,
        InProgress,
        Finished,
        Error,
        Cancelled
    }

    public static async Task WhenAll(IEnumerable<Task> tasks, Action<ProgressState, Task?>? progressFeedback, CancellationToken? cancellationToken = null)
    {
        progressFeedback?.Invoke(ProgressState.Starting, null);
        HashSet<Task> tasksToExecute = tasks.ToHashSet();

        List<Exception> errors = new List<Exception>();

        while (tasksToExecute.Any())
        {
            progressFeedback?.Invoke(ProgressState.InProgress, null);

            Task? taskFinished = null;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                taskFinished = await Task.WhenAny(tasksToExecute);
                progressFeedback?.Invoke(ProgressState.InProgress, taskFinished);

                if (taskFinished.Status is TaskStatus.Faulted or TaskStatus.Canceled)
                {
                    tasksToExecute.Remove(taskFinished);
                    throw taskFinished.Exception ?? new Exception($"Task ended in {taskFinished.Status} status without exception details");
                }
                else if (taskFinished.Status == TaskStatus.RanToCompletion)
                {
                    if (!tasksToExecute.Remove(taskFinished))
                        throw new InvalidOperationException("Task reported as finished... again!");
                }
            }
            catch (AggregateException ex)
            {
                errors.AddRange(ex.InnerExceptions);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }

        if (errors.All(ex => ex is TaskCanceledException))
        {
            progressFeedback?.Invoke(ProgressState.Cancelled, null);
            throw new TaskCanceledException($"All {errors.Count} tasks have been cancelled", new AggregateException(errors));
        }
        else if (errors.Any())
        {
            progressFeedback?.Invoke(ProgressState.Error, null);
            throw new AggregateException(errors);
        }
        else
        {
            progressFeedback?.Invoke(ProgressState.Finished, null);
        }
    }
}
