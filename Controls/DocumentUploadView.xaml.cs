using CheckYoSelfAI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
#if WINDOWS
using Microsoft.UI.Xaml;
#endif

namespace CheckYoSelfAI.Controls;

public partial class DocumentUploadView : ContentView
{
    public DocumentUploadView()
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

        BindingContext = ActivatorUtilities.CreateInstance<DocumentUploadViewModel>(services);
        ConfigureKeyboardNavigation();
    }

    private void OnActionInvoked(object? sender, EventArgs e)
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
            this.FindByName<VisualElement>("CaptureImageButton"),
            this.FindByName<VisualElement>("SelectFileButton"),
            this.FindByName<VisualElement>("ClearSelectionButton"),
            this.FindByName<VisualElement>("RunNextStepButton"),
            this.FindByName<VisualElement>("RunAllRemainingButton"),
            this.FindByName<VisualElement>("OpenModelDocumentationButton"),
            this.FindByName<VisualElement>("ToggleNormalizedViewButton"),
            this.FindByName<VisualElement>("ExportJsonButton"),
            this.FindByName<VisualElement>("ShareResultButton"),
            this.FindByName<VisualElement>("TriggerManualReviewButton"),
            this.FindByName<VisualElement>("TryFallbackModelButton")
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
