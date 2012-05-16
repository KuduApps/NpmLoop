using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kudu.Core.Infrastructure
{
    internal static class FileSystemHelpers
    {
        public static void DeleteDirectorySafe(string path)
        {
            DeleteFileSystemInfo(new DirectoryInfo(path));
        }

        public static void DeleteDirectoryContentsSafe(string path)
        {
            DeleteDirectoryContentsSafe(new DirectoryInfo(path));
        }

        public static void DeleteIfEmpty(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            if (Directory.EnumerateFileSystemEntries(path).Any())
            {
                return;
            }

            // Just delete this directory
            Directory.Delete(path);
        }

        internal static void DeleteFileSafe(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (FileNotFoundException) { }
        }

        private static void DeleteFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            try
            {
                if (fileSystemInfo.Exists)
                {
                    fileSystemInfo.Attributes = FileAttributes.Normal;
                }
            }
            catch
            {
            }

            var directoryInfo = fileSystemInfo as DirectoryInfo;

            if (directoryInfo != null)
            {
                DeleteDirectoryContentsSafe(directoryInfo);
            }

            DoSafeAction(fileSystemInfo.Delete);
        }

        private static void DeleteDirectoryContentsSafe(DirectoryInfo directoryInfo)
        {
            try
            {
                if (directoryInfo.Exists)
                {
                    foreach (var fsi in directoryInfo.GetFileSystemInfos())
                    {
                        DeleteFileSystemInfo(fsi);
                    }
                }
            }
            catch
            {
            }
        }

        private static void DoSafeAction(Action action)
        {
            try
            {
                OperationManager.Attempt(action);
            }
            catch
            {
            }
        }

        
        internal static string GetDestinationPath(string sourceRootPath, string destinationRootPath, FileSystemInfo info)
        {
            string sourcePath = info.FullName;
            sourcePath = sourcePath.Substring(sourceRootPath.Length)
                                   .Trim(Path.DirectorySeparatorChar);

            return Path.Combine(destinationRootPath, sourcePath);
        }

        internal static IDictionary<string, FileInfo> GetFiles(DirectoryInfo info)
        {
            if (info == null)
            {
                return null;
            }
            return info.GetFilesWithRetry().ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
        }

        internal static IDictionary<string, DirectoryInfo> GetDirectories(DirectoryInfo info)
        {
            if (info == null)
            {
                return null;
            }
            return info.GetDirectories().ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);
        }

        // Call DirectoryInfoBase.GetFiles under a retry loop to make the system
        // more resilient when some files are temporarily in use
        internal static FileInfo[] GetFilesWithRetry(this DirectoryInfo info)
        {
            return OperationManager.Attempt(() =>
            {
                return info.GetFiles();
            });
        }
    }
}
