using System;
using System.Linq;

using NSPersonalCloud.Common;
using NSPersonalCloud.Common.Models;
using NSPersonalCloud.WindowsContract;
using NSPersonalCloud.WindowsService.Data;

namespace NSPersonalCloud.WindowsService.IPC
{
    public class StorageService : IPersistentStorage
    {
        public Guid[] GetAllPersonalCloud()
        {
            return Globals.Database.Table<CloudModel>().Select(x => x.Id).ToArray();
        }

        public string GetDeviceName(Guid? cloudId = null)
        {
            if (cloudId == null) return Globals.Database.LoadSetting(UserSettings.DeviceName);
            else throw new NotSupportedException();
        }

        public string GetFileSharingRoot()
        {
            return Globals.Database.LoadSetting(UserSettings.SharingRoot);
        }

        public string GetMountPointForPersonalCloud(Guid id)
        {
            return Globals.Database.GetMountPoint(id);
        }

        public string GetPersonalCloudName(Guid id)
        {
            return Globals.Database.Find<CloudModel>(id)?.Name;
        }

        public bool IsExplorerIntegrationEnabled()
        {
            return Globals.Database.CheckSetting(WindowsUserSettings.EnableVolumeMounting, "1");
        }

        public bool IsFileSharingEnabled(Guid? cloudId = null)
        {
            if (cloudId == null) return Globals.Database.CheckSetting(UserSettings.EnableSharing, "1");
            else throw new NotSupportedException();
        }
    }
}
