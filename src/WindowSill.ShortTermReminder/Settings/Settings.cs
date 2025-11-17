using WindowSill.API;
using WindowSill.ShortTermReminder.Sync;

namespace WindowSill.ShortTermReminder.Settings;

internal static class Settings
{
    /// <summary>
    /// The list of reminders to be saved so we reload them when the app restarts.
    /// </summary>
    internal static readonly SettingDefinition<Reminder[]> Reminders
        = new(Array.Empty<Reminder>(), typeof(Settings).Assembly);

    /// <summary>
    /// Whether to use full-screen notifications or toast-notifications.
    /// </summary>
    internal static readonly SettingDefinition<bool> UseFullScreenNotification
        = new(true, typeof(Settings).Assembly);

    /// <summary>
    /// Whether synchronization is enabled.
    /// </summary>
    internal static readonly SettingDefinition<bool> SyncEnabled
        = new(false, typeof(Settings).Assembly);

    /// <summary>
    /// The type of sync provider to use (None, MicrosoftToDo, GoogleTasks).
    /// </summary>
    internal static readonly SettingDefinition<SyncProviderType> SyncProviderType
        = new(Sync.SyncProviderType.None, typeof(Settings).Assembly);

    /// <summary>
    /// The direction of synchronization (TwoWay, PushOnly, PullOnly).
    /// </summary>
    internal static readonly SettingDefinition<SyncDirection> SyncDirection
        = new(Sync.SyncDirection.TwoWay, typeof(Settings).Assembly);

    /// <summary>
    /// The last time synchronization was performed.
    /// </summary>
    internal static readonly SettingDefinition<DateTime> LastSyncTime
        = new(DateTime.MinValue, typeof(Settings).Assembly);
}
