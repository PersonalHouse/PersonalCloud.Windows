using System;
using System.Collections.Concurrent;
using System.IO;

using JKang.IpcServiceFramework;

using Microsoft.Extensions.Logging;

using Nito.AsyncEx;

using NSPersonalCloud.WindowsContract;

using SQLite;

using Topshelf;

namespace NSPersonalCloud.WindowsService
{
    internal static class Globals
    {
        public const string Version = "2.0.0.1";

        public static HostControl ServiceHost { get; set; }
        public static ILoggerFactory Loggers { get; set; }

        public static WindowsDataStorage CloudConfig { get; set; }
        public static PCLocalService CloudService { get; set; }
        public static VirtualFileSystem CloudFileSystem { get; set; }
        public static SQLiteConnection Database { get; set; }

        public static ConcurrentDictionary<Guid, AsyncContextThread> Volumes { get; set; }

        public static IpcServiceClient<IPopupPresenter> PopupPresenter { get; set; }
        public static IpcServiceClient<ICloudEventHandler> NotificationCenter { get; set; }

        public static string ConfigurationPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Personal Cloud");
    }
}
