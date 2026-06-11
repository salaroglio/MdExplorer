using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace MdExplorer.Services.Execution
{
    /// <summary>
    /// Shared helpers for spawning shell processes. Used by both <see cref="ShellRunner"/>
    /// (one-shot batch runs) and <see cref="ServiceRunner"/> (long-running services) so the
    /// environment-overlay and working-directory logic stays in one place.
    /// </summary>
    public static class ShellEnvironment
    {
        /// <summary>
        /// Reads Machine + User scope environment variables from the Windows registry
        /// (HKLM\...\Environment and HKCU\Environment) and overlays them into
        /// <paramref name="psi"/>.Environment. PATH from both scopes is concatenated
        /// (Machine first, then User), other variables are overridden User-over-Machine.
        /// This guarantees the child process always sees the *current* env at spawn time,
        /// regardless of what MDE inherited at its own launch.
        /// Failures are logged and swallowed so the spawn still proceeds with the
        /// inherited env as a graceful degradation.
        /// </summary>
        public static void RefreshEnvironmentFromRegistry(ProcessStartInfo psi, ILogger logger)
        {
            try
            {
                // Machine scope first — these are defaults
                var machineVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
                string machinePath = null;
                foreach (System.Collections.DictionaryEntry kv in machineVars)
                {
                    var key = kv.Key?.ToString();
                    if (string.IsNullOrEmpty(key)) continue;
                    var value = kv.Value?.ToString() ?? string.Empty;
                    if (string.Equals(key, "Path", StringComparison.OrdinalIgnoreCase))
                    {
                        machinePath = value;
                        psi.Environment["Path"] = value;
                    }
                    else
                    {
                        psi.Environment[key] = value;
                    }
                }

                // User scope — overrides Machine for everything except PATH which is concatenated
                var userVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
                foreach (System.Collections.DictionaryEntry kv in userVars)
                {
                    var key = kv.Key?.ToString();
                    if (string.IsNullOrEmpty(key)) continue;
                    var value = kv.Value?.ToString() ?? string.Empty;
                    if (string.Equals(key, "Path", StringComparison.OrdinalIgnoreCase))
                    {
                        // Concatenate Machine PATH + User PATH (Windows convention).
                        // If Machine PATH was missing, just use User PATH.
                        var combined = string.IsNullOrEmpty(machinePath)
                            ? value
                            : machinePath.TrimEnd(Path.PathSeparator) + Path.PathSeparator + value;
                        psi.Environment["Path"] = combined;
                    }
                    else
                    {
                        psi.Environment[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "[ShellEnvironment] Failed to refresh environment from registry — falling back to inherited env");
            }
        }

        public static string SafeWorkingDirectory(string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory)) return Directory.GetCurrentDirectory();
            try
            {
                return Directory.Exists(workingDirectory) ? workingDirectory : Directory.GetCurrentDirectory();
            }
            catch
            {
                return Directory.GetCurrentDirectory();
            }
        }
    }
}
