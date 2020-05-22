using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using NSPersonalCloud.Apps.Album;
using NSPersonalCloud.WindowsConfigurator.Resources;
using NSPersonalCloud.WindowsConfigurator.ViewModels;

using Ookii.Dialogs.Wpf;

namespace NSPersonalCloud.WindowsConfigurator
{
    public partial class MainWindow : Window
    {
        internal IReadOnlyList<char> DriveLetters { get; private set; }

        private bool initialized;

        private ObservableCollection<StorageConnectionItem> Connections { get; }
        private ObservableCollection<PhotoAlbumItem> Albums { get; }

        public MainWindow()
        {
            InitializeComponent();

            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh") FontFamily = new FontFamily("Microsoft YaHei UI");
            SharingPathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var letters = new List<char> { '—' };
            letters.AddRange(Enumerable.Range(68, 23).Select(x => (char) x));
            DriveLetters = letters.AsReadOnly();

            MountedCloudDrive.ItemsSource = DriveLetters;
            MountedCloudDrive.SelectedIndex = 0;

            Connections = new ObservableCollection<StorageConnectionItem>();
            ConnectionList.ItemsSource = Connections;
            Albums = new ObservableCollection<PhotoAlbumItem>();
            AlbumList.ItemsSource = Albums;

            Task.Run(async () => {
                try
                {
                    var sharingEnabled = await Globals.CloudManager.InvokeAsync(x => x.IsFileSharingEnabled(null))
                                                              .ConfigureAwait(false);
                    var sharingPath = await Globals.CloudManager.InvokeAsync(x => x.GetFileSharingRoot())
                                                           .ConfigureAwait(false);

                    var deviceName = await Globals.CloudManager.InvokeAsync(x => x.GetDeviceName(null))
                                                          .ConfigureAwait(false);
                    var cloudName = await Globals.CloudManager.InvokeAsync(x => x.GetPersonalCloudName(Globals.PersonalCloud.Value))
                                                         .ConfigureAwait(false);

                    var mounted = await Globals.CloudManager.InvokeAsync(x => x.IsExplorerIntegrationEnabled())
                                                       .ConfigureAwait(false);
                    var mountPoint = await Globals.CloudManager.InvokeAsync(x => x.GetMountPointForPersonalCloud(Globals.PersonalCloud.Value))
                                                          .ConfigureAwait(false);

                    var connections = await Globals.CloudManager.InvokeAsync(x => x.GetConnectedServices(Globals.PersonalCloud.Value))
                                                                .ConfigureAwait(false);
                    var albums = await Globals.CloudManager.InvokeAsync(x => x.GetAlbumSettings(Globals.PersonalCloud.Value))
                                                           .ConfigureAwait(false);

                    await Dispatcher.InvokeAsync(() => {
                        SharingSwitch.IsChecked = sharingEnabled;
                        if (!string.IsNullOrEmpty(sharingPath)) SharingPathBox.Text = sharingPath;

                        DeviceNameBox.Text = deviceName;
                        CloudNameBox.Text = cloudName;
                        MountedCloudName.Text = cloudName;
                        MountSwitch.IsChecked = mounted;

                        if (mountPoint?.Length != 3) MountedCloudDrive.SelectedIndex = 0;
                        else
                        {
                            var letterIndex = mountPoint[0] - 68 + 1;
                            MountedCloudDrive.SelectedIndex = letterIndex;
                        }

                        foreach (var item in connections)
                        {
                            var model = new StorageConnectionItem {
                                IsSelected = false,
                                Name = item,
                                Type = "Cloud Storage"
                            };
                            Connections.Add(model);
                        }
                        foreach (var item in albums)
                        {
                            var model = new PhotoAlbumItem {
                                IsSelected = false,
                                Name = item.Name,
                                Path = item.MediaFolder
                            };
                            Albums.Add(model);
                        }
                    });
                }
                catch
                {
                    // Todo
                }

                initialized = true;
            });
        }

        private void OnSharingChecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            var path = SharingPathBox.Text;
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(path, null))
                                          .ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                });
            });
        }

        private void OnSharingUnchecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(null, null))
                                          .ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                });
            });
        }

        private void OnChangePathClicked(object sender, RoutedEventArgs e)
        {
            var browseDialog = new VistaFolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.Personal,
                ShowNewFolderButton = true
            };
            var selected = browseDialog.ShowDialog(this);
            if (selected == true)
            {
                var path = browseDialog.SelectedPath;
                if (!Directory.Exists(path)) return;
                SharingPathBox.Text = path;

                Task.Run(async () => {
                    await Globals.CloudManager.InvokeAsync(x => x.ChangeSharingRoot(path, null))
                                              .ConfigureAwait(false);
                    Dispatcher.Invoke(() => {
                        Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                    });
                });
            }
        }

        private void OnChangeDeviceNameClicked(object sender, RoutedEventArgs e)
        {
            var name = DeviceNameBox.Text;
            foreach (var c in name)
            {
                if (Path.GetInvalidFileNameChars().Contains(c))
                {
                    Alert.MessageQueue.Enqueue(UISettings.AlertInvalidDeviceName);
                    return;
                }
            }

            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.ChangeDeviceName(name, null))
                                          .ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                });
            });
        }

        private void OnInviteClicked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            Task.Run(async () => {
                var code = await Globals.CloudManager.InvokeAsync(x => x.StartBroadcastingInvitation(null))
                                                     .ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    var session = new InviteDevices();
                    session.Setup(code);
                    session.DismissDialog += OnStopVerification;
                    Dialog.DialogContent = session;
                    Dialog.IsOpen = true;
                });
            });
        }

        private void OnStopVerification(object sender, EventArgs e)
        {
            Dialog.IsOpen = false;
            Dialog.DialogContent = null;
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.StopBroadcastingInvitation(null))
                                          .ConfigureAwait(false);
            });
        }

        private void OnMountChecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            if (MountedCloudDrive.SelectedIndex == 0) return;
            var mountPoint = DriveLetters[MountedCloudDrive.SelectedIndex] + @":\";

            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.MountNetworkDrive(Globals.PersonalCloud.Value, mountPoint))
                                     .ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                });
            });
        }

        private void OnMountUnchecked(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;

            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.UnmountAllDrives()).ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                });
            });
        }

        private void OnMountPointChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialized) return;

            if (MountSwitch.IsChecked != true) return;

            var mountPoint = MountedCloudDrive.SelectedIndex == 0 ? null : (DriveLetters[MountedCloudDrive.SelectedIndex] + @":\");
            if (mountPoint != null)
            {
                Task.Run(async () => {
                    await Globals.CloudManager.InvokeAsync(x => x.MountNetworkDrive(Globals.PersonalCloud.Value, mountPoint))
                                         .ConfigureAwait(false);
                    Dispatcher.Invoke(() => {
                        Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                    });
                });
            }
            else
            {
                Task.Run(async () => {
                    await Globals.CloudManager.InvokeAsync(x => x.UnmountNetworkDrive(Globals.PersonalCloud.Value))
                                         .ConfigureAwait(false);
                    Dispatcher.Invoke(() => {
                        Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                    });
                });
            }
        }

        private void OnLeaveClicked(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.LeavePersonalCloud(Globals.PersonalCloud.Value))
                                          .ConfigureAwait(false);
            });
        }

        private void OnQuitClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnConnectionSelected(object sender, SelectionChangedEventArgs e)
        {
            DeleteConnectionButton.IsEnabled = ConnectionList.SelectedIndex >= 0;
        }

        private void OnAlbumSelected(object sender, SelectionChangedEventArgs e)
        {
            DeleteAppButton.IsEnabled = AlbumList.SelectedIndex >= 0;
        }

        private void OnAddToConnections(object sender, RoutedEventArgs e)
        {
            NewConnectionButton.ContextMenu = (ContextMenu) Resources["StorageProviders"];
            NewConnectionButton.ContextMenu.IsOpen = true;
        }

        private void OnAddToConnectionsCollapsed(object sender, ContextMenuEventArgs e)
        {
            NewConnectionButton.ContextMenu = null;
        }

        private void OnDeleteFromConnections(object sender, RoutedEventArgs e)
        {
            if (ConnectionList.SelectedIndex == -1) return;

            var name = Connections[ConnectionList.SelectedIndex];
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.RemoveConnection(Globals.PersonalCloud.Value, name.Name))
                                          .ConfigureAwait(false);
            });
            Connections.RemoveAt(ConnectionList.SelectedIndex);
        }

        private void OnAddToAlbums(object sender, RoutedEventArgs e)
        {
            var browseDialog = new VistaFolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.Personal,
                ShowNewFolderButton = false
            };
            var selected = browseDialog.ShowDialog(this);
            if (selected == true)
            {
                var path = Path.GetFullPath(browseDialog.SelectedPath);
                if (!Directory.Exists(path)) return;
                if (Albums.Any(x => x.Path == path)) return;

                var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
                var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                         "Personal Cloud", "Thumbnails",
                                         DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                Directory.CreateDirectory(cache);
                Albums.Add(new PhotoAlbumItem {
                    IsSelected = false,
                    Name = name,
                    Path = path,
                });

                Task.Run(async () => {
                    var settings = await Globals.CloudManager.InvokeAsync(x => x.GetAlbumSettings(Globals.PersonalCloud.Value))
                                                             .ConfigureAwait(false);
                    settings.Add(new AlbumConfig {
                        Name = name,
                        MediaFolder = path,
                        ThumbnailFolder = cache
                    });
                    await Globals.CloudManager.InvokeAsync(x => x.ChangeAlbumSettings(Globals.PersonalCloud.Value, settings))
                                              .ConfigureAwait(false);
                });
            }
        }

        private void OnDeleteFromAlbums(object sender, RoutedEventArgs e)
        {
            if (AlbumList.SelectedIndex == -1) return;

            var item = Albums[AlbumList.SelectedIndex];
            Albums.RemoveAt(AlbumList.SelectedIndex);

            Task.Run(async () => {
                var settings = await Globals.CloudManager.InvokeAsync(x => x.GetAlbumSettings(Globals.PersonalCloud.Value))
                                                         .ConfigureAwait(false);
                var config = settings.FirstOrDefault(x => x.Name == item.Name && x.MediaFolder == item.Path);
                if (config == null) return;
                settings.Remove(config);
                await Globals.CloudManager.InvokeAsync(x => x.ChangeAlbumSettings(Globals.PersonalCloud.Value, settings))
                                          .ConfigureAwait(false);
            });
        }

        private void OnConnectToAlibaba(object sender, RoutedEventArgs e)
        {
            var form = new NewConnectionForm();
            form.Setup(StorageProviderInstance.TypeAliYun);
            form.DismissDialog += RefreshConnections;
            Dialog.DialogContent = form;
            Dialog.IsOpen = true;
        }

        private void OnConnectToAzure(object sender, RoutedEventArgs e)
        {
            var form = new NewConnectionForm();
            form.Setup(StorageProviderInstance.TypeAzure);
            form.DismissDialog += RefreshConnections;
            Dialog.DialogContent = form;
            Dialog.IsOpen = true;
        }

        private void RefreshConnections(object sender, EventArgs e)
        {
            Dialog.IsOpen = false;
            Dialog.DialogContent = null;
            Task.Run(async () => {
                var connections = await Globals.CloudManager.InvokeAsync(x => x.GetConnectedServices(Globals.PersonalCloud.Value))
                                                            .ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    Connections.Clear();
                    foreach (var item in connections)
                    {
                        var model = new StorageConnectionItem {
                            IsSelected = false,
                            Name = item,
                            Type = "Cloud Storage"
                        };
                        Connections.Add(model);
                    }
                });
            });
        }

        private void StopService(object sender, RoutedEventArgs e)
        {
            ((App) Application.Current).StopService();
        }

        private void RefreshDevices(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.Refresh())
                                          .ConfigureAwait(false);
                Dispatcher.Invoke(() => {
                    Alert.MessageQueue.Enqueue(UISettings.SettingsSaved);
                });
            });
        }
    }
}
