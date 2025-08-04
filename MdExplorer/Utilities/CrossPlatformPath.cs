using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Cross-platform path utilities for Linux/Windows/macOS compatibility
    /// </summary>
    public static class CrossPlatformPath
    {
        /// <summary>
        /// Gets the proper path separator for the current OS
        /// </summary>
        public static char Separator => Path.DirectorySeparatorChar;

        /// <summary>
        /// Gets the proper path separator as string
        /// </summary>
        public static string SeparatorString => Path.DirectorySeparatorChar.ToString();

        /// <summary>
        /// Normalizes a path to use the correct separator for the current OS
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Replace both types of slashes with the correct one for this OS
            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);

            // Remove duplicate separators
            while (path.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar))
            {
                path = path.Replace(
                    Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar,
                    Path.DirectorySeparatorChar.ToString());
            }

            return path;
        }

        /// <summary>
        /// Converts a path to use forward slashes (for URLs, web paths, etc.)
        /// </summary>
        public static string ToUnixPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Combines path segments in a cross-platform way
        /// </summary>
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            // Use Path.Combine but normalize the result
            var combined = Path.Combine(paths);
            return NormalizePath(combined);
        }

        /// <summary>
        /// Gets the last segment of a path (filename or last directory)
        /// </summary>
        public static string GetLastSegment(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            path = NormalizePath(path);
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Gets the parent directory of a path
        /// </summary>
        public static string GetParentDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            path = NormalizePath(path);
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Removes the base path from a full path to get relative path
        /// </summary>
        public static string GetRelativePath(string basePath, string fullPath)
        {
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(fullPath))
                return fullPath;

            basePath = NormalizePath(basePath);
            fullPath = NormalizePath(fullPath);

            // Ensure base path ends with separator for proper replacement
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath += Path.DirectorySeparatorChar;

            if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(basePath.Length);
            }

            // If not a sub-path, try to compute relative path
            try
            {
                return Path.GetRelativePath(basePath, fullPath);
            }
            catch
            {
                return fullPath;
            }
        }

        /// <summary>
        /// Checks if a path is absolute
        /// </summary>
        public static bool IsAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Windows: Check for drive letter (C:\)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.IsPathRooted(path) && 
                       (path.Length >= 2 && path[1] == ':');
            }

            // Unix/Linux/Mac: Check for root (/)
            return Path.IsPathRooted(path);
        }

        /// <summary>
        /// Gets the application data directory for the current OS
        /// </summary>
        public static string GetAppDataDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Follow XDG Base Directory Specification
                var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (!string.IsNullOrEmpty(xdgDataHome))
                    return xdgDataHome;

                var home = Environment.GetEnvironmentVariable("HOME");
                return Path.Combine(home, ".local", "share");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return Path.Combine(home, "Library", "Application Support");
            }

            // Fallback
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        /// <summary>
        /// Gets the MdExplorer data directory
        /// </summary>
        public static string GetMdExplorerDataDirectory()
        {
            return Path.Combine(GetAppDataDirectory(), "MdExplorer");
        }

        /// <summary>
        /// Gets the application data path for MdExplorer and ensures it exists
        /// </summary>
        public static string GetAppDataPath()
        {
            var path = GetMdExplorerDataDirectory();
            EnsureDirectoryExists(path);
            return path;
        }

        /// <summary>
        /// Ensures a directory exists, creating it if necessary
        /// </summary>
        public static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Splits a path into its segments
        /// </summary>
        public static string[] SplitPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return new string[0];

            path = NormalizePath(path);
            return path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Removes invalid characters from a filename
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '-');
            }

            // Also handle Windows reserved names
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", 
                                           "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", 
                                           "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
                
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                if (Array.Exists(reservedNames, name => name.Equals(nameWithoutExtension, StringComparison.OrdinalIgnoreCase)))
                {
                    fileName = "_" + fileName;
                }
            }

            return fileName;
        }
    }
}