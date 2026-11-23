using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MdExplorer.Services.Diagnostics
{
    /// <summary>
    /// La memoria della macchina, letta dal sistema: Windows con <c>GlobalMemoryStatusEx</c> (gli stessi numeri di
    /// Gestione attività), Linux da <c>/proc/meminfo</c>. Altrove, o se la lettura non riesce, restano il totale e la
    /// percentuale che dà il runtime .NET, e gli altri valori sono null: non si inventano.
    /// </summary>
    public static class MachineMemoryReader
    {
        private const long Mb = 1024 * 1024;

        public static MachineMemory Read()
        {
            try
            {
                if (OperatingSystem.IsWindows()) return Windows() ?? Runtime();
                if (OperatingSystem.IsLinux() && File.Exists("/proc/meminfo")) return FromMemInfo(File.ReadAllText("/proc/meminfo")) ?? Runtime();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is EntryPointNotFoundException || ex is DllNotFoundException)
            {
                // Si ripiega su ciò che dà il runtime: meno dettaglio, dichiarato dai null.
            }
            return Runtime();
        }

        private static MachineMemory Runtime()
        {
            var info = GC.GetGCMemoryInfo();
            return new MachineMemory
            {
                TotalMb = info.TotalAvailableMemoryBytes / Mb,
                LoadPercent = info.TotalAvailableMemoryBytes <= 0 ? 0 : (int)Math.Round(100.0 * info.MemoryLoadBytes / info.TotalAvailableMemoryBytes),
            };
        }

        /// <summary>Le righe di <c>/proc/meminfo</c> che servono, in kB. Null se manca il totale.</summary>
        public static MachineMemory FromMemInfo(string text)
        {
            long? Value(string key)
            {
                foreach (var line in text.Split('\n'))
                {
                    if (!line.StartsWith(key + ":", StringComparison.Ordinal)) continue;
                    var parts = line.Substring(key.Length + 1).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 0 && long.TryParse(parts[0], out var kb) ? kb : (long?)null;
                }
                return null;
            }

            var total = Value("MemTotal");
            if (total == null || total <= 0) return null;
            var available = Value("MemAvailable");
            var swapTotal = Value("SwapTotal");
            var swapFree = Value("SwapFree");
            return new MachineMemory
            {
                TotalMb = total.Value / 1024,
                FreeMb = available / 1024,
                LoadPercent = available == null ? 0 : (int)Math.Round(100.0 * (total.Value - available.Value) / total.Value),
                SwapTotalMb = swapTotal / 1024,
                SwapUsedMb = swapTotal != null && swapFree != null ? (swapTotal - swapFree) / 1024 : null,
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryStatusEx
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

        private static MachineMemory Windows()
        {
            var status = new MemoryStatusEx { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
            if (!GlobalMemoryStatusEx(ref status)) return null;
            // ullTotalPageFile è il limite della memoria impegnata (RAM + file di paging), non la dimensione del file.
            return new MachineMemory
            {
                TotalMb = (long)(status.ullTotalPhys / (ulong)Mb),
                FreeMb = (long)(status.ullAvailPhys / (ulong)Mb),
                LoadPercent = (int)status.dwMemoryLoad,
                CommitLimitMb = (long)(status.ullTotalPageFile / (ulong)Mb),
                CommitUsedMb = (long)((status.ullTotalPageFile - status.ullAvailPageFile) / (ulong)Mb),
            };
        }
    }
}
