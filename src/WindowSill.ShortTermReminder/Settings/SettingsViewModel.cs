using CommunityToolkit.Mvvm.ComponentModel;

using WindowSill.API;
using WindowSill.ShortTermReminder.Sync;

namespace WindowSill.ShortTermReminder.Settings;

internal sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsProvider _settingsProvider;

    public SettingsViewModel(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
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
}
