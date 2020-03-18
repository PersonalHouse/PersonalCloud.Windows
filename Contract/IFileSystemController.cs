using System;

namespace Unishare.Apps.WindowsContract
{
    public interface IFileSystemController
    {
        /// <summary>
        /// Invoke to mount a Personal Cloud as network drive.
        /// </summary>
        /// <param name="cloudId">GUID of the cloud.</param>
        /// <param name="mountPoint">Mount point for VFS, e.g. "Z:".</param>
        void MountNetworkDrive(Guid cloudId, string mountPoint);

        /// <summary>
        /// Invoke to unmount Personal Cloud's network drive.
        /// </summary>
        /// <param name="cloudId">GUID of the cloud.</param>
        void UnmountNetworkDrive(Guid cloudId);

        /// <summary>
        /// Invoke to (unmount all network drives and) re-mount them to previous mount points.
        /// </summary>
        void RemountAllDrives();

        /// <summary>
        /// Invoke to unmount all network drives. Their last mount points should be saved internally.
        /// </summary>
        void UnmountAllDrives();

        /// <summary>
        /// Invoke to terminate and restart the driver/service, for debugging and as a last resort to recover from error.
        /// </summary>
        void Restart();
    }
}
