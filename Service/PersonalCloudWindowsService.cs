using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

using DokanFS;

using DokanNet;
using DokanNet.Logging;

using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Nito.AsyncEx;

using NSPersonalCloud.Common;
using NSPersonalCloud.Common.Models;
using NSPersonalCloud.Interfaces.Errors;
using NSPersonalCloud.WindowsContract;
using NSPersonalCloud.WindowsService.Data;
using NSPersonalCloud.WindowsService.IPC;

using SQLite;

using Topshelf;

namespace NSPersonalCloud.WindowsService
{
    internal class PersonalCloudWindowsService : ServiceControl
    {
        private const string PopupClient = "Pop-up Presenter";
        private const string NotificationClient = "Event Handler";

        private HostControl HostControl { get; set; }
        private CancellationTokenSource Runners { get; set; }

        private Microsoft.Extensions.Logging.ILogger Logger { get; set; }

        public PersonalCloudWindowsService()
        {
            Logger = Globals.Loggers.CreateLogger<PersonalCloudWindowsService>();
        }

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
            Globals.Database.CreateTable<WebApp>();

            Globals.CloudFileSystem = new VirtualFileSystem(null);
            Globals.CloudConfig = new WindowsDataStorage();

            Globals.Volumes = new ConcurrentDictionary<Guid, AsyncContextThread>();
        }

        public bool Start(HostControl hostControl)
        {
            Logger.LogInformation("Windows service started.");
            HostControl = hostControl;

            Initialize();

            Runners = new CancellationTokenSource();

            /*
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
            */

            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                                             .ConfigureServices(services => {
                                                 services.AddSingleton<ICloudManager, CloudManagerService>();
                                             })
                                             .ConfigureIpcHost(builder => {
                                                 builder.AddNamedPipeEndpoint<ICloudManager>(Pipes.CloudAdmin);
                                             })
                                             .ConfigureLogging(builder => {
                                                 builder.SetMinimumLevel(LogLevel.Information);
                                             })
                                             .Build().RunAsync();

            var services = new ServiceCollection().AddNamedPipeIpcClient<ICloudEventHandler>(NotificationClient, Pipes.NotificationCenter)
                                                  .BuildServiceProvider();

            Globals.NotificationCenter = services.GetRequiredService<IIpcClientFactory<ICloudEventHandler>>()
                                                                .CreateClient(NotificationClient);

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
            Globals.CloudService = new PCLocalService(Globals.CloudConfig, Globals.Loggers, Globals.CloudFileSystem, resourcesPath);
            Globals.CloudService.OnError += (o, e) => {
                if (e.ErrorCode == ErrorCode.NeedUpdate)
                    Globals.NotificationCenter.InvokeAsync(x => x.ShowAlert("New Version Available",
                        "Upgrade Personal Cloud to connect to some devices in your network."));
            };

            if (!Globals.Database.CheckSetting(UserSettings.LastInstalledVersion, Globals.Version))
            {
                Globals.CloudService.InstallApps().Wait();
            }

            Globals.CloudService.StartService();
            Globals.NotificationCenter.InvokeAsync(x => x.OnServiceStarted());

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
                            Logger.LogError(exception, "OnVolumeIOError exception");
                            Globals.NotificationCenter.InvokeAsync(x => x.OnVolumeIOError(mountPoint, exception));
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
            Globals.NotificationCenter.InvokeAsync(x => x.OnServiceStopped());

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

            Logger.LogInformation("Windows service stopped.");

            return true;
        }
    }
}
