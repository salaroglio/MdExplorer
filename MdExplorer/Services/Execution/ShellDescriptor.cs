using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MdExplorer.Services.Execution
{
    /// <summary>
    /// Describes how to invoke a shell to execute a script file.
    /// Adding a new shell = add a new entry to <see cref="ShellRegistry"/>.
    /// </summary>
    public record ShellDescriptor(
        string Language,
        string Executable,
        string ScriptExtension,
        Func<string, string> BuildArgs,
        bool NormalizeToLf);

    /// <summary>
    /// Registry of known shells, keyed by fenced-code language (case-insensitive).
    /// Aliases (bash/sh/shell → same descriptor; powershell/pwsh → same) live here.
    /// </summary>
    public class ShellRegistry
    {
        private readonly Dictionary<string, ShellDescriptor> _byLanguage;

        public ShellRegistry()
        {
            _byLanguage = BuildDefaults();
        }

        public bool TryGet(string language, out ShellDescriptor descriptor)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                descriptor = null;
                return false;
            }
            return _byLanguage.TryGetValue(language.Trim(), out descriptor);
        }

        public IEnumerable<string> SupportedLanguages => _byLanguage.Keys;

        private static Dictionary<string, ShellDescriptor> BuildDefaults()
        {
            var bashExe = ResolveBashExecutable();
            var pwshExe = ResolvePwshExecutable();
            var cmdExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/sh";

            var bash = new ShellDescriptor(
                Language: "bash",
                Executable: bashExe,
                ScriptExtension: ".sh",
                BuildArgs: scriptPath => $"\"{scriptPath}\"",
                NormalizeToLf: true);

            var powershell = new ShellDescriptor(
                Language: "powershell",
                Executable: pwshExe,
                ScriptExtension: ".ps1",
                BuildArgs: scriptPath => $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                NormalizeToLf: false);

            var cmd = new ShellDescriptor(
                Language: "cmd",
                Executable: cmdExe,
                ScriptExtension: ".cmd",
                BuildArgs: scriptPath => $"/c \"{scriptPath}\"",
                NormalizeToLf: false);

            var result = new Dictionary<string, ShellDescriptor>(StringComparer.OrdinalIgnoreCase)
            {
                ["bash"] = bash,
                ["sh"] = bash,
                ["shell"] = bash,
                ["powershell"] = powershell,
                ["pwsh"] = powershell,
                ["ps1"] = powershell,
                ["cmd"] = cmd,
                ["bat"] = cmd,
                ["batch"] = cmd,
            };
            return result;
        }

        private static string ResolveBashExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var candidates = new[]
                {
                    @"C:\Program Files\Git\bin\bash.exe",
                    @"C:\Program Files (x86)\Git\bin\bash.exe",
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Programs", "Git", "bin", "bash.exe"),
                };
                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate)) return candidate;
                }
                var onPath = FindOnPath("bash.exe");
                return onPath ?? "bash.exe";
            }
            return "/bin/bash";
        }

        private static string ResolvePwshExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return FindOnPath("pwsh.exe")
                       ?? FindOnPath("powershell.exe")
                       ?? "powershell.exe";
            }
            return FindOnPath("pwsh") ?? "pwsh";
        }

        private static string FindOnPath(string executable)
        {
            var pathVar = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVar)) return null;
            foreach (var directory in pathVar.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(directory)) continue;
                try
                {
                    var full = Path.Combine(directory, executable);
                    if (File.Exists(full)) return full;
                }
                catch { /* invalid path entry, skip */ }
            }
            return null;
        }
    }
}
