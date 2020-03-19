using System;
using System.IO;

namespace DokanFS
{
    public interface IWriteableFileSystem
    {
        void CreateDirectory(string filePath);
        void CreateFile(string filePath);
        bool IsEmptyDirectory(string filePath); // Used for DeleteDirectory
        void DeleteDirectory(string filePath);
        void DeleteFile(string filePath);
        void WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset);
        void MoveDirectory(string oldPath, string newPath);
        void MoveFile(string oldPath, string newPath);
        void SetFileAttributes(string fileName, FileAttributes attributes);
        void SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime);
        void FlushFileBuffers(object context);
        void SetFileLength(string fileName, long length);
    }
}
