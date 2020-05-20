using System;

using JKang.IpcServiceFramework;

using NSPersonalCloud.WindowsContract;

namespace NSPersonalCloud.WindowsConfigurator
{
    internal static class Globals
    {
        public static Guid? PersonalCloud { get; set; }
        public static IpcServiceClient<ICloudManager> CloudManager { get; set; }
        public static IpcServiceClient<IFileSystemController> Mounter { get; set; }
        public static IpcServiceClient<IPersistentStorage> Storage { get; set; }

        public static bool IsServiceRunning { get; set; }
    }
}
