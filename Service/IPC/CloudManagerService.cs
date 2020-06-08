using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DokanFS;

using DokanNet;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Nito.AsyncEx;

using NSPersonalCloud.Apps.Album;
using NSPersonalCloud.Common;
using NSPersonalCloud.Common.Models;
using NSPersonalCloud.FileSharing.Aliyun;
using NSPersonalCloud.Interfaces.Errors;
using NSPersonalCloud.WindowsContract;
using NSPersonalCloud.WindowsService.Data;

namespace NSPersonalCloud.WindowsService.IPC
{
    public class CloudManagerService : ICloudManager
    {
        #region File System Controller

        public void MountNetworkDrive(Guid cloudId, string mountPoint)
        {
            if (mountPoint?.Length != 3 || !mountPoint.EndsWith(@":\", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Mount Point is invalid.");

            if (Globals.Volumes.TryGetValue(cloudId, out var thread) && thread != null) UnmountNetworkDrive(cloudId);

            var cloud = Globals.CloudService.PersonalClouds.First(x => new Guid(x.Id) == cloudId);

            var dokanThread = new AsyncContextThread();
            dokanThread.Factory.Run(() => {
                try
                {
                    var disk = new DokanFileSystemAdapter(new PersonalCloudRootFileSystem(cloud, Globals.Loggers.CreateLogger<PersonalCloudRootFileSystem>()));
                    disk.Mount(mountPoint, DokanOptions.EnableNotificationAPI, 5, new DokanyLogger());
                }
                catch (Exception exception)
                {
                    Globals.Loggers.CreateLogger<CloudManagerService>().LogError(exception, "Mount failed.");
                    Globals.Database.SaveSetting(WindowsUserSettings.EnableVolumeMounting, "0");

                    Task.Run(async () => {
                        await Globals.NotificationCenter.InvokeAsync(x => x.OnVolumeIOError(mountPoint, exception))
                                                        .ConfigureAwait(false);
                        await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        Globals.Volumes.TryRemove(cloudId, out _);
                        dokanThread.Dispose();
                    });
                }
            });

            Globals.Database.SaveSetting(WindowsUserSettings.EnableVolumeMounting, "1");
            Globals.Database.SaveMountPoint(cloudId, mountPoint);
            Globals.Volumes[cloudId] = dokanThread;
        }

        public void RemountAllDrives()
        {
            throw new NotSupportedException();
        }

        public void UnmountAllDrives()
        {
            foreach (var volume in Globals.Database.Table<DiskModel>().ToList())
            {
                var mountPoint = Globals.Database.GetMountPoint(volume.Id);
                try { Dokan.RemoveMountPoint(mountPoint); }
                catch { } // Ignored.
                Globals.Database.RemoveMountPoint(volume.Id);
            }

            Globals.Database.SaveSetting(WindowsUserSettings.EnableVolumeMounting, "0");
        }

        public void UnmountNetworkDrive(Guid cloudId)
        {
            var mountPoint = Globals.Database.GetMountPoint(cloudId);
            try { Dokan.RemoveMountPoint(mountPoint); }
            catch { } // Ignored.

            Globals.Database.RemoveMountPoint(cloudId);
            if (Globals.Volumes.TryRemove(cloudId, out var thread)) thread.Dispose();
        }

        #endregion File System Controller

        #region Cloud Manager

        public void ChangeDeviceName(string newName, Guid? cloudId)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Device Name cannot be empty", nameof(newName));

            if (cloudId == null) Globals.CloudService.PersonalClouds[0].NodeDisplayName = newName;
            else Globals.CloudService.PersonalClouds.First(x => new Guid(x.Id) == cloudId).NodeDisplayName = newName;

            Globals.Database.SaveSetting(UserSettings.DeviceName, newName);
            Globals.CloudService.NetworkRefeshNodes();
        }

        public void ChangeSharingRoot(string absolutePath, Guid? cloudId)
        {
            if (absolutePath == null)
            {
                Globals.CloudFileSystem.RootPath = null;
                Globals.Database.SaveSetting(UserSettings.EnableSharing, "0");
                return;
            }

            if (!Directory.Exists(absolutePath)) throw new InvalidOperationException("Path does not exist.");

            Globals.Database.SaveSetting(UserSettings.SharingRoot, absolutePath);
            Globals.Database.SaveSetting(UserSettings.EnableSharing, "1");
            if (cloudId == null) Globals.CloudFileSystem.RootPath = absolutePath;
            else throw new NotSupportedException();
        }

        public Guid? CreatePersonalCloud(string cloudName, string deviceName)
        {
            if (string.IsNullOrEmpty(cloudName)) throw new ArgumentException("Cloud Name cannot be empty.", nameof(cloudName));

            if (string.IsNullOrEmpty(deviceName))
            {
                deviceName = Environment.MachineName;
                if (string.IsNullOrEmpty(deviceName)) throw new ArgumentException("Device Name cannot be empty.", nameof(deviceName));
            }

            var cloud = Globals.CloudService.CreatePersonalCloud(cloudName, deviceName).Result;
            Globals.Database.SaveSetting(UserSettings.DeviceName, deviceName);

            var cloudId = new Guid(cloud.Id);

            #region Auto Mount

            // Todo: Check for Dokany readiness?

            try
            {
                var connectedDrives = DriveInfo.GetDrives().Select(x => x.Name[0]);
                var availableLetter = Algorithms.LowestMissingLetter(connectedDrives);
                if (availableLetter != char.MinValue) MountNetworkDrive(cloudId, availableLetter + @":\");
            }
            catch
            {
                // Ignored.
            }

            #endregion Auto Mount

            Task.Run(async () => {
                await Globals.NotificationCenter.InvokeAsync(x => x.OnPersonalCloudAdded())
                                                .ConfigureAwait(false);
            });
            return cloudId;
        }

        public Guid? JoinPersonalCloud(string invite, string deviceName)
        {
            if (string.IsNullOrEmpty(invite) || !int.TryParse(invite, out var code)) throw new ArgumentException("Invitation is invalid.", nameof(invite));

            if (string.IsNullOrEmpty(deviceName))
            {
                deviceName = Environment.MachineName;
                if (string.IsNullOrEmpty(deviceName)) throw new ArgumentException("Device Name cannot be empty.", nameof(deviceName));
            }

            var cloud = Globals.CloudService.JoinPersonalCloud(code, deviceName).Result;
            Globals.Database.SaveSetting(UserSettings.DeviceName, deviceName);

            var cloudId = new Guid(cloud.Id);

            #region Auto Mount

            // Todo: Check for Dokany readiness?
            try
            {
                var connectedDrives = DriveInfo.GetDrives().Select(x => x.Name[0]);
                var availableLetter = Algorithms.LowestMissingLetter(connectedDrives);
                if (availableLetter != char.MinValue) MountNetworkDrive(cloudId, availableLetter + @":\");
            }
            catch
            {
                // Ignored.
            }

            #endregion Auto Mount

            Task.Run(async () => {
                await Globals.NotificationCenter.InvokeAsync(x => x.OnPersonalCloudAdded())
                                                .ConfigureAwait(false);
            });
            return cloudId;
        }

        public void LeavePersonalCloud(Guid id)
        {
            var cloud = Globals.CloudService.PersonalClouds.First(x => new Guid(x.Id) == id);
            Globals.CloudService.ExitFromCloud(cloud);

            Globals.Database.Delete<CloudModel>(id);
            Globals.Database.Delete<DiskModel>(id);

            Task.Run(async () => {
                await Globals.NotificationCenter.InvokeAsync(x => x.OnLeftPersonalCloud())
                                                .ConfigureAwait(false);
            });
        }

        public string StartBroadcastingInvitation(Guid? id)
        {
            if (id == null) return Globals.CloudService.SharePersonalCloud(Globals.CloudService.PersonalClouds[0]).Result;
            else return Globals.CloudService.SharePersonalCloud(Globals.CloudService.PersonalClouds.First(x => new Guid(x.Id) == id)).Result;
        }

        public void StopBroadcastingInvitation(Guid? id)
        {
            if (id == null) Globals.CloudService.StopSharePersonalCloud(Globals.CloudService.PersonalClouds[0]).Wait();
            else Globals.CloudService.StopSharePersonalCloud(Globals.CloudService.PersonalClouds.First(x => new Guid(x.Id) == id)).Wait();
        }

        public void ConnectToAlibabaCloud(Guid cloudId, string name, OssConfig config, StorageProviderVisibility visibility)
        {
            Globals.CloudService.AddStorageProvider(cloudId.ToString("N"), Guid.NewGuid(), name, config, visibility);
        }

        public void ConnectToAzure(Guid cloudId, string name, AzureBlobConfig config, StorageProviderVisibility visibility)
        {
            Globals.CloudService.AddStorageProvider(cloudId.ToString("N"), Guid.NewGuid(), name, config, visibility);
        }

        public string[] GetConnectedServices(Guid cloudId)
        {
            try
            {
                return Globals.CloudService.GetStorageProviderInstances(cloudId.ToString("N")).Select(x => x.ProviderInfo.Name).ToArray();
            }
            catch (NoSuchCloudException)
            {
                return Array.Empty<string>();
            }
        }

        public void ChangeAlbumSettings(Guid cloudId, List<AlbumConfig> settings)
        {
            var strcfig = JsonConvert.SerializeObject(settings);
            Globals.CloudService.SetAppMgrConfig("Album", cloudId.ToString("N"), strcfig).ConfigureAwait(false);
        }

        public List<AlbumConfig> GetAlbumSettings(Guid cloudId)
        {
            var json = Globals.CloudService.GetAppConfig(cloudId.ToString("N"), "Album");
            if (json != null)
            {
                return JsonConvert.DeserializeObject<List<Apps.Album.AlbumConfig>>(json);
            }
            return new List<Apps.Album.AlbumConfig>();
        }

        public void RemoveConnection(Guid cloudId, string name)
        {
            Globals.CloudService.RemoveStorageProvider(cloudId.ToString("N"), name);
        }

        public void Refresh()
        {
            try
            {
                Globals.CloudService.StartNetwork(true);
            }
            catch
            {
                // Ignored.
            }
        }

        #endregion Cloud Manager

        #region Storage

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

        #endregion Storage
    }
}
