using System;

using JKang.IpcServiceFramework.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NSPersonalCloud.WindowsContract;

namespace NSPersonalCloud.WindowsConfigurator
{
    internal static class Globals
    {
        public static Guid? PersonalCloud { get; set; }

        public static IHost ServiceHost { get; set; }
        public static ServiceProvider ServiceContainer { get; set; }
        public static IIpcClient<ICloudManager> CloudManager { get; set; }

        /*
        public static IIpcClient<IFileSystemController> Mounter { get; set; }
        public static IIpcClient<IPersistentStorage> Storage { get; set; }
        */

        public static bool IsServiceRunning { get; set; }
    }
}
