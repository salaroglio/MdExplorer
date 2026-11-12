using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Gli avvisi dell'ultima analisi completa di ogni cartella (12/11/2026, rilevazione dal cliente: MarkAgent non li vedeva —
    /// librerie mancanti, progetti riferiti che non ci sono, blocchi che non si chiudono — e il report teneva solo quelli
    /// dell'ultima cartella analizzata). Un file per cartella in <c>.md/codegraph/warnings/</c>, riscritto a ogni estrazione
    /// che ha riletto dei file (una che non rilegge niente non produce avvisi e non cancella quelli di prima): lo leggono il
    /// report, <c>code_inventory</c> e chi apre il file.
    /// </summary>
    public static class CodeAnalysisWarnings
    {
        public sealed class Entry
        {
            public string Folder { get; set; }
            public DateTime At { get; set; }
            /// <summary>Quanti file l'estrazione ha riletto, su quanti della cartella: con un'analisi incrementale gli avvisi
            /// legati a un file sono solo quelli dei file riletti; quelli di progetto (librerie, riferimenti) ci sono sempre.</summary>
            public int Reparsed { get; set; }
            public int Files { get; set; }
            public List<string> Warnings { get; set; } = new();
        }

        public static string FolderPath(string projectPath) => Path.Combine(projectPath, ".md", "codegraph", "warnings");

        /// <summary>Il file di una cartella: il nome viene dall'impronta del percorso (una cartella può avere qualunque nome).</summary>
        private static string FileOf(string projectPath, string folder)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes((folder ?? string.Empty).Replace('\\', '/').ToLowerInvariant())))[..16].ToLowerInvariant();
            return Path.Combine(FolderPath(projectPath), hash + ".json");
        }

        public static void Write(string projectPath, string folder, int reparsed, int files, IEnumerable<string> warnings)
        {
            var path = FileOf(projectPath, folder);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var entry = new Entry { Folder = folder, At = DateTime.UtcNow, Reparsed = reparsed, Files = files, Warnings = warnings.Where(w => !string.IsNullOrWhiteSpace(w)).Distinct().ToList() };
            File.WriteAllText(path, JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
        }

        /// <summary>Gli avvisi dell'ultima analisi completa della cartella; null se non ce n'è ancora una (non «nessun avviso»).</summary>
        public static Entry Read(string projectPath, string folder)
        {
            var path = FileOf(projectPath, folder);
            if (!File.Exists(path)) return null;
            try { return JsonSerializer.Deserialize<Entry>(File.ReadAllText(path)); }
            catch (Exception ex) when (ex is JsonException or IOException)
            {
                return new Entry { Folder = folder, Warnings = { UiText.Say($"avvisi dell'ultima analisi non leggibili ({ex.Message})", $"warnings of the last analysis not readable ({ex.Message})") } };
            }
        }

        public static List<Entry> ReadAll(string projectPath)
        {
            var dir = FolderPath(projectPath);
            if (!Directory.Exists(dir)) return new List<Entry>();
            var list = new List<Entry>();
            foreach (var f in Directory.GetFiles(dir, "*.json").OrderBy(f => f, StringComparer.Ordinal))
            {
                try { if (JsonSerializer.Deserialize<Entry>(File.ReadAllText(f)) is { } e) list.Add(e); }
                catch (Exception ex) when (ex is JsonException or IOException)
                {
                    list.Add(new Entry { Folder = Path.GetFileName(f), Warnings = { UiText.Say($"avvisi non leggibili ({ex.Message})", $"warnings not readable ({ex.Message})") } });
                }
            }
            return list.OrderBy(e => e.Folder, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
