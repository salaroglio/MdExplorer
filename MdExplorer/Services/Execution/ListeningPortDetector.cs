using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MdExplorer.Services.Execution
{
    /// <summary>
    /// Best-effort detection of the TCP port a started service is listening on.
    ///
    /// The PID we record for a service is the *shell* (cmd/bash/pwsh); the actual server
    /// (node, dotnet, python, …) is a descendant. So we build the descendant-PID set rooted
    /// at the shell PID and then look for a LISTENING socket owned by any PID in that set.
    ///
    /// Windows-first: process tree via the Toolhelp32 snapshot (no extra NuGet dependency)
    /// and listening sockets via <c>netstat -ano</c>. On non-Windows it returns null (TODO).
    /// </summary>
    public static class ListeningPortDetector
    {
        public static int? DetectListeningPort(int rootPid, ILogger logger = null)
        {
            if (rootPid <= 0) return null;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // TODO: Linux/macOS support (ss -ltnp / lsof). Windows-first for now.
                return null;
            }

            try
            {
                var pids = BuildDescendantPidSet(rootPid);
                return FindListeningPortForPids(pids, logger);
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "[ListeningPortDetector] Detection failed for pid {Pid}", rootPid);
                return null;
            }
        }

        #region process tree (Toolhelp32)

        private const uint TH32CS_SNAPPROCESS = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static HashSet<int> BuildDescendantPidSet(int rootPid)
        {
            var result = new HashSet<int> { rootPid };

            // parent -> children map of the whole process table (point-in-time snapshot)
            var childrenByParent = new Dictionary<int, List<int>>();
            var snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (snapshot == IntPtr.Zero || snapshot == new IntPtr(-1)) return result;
            try
            {
                var entry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>() };
                if (!Process32First(snapshot, ref entry)) return result;
                do
                {
                    var pid = (int)entry.th32ProcessID;
                    var parent = (int)entry.th32ParentProcessID;
                    if (!childrenByParent.TryGetValue(parent, out var list))
                    {
                        list = new List<int>();
                        childrenByParent[parent] = list;
                    }
                    list.Add(pid);
                }
                while (Process32Next(snapshot, ref entry));
            }
            finally
            {
                CloseHandle(snapshot);
            }

            // BFS from root collecting all descendants
            var queue = new Queue<int>();
            queue.Enqueue(rootPid);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!childrenByParent.TryGetValue(current, out var children)) continue;
                foreach (var child in children)
                {
                    if (result.Add(child)) queue.Enqueue(child);
                }
            }
            return result;
        }

        #endregion

        #region listening sockets (netstat)

        private static int? FindListeningPortForPids(HashSet<int> pids, ILogger logger)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netstat.exe",
                Arguments = "-ano -p TCP",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            if (proc == null) return null;
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(3000);

            int? best = null;
            using var reader = new System.IO.StringReader(output);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Columns: Proto  Local Address  Foreign Address  State  PID
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5) continue;
                if (!parts[0].Equals("TCP", StringComparison.OrdinalIgnoreCase)) continue;
                if (!parts[3].Equals("LISTENING", StringComparison.OrdinalIgnoreCase)) continue;
                if (!int.TryParse(parts[4], out var pid)) continue;
                if (!pids.Contains(pid)) continue;

                var port = ParsePort(parts[1]);
                if (port == null) continue;

                // Prefer the lowest port (heuristic: the user-facing server port over ephemeral ones).
                if (best == null || port.Value < best.Value) best = port.Value;
            }
            return best;
        }

        private static int? ParsePort(string localAddress)
        {
            if (string.IsNullOrEmpty(localAddress)) return null;
            // Handles "0.0.0.0:8765", "[::]:8765", "127.0.0.1:8765"
            var idx = localAddress.LastIndexOf(':');
            if (idx < 0 || idx == localAddress.Length - 1) return null;
            return int.TryParse(localAddress.Substring(idx + 1), out var port) ? port : (int?)null;
        }

        #endregion
    }
}
