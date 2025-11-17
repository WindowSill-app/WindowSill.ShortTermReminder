using CommunityToolkit.Mvvm.ComponentModel;

using WindowSill.API;
using WindowSill.ShortTermReminder.Sync;

namespace WindowSill.ShortTermReminder.Settings;

internal sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsProvider _settingsProvider;
    private string _syncStatusMessage = string.Empty;

    public SettingsViewModel(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        UpdateSyncStatus();
    }

    public bool UseFullScreenNotification
    {
        get => _settingsProvider.GetSetting(Settings.UseFullScreenNotification);
        set => _settingsProvider.SetSetting(Settings.UseFullScreenNotification, value);
    }

    public bool SyncEnabled
    {
        get => _settingsProvider.GetSetting(Settings.SyncEnabled);
        set
        {
            _settingsProvider.SetSetting(Settings.SyncEnabled, value);
            OnPropertyChanged();
        }
    }

    public SyncProviderType SyncProviderType
    {
        get => _settingsProvider.GetSetting(Settings.SyncProviderType);
        set
        {
            _settingsProvider.SetSetting(Settings.SyncProviderType, value);
            OnPropertyChanged();
        }
    }

    public SyncDirection SyncDirection
    {
        get => _settingsProvider.GetSetting(Settings.SyncDirection);
        set
        {
            _settingsProvider.SetSetting(Settings.SyncDirection, value);
            OnPropertyChanged();
        }
    }

    public DateTime LastSyncTime
    {
        get => _settingsProvider.GetSetting(Settings.LastSyncTime);
    }

    public string SyncStatusMessage
    {
        get => _syncStatusMessage;
        set
        {
            _syncStatusMessage = value;
            OnPropertyChanged();
        }
    }

    public async Task AuthenticateAsync()
    {
        bool success = await SyncService.Instance.AuthenticateAsync();
        UpdateSyncStatus();
    }

    public async Task SignOutAsync()
    {
        await SyncService.Instance.SignOutAsync();
        UpdateSyncStatus();
    }

    public async Task ManualSyncAsync()
    {
        SyncStatusMessage = "Syncing...";
        bool success = await ShortTermReminderService.Instance.ManualSyncAsync();
        UpdateSyncStatus();
        SyncStatusMessage = success ? "Sync completed successfully" : "Sync failed";
    }

    private void UpdateSyncStatus()
    {
        var provider = SyncService.Instance.CurrentProvider;
        if (provider == null)
        {
            SyncStatusMessage = "No sync provider selected";
        }
        else if (provider.IsAuthenticated)
        {
            var lastSync = LastSyncTime;
            if (lastSync == DateTime.MinValue)
            {
                SyncStatusMessage = $"Connected to {provider.ProviderName} - Never synced";
            }
            else
            {
                SyncStatusMessage = $"Connected to {provider.ProviderName} - Last synced: {lastSync:g}";
            }
        }
        else
        {
            SyncStatusMessage = $"{provider.ProviderName} - Not authenticated";
        }
    }
}
