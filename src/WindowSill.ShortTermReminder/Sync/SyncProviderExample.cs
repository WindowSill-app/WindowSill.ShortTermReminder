/*
 * EXAMPLE: How to Implement a Complete Sync Provider
 * 
 * This file demonstrates how to implement a complete synchronization provider
 * for an external task management service. This example shows the Microsoft To-Do
 * integration pattern.
 */

using Microsoft.Graph;
using Microsoft.Identity.Client;
using WindowSill.API;

namespace WindowSill.ShortTermReminder.Sync;

/// <summary>
/// Example implementation of Microsoft To-Do sync provider
/// NOTE: This requires the following NuGet packages:
/// - Microsoft.Graph (>= 5.0.0)
/// - Microsoft.Identity.Client (>= 4.0.0)
/// </summary>
internal sealed class MicrosoftToDoSyncProviderExample : ISyncProvider
{
    private readonly ISettingsProvider _settingsProvider;
    private IPublicClientApplication? _msalClient;
    private GraphServiceClient? _graphClient;
    private string? _accessToken;

    // Azure AD App Registration settings
    private const string ClientId = "YOUR_CLIENT_ID_HERE";
    private const string TenantId = "common"; // or your tenant ID
    private static readonly string[] Scopes = new[] { "Tasks.ReadWrite" };

    public MicrosoftToDoSyncProviderExample(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
    }

    public string ProviderName => "Microsoft To-Do";

    public bool IsAuthenticated => _graphClient != null && !string.IsNullOrEmpty(_accessToken);

    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            // Initialize MSAL client
            _msalClient = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                .WithRedirectUri("http://localhost") // Or your registered redirect URI
                .Build();

            // Try silent authentication first
            var accounts = await _msalClient.GetAccountsAsync();
            AuthenticationResult? authResult = null;

            if (accounts.Any())
            {
                try
                {
                    authResult = await _msalClient.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // Silent auth failed, need interactive auth
                }
            }

            // If silent auth failed or no accounts exist, do interactive auth
            if (authResult == null)
            {
                authResult = await _msalClient.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
            }

            _accessToken = authResult.AccessToken;

            // Initialize Graph client
            _graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                    await Task.CompletedTask;
                }));

            // Store refresh token securely (example - use Windows Credential Manager in production)
            // CredentialManager.WriteCredential("WindowSill.MicrosoftToDo", authResult.Account.Username, refreshToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        if (_msalClient != null)
        {
            var accounts = await _msalClient.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await _msalClient.RemoveAsync(account);
            }
        }

        _graphClient = null;
        _accessToken = null;
        _msalClient = null;

        // Clear stored credentials
        // CredentialManager.DeleteCredential("WindowSill.MicrosoftToDo");
    }

    public async Task PushRemindersAsync(IEnumerable<Reminder> reminders)
    {
        if (_graphClient == null)
            throw new InvalidOperationException("Not authenticated");

        // Get the default task list
        var taskLists = await _graphClient.Me.Todo.Lists
            .Request()
            .GetAsync();

        var defaultList = taskLists.FirstOrDefault(l => l.WellknownListName == WellknownListName.DefaultList);
        if (defaultList == null)
            return;

        foreach (var reminder in reminders)
        {
            // Check if task already exists (by ExternalId)
            if (!string.IsNullOrEmpty(reminder.ExternalId))
            {
                try
                {
                    // Update existing task
                    var updateTask = new TodoTask
                    {
                        Title = reminder.Title,
                        DueDateTime = new DateTimeTimeZone
                        {
                            DateTime = reminder.ReminderTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                            TimeZone = TimeZoneInfo.Local.Id
                        }
                    };

                    await _graphClient.Me.Todo.Lists[defaultList.Id].Tasks[reminder.ExternalId]
                        .Request()
                        .UpdateAsync(updateTask);
                }
                catch
                {
                    // Task doesn't exist, create new
                    reminder.ExternalId = null;
                }
            }

            if (string.IsNullOrEmpty(reminder.ExternalId))
            {
                // Create new task
                var newTask = new TodoTask
                {
                    Title = reminder.Title,
                    DueDateTime = new DateTimeTimeZone
                    {
                        DateTime = reminder.ReminderTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        TimeZone = TimeZoneInfo.Local.Id
                    }
                };

                var createdTask = await _graphClient.Me.Todo.Lists[defaultList.Id].Tasks
                    .Request()
                    .AddAsync(newTask);

                reminder.ExternalId = createdTask.Id;
            }
        }
    }

    public async Task<IEnumerable<Reminder>> PullRemindersAsync()
    {
        if (_graphClient == null)
            throw new InvalidOperationException("Not authenticated");

        var taskLists = await _graphClient.Me.Todo.Lists
            .Request()
            .GetAsync();

        var defaultList = taskLists.FirstOrDefault(l => l.WellknownListName == WellknownListName.DefaultList);
        if (defaultList == null)
            return Array.Empty<Reminder>();

        var tasks = await _graphClient.Me.Todo.Lists[defaultList.Id].Tasks
            .Request()
            .Filter("status ne 'completed'") // Only get incomplete tasks
            .GetAsync();

        var reminders = new List<Reminder>();
        foreach (var task in tasks)
        {
            if (task.DueDateTime?.DateTime != null)
            {
                var dueDateTime = DateTime.Parse(task.DueDateTime.DateTime);
                
                reminders.Add(new Reminder
                {
                    Title = task.Title ?? "Untitled",
                    ReminderTime = dueDateTime,
                    OriginalReminderDuration = dueDateTime - DateTime.Now,
                    ExternalId = task.Id,
                    LastModified = task.LastModifiedDateTime?.DateTime ?? DateTime.Now
                });
            }
        }

        return reminders;
    }

    public async Task<IEnumerable<Reminder>> SyncAsync(IEnumerable<Reminder> localReminders)
    {
        if (_graphClient == null)
            throw new InvalidOperationException("Not authenticated");

        // 1. Pull remote tasks
        var remoteReminders = await PullRemindersAsync();

        // 2. Merge local and remote
        var merged = new Dictionary<Guid, Reminder>();

        // Add all local reminders
        foreach (var local in localReminders)
        {
            merged[local.Id] = local;
        }

        // Process remote reminders
        foreach (var remote in remoteReminders)
        {
            // Try to find matching local reminder by ExternalId
            var matchingLocal = localReminders.FirstOrDefault(l => l.ExternalId == remote.ExternalId);

            if (matchingLocal != null)
            {
                // Conflict resolution: newest wins
                if (remote.LastModified > matchingLocal.LastModified)
                {
                    merged[matchingLocal.Id] = remote;
                }
            }
            else
            {
                // New remote reminder, add it
                merged[remote.Id] = remote;
            }
        }

        // 3. Push changes back
        await PushRemindersAsync(merged.Values);

        return merged.Values;
    }
}
