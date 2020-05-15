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
                if (Application.Current.MainWindow == null) UIHelpers.ShowLoadingMessage();
                else Application.Current.MainWindow.Show();
            }
        };

        public ICommand HideWindowCommand => new DelegateCommand {
            CanExecuteFunc = () => Application.Current.MainWindow?.IsVisible == true,
            CommandAction = () => Application.Current.MainWindow.Hide()
        };

        public ICommand ExitApplicationCommand => new DelegateCommand {
            CommandAction = () => Application.Current.Shutdown()
        };
    }

#pragma warning restore CA1822
}
