using System;
using System.Collections.Concurrent;
using System.IO;

using JKang.IpcServiceFramework.Client;

using Microsoft.Extensions.Logging;

using Nito.AsyncEx;

using NSPersonalCloud.WindowsContract;

using SQLite;

using Topshelf;

using Zio;
using Zio.FileSystems;

namespace NSPersonalCloud.WindowsService
{
    internal static class Globals
    {
        public const string Version = "2.0.0.1";

        public static HostControl ServiceHost { get; set; }
        public static ILoggerFactory Loggers { get; set; }

        public static WindowsDataStorage CloudConfig { get; set; }
        public static PCLocalService CloudService { get; set; }
        public static IFileSystem CloudFileSystem => _CloudFileSystem;
        private static IFileSystem _CloudFileSystem;

        public static void SetupFS(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                RootPath = null;
                _CloudFileSystem = new MemoryFileSystem();
            }
            else
            {
                RootPath = absolutePath;
#pragma warning disable CA2000 // Dispose objects before losing scope
                var fs = new PhysicalFileSystem();
#pragma warning restore CA2000 // Dispose objects before losing scope
                Directory.CreateDirectory(absolutePath);
                _CloudFileSystem = new SubFileSystem(fs, fs.ConvertPathFromInternal(absolutePath), true);
                if (CloudService != null)
                {
                    CloudService.FileSystem = CloudFileSystem;
                }
            }
            if (CloudService!=null)
            {
                CloudService.BroadcastingIveChanged();
            }
        }
        public static string RootPath { get; set; }
        public static SQLiteConnection Database { get; set; }

        public static ConcurrentDictionary<Guid, AsyncContextThread> Volumes { get; set; }

        public static IIpcClient<ICloudEventHandler> NotificationCenter { get; set; }

        public static string ConfigurationPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                                               "Personal Cloud");
    }
}
