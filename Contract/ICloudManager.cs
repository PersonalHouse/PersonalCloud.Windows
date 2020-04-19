using System;
using System.Collections.Generic;

using NSPersonalCloud.Apps.Album;
using NSPersonalCloud.FileSharing.Aliyun;

namespace Unishare.Apps.WindowsContract
{
    /// <summary>
    /// This interface defines IPC contract for Personal Cloud management.
    /// </summary>
    public interface ICloudManager : IFileSystemController
    {
        /// <summary>
        /// Invoke to update the device name, optionally for a specific cloud.
        /// </summary>
        /// <param name="newName">The new device name.</param>
        /// <param name="cloudId">(Optional) GUID of a target cloud to observe change.</param>
        void ChangeDeviceName(string newName, Guid? cloudId);

        /// <summary>
        /// Invoke to update share APIs' VFS root, optionally for a specific cloud.
        /// </summary>
        /// <param name="absolutePath">Machine-absolute path of new VFS root.</param>
        /// <param name="cloudId">(Optional) GUID of a target cloud to observe change.</param>
        void ChangeSharingRoot(string absolutePath, Guid? cloudId);

        /// <summary>
        /// Invoke to leave from a specific cloud.
        /// </summary>
        /// <param name="id">GUID of the Personal Cloud to leave from.</param>
        void LeavePersonalCloud(Guid id);

        /// <summary>
        /// Invoke to create a new Personal Cloud, and join with this device.
        /// </summary>
        /// <param name="cloudName">Name for the new cloud.</param>
        /// <param name="deviceName">Name to join the cloud as.</param>
        /// <returns>GUID of the newly-created cloud, or <code>null</code> if unsuccessful.</returns>
        Guid? CreatePersonalCloud(string cloudName, string deviceName);

        /// <summary>
        /// Invoke to join a Personal Cloud by invitation.
        /// </summary>
        /// <param name="invite">Invite to use when joining.</param>
        /// <param name="deviceName">Name to join the cloud as.</param>
        /// <returns>GUID of the accepting cloud, or <code>null</code> if unsuccessful.</returns>
        Guid? JoinPersonalCloud(string invite, string deviceName);

        /// <summary>
        /// Invoke to generate a new invitation, and accept devices into the cloud.
        /// </summary>
        /// <param name="id">(Optional) GUID of a target cloud to observe change. If not specified, the first cloud is used.</param>
        /// <returns>Generated invite.</returns>
        string StartBroadcastingInvitation(Guid? id);

        /// <summary>
        /// Invoke to stop accepting devices into the cloud.
        /// </summary>
        /// <param name="id">(Optional) GUID of a target cloud to observe change. If not specified, the first cloud is used.</param>
        void StopBroadcastingInvitation(Guid? id);

        void ConnectToAlibabaCloud(Guid cloudId, string name, OssConfig config);

        string[] GetConnectedServices(Guid cloudId);

        void ChangeAlbumSettings(Guid cloudId, List<AlbumConfig> settings);

        List<AlbumConfig> GetAlbumSettings(Guid cloudId);
    }
}
