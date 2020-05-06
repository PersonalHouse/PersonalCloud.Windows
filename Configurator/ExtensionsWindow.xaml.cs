using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using NSPersonalCloud.Apps.Album;

using Ookii.Dialogs.Wpf;

namespace Unishare.Apps.WindowsConfigurator
{
    public partial class ExtensionsWindow : Window
    {
        private ObservableCollection<AlbumConfig> LibraryPaths { get; }

        public ExtensionsWindow()
        {
            InitializeComponent();

            LibraryPaths = new ObservableCollection<AlbumConfig>();
            PhotosLibraryList.ItemsSource = LibraryPaths;

            Task.Run(async () => {
                var settings = await Globals.CloudManager.InvokeAsync(x => x.GetAlbumSettings(Globals.PersonalCloud.Value)).ConfigureAwait(false);
                foreach (var entry in settings)
                {
                    LibraryPaths.Add(entry);
                }
            });
        }

        private void OnListItemSelected(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = PhotosLibraryList.SelectedIndex >= 0;
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            LibraryPaths.RemoveAt(PhotosLibraryList.SelectedIndex);

            Task.Run(async () => {
                await Globals.CloudManager.InvokeAsync(x => x.ChangeAlbumSettings(Globals.PersonalCloud.Value, LibraryPaths.ToList())).ConfigureAwait(false);
            });
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
                if (LibraryPaths.Any(x => x.MediaFolder == path)) return;

                var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
                var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Personal Cloud", "Thumbnails", DateTime.Now.ToString("yyyyMMddHHmmss"));
                LibraryPaths.Add(new AlbumConfig {
                    Name = name,
                    MediaFolder = path,
                    ThumbnailFolder = cache
                });
                Globals.CloudManager.InvokeAsync(x => x.ChangeAlbumSettings(Globals.PersonalCloud.Value, LibraryPaths.ToList()));
            }
        }
    }
}
