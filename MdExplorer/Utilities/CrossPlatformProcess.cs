using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Cross-platform utilities for process execution and file opening
    /// </summary>
    public static class CrossPlatformProcess
    {
        /// <summary>
        /// Opens a file with the system's default application
        /// </summary>
        public static bool OpenFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            try
            {
                ProcessStartInfo psi;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{filePath}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Use xdg-open on Linux
                    psi = new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"\"{filePath}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Use open on macOS
                    psi = new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"\"{filePath}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                }
                else
                {
                    // Fallback to UseShellExecute
                    psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                }

                using (var process = Process.Start(psi))
                {
                    return process != null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening file {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens a folder in the system's file explorer
        /// </summary>
        public static bool OpenFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return false;

            try
            {
                ProcessStartInfo psi;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = folderPath
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Use xdg-open on Linux
                    psi = new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"\"{folderPath}\""
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Use open on macOS
                    psi = new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"\"{folderPath}\""
                    };
                }
                else
                {
                    // Fallback
                    psi = new ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true
                    };
                }

                using (var process = Process.Start(psi))
                {
                    return process != null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening folder {folderPath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens a URL in the default web browser
        /// </summary>
        public static bool OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                ProcessStartInfo psi;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = url,
                        UseShellExecute = false
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = url,
                        UseShellExecute = false
                    };
                }
                else
                {
                    // Fallback
                    psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                }

                using (var process = Process.Start(psi))
                {
                    return process != null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening URL {url}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executes a shell command cross-platform
        /// </summary>
        public static ProcessResult ExecuteCommand(string command, string workingDirectory = null, int timeoutMs = 30000)
        {
            var result = new ProcessResult();

            try
            {
                ProcessStartInfo psi;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {command}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else
                {
                    // Linux/macOS use bash
                    psi = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{command}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }

                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    psi.WorkingDirectory = workingDirectory;
                }

                using (var process = Process.Start(psi))
                {
                    bool exited = process.WaitForExit(timeoutMs);
                    
                    if (exited)
                    {
                        result.ExitCode = process.ExitCode;
                        result.StandardOutput = process.StandardOutput.ReadToEnd();
                        result.StandardError = process.StandardError.ReadToEnd();
                        result.Success = process.ExitCode == 0;
                    }
                    else
                    {
                        process.Kill();
                        result.Success = false;
                        result.StandardError = "Process timed out";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.StandardError = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Smart discovery of VS Code executable path
        /// </summary>
        public static string DiscoverVSCodePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Check multiple possible locations
                // 1. Check current user's AppData first (most common for user installs)
                var userPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Microsoft VS Code", "Code.exe");
                if (File.Exists(userPath))
                    return userPath;

                // 2. Check Program Files for system-wide installs
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var systemPath = Path.Combine(programFiles, "Microsoft VS Code", "Code.exe");
                if (File.Exists(systemPath))
                    return systemPath;

                // 3. Check Program Files (x86) for 32-bit installs
                var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                if (!string.IsNullOrEmpty(programFilesX86))
                {
                    var systemPathX86 = Path.Combine(programFilesX86, "Microsoft VS Code", "Code.exe");
                    if (File.Exists(systemPathX86))
                        return systemPathX86;
                }

                // 4. Try to find it via registry (if installed via installer)
                try
                {
                    var result = ExecuteCommand("reg query \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\" /s /f \"Visual Studio Code\"");
                    if (result.Success && !string.IsNullOrEmpty(result.StandardOutput))
                    {
                        // Parse registry output to find install location
                        var lines = result.StandardOutput.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains("InstallLocation") && line.Contains("REG_SZ"))
                            {
                                var parts = line.Split(new[] { "REG_SZ" }, StringSplitOptions.None);
                                if (parts.Length > 1)
                                {
                                    var installPath = parts[1].Trim();
                                    var codePath = Path.Combine(installPath, "Code.exe");
                                    if (File.Exists(codePath))
                                        return codePath;
                                }
                            }
                        }
                    }
                }
                catch { }

                // 5. Try to find it in PATH
                var pathResult = ExecuteCommand("where code");
                if (pathResult.Success && !string.IsNullOrWhiteSpace(pathResult.StandardOutput))
                {
                    var firstPath = pathResult.StandardOutput.Split('\n')[0].Trim();
                    if (File.Exists(firstPath))
                        return firstPath;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: Check multiple package managers and locations
                
                // 1. Check if it's in PATH (most common)
                var whichResult = ExecuteCommand("which code");
                if (whichResult.Success && !string.IsNullOrWhiteSpace(whichResult.StandardOutput))
                {
                    return whichResult.StandardOutput.Trim();
                }

                // 2. Check snap installation
                if (File.Exists("/snap/bin/code"))
                    return "/snap/bin/code";

                // 3. Check flatpak installation
                var flatpakResult = ExecuteCommand("flatpak list --app | grep com.visualstudio.code");
                if (flatpakResult.Success && !string.IsNullOrWhiteSpace(flatpakResult.StandardOutput))
                {
                    return "flatpak run com.visualstudio.code";
                }

                // 4. Check standard locations
                var standardPaths = new[] {
                    "/usr/bin/code",
                    "/usr/local/bin/code",
                    "/opt/visual-studio-code/code"
                };
                foreach (var path in standardPaths)
                {
                    if (File.Exists(path))
                        return path;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: Check standard application locations
                
                // 1. Check Applications folder
                var appPath = "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code";
                if (File.Exists(appPath))
                    return appPath;

                // 2. Check if code command is available
                var whichResult = ExecuteCommand("which code");
                if (whichResult.Success && !string.IsNullOrWhiteSpace(whichResult.StandardOutput))
                {
                    return whichResult.StandardOutput.Trim();
                }

                // 3. Check user Applications
                var userAppPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Applications/Visual Studio Code.app/Contents/Resources/app/bin/code");
                if (File.Exists(userAppPath))
                    return userAppPath;
            }

            // If nothing found, return null (not a fallback "code")
            return null;
        }
    }

    /// <summary>
    /// Result of a process execution
    /// </summary>
    public class ProcessResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
    }
}