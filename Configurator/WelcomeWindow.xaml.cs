using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Ookii.Dialogs.Wpf;

using Unishare.Apps.WindowsConfigurator.Resources;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
            FontFamily = new FontFamily("Microsoft YaHei UI");
        }

        private void OnCreateClicked(object sender, RoutedEventArgs e)
        {
            var deviceName = CreateDeviceNameBox.Text;
            foreach (var c in deviceName)
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

            var cloudName = CreateCloudNameBox.Text;

            CreateButton.IsEnabled = JoinButton.IsEnabled = false;

            Task.Run(async () => {
                try
                {
                    await Globals.CloudManager.InvokeAsync(x => x.CreatePersonalCloud(cloudName, deviceName)).ConfigureAwait(false);
                }
                catch
                {
                    Dispatcher.Invoke(() => {
                        using var dialog = new TaskDialog {
                            MainIcon = TaskDialogIcon.Error,
                            WindowTitle = UISettings.Configurator,
                            MainInstruction = "创建个人云失败"
                        };

                        var ok = new TaskDialogButton(ButtonType.Ok);
                        dialog.Buttons.Add(ok);
                        dialog.ShowDialog(this);

                        CreateButton.IsEnabled = JoinButton.IsEnabled = true;
                    });
                }
            });
        }

        private void OnJoinClicked(object sender, RoutedEventArgs e)
        {
            var deviceName = JoinDeviceNameBox.Text;
            foreach (var c in deviceName)
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

            var invite = JoinInvitationBox.Text;

            CreateButton.IsEnabled = JoinButton.IsEnabled = false;

            Task.Run(async () => {
                try
                {
                    await Globals.CloudManager.InvokeAsync(x => x.JoinPersonalCloud(invite, deviceName)).ConfigureAwait(false);
                }
                catch
                {
                    Dispatcher.Invoke(() => {
                        using var dialog = new TaskDialog {
                            MainIcon = TaskDialogIcon.Error,
                            WindowTitle = UISettings.Configurator,
                            MainInstruction = "加入个人云失败"
                        };

                        var ok = new TaskDialogButton(ButtonType.Ok);
                        dialog.Buttons.Add(ok);
                        dialog.ShowDialog(this);

                        CreateButton.IsEnabled = JoinButton.IsEnabled = true;
                    });
                }
            });
        }

        private void OnQuitClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
