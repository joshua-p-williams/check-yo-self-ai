using CheckYoSelfAI.ViewModels;
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
        RecomputeCompactProjection();
    }

    private void RecomputeCompactProjection()
    {
        if (_stageSnapshot.Count == 0)
        {
            CompactCurrentStage = null;
            CompactNextStage = null;
            StageProgressSummary = "0/0 complete";
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
    }
}
