using CommunityToolkit.WinUI.Controls;

using WindowSill.API;

namespace WindowSill.ShortTermReminder.Settings;

internal sealed class SettingsView : UserControl
{
    public SettingsView(ISettingsProvider settingsProvider)
    {
        this.DataContext(
            new SettingsViewModel(settingsProvider),
            (view, viewModel) => view
            .Content(
                new StackPanel()
                    .Spacing(2)
                    .Children(
                        new TextBlock()
                            .Style(x => x.ThemeResource("BodyStrongTextBlockStyle"))
                            .Margin(0, 0, 0, 8)
                            .Text("/WindowSill.ShortTermReminder/Settings/General".GetLocalizedString()),

                         new SettingsCard()
                            .Header("/WindowSill.ShortTermReminder/Settings/NotificationMode/Header".GetLocalizedString())
                            .Description("/WindowSill.ShortTermReminder/Settings/NotificationMode/Description".GetLocalizedString())
                            .HeaderIcon(
                                new FontIcon()
                                    .Glyph("\uE7E7")
                            )
                            .Content(
                                new ToggleSwitch()
                                    .OnContent("/WindowSill.ShortTermReminder/Settings/NotificationModeToggle/OnContent".GetLocalizedString())
                                    .OffContent("/WindowSill.ShortTermReminder/Settings/NotificationModeToggle/OffContent".GetLocalizedString())
                                    .IsOn(
                                        x => x.Binding(() => viewModel.UseFullScreenNotification)
                                              .TwoWay()
                                              .UpdateSourceTrigger(UpdateSourceTrigger.PropertyChanged)
                                    )
                            ),

                        new TextBlock()
                            .Style(x => x.ThemeResource("BodyStrongTextBlockStyle"))
                            .Margin(0, 16, 0, 8)
                            .Text("Synchronization"),

                        new SettingsCard()
                            .Header("Enable Synchronization")
                            .Description("Sync your reminders with external task management services")
                            .HeaderIcon(
                                new FontIcon()
                                    .Glyph("\uE895")
                            )
                            .Content(
                                new ToggleSwitch()
                                    .IsOn(
                                        x => x.Binding(() => viewModel.SyncEnabled)
                                              .TwoWay()
                                              .UpdateSourceTrigger(UpdateSourceTrigger.PropertyChanged)
                                    )
                            ),

                        new SettingsCard()
                            .Header("Sync Provider")
                            .Description("Choose which service to sync with")
                            .HeaderIcon(
                                new FontIcon()
                                    .Glyph("\uE774")
                            )
                            .Content(
                                new ComboBox()
                                    .MinWidth(200)
                                    .ItemsSource(new[]
                                    {
                                        "None",
                                        "Microsoft To-Do",
                                        "Google Tasks"
                                    })
                                    .SelectedIndex(
                                        x => x.Binding(() => viewModel.SyncProviderType)
                                              .TwoWay()
                                              .UpdateSourceTrigger(UpdateSourceTrigger.PropertyChanged)
                                              .Convert((Sync.SyncProviderType type) => (int)type,
                                                      (int index) => (Sync.SyncProviderType)index)
                                    )
                            ),

                        new SettingsCard()
                            .Header("Sync Direction")
                            .Description("Choose how to sync your tasks")
                            .HeaderIcon(
                                new FontIcon()
                                    .Glyph("\uE895")
                            )
                                .Content(
                                new ComboBox()
                                    .MinWidth(200)
                                    .ItemsSource(new[]
                                    {
                                        "Two-Way",
                                        "Push Only",
                                        "Pull Only"
                                    })
                                    .SelectedIndex(
                                        x => x.Binding(() => viewModel.SyncDirection)
                                              .TwoWay()
                                              .UpdateSourceTrigger(UpdateSourceTrigger.PropertyChanged)
                                              .Convert((Sync.SyncDirection direction) => (int)direction,
                                                      (int index) => (Sync.SyncDirection)index)
                                    )
                            ),

                        new SettingsCard()
                            .Header("Sync Status")
                            .Description(
                                x => x.Binding(() => viewModel.SyncStatusMessage)
                            )
                            .HeaderIcon(
                                new FontIcon()
                                    .Glyph("\uE895")
                            )
                            .Content(
                                new StackPanel()
                                    .Orientation(Orientation.Horizontal)
                                    .Spacing(8)
                                    .Children(
                                        new Button()
                                            .Content("Authenticate")
                                            .Click(async (s, e) => await viewModel.AuthenticateAsync()),

                                        new Button()
                                            .Content("Sign Out")
                                            .Click(async (s, e) => await viewModel.SignOutAsync()),

                                        new Button()
                                            .Content("Sync Now")
                                            .Click(async (s, e) => await viewModel.ManualSyncAsync())
                                    )
                            )
                    )
            )
        );
    }
}
