using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            try
            {
                var pids = GetDescendantPids(rootPid);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return FindListeningPortForPids(pids, logger);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return FindListeningPortForPidsLinux(pids, logger);
                return null; // macOS: not supported (no /proc, no toolhelp32)
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "[ListeningPortDetector] Detection failed for pid {Pid}", rootPid);
                return null;
            }
        }

        /// <summary>
        /// All PIDs in the process tree rooted at <paramref name="rootPid"/> (inclusive).
        /// Windows: Toolhelp32 snapshot. Linux: /proc PPid walk. Other: just the root.
        /// Used both for port detection and for tree-kill of a rediscovered service.
        /// </summary>
        public static HashSet<int> GetDescendantPids(int rootPid)
        {
            if (rootPid <= 0) return new HashSet<int>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return BuildDescendantPidSet(rootPid);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return BuildDescendantPidSetLinux(rootPid);
            return new HashSet<int> { rootPid };
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

        #region Linux (/proc)

        // Build the descendant set from /proc/<pid>/status "PPid:" lines (robust against
        // process names containing spaces/parens, unlike field-splitting /proc/<pid>/stat).
        private static HashSet<int> BuildDescendantPidSetLinux(int rootPid)
        {
            var result = new HashSet<int> { rootPid };
            var childrenByParent = new Dictionary<int, List<int>>();

            foreach (var dir in Directory.EnumerateDirectories("/proc"))
            {
                var name = Path.GetFileName(dir);
                if (!int.TryParse(name, out var pid)) continue; // skip non-PID entries
                var ppid = ReadPpidLinux(pid);
                if (ppid < 0) continue;
                if (!childrenByParent.TryGetValue(ppid, out var list))
                {
                    list = new List<int>();
                    childrenByParent[ppid] = list;
                }
                list.Add(pid);
            }

            var queue = new Queue<int>();
            queue.Enqueue(rootPid);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!childrenByParent.TryGetValue(current, out var children)) continue;
                foreach (var child in children)
                    if (result.Add(child)) queue.Enqueue(child);
            }
            return result;
        }

        private static int ReadPpidLinux(int pid)
        {
            try
            {
                foreach (var line in File.ReadLines($"/proc/{pid}/status"))
                {
                    if (line.StartsWith("PPid:", StringComparison.Ordinal))
                    {
                        var v = line.Substring(5).Trim();
                        return int.TryParse(v, out var ppid) ? ppid : -1;
                    }
                }
            }
            catch { /* process vanished or unreadable */ }
            return -1;
        }

        // Listening sockets owned by any PID in the set, via /proc/net/tcp{,6} (LISTEN = state 0A)
        // cross-referenced with the socket inodes held in each PID's /proc/<pid>/fd.
        private static int? FindListeningPortForPidsLinux(HashSet<int> pids, ILogger logger)
        {
            var ourInodes = CollectSocketInodes(pids);
            if (ourInodes.Count == 0) return null;

            int? best = null;
            foreach (var path in new[] { "/proc/net/tcp", "/proc/net/tcp6" })
            {
                try
                {
                    if (!File.Exists(path)) continue;
                    var lines = File.ReadLines(path);
                    var first = true;
                    foreach (var raw in lines)
                    {
                        if (first) { first = false; continue; } // header
                        var parts = raw.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        // sl local_address rem_address st tx_queue rx_queue tr tm->when retrnsmt uid timeout inode ...
                        if (parts.Length < 10) continue;
                        if (!parts[3].Equals("0A", StringComparison.OrdinalIgnoreCase)) continue; // LISTEN
                        if (!long.TryParse(parts[9], out var inode)) continue;
                        if (!ourInodes.Contains(inode)) continue;

                        var port = ParseHexPort(parts[1]);
                        if (port == null) continue;
                        if (best == null || port.Value < best.Value) best = port.Value; // lowest = user-facing
                    }
                }
                catch (Exception ex) { logger?.LogDebug(ex, "[ListeningPortDetector] read {Path} failed", path); }
            }
            return best;
        }

        private static HashSet<long> CollectSocketInodes(HashSet<int> pids)
        {
            var inodes = new HashSet<long>();
            foreach (var pid in pids)
            {
                var fdDir = $"/proc/{pid}/fd";
                IEnumerable<string> fds;
                try { fds = Directory.EnumerateFileSystemEntries(fdDir); }
                catch { continue; }
                foreach (var fd in fds)
                {
                    try
                    {
                        // The fd is a symlink; its target looks like "socket:[123456]".
                        var target = new FileInfo(fd).LinkTarget ?? File.ResolveLinkTarget(fd, false)?.FullName;
                        if (string.IsNullOrEmpty(target)) continue;
                        const string prefix = "socket:[";
                        var idx = target.IndexOf(prefix, StringComparison.Ordinal);
                        if (idx < 0) continue;
                        var start = idx + prefix.Length;
                        var end = target.IndexOf(']', start);
                        if (end < 0) continue;
                        if (long.TryParse(target.Substring(start, end - start), out var inode))
                            inodes.Add(inode);
                    }
                    catch { /* fd vanished */ }
                }
            }
            return inodes;
        }

        // /proc/net/tcp local_address is "HEXIP:HEXPORT" e.g. "0100007F:1F90" or (tcp6) a 32-hex-char IP.
        private static int? ParseHexPort(string localAddress)
        {
            if (string.IsNullOrEmpty(localAddress)) return null;
            var idx = localAddress.LastIndexOf(':');
            if (idx < 0 || idx == localAddress.Length - 1) return null;
            var hex = localAddress.Substring(idx + 1);
            return int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var port) ? port : (int?)null;
        }

        #endregion
    }
}
