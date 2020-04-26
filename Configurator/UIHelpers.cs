using System;
using System.Windows;
using System.Windows.Threading;

using Ookii.Dialogs.Wpf;

using Unishare.Apps.WindowsConfigurator.Resources;

namespace Unishare.Apps.WindowsConfigurator
{
    public static class UIHelpers
    {
        public static void ShowLoadingMessage()
        {
            var dialog = new TaskDialog {
                MainIcon = TaskDialogIcon.Information,
                WindowTitle = UISettings.Cloud,
                MainInstruction = UISettings.ServiceNotRunning,
                Content = UISettings.WaitForService
            };

            var ok = new TaskDialogButton(ButtonType.Ok);
            dialog.Buttons.Add(ok);

            Application.Current.Dispatcher.Invoke(() => {
                dialog.ShowDialog();
                dialog.Dispose();
            });
        }

        public static void ShowAlert(this Window window, string title, string message) => window.Dispatcher.ShowAlert(title, message);

        public static void ShowAlert(this Application app, string title, string message) => app.Dispatcher.ShowAlert(title, message);

        public static void ShowAlert(this Dispatcher dispatcher, string title, string message)
        {
            var dialog = new TaskDialog {
                MainIcon = TaskDialogIcon.Information,
                WindowTitle = UISettings.Cloud,
                MainInstruction = title,
                Content = message
            };

            var ok = new TaskDialogButton(ButtonType.Ok);
            dialog.Buttons.Add(ok);

            dispatcher.Invoke(() => {
                dialog.ShowDialog();
                dialog.Dispose();
            });
        }

        public static void ShowAlert(this Window window, string title, string message, string action, bool actionIsCommand = false, Action callback = null)
            => window.Dispatcher.ShowAlert(title, message, action, actionIsCommand, callback);

        public static void ShowAlert(this Application app, string title, string message, string action, bool actionIsCommand = false, Action callback = null)
            => app.Dispatcher.ShowAlert(title, message, action, actionIsCommand, callback);

        public static void ShowAlert(this Dispatcher dispatcher, string title, string message, string action, bool actionIsCommand = false, Action callback = null)
        {
            var dialog = new TaskDialog {
                MainIcon = TaskDialogIcon.Information,
                WindowTitle = UISettings.Cloud,
                MainInstruction = title,
                Content = message
            };

            var ok = new TaskDialogButton {
                Text = action
            };
            if (actionIsCommand) dialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
            dialog.Buttons.Add(ok);

            dispatcher.Invoke(() => {
                dialog.ShowDialog();
                dialog.Dispose();

                callback?.Invoke();
            });
        }
    }
}
