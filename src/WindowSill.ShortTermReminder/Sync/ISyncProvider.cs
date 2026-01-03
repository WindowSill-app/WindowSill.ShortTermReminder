namespace WindowSill.ShortTermReminder.Sync;

/// <summary>
/// Interface for task synchronization providers (Microsoft To-Do, Google Tasks, etc.)
/// </summary>
internal interface ISyncProvider
{
    /// <summary>
    /// Gets the display name of the sync provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether the provider is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Authenticates with the external service
    /// </summary>
    Task<bool> AuthenticateAsync();

    /// <summary>
    /// Signs out from the external service
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Pushes local reminders to the external service
    /// </summary>
    /// <param name="reminders">Reminders to push</param>
    Task PushRemindersAsync(IEnumerable<Reminder> reminders);

    /// <summary>
    /// Pulls reminders from the external service
    /// </summary>
    /// <returns>Reminders from the external service</returns>
    Task<IEnumerable<Reminder>> PullRemindersAsync();

    /// <summary>
    /// Synchronizes reminders bidirectionally
    /// </summary>
    /// <param name="localReminders">Local reminders</param>
    /// <returns>Merged reminders after sync</returns>
    Task<IEnumerable<Reminder>> SyncAsync(IEnumerable<Reminder> localReminders);
}
