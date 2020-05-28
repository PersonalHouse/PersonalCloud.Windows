using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DokanNet;

using Microsoft.Extensions.Logging;

using NSPersonalCloud;
using NSPersonalCloud.FileSharing;
using NSPersonalCloud.Interfaces.FileSystem;

/*
 * Todo: Remove this section if all fixes to remove warnings are acknowledged and tested.
 *
 * 1. Made 'Logger' and 'RootFs' properties.
 *    (Previously fields.)
 * 2. Added 'StringComparison.Ordinal' modifier on all culture-sensitive input comparisons.
 * 3. Added 'CultureInfo.InvariantCulture' on all culture-sensitive outputs.
 * 4. Added null-check on parameters that are not used with '?.' syntax.
 *    (Previously they throw 'NullReferenceException'; now 'ArgumentNullException'.)
 * 5. 'ValueTask's are converted to 'Task's before '.Wait()' or '.Result'.
 *    (Previously used directory.)
 */

namespace DokanFS
{
    public class PersonalCloudRootFileSystem : IReadableFileSystem, IWriteableFileSystem
    {
        //private readonly PersonalCloud _PersonalCloud;

        private ILogger Logger { get; }

        private IFileSystem RootFs { get; }

        public PersonalCloudRootFileSystem(PersonalCloud personalCloud, ILogger l)
        {
            if (personalCloud is null) throw new ArgumentNullException(nameof(personalCloud));


            Logger = l;
            // RootFs = personalCloud.RootFS;
            // return;
            // _PersonalCloud = personalCloud;
            var dic = new Dictionary<string, IFileSystem>();
            dic["Files"] = personalCloud.RootFS;
            var aif = new AppInFs();
            aif.GetApps = () => personalCloud.Apps;
            aif.GetUrl = (x) => personalCloud.GetWebAppUri(x).ToString();
            dic["Apps"] = aif;
            RootFs = new FileSystemContainer(dic, Logger);
        }

        public object CreateFileContext(string fileName, FileMode mode, bool readAccess, FileShare share, FileOptions options)
        {
            if (mode == FileMode.CreateNew || mode == FileMode.Create)
            {
                CreateFile(fileName);
            }
            return new object();
        }

        public void CloseFileContext(object context)
        {
            // The context is not used, it should always be null.
        }

        public void CreateDirectory(string filePath)
        {
            Logger.LogTrace("CreateDirectory called");
            filePath = filePath?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.CreateDirectoryAsync(filePath).AsTask().Wait();
        }

        public void CreateFile(string filePath)
        {
            Logger.LogTrace("CreateFile called");
            filePath = filePath?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.WriteFileAsync(filePath, new MemoryStream()).AsTask().Wait();
        }

        public bool IsEmptyDirectory(string filePath)
        {
            Logger.LogTrace("IsEmptyDirectory called");
            filePath = filePath?.Replace("\\", "/", StringComparison.Ordinal);
            return !RootFs.EnumerateChildrenAsync(filePath).AsTask().Result.Any();
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
            Logger.LogTrace("DeleteDirectory called");
            filePath = filePath?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.DeleteAsync(filePath).AsTask().Wait();
        }

        public void DeleteFile(string fileName)
        {
            Logger.LogTrace("DeleteFile called");
            fileName = fileName?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.DeleteAsync(fileName).AsTask().Wait();
        }

        public void WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset)
        {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));

            Logger.LogTrace($"WriteFile {fileName}");
            fileName = fileName?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.WritePartialFileAsync(fileName, offset, buffer.Length, new MemoryStream(buffer)).AsTask().Wait();
            bytesWritten = buffer.Length;
        }

        public void FlushFileBuffers(object context)
        {
            // The context is not used, it should always be null.
        }

        public void SetFileLength(string fileName, long length)
        {
            Logger.LogTrace($"SetFileLength called {fileName} {length} ");
            fileName = fileName?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.SetFileLengthAsync(fileName, length).AsTask().Wait();
        }

        public void MoveDirectory(string oldName, string newName)
        {
            Logger.LogTrace($"SetFileLength called {oldName} {newName} ");
            oldName = oldName?.Replace("\\", "/", StringComparison.Ordinal);
            newName = newName?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.RenameAsync(oldName, newName).AsTask().Wait();
        }

        public void MoveFile(string oldName, string newName)
        {
            Logger.LogTrace($"MoveFile called");
            oldName = oldName?.Replace("\\", "/", StringComparison.Ordinal);
            newName = newName?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.RenameAsync(oldName, newName).AsTask().Wait();
        }

        public void SetFileAttributes(string fileName, FileAttributes attributes)
        {
            Logger.LogTrace($"SetFileAttributes called");
            fileName = fileName?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.SetFileAttributesAsync(fileName, attributes).AsTask().Wait();
        }

        public void SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            Logger.LogTrace($"SetFileTime called");
            fileName = fileName?.Replace("\\", "/", StringComparison.Ordinal);
            RootFs.SetFileTimeAsync(fileName, creationTime.Value, lastAccessTime.Value, lastWriteTime.Value).AsTask().Wait();
        }

        public void GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes)
        {
            Logger.LogTrace($"GetDiskFreeSpace called");
            throw new InvalidOperationException();

            // var dinfo = RootFs.GetFreeSpaceAsync().AsTask().Result;
            // 
            // freeBytesAvailable = dinfo.FreeBytesAvailable;
            // totalNumberOfBytes = dinfo.TotalNumberOfBytes;
            // totalNumberOfFreeBytes = dinfo.TotalNumberOfFreeBytes;
        }

        public void GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength)
        {
            Logger.LogTrace($"GetVolumeInformation called");
            volumeLabel = "Personal Cloud";
            fileSystemName = "NTFS";
            maximumComponentLength = 256;

            features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                       FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
                       FileSystemFeatures.UnicodeOnDisk;
        }

        public IList<FileInformation> EnumerateChildren(string filePath, string searchPattern)
        {
            if (searchPattern is null) throw new ArgumentNullException(nameof(searchPattern));

            Logger.LogTrace($"EnumerateChildren {filePath} searchPattern {searchPattern}");
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
            filePath = filePath?.Replace("\\", "/", StringComparison.Ordinal);
            var items = RootFs.EnumerateChildrenAsync(filePath).AsTask().Result;
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
            Logger.LogTrace($"_RealEnumerateChildren {filePath} received {items.Count} item(s), return {files.Count} file(s)");
            return files;
        }

        public void GetFileInformation(string fileName, out FileInformation fileInfo)
        {
            Logger.LogTrace("GetFileInformation called");
            fileName = fileName?.Replace("\\", "/", StringComparison.Ordinal);
            FileSystemEntry finfo = null;

            finfo = RootFs.ReadMetadataAsync(fileName).AsTask().Result;

            Logger.LogTrace($"GetFileInformation {fileName} received {finfo.Name} {finfo.Attributes}");

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
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));

            Logger.LogTrace("ReadFile called");
            fileName = fileName?.Replace("\\", "/", StringComparison.Ordinal);
            var stream = RootFs.ReadPartialFileAsync(fileName, offset, offset + buffer.Length - 1).AsTask().Result;
            Array.Clear(buffer, 0, buffer.Length);
            int remainBytes = buffer.Length;
            bytesRead = 0;
            while (remainBytes > 0)
            {
                int count = stream.Read(buffer, bytesRead, remainBytes);
                if (count == 0) break;
                bytesRead += count;
                remainBytes -= count;
            }
        }
    }
}
