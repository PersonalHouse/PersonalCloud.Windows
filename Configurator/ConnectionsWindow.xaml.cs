using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class ConnectionsWindow : Window
    {
        private ObservableCollection<string> ConnectedServices { get; }

        public ConnectionsWindow()
        {
            InitializeComponent();
            
            ConnectedServices = new ObservableCollection<string>();
            OnlineConnectionsList.ItemsSource = ConnectedServices;

            Task.Run(async () => {
                var connections = await Globals.CloudManager.InvokeAsync(x => x.GetConnectedServices(Globals.PersonalCloud.Value)).ConfigureAwait(false);
                foreach (var connection in connections) ConnectedServices.Add(connection);
            });
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            var child = new ConnectToStorageWindow();
            child.ShowDialog();
            Task.Run(async () => {
                var connections = await Globals.CloudManager.InvokeAsync(x => x.GetConnectedServices(Globals.PersonalCloud.Value)).ConfigureAwait(false);
                ConnectedServices.Clear();
                foreach (var connection in connections) ConnectedServices.Add(connection);
            });
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (OnlineConnectionsList.SelectedIndex == -1) return;

            var name = ConnectedServices[OnlineConnectionsList.SelectedIndex];
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.RemoveConnection(Globals.PersonalCloud.Value, name)).ConfigureAwait(false);
            });
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = OnlineConnectionsList.SelectedIndex >= 0;
        }
    }
}
