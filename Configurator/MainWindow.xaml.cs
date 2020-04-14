using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Ookii.Dialogs.Wpf;

using Unishare.Apps.WindowsConfigurator.Resources;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class MainWindow : Window
    {
        internal IReadOnlyList<char> DriveLetters { get; private set; }

        private bool initialized;

        public MainWindow()
        {
            InitializeComponent();

            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh") FontFamily = new FontFamily("Microsoft YaHei UI");
            SharingPathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var letters = new List<char> { '—' };
            letters.AddRange(Enumerable.Range(68, 23).Select(x => (char) x));
            DriveLetters = letters.AsReadOnly();

            MountedCloudDrive.ItemsSource = DriveLetters;
            MountedCloudDrive.SelectedIndex = 0;

            Task.Run(async () => {
                try
                {
                    var sharingEnabled = await Globals.Storage.InvokeAsync(x => x.IsFileSharingEnabled(null)).ConfigureAwait(false);
                    var sharingPath = await Globals.Storage.InvokeAsync(x => x.GetFileSharingRoot()).ConfigureAwait(false);

                    var deviceName = await Globals.Storage.InvokeAsync(x => x.GetDeviceName(null)).ConfigureAwait(false);
                    var cloudName = await Globals.Storage.InvokeAsync(x => x.GetPersonalCloudName(Globals.PersonalCloud.Value)).ConfigureAwait(false);

                    var mounted = await Globals.Storage.InvokeAsync(x => x.IsExplorerIntegrationEnabled()).ConfigureAwait(false);
                    var mountPoint = await Globals.Storage.InvokeAsync(x => x.GetMountPointForPersonalCloud(Globals.PersonalCloud.Value)).ConfigureAwait(false);

                    Dispatcher.Invoke(() => {
                        SharingSwitch.IsChecked = sharingEnabled;
                        if (!string.IsNullOrEmpty(sharingPath)) SharingPathBox.Text = sharingPath;

                        DeviceNameBox.Text = deviceName;
                        CloudNameBox.Text = cloudName;
                        MountedCloudName.Text = cloudName;
                        MountSwitch.IsChecked = mounted;

                        if (mountPoint?.Length != 3) MountedCloudDrive.SelectedIndex = 0;
                        else
                        {
                            var letterIndex = mountPoint[0] - 68 + 1;
                            MountedCloudDrive.SelectedIndex = letterIndex;
                        }

                        initialized = true;
                    });
                }
                catch
                {
                    initialized = true;
                }
            });
        }

        private void OnSharingChecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            var path = SharingPathBox.Text;
            _ = Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(path, null));
        }

        private void OnSharingUnchecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            _ = Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(null, null));
        }

        private void OnChangePathClicked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            var browseDialog = new VistaFolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.Personal,
                ShowNewFolderButton = true
            };
            var selected = browseDialog.ShowDialog(this);
            if (selected == true)
            {
                var path = browseDialog.SelectedPath;
                if (!Directory.Exists(path)) return;
                SharingPathBox.Text = path;

                _ = Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(path, null));
            }
        }

        private void OnChangeDeviceNameClicked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            var name = DeviceNameBox.Text;
            foreach (var c in name)
            {
                if (Path.GetInvalidFileNameChars().Contains(c))
                {
                    using var dialog = new TaskDialog {
                        MainIcon = TaskDialogIcon.Error,
                        WindowTitle = UISettings.Configurator,
                        MainInstruction = UISettings.AlertInvalidDeviceName,
                        Content = UISettings.AlertInvalidDeviceNameMessage
                    };

                    var ok = new TaskDialogButton(ButtonType.Ok);
                    dialog.Buttons.Add(ok);
                    dialog.ShowDialog(this);
                    return;
                }
            }

            _ = Globals.CloudManager.InvokeAsync(x => x.ChangeDeviceName(name, null));
        }

        private void OnInviteClicked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            Task.Run(async () => {
                var code = await Globals.CloudManager.InvokeAsync(x => x.StartBroadcastingInvitation(null)).ConfigureAwait(false);
                this.ShowAlert("邀请码已生成", "使用此邀请码将其它设备加入个人云：" + Environment.NewLine + Environment.NewLine + code + Environment.NewLine + Environment.NewLine +
                    "关闭窗口后邀请码将失效。", "停止邀请", true, () => _ = Globals.CloudManager.InvokeAsync(x => x.StopBroadcastingInvitation(null)));
            });
        }

        private void OnMountChecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            if (MountedCloudDrive.SelectedIndex == 0) return;
            var mountPoint = DriveLetters[MountedCloudDrive.SelectedIndex] + @":\";
            _ = Globals.Mounter.InvokeAsync(x => x.MountNetworkDrive(Globals.PersonalCloud.Value, mountPoint));
        }

        private void OnMountUnchecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            _ = Globals.Mounter.InvokeAsync(x => x.UnmountAllDrives());
        }

        private void OnMountPointChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!initialized) return;

            if (MountSwitch.IsChecked != true) return;

            var mountPoint = MountedCloudDrive.SelectedIndex == 0 ? null : (DriveLetters[MountedCloudDrive.SelectedIndex] + @":\");
            if (mountPoint != null) _ = Globals.Mounter.InvokeAsync(x => x.MountNetworkDrive(Globals.PersonalCloud.Value, mountPoint));
            else _ = Globals.Mounter.InvokeAsync(x => x.UnmountNetworkDrive(Globals.PersonalCloud.Value));
        }

        private void OnLeaveClicked(object sender, RoutedEventArgs e)
        {
            _ = Globals.CloudManager.InvokeAsync(x => x.LeavePersonalCloud(Globals.PersonalCloud.Value));
        }

        private void OnQuitClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnConnectToStorageClicked(object sender, RoutedEventArgs e)
        {
            var child = new ConnectionsWindow();
            child.ShowDialog();
        }

        private void OnEditExtensionsClicked(object sender, RoutedEventArgs e)
        {
            var child = new ExtensionsWindow();
            child.ShowDialog();
        }
    }
}
