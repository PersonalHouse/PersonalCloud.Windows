using System;
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
    }

#pragma warning restore CA1822
}
