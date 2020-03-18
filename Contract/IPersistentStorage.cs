using System;

namespace Unishare.Apps.WindowsContract
{
    /// <summary>
    /// This interface defines IPC contract for retrieving data from persistent storage.
    /// </summary>
    public interface IPersistentStorage
    {
        /// <summary>
        /// Invoke to retrieve a list of saved Personal Cloud IDs.
        /// </summary>
        /// <returns>GUIDs of persistent personal cloud.</returns>
        Guid[] GetAllPersonalCloud();

        /// <summary>
        /// Invoke to retrieve the display name of a Personal Cloud.
        /// </summary>
        /// <param name="id">GUID of the cloud.</param>
        /// <returns>Display name of the cloud.</returns>
        string GetPersonalCloudName(Guid id);

        /// <summary>
        /// Invoke to retrieve the name for this device.
        /// </summary>
        /// <param name="cloudId">(Optional) GUID of the cloud to obtain info from.</param>
        /// <returns>Name of this device.</returns>
        string GetDeviceName(Guid? cloudId);

        /// <summary>
        /// Invoke to retrieve file sharing state.
        /// </summary>
        /// <param name="cloudId">(Optional) GUID of the cloud to obtain info from.</param>
        /// <returns>Whether user enabled file sharing.</returns>
        bool IsFileSharingEnabled(Guid? cloudId);

        /// <summary>
        /// Invoke to retrieve file sharing path for all cloud.
        /// </summary>
        /// <returns>Machine-absolute path for file sharing.</returns>
        string GetFileSharingRoot();

        /// <summary>
        /// Invoke to retrieve File Explorer integration state.
        /// </summary>
        /// <returns>Whether user enabled mounting of network drives.</returns>
        bool IsExplorerIntegrationEnabled();

        /// <summary>
        /// Invoke to retrieve mount point for Personal Cloud.
        /// </summary>
        /// <param name="id">GUID of the cloud.</param>
        /// <returns>
        /// <para>
        /// Last mount point for this cloud, e.g. "Z:"; or <code>null</code> if disabled.
        /// </para>
        /// <para>
        /// The original mount point should be returned even if user disabled network drives globally.
        /// Return <code>null</code> only if user specifically disabled mounting for this Personal Cloud;
        /// in other words, only when this cloud is not mounted while others are.
        /// </para>
        /// </returns>
        string GetMountPointForPersonalCloud(Guid id);
    }
}
