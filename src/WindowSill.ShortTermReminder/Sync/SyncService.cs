using CommunityToolkit.Diagnostics;
using WindowSill.API;

namespace WindowSill.ShortTermReminder.Sync;

/// <summary>
/// Central service for managing task synchronization
/// </summary>
internal sealed class SyncService
{
    private static SyncService? _instance;
    internal static SyncService Instance => _instance ??= new SyncService();

    private ISettingsProvider? _settingsProvider;
    private ISyncProvider? _currentProvider;
    private readonly object _syncLock = new();

    private SyncService()
    {
    }

    internal async Task InitializeAsync(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        
        // Load saved provider settings
        SyncProviderType providerType = _settingsProvider.GetSetting(Settings.Settings.SyncProviderType);
        if (providerType != SyncProviderType.None)
        {
            await SetProviderAsync(providerType);
        }
    }

    internal async Task<bool> SetProviderAsync(SyncProviderType providerType)
    {
        Guard.IsNotNull(_settingsProvider);

        lock (_syncLock)
        {
            _currentProvider = providerType switch
            {
                SyncProviderType.MicrosoftToDo => new MicrosoftToDoSyncProvider(_settingsProvider),
                SyncProviderType.GoogleTasks => new GoogleTasksSyncProvider(_settingsProvider),
                _ => null
            };
        }

        if (_currentProvider != null)
        {
            _settingsProvider.SetSetting(Settings.Settings.SyncProviderType, providerType);
            return true;
        }

        return false;
    }

    internal ISyncProvider? CurrentProvider
    {
        get
        {
            lock (_syncLock)
            {
                return _currentProvider;
            }
        }
    }

    internal async Task<bool> AuthenticateAsync()
    {
        if (_currentProvider == null)
            return false;

        return await _currentProvider.AuthenticateAsync();
    }

    internal async Task SignOutAsync()
    {
        if (_currentProvider != null)
        {
            await _currentProvider.SignOutAsync();
        }

        Guard.IsNotNull(_settingsProvider);
        _settingsProvider.SetSetting(Settings.Settings.SyncProviderType, SyncProviderType.None);
        
        lock (_syncLock)
        {
            _currentProvider = null;
        }
    }

    internal async Task<bool> SyncRemindersAsync(IEnumerable<Reminder> localReminders)
    {
        Guard.IsNotNull(_settingsProvider);
        
        if (_currentProvider == null || !_currentProvider.IsAuthenticated)
            return false;

        if (!_settingsProvider.GetSetting(Settings.Settings.SyncEnabled))
            return false;

        try
        {
            SyncDirection direction = _settingsProvider.GetSetting(Settings.Settings.SyncDirection);
            
            switch (direction)
            {
                case SyncDirection.TwoWay:
                    var mergedReminders = await _currentProvider.SyncAsync(localReminders);
                    // The caller should handle updating the local reminders
                    break;

                case SyncDirection.PushOnly:
                    await _currentProvider.PushRemindersAsync(localReminders);
                    break;

                case SyncDirection.PullOnly:
                    var pulledReminders = await _currentProvider.PullRemindersAsync();
                    // The caller should handle updating the local reminders
                    break;
            }

            _settingsProvider.SetSetting(Settings.Settings.LastSyncTime, DateTime.Now);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
