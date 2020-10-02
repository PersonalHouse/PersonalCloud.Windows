using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

using Microsoft.Extensions.Logging;

using NSPersonalCloud;
using NSPersonalCloud.FileSharing;
using NSPersonalCloud.Interfaces.FileSystem;

namespace DokanFS
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "<Pending>")]
    public class PersonalCloudRootFileSystem : IReadableFileSystem, IWriteableFileSystem
    {
        //private readonly PersonalCloud _PersonalCloud;

        private Microsoft.Extensions.Logging.ILogger Logger;
        IFileSystem RootFs;

        public PersonalCloudRootFileSystem(PersonalCloud personalCloud, Microsoft.Extensions.Logging.ILogger l)
        {
            Logger = l;
//              RootFs = personalCloud.RootFS;
//              return;
            //_PersonalCloud = personalCloud;
            var dic = new Dictionary<string, IFileSystem>();
            dic["Files"] = personalCloud.RootFS;
            var aif = new AppInFs(l);
            aif.GetApps = () =>  personalCloud.Apps;
            aif.GetUrl = (x) => personalCloud.GetWebAppUri(x).ToString();
            dic["Apps"] = aif;
            RootFs = new FileSystemContainer(dic, Logger);
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
            Logger.LogTrace("CreateDirectory called");
            filePath = filePath?.Replace("\\", "/");
            RootFs.CreateDirectoryAsync(filePath).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void CreateFile(string filePath)
        {
            Logger.LogTrace("CreateFile called");
            filePath = filePath?.Replace("\\", "/");
            RootFs.WriteFileAsync(filePath, new MemoryStream()).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public bool IsEmptyDirectory(string filePath)
        {
            Logger.LogTrace("IsEmptyDirectory called");
            filePath = filePath?.Replace("\\", "/");
            return !RootFs.EnumerateChildrenAsync(filePath).ConfigureAwait(false).GetAwaiter().GetResult().Any();
        }

        public void CheckNodeExists(string filePath, out bool isDirectory, out bool isFile)
        {
            GetFileInformation(filePath, out var fileInfo);
            if (fileInfo != null)
            {
                if (((FileInformation)fileInfo).Attributes.HasFlag(FileAttributes.Directory))
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
            else
            {
                isDirectory = false;
                isFile = false;
            }
        }

        public void DeleteDirectory(string filePath)
        {
            Logger.LogTrace("DeleteDirectory called");
            filePath = filePath?.Replace("\\", "/");
            RootFs.DeleteAsync(filePath).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void DeleteFile(string fileName)
        {
            Logger.LogTrace("DeleteFile called");
            fileName = fileName?.Replace("\\", "/");
            RootFs.DeleteAsync(fileName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset)
        {
            Logger.LogTrace($"WriteFile called");
            fileName = fileName?.Replace("\\", "/");
            RootFs.WritePartialFileAsync(fileName, offset, buffer.Length, new MemoryStream(buffer)).AsTask().Wait();
            bytesWritten = buffer.Length;
        }

        public void FlushFileBuffers(object context)
        {
            // The context is not used, it should always be null.
        }

        public void SetFileLength(string fileName, long length)
        {
            Logger.LogTrace($"SetFileLength called");
            fileName = fileName?.Replace("\\", "/");
            RootFs.SetFileLengthAsync(fileName, length).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void MoveDirectory(string oldName, string newName)
        {
            Logger.LogTrace($"SetFileLength called ");
            oldName = oldName?.Replace("\\", "/");
            newName = newName?.Replace("\\", "/");
            RootFs.RenameAsync(oldName, newName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void MoveFile(string oldName, string newName)
        {
            Logger.LogTrace($"MoveFile called");
            oldName = oldName?.Replace("\\", "/");
            newName = newName?.Replace("\\", "/");
            RootFs.RenameAsync(oldName, newName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void SetFileAttributes(string fileName, FileAttributes attributes)
        {
            Logger.LogTrace($"SetFileAttributes called");
            fileName = fileName?.Replace("\\", "/");
            RootFs.SetFileAttributesAsync(fileName, attributes).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            Logger.LogTrace($"SetFileTime called");
            fileName = fileName?.Replace("\\", "/");
            RootFs.SetFileTimeAsync(fileName, creationTime.Value, lastAccessTime.Value, lastWriteTime.Value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes)
        {
            Logger.LogTrace($"GetDiskFreeSpace called");
            totalNumberOfBytes = 2L * 1024 * 1024 * 1024*1014;
            freeBytesAvailable = totalNumberOfFreeBytes= 1024L * 1024 * 1024 * 1014;
            return;
            //throw new InvalidOperationException();

//             var dinfo = RootFs.GetFreeSpaceAsync().AsTask().Result;
// 
//             freeBytesAvailable = dinfo.FreeBytesAvailable;
//             totalNumberOfBytes = dinfo.TotalNumberOfBytes;
//             totalNumberOfFreeBytes = dinfo.TotalNumberOfFreeBytes;
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

            Logger.LogTrace($"EnumerateChildren called");
            if (searchPattern.IndexOfAny(new char[] { '?', '*' }) < 0)
            {
                var result = new List<FileInformation>();
                GetFileInformation(Path.Combine(filePath, searchPattern), out var fileInfo);
                if (fileInfo != null)
                {
                    result.Add((FileInformation)fileInfo);
                }
                return result;
            }
            else
            {
                return _RealEnumerateChildren(filePath, searchPattern);
            }
        }

        private IList<FileInformation> _RealEnumerateChildren(string filePath, string searchPattern)
        {
            filePath = filePath?.Replace("\\", "/");
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
            //Logger.LogTrace($"_RealEnumerateChildren {filePath} received {items.Count} item(s), return {files.Count} file(s)");
            return files;
        }

        public void GetFileInformation(string fileName, out FileInformation? fileInfo)
        {
            Logger.LogTrace("GetFileInformation called");
            fileName = fileName?.Replace("\\", "/");
            FileSystemEntry finfo = null;

            try
            {
                finfo = RootFs.ReadMetadataAsync(fileName).AsTask().Result;
            }
            catch
            {
            }

            //Logger.LogTrace($"GetFileInformation {fileName} received {finfo.Name} {finfo.Attributes}");

            if (finfo != null)
            {
                fileInfo = new FileInformation {
                    Attributes = FileAttributes.Normal | (finfo.IsDirectory ? FileAttributes.Directory : 0) | (finfo.IsHidden ? FileAttributes.Hidden : 0) | (finfo.IsReadOnly ? FileAttributes.ReadOnly : 0),
                    CreationTime = finfo.CreationDate,
                    LastAccessTime = null,
                    LastWriteTime = null,
                    Length = finfo.Size ?? 0,
                    FileName = finfo.Name
                };
            }
            else
            {
                fileInfo = null;
            }
        }

        public void ReadFile(string fileName, long offset, int length, byte[] buffer, out int bytesRead)
        {
            Logger.LogTrace($"ReadFile called fileName {fileName} offset {offset} length {length}");
            bytesRead = 0;
            try
            {
                fileName = fileName?.Replace("\\", "/");
                var stream = RootFs.ReadPartialFileAsync(fileName, offset, offset + buffer.Length - 1).ConfigureAwait(false).GetAwaiter().GetResult();
                Array.Clear(buffer, 0, buffer.Length);
                int remainBytes = buffer.Length;
                while (remainBytes > 0)
                {
                    int count = stream.Read(buffer, bytesRead, remainBytes);
                    if (count == 0) break;
                    bytesRead += count;
                    remainBytes -= count;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"ReadFile exception {ex.Message} {ex.StackTrace}");
            }
            Logger.LogTrace($"ReadFile return with  bytesRead {bytesRead}");
        }
    }
}
