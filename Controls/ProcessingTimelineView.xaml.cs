using CheckYoSelfAI.ViewModels;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace CheckYoSelfAI.Controls;

public partial class ProcessingTimelineView : ContentView
{
    private const double CompactModeMaxWidth = 640;

    private readonly ObservableCollection<ProcessingTimelineStage> _stageSnapshot = [];
    private INotifyCollectionChanged? _stageCollectionNotifier;
    private bool _isCompactLayout = true;
    private ProcessingTimelineStage? _compactCurrentStage;
    private ProcessingTimelineStage? _compactNextStage;
    private string _stageProgressSummary = "0/0 complete";
    private double _stageProgressValue;

    public static readonly BindableProperty StagesProperty = BindableProperty.Create(
        nameof(Stages),
        typeof(IEnumerable<ProcessingTimelineStage>),
        typeof(ProcessingTimelineView),
        default(IEnumerable<ProcessingTimelineStage>),
        propertyChanged: OnStagesChanged);

    public static readonly BindableProperty CurrentStageTextProperty = BindableProperty.Create(
        nameof(CurrentStageText),
        typeof(string),
        typeof(ProcessingTimelineView),
        default(string));

    public static readonly BindableProperty HasCurrentStageProperty = BindableProperty.Create(
        nameof(HasCurrentStage),
        typeof(bool),
        typeof(ProcessingTimelineView),
        false);

    public static readonly BindableProperty HasErrorProperty = BindableProperty.Create(
        nameof(HasError),
        typeof(bool),
        typeof(ProcessingTimelineView),
        false);

    public static readonly BindableProperty ErrorMessageProperty = BindableProperty.Create(
        nameof(ErrorMessage),
        typeof(string),
        typeof(ProcessingTimelineView),
        default(string));

    public static readonly BindableProperty ShowDemoControlsProperty = BindableProperty.Create(
        nameof(ShowDemoControls),
        typeof(bool),
        typeof(ProcessingTimelineView),
        false);

    public static readonly BindableProperty NextStepHintProperty = BindableProperty.Create(
        nameof(NextStepHint),
        typeof(string),
        typeof(ProcessingTimelineView),
        default(string));

    public static readonly BindableProperty EmptyStateMessageProperty = BindableProperty.Create(
        nameof(EmptyStateMessage),
        typeof(string),
        typeof(ProcessingTimelineView),
        default(string));

    public static readonly BindableProperty NextStepActionTextProperty = BindableProperty.Create(
        nameof(NextStepActionText),
        typeof(string),
        typeof(ProcessingTimelineView),
        "Run Next Step");

    public static readonly BindableProperty ProcessNextStageCommandProperty = BindableProperty.Create(
        nameof(ProcessNextStageCommand),
        typeof(ICommand),
        typeof(ProcessingTimelineView),
        default(ICommand));

    public static readonly BindableProperty ProcessAllStagesCommandProperty = BindableProperty.Create(
        nameof(ProcessAllStagesCommand),
        typeof(ICommand),
        typeof(ProcessingTimelineView),
        default(ICommand));

    public static readonly BindableProperty ShowUploadControlsProperty = BindableProperty.Create(
        nameof(ShowUploadControls),
        typeof(bool),
        typeof(ProcessingTimelineView),
        false);

    public static readonly BindableProperty CaptureImageCommandProperty = BindableProperty.Create(
        nameof(CaptureImageCommand),
        typeof(ICommand),
        typeof(ProcessingTimelineView),
        default(ICommand));

    public static readonly BindableProperty SelectImageCommandProperty = BindableProperty.Create(
        nameof(SelectImageCommand),
        typeof(ICommand),
        typeof(ProcessingTimelineView),
        default(ICommand));

    public ProcessingTimelineView()
    {
        InitializeComponent();
        RebuildStageSnapshot();
    }

    public bool IsCompactLayout
    {
        get => _isCompactLayout;
        private set
        {
            if (_isCompactLayout == value)
            {
                return;
            }

            _isCompactLayout = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsExpandedLayout));
        }
    }

    public bool IsExpandedLayout => !IsCompactLayout;

    public IReadOnlyList<ProcessingTimelineStage> StageSnapshot => _stageSnapshot;

    public ProcessingTimelineStage? CompactCurrentStage
    {
        get => _compactCurrentStage;
        private set
        {
            if (_compactCurrentStage == value)
            {
                return;
            }

            _compactCurrentStage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasCompactCurrentStage));
        }
    }

    public bool HasCompactCurrentStage => CompactCurrentStage != null;

    public ProcessingTimelineStage? CompactNextStage
    {
        get => _compactNextStage;
        private set
        {
            if (_compactNextStage == value)
            {
                return;
            }

            _compactNextStage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasCompactNextStage));
        }
    }

    public bool HasCompactNextStage => CompactNextStage != null;

    public string StageProgressSummary
    {
        get => _stageProgressSummary;
        private set
        {
            if (_stageProgressSummary == value)
            {
                return;
            }

            _stageProgressSummary = value;
            OnPropertyChanged();
        }
    }

    public double StageProgressValue
    {
        get => _stageProgressValue;
        private set
        {
            if (_stageProgressValue == value)
            {
                return;
            }

            _stageProgressValue = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<ProcessingTimelineStage>? Stages
    {
        get => (IEnumerable<ProcessingTimelineStage>?)GetValue(StagesProperty);
        set => SetValue(StagesProperty, value);
    }

    public string? CurrentStageText
    {
        get => (string?)GetValue(CurrentStageTextProperty);
        set => SetValue(CurrentStageTextProperty, value);
    }

    public bool HasCurrentStage
    {
        get => (bool)GetValue(HasCurrentStageProperty);
        set => SetValue(HasCurrentStageProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool ShowDemoControls
    {
        get => (bool)GetValue(ShowDemoControlsProperty);
        set => SetValue(ShowDemoControlsProperty, value);
    }

    public string? NextStepHint
    {
        get => (string?)GetValue(NextStepHintProperty);
        set => SetValue(NextStepHintProperty, value);
    }

    public string? EmptyStateMessage
    {
        get => (string?)GetValue(EmptyStateMessageProperty);
        set => SetValue(EmptyStateMessageProperty, value);
    }

    public string NextStepActionText
    {
        get => (string)GetValue(NextStepActionTextProperty);
        set => SetValue(NextStepActionTextProperty, value);
    }

    public ICommand? ProcessNextStageCommand
    {
        get => (ICommand?)GetValue(ProcessNextStageCommandProperty);
        set => SetValue(ProcessNextStageCommandProperty, value);
    }

    public ICommand? ProcessAllStagesCommand
    {
        get => (ICommand?)GetValue(ProcessAllStagesCommandProperty);
        set => SetValue(ProcessAllStagesCommandProperty, value);
    }

    public bool ShowUploadControls
    {
        get => (bool)GetValue(ShowUploadControlsProperty);
        set => SetValue(ShowUploadControlsProperty, value);
    }

    public ICommand? CaptureImageCommand
    {
        get => (ICommand?)GetValue(CaptureImageCommandProperty);
        set => SetValue(CaptureImageCommandProperty, value);
    }

    public ICommand? SelectImageCommand
    {
        get => (ICommand?)GetValue(SelectImageCommandProperty);
        set => SetValue(SelectImageCommandProperty, value);
    }

    public bool ShowEmptyStateMessage => !HasCompactCurrentStage && !string.IsNullOrWhiteSpace(EmptyStateMessage);

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0)
        {
            return;
        }

        IsCompactLayout = width <= CompactModeMaxWidth;
    }

    private static void OnStagesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ProcessingTimelineView view)
        {
            return;
        }

        view.UnsubscribeStages(oldValue as IEnumerable<ProcessingTimelineStage>);
        view.SubscribeStages(newValue as IEnumerable<ProcessingTimelineStage>);
        view.RebuildStageSnapshot();
    }

    private void SubscribeStages(IEnumerable<ProcessingTimelineStage>? stages)
    {
        if (stages == null)
        {
            return;
        }

        foreach (var stage in stages)
        {
            stage.PropertyChanged += OnStagePropertyChanged;
        }

        if (stages is INotifyCollectionChanged collectionChanged)
        {
            _stageCollectionNotifier = collectionChanged;
            _stageCollectionNotifier.CollectionChanged += OnStagesCollectionChanged;
        }
    }

    private void UnsubscribeStages(IEnumerable<ProcessingTimelineStage>? stages)
    {
        if (stages != null)
        {
            foreach (var stage in stages)
            {
                stage.PropertyChanged -= OnStagePropertyChanged;
            }
        }

        if (_stageCollectionNotifier != null)
        {
            _stageCollectionNotifier.CollectionChanged -= OnStagesCollectionChanged;
            _stageCollectionNotifier = null;
        }
    }

    private void OnStagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var oldItem in e.OldItems.OfType<ProcessingTimelineStage>())
            {
                oldItem.PropertyChanged -= OnStagePropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (var newItem in e.NewItems.OfType<ProcessingTimelineStage>())
            {
                newItem.PropertyChanged += OnStagePropertyChanged;
            }
        }

        RebuildStageSnapshot();
    }

    private void OnStagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ProcessingTimelineStage.State)
            or nameof(ProcessingTimelineStage.Duration)
            or nameof(ProcessingTimelineStage.ErrorMessage))
        {
            RecomputeCompactProjection();
        }
    }

    private void RebuildStageSnapshot()
    {
        _stageSnapshot.Clear();

        var orderedStages = Stages?.OrderBy(stage => stage.Order).ToList() ?? [];
        foreach (var stage in orderedStages)
        {
            _stageSnapshot.Add(stage);
        }

        OnPropertyChanged(nameof(StageSnapshot));
        OnPropertyChanged(nameof(ShowEmptyStateMessage));
        RecomputeCompactProjection();
    }

    private void RecomputeCompactProjection()
    {
        if (_stageSnapshot.Count == 0)
        {
            CompactCurrentStage = null;
            CompactNextStage = null;
            StageProgressSummary = "0/0 complete";
            StageProgressValue = 0;
            OnPropertyChanged(nameof(ShowEmptyStateMessage));
            return;
        }

        var current = _stageSnapshot.FirstOrDefault(stage => stage.IsInProgress)
            ?? _stageSnapshot.FirstOrDefault(stage => stage.IsFailed)
            ?? _stageSnapshot.FirstOrDefault(stage => stage.IsPending)
            ?? _stageSnapshot.LastOrDefault();

        CompactCurrentStage = current;

        if (current == null)
        {
            CompactNextStage = null;
        }
        else
        {
            var currentIndex = _stageSnapshot.IndexOf(current);
            CompactNextStage = currentIndex >= 0 && currentIndex < _stageSnapshot.Count - 1
                ? _stageSnapshot[currentIndex + 1]
                : null;
        }

        var completedCount = _stageSnapshot.Count(stage => stage.IsCompleted);
        StageProgressSummary = $"{completedCount}/{_stageSnapshot.Count} complete";
        StageProgressValue = _stageSnapshot.Count == 0
            ? 0
            : (double)completedCount / _stageSnapshot.Count;
        OnPropertyChanged(nameof(ShowEmptyStateMessage));
    }

    private void OnDemoControlInvoked(object? sender, EventArgs e)
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch
        {
        }
    }
}
