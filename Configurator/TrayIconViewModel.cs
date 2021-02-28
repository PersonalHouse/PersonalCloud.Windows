using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NSPersonalCloud.WindowsConfigurator
{
#pragma warning disable CA1822

    public class TrayIconViewModel
    {
        public ICommand ShowWindowCommand => new DelegateCommand {
            CanExecuteFunc = () => Application.Current.MainWindow?.IsVisible != true,
            CommandAction = () => {
                if (Application.Current.MainWindow == null)
                {
                    Application.Current.MainWindow = new MainWindow();
                    Application.Current.MainWindow.Show();

                    Task.Run(async () => {
                        try
                        {
                            var cts = new CancellationTokenSource();
                            cts.CancelAfter(2 * 1000);
                            var ret = await Globals.CloudManager.InvokeAsync(x => x.Ping(666),cts.Token)
                                                 .ConfigureAwait(false);
                            if (ret == 666)
                            {
                                return;
                            }
                        }
                        catch 
                        {
                        }
                        Application.Current.Dispatcher.Invoke(() => {
                            UIHelpers.ShowLoadingMessage();
                            Application.Current.MainWindow.Hide();
                            Application.Current.MainWindow = null;
                        });
                    });
                }
                else Application.Current.MainWindow.Show();
            }
        };

        public ICommand HideWindowCommand => new DelegateCommand {
            CanExecuteFunc = () => Application.Current.MainWindow?.IsVisible == true,
            CommandAction = () => Application.Current.MainWindow.Hide()
        };

        public ICommand RestartServiceCommand => new DelegateCommand {
            CanExecuteFunc = () => true,
            CommandAction = () => ((App) Application.Current).RestartService()
        };

        public ICommand StopServiceCommand => new DelegateCommand {
            CanExecuteFunc = () => true,
            CommandAction = () => ((App) Application.Current).StopService()
        };

        public ICommand ExitApplicationCommand => new DelegateCommand {
            CommandAction = () => Application.Current.Shutdown()
        };
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public ICommand OpenWebSite => new DelegateCommand {
            CommandAction = () => OpenUrl("https://Personal.House")
        };



        [DllImport("msi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int MsiEnumRelatedProducts(string lpUpgradeCode, int dwReserved,
            int iProductIndex, StringBuilder lpProductBuf);

        [DllImport("msi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Int32 MsiGetProductInfo(string product, string property,
            [Out] StringBuilder valueBuf, ref Int32 cchValueBuf);

        public string PHVersion { get {

                string upgradeCode = "{B8B67678-128E-47D8-BE23-90132BCF1058}";
                StringBuilder sbProductCode = new StringBuilder(39); // A buffer to receive the product code GUID.
                                                                     // The first 38 characters are for the GUID, and the last character is for the terminating null character.

                for (int iProductIndex = 0; ; iProductIndex++)
                {
                    int iRes = MsiEnumRelatedProducts(upgradeCode, 0, iProductIndex, sbProductCode);
                    if (iRes != 0)
                    {
                        break;
                    }
                    int len = 512;
                    StringBuilder builder = new StringBuilder(len);
                    int ok = MsiGetProductInfo(sbProductCode.ToString(), "VersionString", builder, ref len);
                    if (ok == 0)
                    {
                        return $"Personal House {builder.ToString()}";
                    }
                }

                return "Personal House";

            } }
    }

#pragma warning restore CA1822
}
