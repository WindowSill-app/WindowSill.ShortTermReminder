using WindowSill.API;

namespace WindowSill.ShortTermReminder.Sync;

/// <summary>
/// Synchronization provider for Microsoft To-Do / Outlook Tasks
/// </summary>
internal sealed class MicrosoftToDoSyncProvider : ISyncProvider
{
    private readonly ISettingsProvider _settingsProvider;
    private bool _isAuthenticated;

    public MicrosoftToDoSyncProvider(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        _isAuthenticated = false;
    }

    public string ProviderName => "Microsoft To-Do";

    public bool IsAuthenticated => _isAuthenticated;

    public async Task<bool> AuthenticateAsync()
    {
        // TODO: Implement Microsoft Graph authentication
        // This would use Microsoft.Identity.Client for OAuth2/MSAL authentication
        await Task.CompletedTask;
        _isAuthenticated = false;
        return false;
    }

    public async Task SignOutAsync()
    {
        // TODO: Clear stored tokens
        await Task.CompletedTask;
        _isAuthenticated = false;
    }

    public async Task PushRemindersAsync(IEnumerable<Reminder> reminders)
    {
        if (!_isAuthenticated)
            throw new InvalidOperationException("Not authenticated");

        // TODO: Implement pushing reminders to Microsoft To-Do via Microsoft Graph API
        // Use the /me/todo/lists/{listId}/tasks endpoint
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Reminder>> PullRemindersAsync()
    {
        if (!_isAuthenticated)
            throw new InvalidOperationException("Not authenticated");

        // TODO: Implement pulling reminders from Microsoft To-Do via Microsoft Graph API
        // Use the /me/todo/lists/{listId}/tasks endpoint
        await Task.CompletedTask;
        return Array.Empty<Reminder>();
    }

    public async Task<IEnumerable<Reminder>> SyncAsync(IEnumerable<Reminder> localReminders)
    {
        if (!_isAuthenticated)
            throw new InvalidOperationException("Not authenticated");

        // TODO: Implement two-way sync logic
        // 1. Pull remote tasks
        // 2. Compare with local reminders
        // 3. Resolve conflicts (newest wins, or use last sync time)
        // 4. Push local changes
        // 5. Update local with remote changes
        await Task.CompletedTask;
        return localReminders;
    }
}
