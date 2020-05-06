using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

using DokanFS;

using DokanNet;
using DokanNet.Logging;

using JKang.IpcServiceFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nito.AsyncEx;

using NSPersonalCloud;
using NSPersonalCloud.Interfaces.Errors;

using SQLite;

using Topshelf;

using Unishare.Apps.Common;
using Unishare.Apps.Common.Models;
using Unishare.Apps.WindowsContract;
using Unishare.Apps.WindowsService.Data;
using Unishare.Apps.WindowsService.IPC;

namespace Unishare.Apps.WindowsService
{
    internal class PersonalCloudWindowsService : ServiceControl
    {
        private HostControl HostControl { get; set; }
        private CancellationTokenSource Runners { get; set; }

        private static void Initialize()
        {
            SQLitePCL.Batteries_V2.Init();

            Directory.CreateDirectory(Globals.ConfigurationPath);
            var databasePath = Path.Combine(Globals.ConfigurationPath, "Preferences.sqlite3");
            Globals.Database = new SQLiteConnection(databasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
            Globals.Database.CreateTable<KeyValueModel>();
            Globals.Database.CreateTable<CloudModel>();
            Globals.Database.CreateTable<DiskModel>();
            Globals.Database.CreateTable<AlibabaOSS>();
            Globals.Database.CreateTable<AzureBlob>();

            Globals.CloudFileSystem = new VirtualFileSystem(null);
            Globals.CloudConfig = new WindowsDataStorage();

            Globals.Volumes = new ConcurrentDictionary<Guid, AsyncContextThread>();
        }

        public bool Start(HostControl hostControl)
        {
            HostControl = hostControl;

            Initialize();

            Runners = new CancellationTokenSource();

            var ipcServiceCollection = new ServiceCollection()
                .AddSingleton<CloudManagerService>()
                .AddSingleton<StorageService>()
                .AddIpc(builder => {
                    builder.AddNamedPipe()
                           .AddService<ICloudManager>(services => services.GetRequiredService<CloudManagerService>())
                           .AddService<IFileSystemController>(services => services.GetRequiredService<CloudManagerService>())
                           .AddService<IPersistentStorage>(services => services.GetRequiredService<StorageService>());
                });
            _ = new IpcServiceHostBuilder(ipcServiceCollection.BuildServiceProvider())
                .AddNamedPipeEndpoint<ICloudManager>("Cloud Manager", Pipes.CloudAdmin, true)
                .AddNamedPipeEndpoint<IFileSystemController>("Dokan Controller", Pipes.DiskMounter, true)
                .AddNamedPipeEndpoint<IPersistentStorage>("Storage", Pipes.UserSettings, true)
                .Build().RunAsync(Runners.Token);

            Globals.PopupPresenter = new IpcServiceClientBuilder<IPopupPresenter>().UseNamedPipe(Pipes.Messenger).Build();
            Globals.NotificationCenter = new IpcServiceClientBuilder<ICloudEventHandler>().UseNamedPipe(Pipes.NotificationCenter).Build();

            #region Restore last-known state of File Sharing

            if (Globals.Database.CheckSetting(UserSettings.EnableSharing, "1"))
            {
                var sharedPath = Globals.Database.LoadSetting(UserSettings.SharingRoot);
                if (string.IsNullOrEmpty(sharedPath) || !Directory.Exists(sharedPath))
                {
                    Globals.Database.Delete<KeyValueModel>(UserSettings.SharingRoot);
                    Globals.Database.SaveSetting(UserSettings.EnableSharing, "0");
                    sharedPath = null;
                }

                Globals.CloudFileSystem.RootPath = sharedPath;
            }

            #endregion Restore last-known state of File Sharing


            var resourcesPath = Path.Combine(Globals.ConfigurationPath, "Static");
            Directory.CreateDirectory(resourcesPath);
            Globals.CloudService = new PCLocalService(Globals.CloudConfig, new LoggerFactory(), Globals.CloudFileSystem, resourcesPath);
            Globals.CloudService.OnError += (o, e) => {
                if (e.ErrorCode == ErrorCode.NeedUpdate)
                    _ = Globals.PopupPresenter.InvokeAsync(x => x.ShowAlert("个人云版本过低", "您必须升级个人云才能访问其它设备。"));
            };
            Globals.CloudService.StartService();
            _ = Globals.NotificationCenter.InvokeAsync(x => x.OnServiceStarted());

            #region Restore last-known state of Network Drives

            if (Globals.Database.CheckSetting(WindowsUserSettings.EnableVolumeMounting, "1"))
            {

                foreach (var cloud in Globals.CloudService.PersonalClouds)
                {
                    var cloudId = new Guid(cloud.Id);
                    var mountPoint = Globals.Database.GetMountPoint(cloudId);

                    var dokanThread = new AsyncContextThread();
                    dokanThread.Factory.Run(() => {
                        try
                        {
                            var disk = new DokanFileSystemAdapter(new PersonalCloudRootFileSystem(cloud));
                            disk.Mount(mountPoint, DokanOptions.EnableNotificationAPI, 5, new NullLogger());
                        }
                        catch (Exception exception)
                        {
                            _ = Globals.NotificationCenter.InvokeAsync(x => x.OnVolumeIOError(mountPoint, exception));
                        }
                    });

                    Globals.Volumes[cloudId] = dokanThread;
                }
            }

            #endregion

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Runners.Cancel();
            _ = Globals.NotificationCenter.InvokeAsync(x => x.OnServiceStopped());

            foreach (var volume in Globals.Database.Table<DiskModel>().ToList())
            {
                try
                {
                    var mountPoint = Globals.Database.GetMountPoint(volume.Id);
                    Dokan.RemoveMountPoint(mountPoint);
                    if (Globals.Volumes.TryRemove(volume.Id, out var thread)) thread.Dispose();
                }
                catch
                {
                    // Ignored. Terminating.
                }
            }

            Globals.CloudService.Dispose();
            Globals.Database.Dispose();
            return true;
        }
    }
}
