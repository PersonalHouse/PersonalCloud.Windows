using System.Windows;

using NSPersonalCloud.FileSharing.Aliyun;

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
                try
                {
                    Globals.CloudManager.InvokeAsync(x => x.ConnectToAlibabaCloud(ConnectionNameBox.Text, config));
                    Close();
                }
                catch
                {
                    this.ShowAlert("IPC Error", "An error occurred communicating with target Service.");
                }
            }
            else
            {
                this.ShowAlert("Unable to Verify Credentials", "Alibaba Cloud does not accept these credentials.");
            }
        }
    }
}
