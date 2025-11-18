namespace jwl.Infra.Deprecated;

internal class MultiTask
{
    public ProgressState State { get; private set; } = ProgressState.Unknown;
    public Action<MultiTask>? OnStateChange { get; init; }
    public Action<Task>? OnTaskAwaited { get; init; }

    public MultiTask()
    {
    }

    public enum ProgressState
    {
        Unknown,
        Starting,
        BeforeTaskAwait,
        AfterTaskAwait,
        Finished,
        Error,
        Cancelled
    }

    public async Task WhenAll(IEnumerable<Task> tasks, CancellationToken? cancellationToken = null)
    {
        State = ProgressState.Starting;
        OnStateChange?.Invoke(this);

        var tasksToExecute = tasks.ToHashSet();
        var errors = new List<Exception>();

        while (tasksToExecute.Any())
        {
            State = ProgressState.BeforeTaskAwait;
            OnStateChange?.Invoke(this);

            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                Task finishedTask = await Task.WhenAny(tasksToExecute);

                State = ProgressState.AfterTaskAwait;
                OnStateChange?.Invoke(this);
                OnTaskAwaited?.Invoke(finishedTask);

                if (finishedTask.Status is TaskStatus.Faulted or TaskStatus.Canceled)
                {
                    tasksToExecute.Remove(finishedTask);
                    throw finishedTask.Exception ?? new Exception($"Task ended in {finishedTask.Status} status without exception details");
                }
                else if (finishedTask.Status == TaskStatus.RanToCompletion)
                {
                    if (!tasksToExecute.Remove(finishedTask))
                    {
                        throw new InvalidOperationException("Task reported as finished... again!");
                    }
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
                OnStateChange?.Invoke(this);

                throw new TaskCanceledException($"All {errors.Count} tasks have been cancelled", new AggregateException(errors));
            }
            else
            {
                State = ProgressState.Error;
                OnStateChange?.Invoke(this);

                throw new AggregateException(errors);
            }
        }
        else
        {
            State = ProgressState.Finished;
            OnStateChange?.Invoke(this);
        }
    }
}