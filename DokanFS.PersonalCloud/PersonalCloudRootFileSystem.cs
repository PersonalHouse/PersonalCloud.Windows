using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

using NSPersonalCloud;

namespace DokanFS
{
    public class PersonalCloudRootFileSystem : IReadableFileSystem, IWriteableFileSystem
    {
        private readonly PersonalCloud _PersonalCloud;

        public PersonalCloudRootFileSystem(PersonalCloud personalCloud)
        {
            _PersonalCloud = personalCloud;
        }

        public object CreateFileContext(string fileName, FileMode mode, bool readAccess, FileShare share, FileOptions options)
        {
            if (mode == FileMode.CreateNew || mode == FileMode.Create)
            {
                this.CreateFile(fileName);
            }
            return new object();
        }

        public void CloseFileContext(object context)
        {
            // The context is not used, it should always be null.
        }

        public void CreateDirectory(string filePath)
        {
            filePath = filePath?.Replace("\\", "/");
            _PersonalCloud.RootFS.CreateDirectoryAsync(filePath).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void CreateFile(string filePath)
        {
            filePath = filePath?.Replace("\\", "/");
            _PersonalCloud.RootFS.WriteFileAsync(filePath, new MemoryStream()).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public bool IsEmptyDirectory(string filePath)
        {
            filePath = filePath?.Replace("\\", "/");
            return !_PersonalCloud.RootFS.EnumerateChildrenAsync(filePath).ConfigureAwait(false).GetAwaiter().GetResult().Any();
        }

        public void CheckNodeExists(string filePath, out bool isDirectory, out bool isFile)
        {
            GetFileInformation(filePath, out var fileInfo);
            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
            {
                isDirectory = true;
                isFile = false;
            }
            else
            {
                isDirectory = false;
                isFile = true;
            }
        }

        public void DeleteDirectory(string filePath)
        {
            filePath = filePath?.Replace("\\", "/");
            _PersonalCloud.RootFS.DeleteAsync(filePath).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void DeleteFile(string fileName)
        {
            fileName = fileName?.Replace("\\", "/");
            _PersonalCloud.RootFS.DeleteAsync(fileName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset)
        {
            fileName = fileName?.Replace("\\", "/");
            _PersonalCloud.RootFS.WritePartialFileAsync(fileName, offset, buffer.Length, new MemoryStream(buffer)).ConfigureAwait(false).GetAwaiter().GetResult();
            bytesWritten = buffer.Length;
        }

        public void FlushFileBuffers(object context)
        {
            // The context is not used, it should always be null.
        }

        public void SetFileLength(string fileName, long length)
        {
            fileName = fileName?.Replace("\\", "/");
            _PersonalCloud.RootFS.SetFileLengthAsync(fileName, length).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void MoveDirectory(string oldName, string newName)
        {
            oldName = oldName?.Replace("\\", "/");
            newName = newName?.Replace("\\", "/");
            _PersonalCloud.RootFS.RenameAsync(oldName, newName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void MoveFile(string oldName, string newName)
        {
            oldName = oldName?.Replace("\\", "/");
            newName = newName?.Replace("\\", "/");
            _PersonalCloud.RootFS.RenameAsync(oldName, newName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void SetFileAttributes(string fileName, FileAttributes attributes)
        {
            fileName = fileName?.Replace("\\", "/");
            _PersonalCloud.RootFS.SetFileAttributesAsync(fileName, attributes).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            fileName = fileName?.Replace("\\", "/");
            _PersonalCloud.RootFS.SetFileTimeAsync(fileName, creationTime.Value, lastAccessTime.Value, lastWriteTime.Value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes)
        {
            var dinfo = _PersonalCloud.RootFS.GetFreeSpaceAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            freeBytesAvailable = dinfo.FreeBytesAvailable;
            totalNumberOfBytes = dinfo.TotalNumberOfBytes;
            totalNumberOfFreeBytes = dinfo.TotalNumberOfFreeBytes;
        }

        public void GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength)
        {
            volumeLabel = "Home Cloud";
            fileSystemName = "NTFS";
            maximumComponentLength = 256;

            features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                       FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
                       FileSystemFeatures.UnicodeOnDisk;
        }

        public IList<FileInformation> EnumerateChildren(string filePath, string searchPattern)
        {
            if (searchPattern.IndexOfAny(new char[] { '?', '*' }) < 0)
            {
                GetFileInformation(Path.Combine(filePath, searchPattern), out var fileInfo);
                return new[] { fileInfo };
            }
            else
            {
                return _RealEnumerateChildren(filePath, searchPattern);
            }
        }

        private IList<FileInformation> _RealEnumerateChildren(string filePath, string searchPattern)
        {
            filePath = filePath?.Replace("\\", "/");
            var items = _PersonalCloud.RootFS.EnumerateChildrenAsync(filePath).ConfigureAwait(false).GetAwaiter().GetResult();
            IList<FileInformation> files = items
                .Where(finfo => DokanHelper.DokanIsNameInExpression(searchPattern, finfo.Name, true))
                .Select(finfo => new FileInformation {
                    Attributes = FileAttributes.Normal | (finfo.IsDirectory ? FileAttributes.Directory : 0) | (finfo.IsHidden ? FileAttributes.Hidden : 0),// | (finfo.IsReadOnly ? FileAttributes.ReadOnly : 0),
                CreationTime = finfo.CreationDate,
                    LastAccessTime = null,
                    LastWriteTime = null,
                    Length = finfo.Size ?? 0,
                    FileName = finfo.Name
                }).ToArray();
            Console.WriteLine($"_RealEnumerateChildren received {items.Count} item(s), return {files.Count} file(s)");
            return files;
        }

        public void GetFileInformation(string fileName, out FileInformation fileInfo)
        {
            fileName = fileName?.Replace("\\", "/");

            var finfo = _PersonalCloud.RootFS.ReadMetadataAsync(fileName).ConfigureAwait(false).GetAwaiter().GetResult();

            fileInfo = new FileInformation {
                Attributes = FileAttributes.Normal | (finfo.IsDirectory ? FileAttributes.Directory : 0) | (finfo.IsHidden ? FileAttributes.Hidden : 0) | (finfo.IsReadOnly ? FileAttributes.ReadOnly : 0),
                CreationTime = finfo.CreationDate,
                LastAccessTime = null,
                LastWriteTime = null,
                Length = finfo.Size ?? 0,
                FileName = finfo.Name
            };
        }

        public void ReadFile(string fileName, long offset, int length, byte[] buffer, out int bytesRead)
        {
            fileName = fileName?.Replace("\\", "/");
            var stream = _PersonalCloud.RootFS.ReadPartialFileAsync(fileName, offset, offset + buffer.Length - 1).ConfigureAwait(false).GetAwaiter().GetResult();
            Array.Clear(buffer, 0, buffer.Length);
            bytesRead = stream.Read(buffer, 0, buffer.Length);
        }
    }
}
