using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using NSPersonalCloud;
using NSPersonalCloud.FileSharing.Aliyun;

using Unishare.Apps.WindowsConfigurator.Resources;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class ConnectToStorageWindow : Window
    {
        private bool initialized;

        public ConnectToStorageWindow()
        {
            InitializeComponent();
            initialized = true;
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            switch (ProviderName.SelectedIndex)
            {
                case 0: // Alibaba Cloud
                {
                    VerifyAlibabaCredentials();
                    return;
                }

                case 1: // Azure Blob Storage
                {
                    VerifyAzureCredentials();
                    return;
                }
            }
        }

        private void OnSwitchingProvider(object sender, SelectionChangedEventArgs e)
        {
            if (!initialized) return;

            switch (ProviderName.SelectedIndex)
            {
                case 0: // Alibaba Cloud
                {
                    AzureSection.Visibility = Visibility.Collapsed;
                    AlibabaSection.Visibility = Visibility.Visible;
                    return;
                }
                case 1: // Azure Blob Storage
                {
                    AlibabaSection.Visibility = Visibility.Collapsed;
                    AzureSection.Visibility = Visibility.Visible;
                    return;
                }
            }
        }

        private void VerifyAlibabaCredentials()
        {
            var name = AlibabaConnectionNameBox.Text;
            var invalidCharHit = false;
            foreach (var character in VirtualFileSystem.InvalidCharacters)
            {
                if (name?.Contains(character) == true) invalidCharHit = true;
            }
            if (string.IsNullOrEmpty(name) || invalidCharHit)
            {
                this.ShowAlert(UIStorage.ConnectionBadName, null);
                return;
            }

            var endpoint = AlibabaEndpointBox.Text;
            if (string.IsNullOrEmpty(endpoint))
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                return;
            }

            var bucket = AlibabaBucketBox.Text;
            if (string.IsNullOrEmpty(bucket))
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                return;
            }

            var accessId = AlibabaAccountBox.Text;
            if (string.IsNullOrEmpty(accessId))
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                return;
            }

            var accessSecret = AlibabaSecretBox.Text;
            if (string.IsNullOrEmpty(accessSecret))
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                return;
            }

            var config = new OssConfig {
                OssEndpoint = endpoint,
                BucketName = bucket,
                AccessKeyId = accessId,
                AccessKeySecret = accessSecret
            };

            if (config.Verify())
            {
                Task.Run(async () => {
                    try
                    {
                        await Globals.CloudManager.InvokeAsync(x => x.ConnectToAlibabaCloud(Globals.PersonalCloud.Value, name, config)).ConfigureAwait(false);
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

        private void VerifyAzureCredentials()
        {
            var name = AzureConnectionNameBox.Text;
            var invalidCharHit = false;
            foreach (var character in VirtualFileSystem.InvalidCharacters)
            {
                if (name?.Contains(character) == true) invalidCharHit = true;
            }
            if (string.IsNullOrEmpty(name) || invalidCharHit)
            {
                this.ShowAlert(UIStorage.ConnectionBadName, null);
                return;
            }

            var endpoint = AzureEndpointBox.Text;
            if (string.IsNullOrEmpty(endpoint))
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                return;
            }

            var accountName = AzureAccountBox.Text;
            /*
            if (string.IsNullOrEmpty(accountName) &&
                (endpoint == "core.windows.net" || endpoint == "blob.core.windows.net" ||
                endpoint == "core.chinacloudapi.cn" || endpoint == "blob.core.chinacloudapi.cn"))
            {
                this.ShowAlert(this.Localize("Online.BadCredential"), this.Localize("Azure.BadAccountName"), action => {
                    AccountName.BecomeFirstResponder();
                });
                return;
            }
            */

            var accessKey = AzureKeyBox.Text;
            if (string.IsNullOrEmpty(accessKey))
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                return;
            }

            var container = AzureContainerBox.Text;
            if (string.IsNullOrEmpty(container))
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                return;
            }

            string connection;
            if (endpoint.Contains(accountName, StringComparison.Ordinal)) accountName = null;
            if (endpoint.StartsWith("http://", StringComparison.Ordinal)) endpoint.Replace("http://", "https://");
            if (endpoint.StartsWith("https://", StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(accountName))
                {
                    if (string.IsNullOrEmpty(accessKey)) connection = endpoint;
                    else connection = $"BlobEndpoint={endpoint};SharedAccessSignature={accessKey}";
                }
                else
                {
                    this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(accountName))
                {
                    this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                    return;
                }
                else connection = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accessKey};EndpointSuffix={endpoint}";
            }

            var config = new AzureBlobConfig {
                ConnectionString = connection,
                BlobName = container
            };

            if (config.Verify())
            {
                Task.Run(async () => {
                    try
                    {
                        await Globals.CloudManager.InvokeAsync(x => x.ConnectToAzure(Globals.PersonalCloud.Value, name, config)).ConfigureAwait(false);
                    }
                    finally
                    {
                        Dispatcher.Invoke(Close);
                    }
                });
            }
            else
            {
                this.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
            }
        }
    }
}
