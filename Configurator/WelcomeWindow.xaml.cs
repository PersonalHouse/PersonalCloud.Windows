using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Ookii.Dialogs.Wpf;

using NSPersonalCloud.WindowsConfigurator.Resources;

namespace NSPersonalCloud.WindowsConfigurator
{
    public partial class WelcomeWindow : Window
    {
        private bool initialized;

        public WelcomeWindow()
        {
            InitializeComponent();
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh") FontFamily = new FontFamily("Microsoft YaHei UI");

            initialized = true;
        }

        private void OnCreateClicked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

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
                            MainInstruction = UILanding.ErrorCreatingCloud
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
            if (!initialized) return;

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
                            MainInstruction = UILanding.ErrorEnrollingDevice
                        };

                        var ok = new TaskDialogButton(ButtonType.Ok);
                        dialog.Buttons.Add(ok);
                        dialog.ShowDialog(this);

                        CreateButton.IsEnabled = JoinButton.IsEnabled = true;
                    });
                }
            });
        }

        private void OnExpanded(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            if (sender == CreateSectionExpander) JoinSectionExpander.IsExpanded = false;
            else if (sender == JoinSectionExpander) CreateSectionExpander.IsExpanded = false;
        }
    }
}
