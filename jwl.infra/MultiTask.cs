namespace jwl.infra;

public class MultiTask
{
    public Action<ProgressState>? ProgressStateFeedback { get; init; }
    public Action<Task>? TaskFeedback { get; init; }

    public MultiTask()
    {
    }

    public enum ProgressState
    {
        Unknown,
        Starting,
        InProgress,
        Finished,
        Error,
        Cancelled
    }

    public async Task WhenAll(IEnumerable<Task> tasks, CancellationToken? cancellationToken = null)
    {
        ProgressStateFeedback?.Invoke(ProgressState.Starting);

        HashSet<Task> tasksToExecute = tasks.ToHashSet();
        List<Exception> errors = new List<Exception>();

        while (tasksToExecute.Any())
        {
            ProgressStateFeedback?.Invoke(ProgressState.InProgress);

            Task? taskFinished = null;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                taskFinished = await Task.WhenAny(tasksToExecute);
                TaskFeedback?.Invoke(taskFinished);

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
            ProgressStateFeedback?.Invoke(ProgressState.Cancelled);
            throw new TaskCanceledException($"All {errors.Count} tasks have been cancelled", new AggregateException(errors));
        }
        else if (errors.Any())
        {
            ProgressStateFeedback?.Invoke(ProgressState.Error);
            throw new AggregateException(errors);
        }
        else
        {
            ProgressStateFeedback?.Invoke(ProgressState.Finished);
        }
    }
}
