using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckYoSelfAI.ViewModels;

public enum ProcessingTimelineStageState
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public sealed class ProcessingTimelineStage : ObservableObject
{
    private ProcessingTimelineStageState _state;
    private TimeSpan? _duration;
    private string? _errorMessage;

    public ProcessingTimelineStage(string title, string description, int order, bool isLast = false)
    {
        Title = title;
        Description = description;
        Order = order;
        IsLast = isLast;
        _state = ProcessingTimelineStageState.Pending;
    }

    public string Title { get; }

    public string Description { get; }

    public int Order { get; }

    public bool IsLast { get; }

    public bool IsNotLast => !IsLast;

    public ProcessingTimelineStageState State
    {
        get => _state;
        private set
        {
            if (SetProperty(ref _state, value))
            {
                OnPropertyChanged(nameof(IsPending));
                OnPropertyChanged(nameof(IsInProgress));
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(IsFailed));
                OnPropertyChanged(nameof(IndicatorText));
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public TimeSpan? Duration
    {
        get => _duration;
        private set
        {
            if (SetProperty(ref _duration, value))
            {
                OnPropertyChanged(nameof(DurationDisplay));
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }
    }

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsPending => State == ProcessingTimelineStageState.Pending;

    public bool IsInProgress => State == ProcessingTimelineStageState.InProgress;

    public bool IsCompleted => State == ProcessingTimelineStageState.Completed;

    public bool IsFailed => State == ProcessingTimelineStageState.Failed;

    public string IndicatorText => State switch
    {
        ProcessingTimelineStageState.Completed => "✓",
        ProcessingTimelineStageState.Failed => "!",
        _ => Order.ToString()
    };

    public string StatusText => State switch
    {
        ProcessingTimelineStageState.Pending => "Pending",
        ProcessingTimelineStageState.InProgress => "In progress",
        ProcessingTimelineStageState.Completed => "Completed",
        ProcessingTimelineStageState.Failed => "Failed",
        _ => "Pending"
    };

    public string DurationDisplay => Duration.HasValue
        ? $"{Duration.Value.TotalMilliseconds:0} ms"
        : "--";

    public void SetPending()
    {
        ErrorMessage = null;
        Duration = null;
        State = ProcessingTimelineStageState.Pending;
    }

    public void SetInProgress()
    {
        ErrorMessage = null;
        Duration = null;
        State = ProcessingTimelineStageState.InProgress;
    }

    public void SetCompleted(TimeSpan duration)
    {
        ErrorMessage = null;
        Duration = duration;
        State = ProcessingTimelineStageState.Completed;
    }

    public void SetFailed(TimeSpan duration, string? errorMessage)
    {
        Duration = duration;
        ErrorMessage = errorMessage;
        State = ProcessingTimelineStageState.Failed;
    }
}
