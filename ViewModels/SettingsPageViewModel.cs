using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.ViewModels;

public class SettingsPageViewModel : BaseViewModel
{
    private static readonly Regex ApiKeyRegex = new("^[a-fA-F0-9]+$", RegexOptions.Compiled);

    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;

    private string _endpoint = string.Empty;
    private string _apiKey = string.Empty;
    private string _region = "eastus";
    private bool _autoSaveResults = true;
    private bool _showConfidenceScores = true;
    private bool _isTestingConnection;
    private bool _isSaving;
    private string? _testConnectionStatusMessage;
    private string? _endpointError;
    private string? _apiKeyError;
    private string? _regionError;

    public SettingsPageViewModel(
        ISettingsService settingsService,
        INavigationService navigationService,
        ILogger<SettingsPageViewModel> logger)
        : base(logger)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        Title = "Settings";

        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync, CanSaveSettings);
        TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, CanTestConnection);
    }

    public IAsyncRelayCommand SaveSettingsCommand { get; }

    public IAsyncRelayCommand TestConnectionCommand { get; }

    public string Endpoint
    {
        get => _endpoint;
        set
        {
            if (SetProperty(ref _endpoint, value))
            {
                ValidateEndpoint();
                RefreshCommandStates();
            }
        }
    }

    public string ApiKey
    {
        get => _apiKey;
        set
        {
            if (SetProperty(ref _apiKey, value))
            {
                ValidateApiKey();
                RefreshCommandStates();
            }
        }
    }

    public string Region
    {
        get => _region;
        set
        {
            if (SetProperty(ref _region, value))
            {
                ValidateRegion();
                RefreshCommandStates();
            }
        }
    }

    public bool AutoSaveResults
    {
        get => _autoSaveResults;
        set => SetProperty(ref _autoSaveResults, value);
    }

    public bool ShowConfidenceScores
    {
        get => _showConfidenceScores;
        set => SetProperty(ref _showConfidenceScores, value);
    }

    public bool IsTestingConnection
    {
        get => _isTestingConnection;
        private set
        {
            if (SetProperty(ref _isTestingConnection, value))
            {
                OnPropertyChanged(nameof(IsNotTestingConnection));
                RefreshCommandStates();
            }
        }
    }

    public bool IsNotTestingConnection => !IsTestingConnection;

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetProperty(ref _isSaving, value))
            {
                OnPropertyChanged(nameof(IsNotSaving));
                RefreshCommandStates();
            }
        }
    }

    public bool IsNotSaving => !IsSaving;

    public string? TestConnectionStatusMessage
    {
        get => _testConnectionStatusMessage;
        private set
        {
            if (SetProperty(ref _testConnectionStatusMessage, value))
            {
                OnPropertyChanged(nameof(HasTestConnectionStatusMessage));
            }
        }
    }

    public bool HasTestConnectionStatusMessage => !string.IsNullOrWhiteSpace(TestConnectionStatusMessage);

    public string? EndpointError
    {
        get => _endpointError;
        private set
        {
            if (SetProperty(ref _endpointError, value))
            {
                OnPropertyChanged(nameof(HasEndpointError));
            }
        }
    }

    public bool HasEndpointError => !string.IsNullOrWhiteSpace(EndpointError);

    public string? ApiKeyError
    {
        get => _apiKeyError;
        private set
        {
            if (SetProperty(ref _apiKeyError, value))
            {
                OnPropertyChanged(nameof(HasApiKeyError));
            }
        }
    }

    public bool HasApiKeyError => !string.IsNullOrWhiteSpace(ApiKeyError);

    public string? RegionError
    {
        get => _regionError;
        private set
        {
            if (SetProperty(ref _regionError, value))
            {
                OnPropertyChanged(nameof(HasRegionError));
            }
        }
    }

    public bool HasRegionError => !string.IsNullOrWhiteSpace(RegionError);

    public bool HasValidationErrors => HasEndpointError || HasApiKeyError || HasRegionError;

    protected override void OnBusyChanged(bool isBusy)
    {
        base.OnBusyChanged(isBusy);
        RefreshCommandStates();
    }

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var endpoint = await _settingsService.GetAsync(SettingKeys.AzureEndpoint, string.Empty);
        var apiKey = await _settingsService.GetSecureAsync(SettingKeys.AzureApiKey, string.Empty);
        var region = await _settingsService.GetAsync(SettingKeys.AzureRegion, "eastus");

        var autoSaveResults = await _settingsService.GetAsync(SettingKeys.AutoSaveResults, true);
        var showConfidenceScores = await _settingsService.GetAsync(SettingKeys.ShowConfidenceScores, true);

        _endpoint = endpoint ?? string.Empty;
        _apiKey = apiKey ?? string.Empty;
        _region = region ?? "eastus";
        _autoSaveResults = autoSaveResults;
        _showConfidenceScores = showConfidenceScores;

        OnPropertyChanged(nameof(Endpoint));
        OnPropertyChanged(nameof(ApiKey));
        OnPropertyChanged(nameof(Region));
        OnPropertyChanged(nameof(AutoSaveResults));
        OnPropertyChanged(nameof(ShowConfidenceScores));

        ValidateAll();
        RefreshCommandStates();
    }

    private bool CanSaveSettings()
    {
        return IsNotBusy && !IsSaving && !IsTestingConnection;
    }

    private bool CanTestConnection()
    {
        return IsNotBusy && !IsSaving && !IsTestingConnection;
    }

    private async Task SaveSettingsAsync()
    {
        if (!ValidateAll())
        {
            await _navigationService.DisplayAlertAsync("Validation", "Please fix validation errors before saving.");
            return;
        }

        await ExecuteWithBusyAsync(async _ =>
        {
            IsSaving = true;

            var azureSettings = new AzureSettings
            {
                Endpoint = Endpoint.Trim(),
                ApiKey = ApiKey.Trim(),
                Region = Region.Trim().ToLowerInvariant()
            };

            var validationResults = await _settingsService.ValidateSettingsAsync(azureSettings);
            var firstError = validationResults.FirstOrDefault();
            if (firstError != null)
            {
                await _navigationService.DisplayAlertAsync("Validation", firstError.ErrorMessage ?? "Invalid Azure settings.");
                return;
            }

            await _settingsService.SetAsync(SettingKeys.AzureEndpoint, azureSettings.Endpoint);
            await _settingsService.SetSecureAsync(SettingKeys.AzureApiKey, azureSettings.ApiKey);
            await _settingsService.SetAsync(SettingKeys.AzureRegion, azureSettings.Region);

            await _settingsService.SetAsync(SettingKeys.AutoSaveResults, AutoSaveResults);
            await _settingsService.SetAsync(SettingKeys.ShowConfidenceScores, ShowConfidenceScores);

            await _navigationService.DisplayAlertAsync("Settings", "Settings saved successfully.");

            var canGoBack = await _navigationService.CanGoBackAsync();
            if (canGoBack)
            {
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _navigationService.NavigateToAsync($"//{NavigationRoutes.Home}");
            }
        }, "Unable to save settings right now.");

        IsSaving = false;
    }

    private async Task TestConnectionAsync()
    {
        if (!ValidateAll())
        {
            TestConnectionStatusMessage = "Fix validation errors before testing connection.";
            return;
        }

        await ExecuteWithBusyAsync(async _ =>
        {
            IsTestingConnection = true;
            TestConnectionStatusMessage = "Testing connection...";

            await Task.Delay(1200);

            TestConnectionStatusMessage = "Connection test placeholder succeeded. Full Azure probe will be added in service integration.";
        }, "Unable to test connection right now.");

        IsTestingConnection = false;
    }

    private bool ValidateAll()
    {
        ValidateEndpoint();
        ValidateApiKey();
        ValidateRegion();

        OnPropertyChanged(nameof(HasValidationErrors));
        return !HasValidationErrors;
    }

    private void ValidateEndpoint()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            EndpointError = "Endpoint URL is required.";
            OnPropertyChanged(nameof(HasValidationErrors));
            return;
        }

        if (!Uri.TryCreate(Endpoint.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            EndpointError = "Endpoint must be a valid URL.";
            OnPropertyChanged(nameof(HasValidationErrors));
            return;
        }

        EndpointError = null;
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    private void ValidateApiKey()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            ApiKeyError = "API key is required.";
            OnPropertyChanged(nameof(HasValidationErrors));
            return;
        }

        var trimmed = ApiKey.Trim();
        if (trimmed.Length < 32)
        {
            ApiKeyError = "API key must be at least 32 characters.";
            OnPropertyChanged(nameof(HasValidationErrors));
            return;
        }

        if (!ApiKeyRegex.IsMatch(trimmed))
        {
            ApiKeyError = "API key should contain only hexadecimal characters.";
            OnPropertyChanged(nameof(HasValidationErrors));
            return;
        }

        ApiKeyError = null;
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    private void ValidateRegion()
    {
        if (string.IsNullOrWhiteSpace(Region))
        {
            RegionError = "Region is required.";
            OnPropertyChanged(nameof(HasValidationErrors));
            return;
        }

        RegionError = null;
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    private void RefreshCommandStates()
    {
        SaveSettingsCommand.NotifyCanExecuteChanged();
        TestConnectionCommand.NotifyCanExecuteChanged();
    }
}
