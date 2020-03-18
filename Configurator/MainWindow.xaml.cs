using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Ookii.Dialogs.Wpf;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "zh") FontFamily = new FontFamily("Microsoft YaHei UI");

            SharingPathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Task.Run(async () => {
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

                    if (mountPoint == @"N:\") MountedCloudDrive.SelectedIndex = 0;
                    else if (mountPoint == @"P:\") MountedCloudDrive.SelectedIndex = 1;
                    else if (mountPoint == @"Z:\") MountedCloudDrive.SelectedIndex = 2;

                    MountSwitch.IsChecked = mounted;
                });
            });
        }

        private void OnSharingChecked(object sender, RoutedEventArgs e)
        {
            var path = SharingPathBox.Text;
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(path, null)).ConfigureAwait(false);
            });
        }

        private void OnSharingUnchecked(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(null, null)).ConfigureAwait(false);
            });
        }

        private void OnChangePathClicked(object sender, RoutedEventArgs e)
        {
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

                Task.Run(async () => {
                    await Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(path, null)).ConfigureAwait(false);
                });
            }
        }

        private void OnChangeDeviceNameClicked(object sender, RoutedEventArgs e)
        {
            var name = DeviceNameBox.Text;
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.ChangeDeviceName(name, null)).ConfigureAwait(false);
            });
        }

        private void OnInviteClicked(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => {
                var code = await Globals.CloudManager.InvokeAsync(x => x.StartBroadcastingInvitation(null)).ConfigureAwait(false);
                this.ShowAlert("邀请码已生成", "使用此邀请码将其它设备加入个人云：" + Environment.NewLine + Environment.NewLine + code + Environment.NewLine + Environment.NewLine +
                    "关闭此窗口后邀请码将失效。", "停止邀请", true, () => {
                        Task.Run(async () => {
                            await Globals.CloudManager.InvokeAsync(x => x.StopBroadcastingInvitation(null)).ConfigureAwait(false);
                        });
                    });
            });
        }

        private void OnMountChecked(object sender, RoutedEventArgs e)
        {
            string mountPoint;
            if (MountedCloudDrive.SelectedIndex == 0) mountPoint = @"N:\";
            else if (MountedCloudDrive.SelectedIndex == 1) mountPoint = @"P:\";
            else mountPoint = @"Z:\";

            _ = Globals.Mounter.InvokeAsync(x => x.MountNetworkDrive(Globals.PersonalCloud.Value, mountPoint));
        }

        private void OnMountUnchecked(object sender, RoutedEventArgs e)
        {
            _ = Globals.Mounter.InvokeAsync(x => x.UnmountNetworkDrive(Globals.PersonalCloud.Value));
        }

        private void OnMountPointChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string mountPoint;
            if (MountedCloudDrive.SelectedIndex == 0) mountPoint = @"N:\";
            else if (MountedCloudDrive.SelectedIndex == 1) mountPoint = @"P:\";
            else mountPoint = @"Z:\";

            if (MountSwitch.IsChecked == true) _ = Globals.Mounter.InvokeAsync(x => x.MountNetworkDrive(Globals.PersonalCloud.Value, mountPoint));
        }

        private void OnLeaveClicked(object sender, RoutedEventArgs e)
        {
            _ = Globals.CloudManager.InvokeAsync(x => x.LeavePersonalCloud(Globals.PersonalCloud.Value));
        }
    }
}
