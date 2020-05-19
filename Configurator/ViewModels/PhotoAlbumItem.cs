using NSPersonalCloud.Interfaces.Apps;

namespace NSPersonalCloud.WindowsConfigurator.ViewModels
{
    public class PhotoAlbumItem : ISelectableItem
    {
        public bool IsSelected { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }
    }
}
