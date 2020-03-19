using System.Collections.Generic;
using System.IO;
using DokanNet;

namespace DokanFS
{
    public interface IReadableFileSystem
    {
        #region Volume Metadata

        void GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes);
        void GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength);

        #endregion Volume Metadata

        #region File Context

        object CreateFileContext(string fileName, FileMode mode, bool readAccess, FileShare share, FileOptions options);
        void CloseFileContext(object context);

        #endregion File Context

        void CheckNodeExists(string filePath, out bool isDirectory, out bool isFile);

        IList<FileInformation> EnumerateChildren(string filePath, string searchPattern);
        void GetFileInformation(string fileName, out FileInformation fileInfo);

        void ReadFile(string fileName, long offset, int length, byte[] buffer, out int bytesRead);
    }
}
