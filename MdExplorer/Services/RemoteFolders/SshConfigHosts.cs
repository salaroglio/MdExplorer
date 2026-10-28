using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// Gli alias di <c>~/.ssh/config</c> (righe <c>Host</c> senza caratteri jolly), per proporli quando si collega una
    /// cartella remota (sprint 2026-10-21-Cartella-Remota-Via-SSH, F3). Segue gli <c>Include</c> (relativi a <c>~/.ssh</c>,
    /// con <c>*</c> nel nome del file). Solo una proposta: l'utente può scrivere qualunque host.
    /// </summary>
    public static class SshConfigHosts
    {
        public static string DefaultConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");

        public static IReadOnlyList<string> Read(string configPath)
        {
            var hosts = new List<string>();
            Collect(configPath, hosts, new HashSet<string>(StringComparer.Ordinal), 0);
            return hosts.Distinct(StringComparer.Ordinal).ToList();
        }

        private static void Collect(string path, List<string> hosts, HashSet<string> seen, int depth)
        {
            if (depth > 8 || !File.Exists(path) || !seen.Add(Path.GetFullPath(path))) return;
            var sshDir = Path.GetDirectoryName(DefaultConfigPath);
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                var parts = line.Split(new[] { ' ', '\t', '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                var keyword = parts[0];
                if (keyword.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    hosts.AddRange(parts.Skip(1).Where(h => h.IndexOfAny(new[] { '*', '?', '!' }) < 0));
                else if (keyword.Equals("Include", StringComparison.OrdinalIgnoreCase))
                    foreach (var pattern in parts.Skip(1))
                        foreach (var file in Expand(pattern, sshDir))
                            Collect(file, hosts, seen, depth + 1);
            }
        }

        private static IEnumerable<string> Expand(string pattern, string sshDir)
        {
            var p = pattern.StartsWith("~/", StringComparison.Ordinal)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), pattern.Substring(2))
                : Path.IsPathRooted(pattern) ? pattern : Path.Combine(sshDir, pattern);
            var dir = Path.GetDirectoryName(p);
            var name = Path.GetFileName(p);
            if (dir == null || !Directory.Exists(dir)) return Enumerable.Empty<string>();
            return name.Contains('*') || name.Contains('?')
                ? Directory.EnumerateFiles(dir, name).OrderBy(f => f, StringComparer.Ordinal)
                : new[] { p };
        }
    }
}
