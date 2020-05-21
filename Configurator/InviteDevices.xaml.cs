using System;
using System.Windows;
using System.Windows.Controls;

namespace NSPersonalCloud.WindowsConfigurator
{
    public partial class InviteDevices : UserControl
    {
        public event EventHandler DismissDialog;

        public InviteDevices()
        {
            InitializeComponent();
        }

        public void Setup(string code)
        {
            InviteLabel.Text = code;
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            DismissDialog?.Invoke(this, EventArgs.Empty);
        }
    }
}
