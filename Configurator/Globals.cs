using System;

using JKang.IpcServiceFramework.Client;

using NSPersonalCloud.WindowsContract;

namespace NSPersonalCloud.WindowsConfigurator
{
    internal static class Globals
    {
        public static Guid? PersonalCloud { get; set; }
        public static IIpcClient<ICloudManager> CloudManager { get; set; }

        /*
        public static IIpcClient<IFileSystemController> Mounter { get; set; }
        public static IIpcClient<IPersistentStorage> Storage { get; set; }
        */

        public static bool IsServiceRunning { get; set; }
    }
}
