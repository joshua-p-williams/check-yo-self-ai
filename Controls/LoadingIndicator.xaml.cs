namespace CheckYoSelfAI.Controls;

public partial class LoadingIndicator : ContentView
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive),
        typeof(bool),
        typeof(LoadingIndicator),
        false,
        propertyChanged: OnIsActiveChanged);

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
        if (this.FindByName<Border>("LoadingCard") is Border loadingCard)
        {
            loadingCard.Scale = 0.96;
        }
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

    private static void OnIsActiveChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingIndicator control && newValue is bool isActive)
        {
            _ = control.AnimateVisibilityAsync(isActive);
        }
    }

    private async Task AnimateVisibilityAsync(bool isActive)
    {
        var rootContainer = this.FindByName<Grid>("RootContainer");
        var loadingCard = this.FindByName<Border>("LoadingCard");
        if (rootContainer == null || loadingCard == null)
        {
            return;
        }

        if (isActive)
        {
            rootContainer.Opacity = 0;
            loadingCard.Scale = 0.96;
            await Task.WhenAll(
                rootContainer.FadeTo(1, 140, Easing.CubicInOut),
                loadingCard.ScaleTo(1, 180, Easing.CubicOut));
            return;
        }

        await rootContainer.FadeTo(0, 120, Easing.CubicIn);
        loadingCard.Scale = 0.96;
    }
}
