using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using MaterialDesignThemes.Wpf;

using NSPersonalCloud.FileSharing.Aliyun;
using NSPersonalCloud.WindowsConfigurator.Resources;

namespace NSPersonalCloud.WindowsConfigurator
{
    public partial class NewConnectionForm : UserControl
    {
        public event EventHandler DismissDialog;

        private string storageProvider;
        private bool hasPendingTask;

        public NewConnectionForm()
        {
            InitializeComponent();
        }

        public void Setup(string provider)
        {
            storageProvider = provider;

            switch (provider)
            {
                case StorageProviderInstance.TypeAliYun:
                {
                    Header.Text = UIStorage.ProviderAliYun;
                    ConnectionNameLabel.Text = UIStorage.AliYunConnectionName;
                    ConnectionNameBox.Text = UIStorage.ProviderAliYun;
                    EndpointLabel.Text = UIStorage.AliYunEndpoint;
                    ContainerLabel.Text = UIStorage.AliYunBucket;
                    AccessIDLabel.Text = UIStorage.AliYunAccessID;
                    AccessSecretLabel.Text = UIStorage.AliYunAccessKey;
                    return;
                }

                case StorageProviderInstance.TypeAzure:
                {
                    Header.Text = UIStorage.ProviderAzure;
                    ConnectionNameLabel.Text = UIStorage.AliYunConnectionName;
                    ConnectionNameBox.Text = UIStorage.ProviderAzure;
                    EndpointLabel.Text = UIStorage.AzureEndpoint;
                    ContainerLabel.Text = UIStorage.AzureContainer;
                    AccessIDLabel.Text = UIStorage.AzureAccountName;
                    AccessSecretLabel.Text = UIStorage.AzureKey;
                    return;
                }
            }
        }

        private void SaveConnection(object sender, RoutedEventArgs e)
        {
            if (hasPendingTask) return;

            switch (storageProvider)
            {
                case StorageProviderInstance.TypeAliYun:
                {
                    VerifyAlibabaCredentials();
                    return;
                }

                case StorageProviderInstance.TypeAzure:
                {
                    VerifyAzureCredentials();
                    return;
                }
            }
        }

        private void ToggleBusyIndicator(bool isBusy)
        {
            hasPendingTask = isBusy;
            ButtonProgressAssist.SetIsIndicatorVisible(SaveButton, isBusy);
        }

        private void VerifyAlibabaCredentials()
        {
            ToggleBusyIndicator(true);

            var name = ConnectionNameBox.Text;
            var invalidCharHit = false;
            foreach (var character in VirtualFileSystem.InvalidCharacters)
            {
                if (name?.Contains(character) == true) invalidCharHit = true;
            }
            if (string.IsNullOrEmpty(name) || invalidCharHit)
            {
                Dispatcher.ShowAlert(UIStorage.ConnectionBadName, null);
                ToggleBusyIndicator(false);
                return;
            }

            var endpoint = EndpointBox.Text;
            if (string.IsNullOrEmpty(endpoint))
            {
                Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                ToggleBusyIndicator(false);
                return;
            }

            var bucket = ContainerBox.Text;
            if (string.IsNullOrEmpty(bucket))
            {
                Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                ToggleBusyIndicator(false);
                return;
            }

            var accessId = AccessIDBox.Text;
            if (string.IsNullOrEmpty(accessId))
            {
                Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                ToggleBusyIndicator(false);
                return;
            }

            var accessSecret = AccessSecretBox.Text;
            if (string.IsNullOrEmpty(accessSecret))
            {
                Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                ToggleBusyIndicator(false);
                return;
            }

            var config = new OssConfig {
                OssEndpoint = endpoint,
                BucketName = bucket,
                AccessKeyId = accessId,
                AccessKeySecret = accessSecret
            };

            var shareScope = ShareCredentialsBox.IsChecked == true ? StorageProviderVisibility.Public : StorageProviderVisibility.Private;

            Task.Run(async () => {
                if (config.Verify())
                {
                    try
                    {
                        await Globals.CloudManager.InvokeAsync(x => x.ConnectToAlibabaCloud(Globals.PersonalCloud.Value, name, config, shareScope)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await Task.Delay(3000).ConfigureAwait(false);
                        Dispatcher.Invoke(() => {
                            ToggleBusyIndicator(false);
                            DismissDialog?.Invoke(this, EventArgs.Empty);
                        });
                    }
                }
                else
                {
                    Dispatcher.Invoke(() => {
                        Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAliYun);
                        ToggleBusyIndicator(false);
                    });
                }
            });
        }

        private void VerifyAzureCredentials()
        {
            ToggleBusyIndicator(true);

            var name = ConnectionNameBox.Text;
            var invalidCharHit = false;
            foreach (var character in VirtualFileSystem.InvalidCharacters)
            {
                if (name?.Contains(character) == true) invalidCharHit = true;
            }
            if (string.IsNullOrEmpty(name) || invalidCharHit)
            {
                Dispatcher.ShowAlert(UIStorage.ConnectionBadName, null);
                ToggleBusyIndicator(false);
                return;
            }

            var endpoint = EndpointBox.Text;
            if (string.IsNullOrEmpty(endpoint))
            {
                Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                ToggleBusyIndicator(false);
                return;
            }

            var accountName = AccessIDBox.Text;
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

            var accessKey = AccessSecretBox.Text;
            if (string.IsNullOrEmpty(accessKey))
            {
                Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                ToggleBusyIndicator(false);
                return;
            }

            var container = ContainerBox.Text;
            if (string.IsNullOrEmpty(container))
            {
                Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                ToggleBusyIndicator(false);
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
                    Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                    ToggleBusyIndicator(false);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(accountName))
                {
                    Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                    ToggleBusyIndicator(false);
                    return;
                }
                else connection = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accessKey};EndpointSuffix={endpoint}";
            }

            var config = new AzureBlobConfig {
                ConnectionString = connection,
                BlobName = container
            };
            var shareScope = ShareCredentialsBox.IsChecked == true ? StorageProviderVisibility.Public : StorageProviderVisibility.Private;

            Task.Run(async () => {
                if (config.Verify())
                {

                    try
                    {
                        await Globals.CloudManager.InvokeAsync(x => x.ConnectToAzure(Globals.PersonalCloud.Value, name, config, shareScope)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await Task.Delay(3000).ConfigureAwait(false);
                        Dispatcher.Invoke(() => {
                            ToggleBusyIndicator(false);
                            DismissDialog?.Invoke(this, EventArgs.Empty);
                        });
                    }
                }
                else
                {
                    Dispatcher.Invoke(() => {
                        Dispatcher.ShowAlert(UIStorage.ErrorAuthenticating, UIStorage.ErrorAuthenticatingAzure);
                        ToggleBusyIndicator(false);
                    });
                }
            });
        }

        private void Discard(object sender, RoutedEventArgs e)
        {
            if (hasPendingTask) return;

            DismissDialog?.Invoke(this, EventArgs.Empty);
        }
    }
}
