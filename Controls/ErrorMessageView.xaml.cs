using CommunityToolkit.Mvvm.Input;

namespace CheckYoSelfAI.Controls;

public enum ErrorMessageSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public partial class ErrorMessageView : ContentView
{
    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(ErrorMessageView),
        default(string),
        propertyChanged: OnDisplayPropertyChanged);

    public static readonly BindableProperty SeverityProperty = BindableProperty.Create(
        nameof(Severity),
        typeof(ErrorMessageSeverity),
        typeof(ErrorMessageView),
        ErrorMessageSeverity.Error,
        propertyChanged: OnDisplayPropertyChanged);

    public static readonly BindableProperty IsDismissibleProperty = BindableProperty.Create(
        nameof(IsDismissible),
        typeof(bool),
        typeof(ErrorMessageView),
        true,
        propertyChanged: OnDisplayPropertyChanged);

    public static readonly BindableProperty IsPersistentProperty = BindableProperty.Create(
        nameof(IsPersistent),
        typeof(bool),
        typeof(ErrorMessageView),
        false,
        propertyChanged: OnDisplayPropertyChanged);

    private bool _isDismissed;

    public ErrorMessageView()
    {
        InitializeComponent();
        DismissCommand = new RelayCommand(Dismiss);
    }

    public IRelayCommand DismissCommand { get; }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ErrorMessageSeverity Severity
    {
        get => (ErrorMessageSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public bool IsDismissible
    {
        get => (bool)GetValue(IsDismissibleProperty);
        set => SetValue(IsDismissibleProperty, value);
    }

    public bool IsPersistent
    {
        get => (bool)GetValue(IsPersistentProperty);
        set => SetValue(IsPersistentProperty, value);
    }

    public bool IsVisibleMessage => !string.IsNullOrWhiteSpace(Message) && (IsPersistent || !_isDismissed);

    public string IconGlyph => Severity switch
    {
        ErrorMessageSeverity.Info => "ℹ",
        ErrorMessageSeverity.Warning => "⚠",
        ErrorMessageSeverity.Error => "⛔",
        ErrorMessageSeverity.Critical => "✖",
        _ => "⚠"
    };

    public Color BackgroundColorValue => Severity switch
    {
        ErrorMessageSeverity.Info => Color.FromArgb("#E3F2FD"),
        ErrorMessageSeverity.Warning => Color.FromArgb("#FFF3E0"),
        ErrorMessageSeverity.Error => Color.FromArgb("#FFEBEE"),
        ErrorMessageSeverity.Critical => Color.FromArgb("#FFCDD2"),
        _ => Color.FromArgb("#FFF3E0")
    };

    public Color StrokeColorValue => Severity switch
    {
        ErrorMessageSeverity.Info => Color.FromArgb("#2196F3"),
        ErrorMessageSeverity.Warning => Color.FromArgb("#FF9800"),
        ErrorMessageSeverity.Error => Color.FromArgb("#F44336"),
        ErrorMessageSeverity.Critical => Color.FromArgb("#B71C1C"),
        _ => Color.FromArgb("#FF9800")
    };

    public Color TextColorValue => Severity switch
    {
        ErrorMessageSeverity.Info => Color.FromArgb("#0D47A1"),
        ErrorMessageSeverity.Warning => Color.FromArgb("#E65100"),
        ErrorMessageSeverity.Error => Color.FromArgb("#B71C1C"),
        ErrorMessageSeverity.Critical => Color.FromArgb("#B71C1C"),
        _ => Color.FromArgb("#E65100")
    };

    private static void OnDisplayPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ErrorMessageView view)
        {
            return;
        }

        if (!view.IsPersistent)
        {
            view._isDismissed = false;
        }

        view.RefreshComputedProperties();
    }

    private void Dismiss()
    {
        if (IsPersistent)
        {
            return;
        }

        _isDismissed = true;
        OnPropertyChanged(nameof(IsVisibleMessage));
    }

    private void RefreshComputedProperties()
    {
        OnPropertyChanged(nameof(IsVisibleMessage));
        OnPropertyChanged(nameof(IconGlyph));
        OnPropertyChanged(nameof(BackgroundColorValue));
        OnPropertyChanged(nameof(StrokeColorValue));
        OnPropertyChanged(nameof(TextColorValue));
    }
}
