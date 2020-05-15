using System;

using SQLite;

namespace NSPersonalCloud.WindowsService.Data
{
    [Table("Disk")]
    public class DiskModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string MountPoint { get; set; }
    }
}
