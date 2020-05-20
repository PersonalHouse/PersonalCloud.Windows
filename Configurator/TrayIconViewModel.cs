using System.Windows;
using System.Windows.Input;

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
                    if (Globals.IsServiceRunning)
                    {
                        Application.Current.MainWindow = new MainWindow();
                        Application.Current.MainWindow.Show();
                    }
                    else UIHelpers.ShowLoadingMessage();
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
