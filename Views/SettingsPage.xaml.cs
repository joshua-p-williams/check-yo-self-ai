using CheckYoSelfAI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
