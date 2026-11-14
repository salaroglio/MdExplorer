using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>La chiusura di un seme come l'utente l'ha confermata l'ultima volta (F3b).</summary>
    public sealed class CodeSeedClosure
    {
        public string Seed { get; set; }
        public string Language { get; set; }
        public List<string> Files { get; set; } = new();
        public string ConfirmedAt { get; set; }
    }

    /// <summary>
    /// Le chiusure confermate dei semi, su QUESTA macchina (F3b, S14): non vanno in Git — si ricalcolano dai semi del
    /// <c>.development.yml</c> — ma servono per dire «che cosa è cambiato» e perché la pulizia di un altro contesto non tolga i
    /// file di un seme (F1b). Un file per progetto nella cartella dati di MDE, accanto ai database del motore; un dato ricavato,
    /// che se manca si rifà alla prossima analisi del seme.
    /// </summary>
    public sealed class CodeSeedStore
    {
        private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly object _gate = new();

        private static string FileOf(string projectPath)
        {
            var normalized = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized.ToLowerInvariant()))).Substring(0, 16).ToLowerInvariant();
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MdExplorer", "code-seeds", hash + ".json");
        }

        public IReadOnlyDictionary<string, CodeSeedClosure> All(string projectPath)
        {
            lock (_gate)
            {
                var file = FileOf(projectPath);
                if (!File.Exists(file)) return new Dictionary<string, CodeSeedClosure>(StringComparer.Ordinal);
                var list = JsonSerializer.Deserialize<List<CodeSeedClosure>>(File.ReadAllText(file), Json) ?? new List<CodeSeedClosure>();
                return list.Where(c => c.Seed != null).GroupBy(c => c.Seed).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
            }
        }

        public CodeSeedClosure Get(string projectPath, string seed) => All(projectPath).TryGetValue(seed, out var c) ? c : null;

        public void Save(string projectPath, CodeSeedClosure closure) => Update(projectPath, all => all[closure.Seed] = closure);

        public void Remove(string projectPath, string seed) => Update(projectPath, all => all.Remove(seed));

        private void Update(string projectPath, Action<Dictionary<string, CodeSeedClosure>> change)
        {
            lock (_gate)
            {
                var all = new Dictionary<string, CodeSeedClosure>(All(projectPath), StringComparer.Ordinal);
                change(all);
                var file = FileOf(projectPath);
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                var temp = file + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(all.Values.OrderBy(c => c.Seed, StringComparer.Ordinal).ToList(), Json));
                File.Move(temp, file, overwrite: true);
            }
        }
    }
}
