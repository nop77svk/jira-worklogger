namespace jwl.infra;

public class MultiTask
{
    public enum ProgressState
    {
        Unknown,
        Starting,
        BeforeTaskWait,
        AfterTaskWait,
        Finished,
        Error,
        Cancelled
    }

    public ProgressState State { get; private set; } = ProgressState.Unknown;

    public Action<MultiTask>? ProcessFeedback { get; init; }
    public Action<Task>? TaskFeedback { get; init; }

    public MultiTask()
    {
    }

    public async Task WhenAll(IEnumerable<Task> tasks, CancellationToken? cancellationToken = null)
    {
        State = ProgressState.Starting;
        ProcessFeedback?.Invoke(this);

        HashSet<Task> tasksToExecute = tasks.ToHashSet();
        List<Exception> errors = new List<Exception>();

        while (tasksToExecute.Any())
        {
            State = ProgressState.BeforeTaskWait;
            ProcessFeedback?.Invoke(this);

            Task? taskFinished = null;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                taskFinished = await Task.WhenAny(tasksToExecute);

                State = ProgressState.AfterTaskWait;
                ProcessFeedback?.Invoke(this);
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

        if (errors.Any())
        {
            if (errors.All(ex => ex is TaskCanceledException))
            {
                State = ProgressState.Cancelled;
                ProcessFeedback?.Invoke(this);

                throw new TaskCanceledException($"All {errors.Count} tasks have been cancelled", new AggregateException(errors));
            }
            else
            {
                State = ProgressState.Error;
                ProcessFeedback?.Invoke(this);

                throw new AggregateException(errors);
            }
        }
        else
        {
            State = ProgressState.Finished;
            ProcessFeedback?.Invoke(this);
        }
    }
}
