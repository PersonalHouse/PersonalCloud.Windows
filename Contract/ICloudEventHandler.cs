using System;

namespace Unishare.Apps.WindowsContract
{
    public interface ICloudEventHandler
    {
        void OnServiceStarted();

        void OnServiceStopped();

        void OnMountedVolumesChanged();

        void OnVolumeIOError(string mountPoint, Exception exception);

        void OnPersonalCloudAdded();

        void OnLeftPersonalCloud();
    }
}
