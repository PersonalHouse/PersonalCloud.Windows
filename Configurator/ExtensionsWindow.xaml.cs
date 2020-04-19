using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using NSPersonalCloud.Apps.Album;

using Ookii.Dialogs.Wpf;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class ExtensionsWindow : Window
    {
        private ObservableCollection<string> LibraryPaths { get; }

        public ExtensionsWindow()
        {
            InitializeComponent();

            // Load.
            LibraryPaths = new ObservableCollection<string>();
            PhotosLibraryList.ItemsSource = LibraryPaths;
        }

        private void OnListItemSelected(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = PhotosLibraryList.SelectedIndex >= 0;
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            LibraryPaths.RemoveAt(PhotosLibraryList.SelectedIndex);
            
            // Notify applet of path changes.
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
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

                if (LibraryPaths.Contains(path)) return;
                LibraryPaths.Add(path);

                var configs = new List<AlbumConfig>(LibraryPaths.Count);
                foreach (var album in LibraryPaths) {
                    var name = Path.GetFileName(album.TrimEnd(Path.DirectorySeparatorChar));
                    var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Personal Cloud", "Thumbnails", name);
                    configs.Add(new AlbumConfig {
                        Name = name,
                        MediaFolder = album,
                        ThumbnailFolder = cache
                    });
                }
                Globals.CloudManager.InvokeAsync(x => x.ChangeAlbumSettings(Globals.PersonalCloud.Value, configs));
            }
        }
    }
}
