namespace WindowSill.ShortTermReminder.Sync;

/// <summary>
/// Enumeration for sync provider types
/// </summary>
internal enum SyncProviderType
{
    None = 0,
    MicrosoftToDo = 1,
    GoogleTasks = 2
}

/// <summary>
/// Enumeration for sync direction
/// </summary>
internal enum SyncDirection
{
    /// <summary>
    /// Two-way sync: changes in both directions
    /// </summary>
    TwoWay = 0,

    /// <summary>
    /// Push only: local changes to external service
    /// </summary>
    PushOnly = 1,

    /// <summary>
    /// Pull only: external changes to local
    /// </summary>
    PullOnly = 2
}
