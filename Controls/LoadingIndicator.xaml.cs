namespace CheckYoSelfAI.Controls;

public partial class LoadingIndicator : ContentView
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive),
        typeof(bool),
        typeof(LoadingIndicator),
        false);

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(LoadingIndicator),
        default(string),
        propertyChanged: OnMessageChanged);

    public static readonly BindableProperty ShowOverlayProperty = BindableProperty.Create(
        nameof(ShowOverlay),
        typeof(bool),
        typeof(LoadingIndicator),
        false);

    public static readonly BindableProperty IndicatorSizeProperty = BindableProperty.Create(
        nameof(IndicatorSize),
        typeof(double),
        typeof(LoadingIndicator),
        32d);

    public static readonly BindableProperty IndicatorColorProperty = BindableProperty.Create(
        nameof(IndicatorColor),
        typeof(Color),
        typeof(LoadingIndicator),
        Colors.Blue);

    public LoadingIndicator()
    {
        InitializeComponent();
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool ShowOverlay
    {
        get => (bool)GetValue(ShowOverlayProperty);
        set => SetValue(ShowOverlayProperty, value);
    }

    public double IndicatorSize
    {
        get => (double)GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingIndicator control)
        {
            control.OnPropertyChanged(nameof(HasMessage));
        }
    }
}
