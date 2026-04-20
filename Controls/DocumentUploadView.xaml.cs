using CheckYoSelfAI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
