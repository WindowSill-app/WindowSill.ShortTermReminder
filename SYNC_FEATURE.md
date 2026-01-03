# Task Synchronization Feature

This document describes the task synchronization feature for WindowSill Short Term Reminder.

## Overview

The synchronization feature allows users to sync their short-term reminders with external task management services, including:
- Microsoft To-Do
- Outlook Tasks
- Google Tasks

## Architecture

### Core Components

#### ISyncProvider Interface
Defines the contract for all sync providers:
- `AuthenticateAsync()`: Authenticates with the external service
- `SignOutAsync()`: Signs out from the external service
- `PushRemindersAsync()`: Pushes local reminders to external service
- `PullRemindersAsync()`: Pulls reminders from external service
- `SyncAsync()`: Performs bidirectional synchronization

#### SyncService
Central service managing sync operations:
- Manages the active sync provider
- Coordinates sync operations
- Handles provider switching
- Stores sync configuration

#### Sync Providers
- **MicrosoftToDoSyncProvider**: For Microsoft To-Do / Outlook Tasks
- **GoogleTasksSyncProvider**: For Google Tasks

### Data Model

#### Reminder Extensions
The `Reminder` class has been extended with:
- `ExternalId`: Maps to the external service's task ID
- `LastModified`: Tracks when the reminder was last modified

#### Settings
New settings added:
- `SyncEnabled`: Whether synchronization is enabled
- `SyncProviderType`: Which provider to use (None/MicrosoftToDo/GoogleTasks)
- `SyncDirection`: Sync direction (TwoWay/PushOnly/PullOnly)
- `LastSyncTime`: Timestamp of last successful sync

## User Interface

### Settings View
The settings view includes:
1. **Enable Synchronization Toggle**: Turns sync on/off
2. **Sync Provider ComboBox**: Select which service to sync with
3. **Sync Direction ComboBox**: Choose sync direction
4. **Sync Status Card**: Shows authentication status and last sync time
5. **Action Buttons**:
   - Authenticate: Initiates OAuth flow
   - Sign Out: Disconnects from service
   - Sync Now: Triggers manual sync

## Sync Behavior

### Automatic Sync
Sync is automatically triggered (in background) when:
- A new reminder is added
- A reminder is deleted
- A reminder is snoozed

All automatic sync operations are non-blocking and fail silently to avoid disrupting the user experience.

### Manual Sync
Users can trigger a manual sync from the settings view using the "Sync Now" button.

### Sync Directions

#### Two-Way Sync (Default)
- Changes in both directions are synchronized
- Conflicts are resolved (newest wins)
- Both local and remote tasks stay in sync

#### Push Only
- Only local changes are pushed to the external service
- Remote changes are ignored
- Useful for backup scenarios

#### Pull Only
- Only remote changes are pulled to local
- Local changes are not pushed
- Useful for read-only scenarios

## Implementation Status

### Completed ✅
- Core architecture and interfaces
- Sync service implementation
- UI for sync configuration
- Automatic sync triggers
- Manual sync functionality
- Settings persistence

### Pending ⏳
- Microsoft Graph API integration
- Google Tasks API integration
- OAuth2 authentication flows
- Secure credential storage
- Conflict resolution strategies
- Error handling and retry logic
- Rate limiting and throttling

## Future Implementation Steps

### For Microsoft To-Do Integration

1. **Add NuGet Package**:
   ```xml
   <PackageReference Include="Microsoft.Graph" Version="5.x.x" />
   <PackageReference Include="Microsoft.Identity.Client" Version="4.x.x" />
   ```

2. **Implement Authentication**:
   - Register app in Azure AD
   - Implement MSAL authentication flow
   - Request `Tasks.ReadWrite` permission
   - Store tokens securely using Windows Credential Manager

3. **Implement API Operations**:
   - Map `Reminder` to `TodoTask`
   - Use Microsoft Graph `todoTasks` endpoints
   - Handle pagination for large task lists

### For Google Tasks Integration

1. **Add NuGet Package**:
   ```xml
   <PackageReference Include="Google.Apis.Tasks.v1" Version="1.x.x" />
   <PackageReference Include="Google.Apis.Auth" Version="1.x.x" />
   ```

2. **Implement Authentication**:
   - Register app in Google Cloud Console
   - Implement OAuth2 flow
   - Request `https://www.googleapis.com/auth/tasks` scope
   - Store tokens securely

3. **Implement API Operations**:
   - Map `Reminder` to Google `Task`
   - Use Google Tasks API v1
   - Handle pagination and rate limits

## Security Considerations

1. **Token Storage**: Use Windows Credential Manager or similar secure storage
2. **HTTPS Only**: All API calls must use HTTPS
3. **Token Refresh**: Implement automatic token refresh
4. **Scope Minimization**: Request only necessary permissions
5. **Error Handling**: Don't expose sensitive data in error messages

## Testing Recommendations

1. Test authentication flows for both providers
2. Test all sync directions
3. Test conflict resolution
4. Test sync with large numbers of reminders
5. Test offline scenarios
6. Test token expiration and refresh
7. Test error recovery

## Known Limitations

1. **One Provider at a Time**: Users can only connect to one provider at a time
2. **No Multi-Device Sync**: If using the same external service from multiple devices, conflicts may occur
3. **Time-Based Reminders**: External services may not support exact time-based reminders as WindowSill does
4. **Full-Screen Notifications**: External services don't support WindowSill's full-screen notification feature

## Troubleshooting

### Sync Not Working
1. Check if sync is enabled in settings
2. Verify authentication status
3. Check network connectivity
4. Review last sync time and error messages

### Authentication Failed
1. Verify app registration credentials
2. Check required permissions are granted
3. Clear cached tokens and re-authenticate
4. Check for expired refresh tokens

### Data Not Syncing
1. Check sync direction settings
2. Verify reminders have `LastModified` timestamps
3. Check for conflicts
4. Try manual sync to see detailed errors
