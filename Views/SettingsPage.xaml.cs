using CheckYoSelfAI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
#if WINDOWS
using Microsoft.UI.Xaml;
#endif

namespace CheckYoSelfAI.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (BindingContext != null)
        {
            return;
        }

        var services = Handler?.MauiContext?.Services;
        if (services == null)
        {
            return;
        }

        BindingContext = ActivatorUtilities.CreateInstance<SettingsPageViewModel>(services);
        ConfigureKeyboardNavigation();
    }

    private void OnActionInvoked(object? sender, EventArgs e)
    {
        TryPerformHapticFeedback();
    }

    private void OnPreferenceToggled(object? sender, ToggledEventArgs e)
    {
        TryPerformHapticFeedback();
    }

    private static void TryPerformHapticFeedback()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch
        {
        }
    }

    private void ConfigureKeyboardNavigation()
    {
#if WINDOWS
        var orderedControls = new VisualElement?[]
        {
            this.FindByName<VisualElement>("EndpointEntry"),
            this.FindByName<VisualElement>("ApiKeyEntry"),
            this.FindByName<VisualElement>("RegionEntry"),
            this.FindByName<VisualElement>("TestConnectionButton"),
            this.FindByName<VisualElement>("AutoSaveSwitch"),
            this.FindByName<VisualElement>("ShowConfidenceSwitch"),
            this.FindByName<VisualElement>("SaveSettingsButton")
        };

        for (var index = 0; index < orderedControls.Length; index++)
        {
            if (orderedControls[index]?.Handler?.PlatformView is FrameworkElement element)
            {
                element.IsTabStop = true;
                element.TabIndex = index;
            }
        }
#endif
    }
}
