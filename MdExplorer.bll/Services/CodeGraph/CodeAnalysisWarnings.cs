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
            /// <summary>
            /// Il tipo di ogni avviso (<see cref="CodeWarnings"/>), nello stesso ordine di <see cref="Warnings"/> (14/11/2026); null
            /// nei file scritti prima dei tipi: allora di quegli avvisi il tipo non si sa.
            /// </summary>
            public List<string> Kinds { get; set; }

            /// <summary>Il tipo dell'avviso alla posizione data; null se il file è di prima dei tipi.</summary>
            public string KindAt(int index) => Kinds != null && index < Kinds.Count ? Kinds[index] : null;
        }

        public static string FolderPath(string projectPath) => Path.Combine(projectPath, ".md", "codegraph", "warnings");

        /// <summary>Il file di una cartella: il nome viene dall'impronta del percorso (una cartella può avere qualunque nome).</summary>
        private static string FileOf(string projectPath, string folder)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes((folder ?? string.Empty).Replace('\\', '/').ToLowerInvariant())))[..16].ToLowerInvariant();
            return Path.Combine(FolderPath(projectPath), hash + ".json");
        }

        public static void Write(string projectPath, string folder, int reparsed, int files, CodeWarnings warnings)
        {
            var path = FileOf(projectPath, folder);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            // lo stesso testo una volta sola, col tipo della prima volta
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var kept = warnings.Typed.Where(w => !string.IsNullOrWhiteSpace(w.Text) && seen.Add(w.Text)).ToList();
            var entry = new Entry
            {
                Folder = folder, At = DateTime.UtcNow, Reparsed = reparsed, Files = files,
                Warnings = kept.Select(w => w.Text).ToList(), Kinds = kept.Select(w => w.Kind).ToList(),
            };
            File.WriteAllText(path, JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
        }

        /// <summary>Avvisi senza tipo (testi e basta): finiscono tutti in <see cref="CodeWarnings.Other"/>.</summary>
        public static void Write(string projectPath, string folder, int reparsed, int files, IEnumerable<string> warnings)
        {
            var typed = new CodeWarnings();
            typed.AddRange(warnings);
            Write(projectPath, folder, reparsed, files, typed);
        }

        /// <summary>Un tipo di avviso nella sintesi: quanti sono e uno d'esempio (il primo), accorciato se è lungo — è un esempio, l'elenco intero si chiede per tipo.</summary>
        public sealed record KindSummary(string Kind, int Count, string Example);

        /// <summary>Quanto è lungo al massimo l'esempio di un tipo: un avviso con un elenco di file può essere di migliaia di caratteri.</summary>
        public const int ExampleChars = 300;

        /// <summary>Il tipo usato nella sintesi per gli avvisi scritti prima dei tipi: di loro il tipo non si sa.</summary>
        public const string KindNotRecorded = "kind-not-recorded";

        /// <summary>
        /// La sintesi degli avvisi di una cartella (14/11/2026, utente: «anziché una lista sarebbe meglio dargli una sintesi,
        /// perché in maniera progressiva sappia come esplorare»): per tipo, quanti e un esempio; prima i più numerosi. I conti
        /// sono di tutti gli avvisi.
        /// </summary>
        public static List<KindSummary> Summary(Entry entry)
            => entry.Warnings.Select((text, i) => (Kind: entry.KindAt(i) ?? KindNotRecorded, Text: text))
                .GroupBy(w => w.Kind, StringComparer.Ordinal)
                .Select(g =>
                {
                    var first = g.First().Text;
                    var example = first.Length <= ExampleChars ? first
                        : first[..ExampleChars] + UiText.Say($"… (+{first.Length - ExampleChars} caratteri: è un esempio accorciato)", $"… (+{first.Length - ExampleChars} characters: a shortened example)");
                    return new KindSummary(g.Key, g.Count(), example);
                })
                .OrderByDescending(k => k.Count).ThenBy(k => k.Kind, StringComparer.Ordinal).ToList();

        /// <summary>Gli avvisi di un tipo, interi; <c>all</c> = tutti.</summary>
        public static List<string> OfKind(Entry entry, string kind)
            => entry.Warnings.Where((_, i) => kind == "all" || (entry.KindAt(i) ?? KindNotRecorded) == kind).ToList();

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
