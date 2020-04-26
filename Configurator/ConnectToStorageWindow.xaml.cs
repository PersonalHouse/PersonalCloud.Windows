using System.Threading.Tasks;
using System.Windows;

using NSPersonalCloud.FileSharing.Aliyun;

using Unishare.Apps.WindowsConfigurator.Resources;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class ConnectToStorageWindow : Window
    {
        public ConnectToStorageWindow()
        {
            InitializeComponent();
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            var config = new OssConfig {
                OssEndpoint = EndpointBox.Text,
                AccessKeyId = AccountBox.Text,
                AccessKeySecret = SecretBox.Text,
                BucketName = BucketBox.Text
            };

            if (config.Verify())
            {
                Task.Run(async () => {
                    try
                    {
                        await Globals.CloudManager.InvokeAsync(x => x.ConnectToAlibabaCloud(Globals.PersonalCloud.Value, ConnectionNameBox.Text, config)).ConfigureAwait(false);
                    }
                    finally
                    {
                        Dispatcher.Invoke(Close);
                    }
                });
            }
            else
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
            }
        }
    }
}
