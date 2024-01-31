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
            Task? taskFinished = null;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                taskFinished = await Task.WhenAny(tasksToExecute);

                if (taskFinished.Status is TaskStatus.Faulted or TaskStatus.Canceled)
                {
                    tasksToExecute.Remove(taskFinished);

                    throw taskFinished.Exception ?? new Exception($"Task ended in {taskFinished.Status} status without exception details");
                }
                else if (taskFinished.Status == TaskStatus.RanToCompletion)
                {
                    if (tasksToExecute.Contains(taskFinished))
                    {
                        tasksToExecute.Remove(taskFinished);
                        progressFeedback?.Invoke(ProgressState.Finished, taskFinished);
                    }
                    else
                    {
                        throw new InvalidOperationException("Task reported as finished... again!");
                    }
                }
            }
            catch (AggregateException ex)
            {
                progressFeedback?.Invoke(ProgressState.Error, taskFinished);
                errors.AddRange(ex.InnerExceptions);
            }
            catch (TaskCanceledException ex)
            {
                progressFeedback?.Invoke(ProgressState.Cancelled, ex.Task);
                errors.Add(ex);
            }
            catch (Exception ex)
            {
                progressFeedback?.Invoke(ProgressState.Error, taskFinished);
                errors.Add(ex);
            }
        }

        if (errors.Any())
        {
            if (errors.All(ex => ex is TaskCanceledException))
                throw new TaskCanceledException($"All {errors.Count} tasks have been cancelled", new AggregateException(errors));
            else
                throw new AggregateException(errors);
        }

        progressFeedback?.Invoke(ProgressState.Finished, null);
    }
}
