using CommunityToolkit.Diagnostics;

using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

using System.Collections.ObjectModel;

using WindowSill.API;
using WindowSill.ShortTermReminder.UI;

namespace WindowSill.ShortTermReminder;

internal sealed class ShortTermReminderService
{
    internal static ShortTermReminderService Instance { get; } = new ShortTermReminderService();

    private ISettingsProvider? _settingsProvider;

    private ShortTermReminderService()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        AppNotificationManager.Default.NotificationInvoked += OnToastNotificationInvoked;
        AppNotificationManager.Default.Register();

        ViewList.Add(
            new SillListViewPopupItem(
                '\uF8AA',
                "/WindowSill.ShortTermReminder/NewReminderSillListViewPopupItem/NewReminderTooltip".GetLocalizedString(),
                NewReminderPopup.CreateView()));
    }

    internal ObservableCollection<SillListViewItem> ViewList { get; } = new();

    internal async Task InitializeAsync(ISettingsProvider settingsProvider)
    {
        await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            Guard.IsNotNull(settingsProvider);
            _settingsProvider = settingsProvider;

            Reminder[] reminders = _settingsProvider.GetSetting(Settings.Settings.Reminders);
            for (int i = 0; i < reminders?.Length; i++)
            {
                ViewList.Add(ReminderSillListViewPopupItem.CreateView(reminders[i]));
            }
        });
    }

    internal void AddNewReminder(string reminderText, TimeSpan originalReminderDuration, DateTime reminderTime)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        Guard.IsNotNull(_settingsProvider);

        var reminder = new Reminder
        {
            Title = reminderText,
            OriginalReminderDuration = originalReminderDuration,
            ReminderTime = reminderTime
        };

        int insertIndex;

        for (insertIndex = 1; insertIndex < ViewList.Count; insertIndex++)
        {
            if (ViewList[insertIndex].DataContext is ReminderSillListViewPopupItem reminderItem)
            {
                if (reminderItem.Reminder.ReminderTime > reminderTime)
                {
                    break;
                }
            }
        }

        ViewList.Insert(insertIndex, ReminderSillListViewPopupItem.CreateView(reminder));

        SaveReminders();
    }

    internal void DeleteReminder(Guid reminderId)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        Guard.IsNotNull(_settingsProvider);

        SillListViewItem? itemToRemove = ViewList
            .FirstOrDefault(
                r => r.DataContext is ReminderSillListViewPopupItem reminderSillListViewPopupItem
                && reminderSillListViewPopupItem.Reminder.Id == reminderId);

        if (itemToRemove is not null)
        {
            ViewList.Remove(itemToRemove);
            ((ReminderSillListViewPopupItem)itemToRemove.DataContext).Dispose();
        }

        SaveReminders();
    }

    internal void SnoozeReminder(Reminder reminder, TimeSpan snoozeDuration)
    {
        Guard.IsNotNull(_settingsProvider);

        reminder.OriginalReminderDuration = snoozeDuration;
        reminder.ReminderTime = DateTime.Now + snoozeDuration;

        ReminderSillListViewPopupItem? itemToUpdate
            = ViewList.Select(v => v.DataContext)
            .OfType<ReminderSillListViewPopupItem>()
            .FirstOrDefault(r => r.Reminder == reminder);
        itemToUpdate?.EnsureTimerRunning();

        SaveReminders();
    }

    internal async Task NotifyUserAsync(Reminder reminder)
    {
        Guard.IsNotNull(_settingsProvider);
        if (_settingsProvider.GetSetting(Settings.Settings.UseFullScreenNotification))
        {
            var fullScreenNotificationWindow = new FullScreenNotificationWindow(reminder);
            await fullScreenNotificationWindow.ShowAsync();
        }
        else
        {
            AppNotification notification
                = new AppNotificationBuilder()
                    .AddArgument("reminderId", reminder.Id.ToString())
                    .AddText("/WindowSill.ShortTermReminder/Misc/ToastNotificationTitle".GetLocalizedString())
                    .AddText(reminder.Title)
                    .SetAudioEvent(AppNotificationSoundEvent.Reminder)
                    .BuildNotification();
            notification.ExpiresOnReboot = true;
            notification.Priority = AppNotificationPriority.High;

            AppNotificationManager.Default.Show(notification);
        }
    }

    private void SaveReminders()
    {
        Guard.IsNotNull(_settingsProvider);
        Reminder[] reminders
            = ViewList
            .Select(viewItem => viewItem.DataContext)
            .OfType<ReminderSillListViewPopupItem>()
            .Select(reminderItem => reminderItem.Reminder)
            .ToArray();
        _settingsProvider.SetSetting(Settings.Settings.Reminders, reminders);
    }

    private void OnToastNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        if (args.Arguments.TryGetValue("reminderId", out string? reminderIdString) && Guid.TryParse(reminderIdString, out Guid reminderId))
        {
            ThreadHelper.RunOnUIThreadAsync(() =>
            {
                DeleteReminder(reminderId);
            });
        }
    }
}
