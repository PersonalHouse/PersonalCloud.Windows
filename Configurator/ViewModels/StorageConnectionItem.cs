namespace NSPersonalCloud.WindowsConfigurator.ViewModels
{
    public class StorageConnectionItem : ISelectableItem
    {
        public bool IsSelected { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }
    }
}
