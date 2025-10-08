using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Cross-platform Java discovery utility
    /// </summary>
    public static class JavaDiscovery
    {
        /// <summary>
        /// Discovers Java executable path across different operating systems
        /// </summary>
        /// <returns>Path to java executable or null if not found</returns>
        public static string DiscoverJavaPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return DiscoverJavaWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return DiscoverJavaLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return DiscoverJavaMacOS();
            }
            
            return null;
        }

        private static string DiscoverJavaWindows()
        {
            var javaPaths = new List<string>();

            // 1. Check JAVA_HOME environment variable
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javaExe))
                    return javaExe;
                
                // Some installations use javaw.exe for GUI applications
                var javawExe = Path.Combine(javaHome, "bin", "javaw.exe");
                if (File.Exists(javawExe))
                    return javawExe;
            }

            // 2. Check PATH environment variable
            var pathResult = ExecuteCommand("where java");
            if (pathResult.Success && !string.IsNullOrWhiteSpace(pathResult.StandardOutput))
            {
                var firstPath = pathResult.StandardOutput.Split('\n')[0].Trim();
                if (File.Exists(firstPath))
                    return firstPath;
            }

            // 3. Check common installation directories
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            
            var commonPaths = new List<string>
            {
                Path.Combine(programFiles, "Java"),
                Path.Combine(programFiles, "Eclipse Adoptium"),
                Path.Combine(programFiles, "Microsoft"),
                Path.Combine(programFiles, "Zulu"),
                Path.Combine(programFiles, "Amazon Corretto"),
                Path.Combine(programFiles, "BellSoft"),
                "C:\\Program Files\\Java",
                "C:\\Java"
            };

            if (!string.IsNullOrEmpty(programFilesX86))
            {
                commonPaths.Add(Path.Combine(programFilesX86, "Java"));
                commonPaths.Add(Path.Combine(programFilesX86, "Eclipse Adoptium"));
            }

            foreach (var basePath in commonPaths.Where(Directory.Exists))
            {
                try
                {
                    var javaFolders = Directory.GetDirectories(basePath)
                        .Where(d => d.Contains("jdk", StringComparison.OrdinalIgnoreCase) || 
                                   d.Contains("jre", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(d => d); // Latest version first

                    foreach (var javaFolder in javaFolders)
                    {
                        var javaExe = Path.Combine(javaFolder, "bin", "java.exe");
                        if (File.Exists(javaExe))
                            return javaExe;
                        
                        var javawExe = Path.Combine(javaFolder, "bin", "javaw.exe");
                        if (File.Exists(javawExe))
                            return javawExe;
                    }
                }
                catch { }
            }

            // 4. Check Windows Registry
            try
            {
                return CheckWindowsRegistry();
            }
            catch { }

            return null;
        }

        private static string CheckWindowsRegistry()
        {
            // Try to find Java via registry using command line
            try
            {
                var registryPaths = new[]
                {
                    @"HKLM\SOFTWARE\JavaSoft\Java Runtime Environment",
                    @"HKLM\SOFTWARE\JavaSoft\Java Development Kit",
                    @"HKLM\SOFTWARE\JavaSoft\JRE",
                    @"HKLM\SOFTWARE\JavaSoft\JDK",
                    @"HKLM\SOFTWARE\Wow6432Node\JavaSoft\Java Runtime Environment",
                    @"HKLM\SOFTWARE\Wow6432Node\JavaSoft\Java Development Kit"
                };

                foreach (var regPath in registryPaths)
                {
                    // Use reg.exe to query registry without Microsoft.Win32 dependency
                    var result = ExecuteCommand($"reg query \"{regPath}\" /v CurrentVersion 2>nul");
                    if (result.Success && !string.IsNullOrWhiteSpace(result.StandardOutput))
                    {
                        // Parse output to find version
                        var lines = result.StandardOutput.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains("CurrentVersion") && line.Contains("REG_SZ"))
                            {
                                var parts = line.Split(new[] { "REG_SZ" }, StringSplitOptions.None);
                                if (parts.Length > 1)
                                {
                                    var version = parts[1].Trim();
                                    
                                    // Query JavaHome for this version
                                    var javaHomeResult = ExecuteCommand($"reg query \"{regPath}\\{version}\" /v JavaHome 2>nul");
                                    if (javaHomeResult.Success && !string.IsNullOrWhiteSpace(javaHomeResult.StandardOutput))
                                    {
                                        var homeLines = javaHomeResult.StandardOutput.Split('\n');
                                        foreach (var homeLine in homeLines)
                                        {
                                            if (homeLine.Contains("JavaHome") && homeLine.Contains("REG_SZ"))
                                            {
                                                var homeParts = homeLine.Split(new[] { "REG_SZ" }, StringSplitOptions.None);
                                                if (homeParts.Length > 1)
                                                {
                                                    var javaHomePath = homeParts[1].Trim();
                                                    var javaExe = Path.Combine(javaHomePath, "bin", "java.exe");
                                                    if (File.Exists(javaExe))
                                                        return javaExe;
                                                    
                                                    var javawExe = Path.Combine(javaHomePath, "bin", "javaw.exe");
                                                    if (File.Exists(javawExe))
                                                        return javawExe;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private static string DiscoverJavaLinux()
        {
            // 1. Check JAVA_HOME environment variable
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java");
                if (File.Exists(javaExe))
                    return javaExe;
            }

            // 2. Check if java is in PATH
            var whichResult = ExecuteCommand("which java");
            if (whichResult.Success && !string.IsNullOrWhiteSpace(whichResult.StandardOutput))
            {
                var javaPath = whichResult.StandardOutput.Trim();
                // Resolve symlinks to get actual path
                var readlinkResult = ExecuteCommand($"readlink -f {javaPath}");
                if (readlinkResult.Success && !string.IsNullOrWhiteSpace(readlinkResult.StandardOutput))
                {
                    return readlinkResult.StandardOutput.Trim();
                }
                return javaPath;
            }

            // 3. Check update-alternatives (Debian/Ubuntu)
            var alternativesResult = ExecuteCommand("update-alternatives --list java 2>/dev/null");
            if (alternativesResult.Success && !string.IsNullOrWhiteSpace(alternativesResult.StandardOutput))
            {
                var paths = alternativesResult.StandardOutput.Split('\n')
                    .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p.Trim()))
                    .Select(p => p.Trim());
                
                if (paths.Any())
                    return paths.First();
            }

            // 4. Check common Linux installation paths
            var commonPaths = new[]
            {
                "/usr/bin/java",
                "/usr/lib/jvm/default-java/bin/java",
                "/usr/lib/jvm/java-11-openjdk-amd64/bin/java",
                "/usr/lib/jvm/java-17-openjdk-amd64/bin/java",
                "/usr/lib/jvm/java-8-openjdk-amd64/bin/java",
                "/usr/local/java/bin/java",
                "/opt/java/bin/java"
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // 5. Check /usr/lib/jvm for any Java installation
            if (Directory.Exists("/usr/lib/jvm"))
            {
                try
                {
                    var jvmDirs = Directory.GetDirectories("/usr/lib/jvm")
                        .Where(d => !d.EndsWith(".old") && !d.Contains("backup"))
                        .OrderByDescending(d => d); // Prefer newer versions

                    foreach (var jvmDir in jvmDirs)
                    {
                        var javaPath = Path.Combine(jvmDir, "bin", "java");
                        if (File.Exists(javaPath))
                            return javaPath;
                    }
                }
                catch { }
            }

            // 6. Check Flatpak
            var flatpakResult = ExecuteCommand("flatpak list --runtime | grep java");
            if (flatpakResult.Success && !string.IsNullOrWhiteSpace(flatpakResult.StandardOutput))
            {
                // Try to find flatpak Java runtime
                var flatpakJavaResult = ExecuteCommand("flatpak run --command=java org.freedesktop.Sdk//21.08 --version 2>/dev/null");
                if (flatpakJavaResult.Success)
                {
                    return "flatpak run --command=java org.freedesktop.Sdk//21.08";
                }
            }

            return null;
        }

        private static string DiscoverJavaMacOS()
        {
            // 1. Check JAVA_HOME environment variable
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java");
                if (File.Exists(javaExe))
                    return javaExe;
            }

            // 2. Use java_home utility (macOS specific)
            var javaHomeResult = ExecuteCommand("/usr/libexec/java_home 2>/dev/null");
            if (javaHomeResult.Success && !string.IsNullOrWhiteSpace(javaHomeResult.StandardOutput))
            {
                var homePath = javaHomeResult.StandardOutput.Trim();
                var javaPath = Path.Combine(homePath, "bin", "java");
                if (File.Exists(javaPath))
                    return javaPath;
            }

            // 3. Check if java is in PATH
            var whichResult = ExecuteCommand("which java");
            if (whichResult.Success && !string.IsNullOrWhiteSpace(whichResult.StandardOutput))
            {
                return whichResult.StandardOutput.Trim();
            }

            // 4. Check common macOS installation paths
            var commonPaths = new[]
            {
                "/usr/bin/java",
                "/System/Library/Frameworks/JavaVM.framework/Versions/Current/Commands/java",
                "/Library/Java/JavaVirtualMachines/default/Contents/Home/bin/java"
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // 5. Check /Library/Java/JavaVirtualMachines
            if (Directory.Exists("/Library/Java/JavaVirtualMachines"))
            {
                try
                {
                    var jdkDirs = Directory.GetDirectories("/Library/Java/JavaVirtualMachines")
                        .OrderByDescending(d => d); // Prefer newer versions

                    foreach (var jdkDir in jdkDirs)
                    {
                        var javaPath = Path.Combine(jdkDir, "Contents", "Home", "bin", "java");
                        if (File.Exists(javaPath))
                            return javaPath;
                    }
                }
                catch { }
            }

            // 6. Check Homebrew installation
            var brewPaths = new[]
            {
                "/opt/homebrew/opt/openjdk/bin/java",
                "/usr/local/opt/openjdk/bin/java",
                "/opt/homebrew/opt/java/bin/java",
                "/usr/local/opt/java/bin/java"
            };

            foreach (var path in brewPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        /// <summary>
        /// Verifies if the discovered Java path is valid and returns version info
        /// </summary>
        public static JavaInfo GetJavaInfo(string javaPath = null)
        {
            if (string.IsNullOrEmpty(javaPath))
            {
                javaPath = DiscoverJavaPath();
            }

            if (string.IsNullOrEmpty(javaPath))
            {
                return new JavaInfo { IsValid = false };
            }

            try
            {
                var result = ExecuteCommand($"\"{javaPath}\" -version");
                
                // Java outputs version info to stderr, not stdout
                var versionOutput = !string.IsNullOrEmpty(result.StandardError) 
                    ? result.StandardError 
                    : result.StandardOutput;

                if (!string.IsNullOrEmpty(versionOutput))
                {
                    var lines = versionOutput.Split('\n');
                    var versionLine = lines.FirstOrDefault(l => l.Contains("version"));
                    
                    return new JavaInfo
                    {
                        IsValid = true,
                        Path = javaPath,
                        Version = versionLine ?? versionOutput,
                        FullOutput = versionOutput
                    };
                }
            }
            catch { }

            return new JavaInfo { IsValid = false, Path = javaPath };
        }
        /// <summary>
        /// Simple command execution helper
        /// </summary>
        private static ProcessResult ExecuteCommand(string command, int timeoutMs = 5000)
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
    }

    /// <summary>
    /// Java installation information
    /// </summary>
    public class JavaInfo
    {
        public bool IsValid { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public string FullOutput { get; set; }
    }

    /// <summary>
    /// Result of a process execution
    /// </summary>
    internal class ProcessResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
    }
}