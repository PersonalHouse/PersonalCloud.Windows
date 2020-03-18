using System;
using System.Windows.Input;

namespace Unishare.Apps.WindowsConfigurator
{
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction?.Invoke();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc?.Invoke() != false;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
