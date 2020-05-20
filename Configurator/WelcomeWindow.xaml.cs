using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using MaterialDesignThemes.Wpf;

using NSPersonalCloud.WindowsConfigurator.Resources;

namespace NSPersonalCloud.WindowsConfigurator
{
    public partial class WelcomeWindow : Window
    {
        private readonly bool initialized;

        private RadioButton lastChoice;
        private bool hasPendingTask;

        public WelcomeWindow()
        {
            InitializeComponent();

            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh") FontFamily = new FontFamily("Microsoft YaHei UI");
            DeviceNameBox.Text = Environment.MachineName;
            lastChoice = CreateTab;

            initialized = true;
        }

        private void SwitchTab(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            if (sender == lastChoice) return;
            if (sender is RadioButton button) lastChoice = button;

            if (sender == CreateTab)
            {
                JoinTab.IsChecked = false;
                CreateTab.IsChecked = true;
                CloudNameBox.Text = string.Empty;
                CloudLabel.Text = UILanding.CloudName;
                GoButton.Content = UILanding.CreateCloud;
            }
            else if (sender == JoinTab)
            {
                CreateTab.IsChecked = false;
                JoinTab.IsChecked = true;
                CloudNameBox.Text = string.Empty;
                CloudLabel.Text = UILanding.Invitation;
                GoButton.Content = UILanding.JoinCloud;
            }
        }

        private void EnrollDevice(object sender, RoutedEventArgs e)
        {
            if (!initialized || hasPendingTask) return;

            var deviceName = DeviceNameBox.Text;
            foreach (var c in deviceName)
            {
                if (Path.GetInvalidFileNameChars().Contains(c))
                {
                    DialogText.Text = UISettings.AlertInvalidDeviceName;
                    Dialog.IsOpen = true;
                    return;
                }
            }

            if (CreateTab.IsChecked == true) CreatePersonalCloud(deviceName, CloudNameBox.Text);
            else if (JoinTab.IsChecked == true) JoinPersonalCloud(deviceName, CloudNameBox.Text);
        }

        private void ToggleBusyIndicator(bool isBusy)
        {
            hasPendingTask = isBusy;
            ButtonProgressAssist.SetIsIndicatorVisible(GoButton, isBusy);
        }

        private void CreatePersonalCloud(string deviceName, string cloudName)
        {
            ToggleBusyIndicator(true);

            Task.Run(async () => {
                // Animation
                await Task.Delay(3000).ConfigureAwait(false);

                try
                {
                    await Globals.CloudManager.InvokeAsync(x => x.CreatePersonalCloud(cloudName, deviceName)).ConfigureAwait(false);
                }
                catch
                {
                    Dispatcher.Invoke(() => {
                        DialogText.Text = UILanding.ErrorCreatingCloud;
                        Dialog.IsOpen = true;

                        ToggleBusyIndicator(false);
                    });
                }
            });
        }

        private void JoinPersonalCloud(string deviceName, string invite)
        {
            ToggleBusyIndicator(true);

            Task.Run(async () => {
                // Animation
                await Task.Delay(3000).ConfigureAwait(false);

                try
                {
                    await Globals.CloudManager.InvokeAsync(x => x.JoinPersonalCloud(invite, deviceName)).ConfigureAwait(false);
                }
                catch
                {
                    Dispatcher.Invoke(() => {
                        DialogText.Text = UILanding.ErrorCreatingCloud;
                        Dialog.IsOpen = true;

                        ToggleBusyIndicator(false);
                    });
                }
            });
        }

        private void HideWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
