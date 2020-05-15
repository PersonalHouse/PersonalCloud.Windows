using System;

using NSPersonalCloud;

using SQLite;

namespace NSPersonalCloud.WindowsService.Data
{
    internal static class WindowsUserSettings
    {
        public const string EnableVolumeMounting = "Windows.Dokan";

        public static string GetMountPoint(this SQLiteConnection database, Guid cloudId)
        {
            return database.Find<DiskModel>(cloudId)?.MountPoint;
        }

        public static string GetMountPoint(this SQLiteConnection database, PersonalCloud cloud) => database.GetMountPoint(new Guid(cloud.Id));

        public static void SaveMountPoint(this SQLiteConnection database, Guid cloudId, string mountPoint)
        {
            if (mountPoint?.Length != 3 || !mountPoint.EndsWith(@":\", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Mount Point is invalid.");

            database.InsertOrReplace(new DiskModel {
                Id = cloudId,
                MountPoint = mountPoint
            });
        }

        public static void SaveMountPoint(this SQLiteConnection database, PersonalCloud cloud, string mountPoint)
        {
            if (cloud is null) throw new ArgumentNullException(nameof(cloud));
            database.SaveMountPoint(new Guid(cloud.Id), mountPoint);
        }

        public static void RemoveMountPoint(this SQLiteConnection database, Guid cloudId)
        {
            database.Delete<DiskModel>(cloudId);
        }

        public static void RemoveMountPoint(this SQLiteConnection database, PersonalCloud cloud)
        {
            if (cloud is null) throw new ArgumentNullException(nameof(cloud));
            database.RemoveMountPoint(new Guid(cloud.Id));
        }
    }
}
