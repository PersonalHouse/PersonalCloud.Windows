using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using NSPersonalCloud.WindowsConfigurator.Resources;

namespace NSPersonalCloud.WindowsConfigurator
{
    public partial class WelcomeWindow : Window
    {
        private readonly bool initialized;

        private RadioButton lastChoice;

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
            var deviceName = DeviceNameBox.Text;
            foreach (var c in deviceName)
            {
                if (Path.GetInvalidFileNameChars().Contains(c))
                {
                    Alert.MessageQueue.Enqueue(UISettings.AlertInvalidDeviceName);
                    return;
                }
            }

            if (CreateTab.IsChecked == true) CreatePersonalCloud(deviceName, CloudNameBox.Text);
            else if (JoinTab.IsChecked == true) JoinPersonalCloud(deviceName, CloudNameBox.Text);
        }

        private void CreatePersonalCloud(string deviceName, string cloudName)
        {
            ProgressText.Text = UILanding.AlertCreating;
            Dialog.IsOpen = true;

            Task.Run(async () => {
                // Animation
                var t = Task.Delay(3000).ConfigureAwait(false);

                try
                {
                    await Globals.CloudManager.InvokeAsync(x => x.CreatePersonalCloud(cloudName, deviceName)).ConfigureAwait(false);
                    await t;
                }
                catch
                {
                    Dispatcher.Invoke(() => {
                        Alert.MessageQueue.Enqueue(UILanding.AlertCannotCreate);
                    });
                }finally
                {
                    Dispatcher.Invoke(() => {
                        Dialog.IsOpen = false;
                    });
                }
            });
        }

        private void JoinPersonalCloud(string deviceName, string invite)
        {
            ProgressText.Text = UILanding.AlertJoining;
            Dialog.IsOpen = true;

            Task.Run(async () => {
                // Animation
                var t = Task.Delay(3000).ConfigureAwait(false);

                try
                {
                    await Globals.CloudManager.InvokeAsync(x => x.JoinPersonalCloud(invite, deviceName)).ConfigureAwait(false);
                    await t;
                }
                catch
                {
                    await Globals.CloudManager.InvokeAsync(x => x.Refresh()).ConfigureAwait(false);
                    Dispatcher.Invoke(() => {
                        Alert.MessageQueue.Enqueue(UILanding.AlertCannotEnroll);
                        Dialog.IsOpen = false;
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() => {
                        Alert.MessageQueue.Enqueue(UILanding.AlertCannotEnroll);
                        Dialog.IsOpen = false;
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
