using System;
using System.Threading.Tasks;
using System.Windows;

using NSPersonalCloud.WindowsConfigurator.Resources;
using NSPersonalCloud.WindowsContract;

namespace NSPersonalCloud.WindowsConfigurator.IPC
{
    public class NotificationCenter : ICloudEventHandler
    {
        public void OnServiceStarted()
        {
//             if (Application.Current is null) return;
// 
//             Application.Current.Dispatcher.Invoke(() => {
//                 Application.Current.MainWindow?.Close();
//                 Application.Current.MainWindow = null;
//             });
// 
//             Task.Run(async () => {
//                 var cloud = await Globals.CloudManager.InvokeAsync(x => x.GetAllPersonalCloud()).ConfigureAwait(false);
//                 Globals.PersonalCloud = cloud.Length == 0 ? null : (Guid?) cloud[0];
// 
//                 Application.Current.Dispatcher.Invoke(() => {
//                     if (Globals.PersonalCloud != null) Application.Current.MainWindow = new MainWindow();
//                     else Application.Current.MainWindow = new WelcomeWindow();
//                     if (!Globals.DoNotShowMainWinOnLaunch) Application.Current.MainWindow.Show();
//                     Globals.DoNotShowMainWinOnLaunch = false;
//                 });
//             });
        }

        public void OnLeftPersonalCloud()
        {
            if (Application.Current is null) return;

            Application.Current.Dispatcher.Invoke(() => {
                var shouldShow = false;
                if (Application.Current.MainWindow?.IsVisible == true) shouldShow = true;
                Application.Current.MainWindow?.Close();
                Application.Current.MainWindow = null;

                Globals.PersonalCloud = null;
                Application.Current.MainWindow = new WelcomeWindow();
                if (shouldShow) Application.Current.MainWindow.Show();
            });
        }

        public void OnMountedVolumesChanged()
        {
            // Todo
        }

        public void OnPersonalCloudAdded()
        {
            if (Application.Current is null) return;

            var shouldShow = false;
            Application.Current.Dispatcher.Invoke(() => {
                if (Application.Current.MainWindow?.IsVisible == true) shouldShow = true;
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = null;
            });

            Task.Run(async () => {
                var cloud = await Globals.CloudManager.InvokeAsync(x => x.GetAllPersonalCloud()).ConfigureAwait(false);
                Globals.PersonalCloud = cloud[0];

                Application.Current.Dispatcher.Invoke(() => {
                    Application.Current.MainWindow = new MainWindow();
                    if (shouldShow) Application.Current.MainWindow.Show();
                });
            });
        }

        public void OnServiceStopped()
        {
            if (Application.Current is null) return;

            Application.Current.Dispatcher.Invoke(() => {
                Application.Current.MainWindow?.Close();
                Application.Current.MainWindow = null;
            });
        }

        public void OnVolumeIOError(string mountPoint, Exception exception)
        {
            Application.Current?.ShowAlert(UISettings.AlertCannotMountDrive, UISettings.AlertCannotMountDriveTroubleshoot);
        }

        #region Pop-up Alert

        public void ShowAlert(string title, string message)
        {
            Application.Current?.Dispatcher?.ShowAlert(title, message);
        }

        #endregion
    }
}
