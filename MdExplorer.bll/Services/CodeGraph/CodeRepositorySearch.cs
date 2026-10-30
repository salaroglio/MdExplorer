using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Dove, nel repository, potrebbe stare un tipo che il grafo conosce solo per nome (D43 F3).</summary>
    public sealed class CodeDefinitionHit
    {
        public string Type { get; init; }
        /// <summary>
        /// <c>source</c> (sorgente col percorso del package), <c>source-same-name</c> (solo lo stesso nome di file),
        /// <c>compiled</c> (un <c>.class</c>), <c>jar</c> (dentro un archivio), <c>config</c> (nominato in un file di configurazione).
        /// </summary>
        public string Kind { get; init; }
        /// <summary>Il file trovato, assoluto; per un jar l'archivio.</summary>
        public string File { get; init; }
        /// <summary>La voce dentro il jar, o la riga della configurazione.</summary>
        public string Detail { get; init; }
        /// <summary>Il file sta dentro una cartella program: avrebbe dovuto essere analizzato.</summary>
        public bool InProgram { get; init; }
        /// <summary>Che cosa fare perché il grafo lo veda, in parole semplici.</summary>
        public string Remedy { get; init; }
    }

    /// <summary>
    /// Cerca nel repository le definizioni dei tipi esterni che il grafo non ha analizzato (D43 F3, utente 30/10/2026:
    /// «indicare in quali punti questa informazione è presente nel repository e però NON è stata analizzata dagli ast e
    /// dalla descrizione degli llm»). Una passata sui file delle radici: sorgenti con quel nome, <c>.class</c>, voci dei
    /// jar (si legge solo l'indice dell'archivio), file di configurazione che nominano il tipo. Non entra in <c>.git</c>,
    /// <c>.svn</c>, <c>node_modules</c> e nelle cartelle di MdExplorer; entra in <c>target</c>, <c>build</c>, <c>lib</c>:
    /// lì stanno proprio jar e classi compilate.
    /// </summary>
    public static class CodeRepositorySearch
    {
        public const int MaxHitsPerType = 8;
        public const long MaxConfigBytes = 2 * 1024 * 1024;

        private static readonly HashSet<string> Skipped = new(StringComparer.OrdinalIgnoreCase)
            { ".git", ".svn", ".hg", "node_modules", ".mde-code", ".md", ".mde-doc", ".vs", ".idea", ".gradle", "__pycache__", ".angular", ".worktrees" };
        private static readonly HashSet<string> SourceExtensions = new(StringComparer.OrdinalIgnoreCase)
            { ".java", ".kt", ".scala", ".groovy", ".cs", ".vb", ".ts", ".js", ".py" };
        private static readonly HashSet<string> Archives = new(StringComparer.OrdinalIgnoreCase) { ".jar", ".war", ".ear", ".zip" };
        private static readonly HashSet<string> ConfigExtensions = new(StringComparer.OrdinalIgnoreCase)
            { ".xml", ".properties", ".yml", ".yaml", ".json", ".conf", ".cfg", ".ini", ".gradle", ".kts" };

        public static List<CodeDefinitionHit> Search(IEnumerable<string> roots, IReadOnlyCollection<string> types, Func<string, bool> inProgram, CancellationToken ct = default)
        {
            var wanted = types.Where(t => !string.IsNullOrWhiteSpace(t) && t != CodeExternals.UnknownOwner).Distinct(StringComparer.Ordinal).ToList();
            var hits = new List<CodeDefinitionHit>();
            if (wanted.Count == 0) return hits;
            // Outer.Inner può stare in Outer.java (o Outer$Inner.class): si cerca il nome di file più esterno che abbia la maiuscola
            var byFileStem = new Dictionary<string, List<(string Type, string PackagePath)>>(StringComparer.Ordinal);
            var byClassEntry = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var t in wanted)
            {
                var parts = t.Split('.');
                var first = Array.FindIndex(parts, p => p.Length > 0 && char.IsUpper(p[0]));
                if (first < 0) first = parts.Length - 1;
                var stem = parts[first];
                var packagePath = string.Join("/", parts.Take(first));
                if (!byFileStem.TryGetValue(stem, out var l)) byFileStem[stem] = l = new();
                l.Add((t, packagePath));
                byClassEntry[(packagePath.Length == 0 ? "" : packagePath + "/") + string.Join("$", parts.Skip(first)) + ".class"] = t;
            }
            var count = wanted.ToDictionary(t => t, _ => 0, StringComparer.Ordinal);
            void Add(CodeDefinitionHit h)
            {
                if (count[h.Type] >= MaxHitsPerType) return;
                count[h.Type]++;
                hits.Add(h);
            }

            foreach (var root in roots.Where(Directory.Exists).Select(Path.GetFullPath).Distinct(StringComparer.Ordinal))
            {
                var stack = new Stack<string>();
                stack.Push(root);
                while (stack.Count > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    var dir = stack.Pop();
                    IEnumerable<string> entries;
                    try { entries = Directory.EnumerateFileSystemEntries(dir).ToList(); }
                    catch (Exception ex) when (ex is UnauthorizedAccessException or IOException) { continue; }
                    foreach (var e in entries)
                    {
                        if (Directory.Exists(e)) { if (!Skipped.Contains(Path.GetFileName(e))) stack.Push(e); continue; }
                        var ext = Path.GetExtension(e);
                        var unix = e.Replace('\\', '/');
                        if (SourceExtensions.Contains(ext) && byFileStem.TryGetValue(Path.GetFileNameWithoutExtension(e), out var candidates))
                            foreach (var (type, packagePath) in candidates)
                            {
                                var exact = packagePath.Length == 0 || Path.GetDirectoryName(unix)!.Replace('\\', '/').EndsWith("/" + packagePath, StringComparison.Ordinal);
                                var program = inProgram(e);
                                Add(new CodeDefinitionHit { Type = type, Kind = exact ? "source" : "source-same-name", File = e, InProgram = program, Remedy = SourceRemedy(program, exact) });
                            }
                        else if (ext.Equals(".class", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var kv in byClassEntry.Where(kv => unix.EndsWith("/" + kv.Key, StringComparison.Ordinal)))
                                Add(new CodeDefinitionHit { Type = kv.Value, Kind = "compiled", File = e, InProgram = inProgram(e), Remedy = CompiledRemedy });
                        }
                        else if (Archives.Contains(ext)) SearchArchive(e, byClassEntry, Add, inProgram(e));
                        else if (ConfigExtensions.Contains(ext)) SearchConfig(e, wanted, Add, inProgram(e));
                    }
                }
            }
            return hits.OrderBy(h => h.Type, StringComparer.Ordinal).ThenBy(h => Rank(h.Kind)).ThenBy(h => h.File, StringComparer.Ordinal).ToList();
        }

        private static int Rank(string kind) => kind switch { "source" => 0, "jar" => 1, "compiled" => 2, "source-same-name" => 3, _ => 4 };

        private static void SearchArchive(string file, Dictionary<string, string> byClassEntry, Action<CodeDefinitionHit> add, bool program)
        {
            try
            {
                using var zip = ZipFile.OpenRead(file);
                foreach (var entry in zip.Entries)
                {
                    var name = entry.FullName.Replace('\\', '/');
                    // un war/ear ha le classi sotto WEB-INF/classes/ o dentro jar annidati (non si aprono: si dice il jar)
                    foreach (var kv in byClassEntry.Where(kv => name == kv.Key || name.EndsWith("/" + kv.Key, StringComparison.Ordinal)))
                        add(new CodeDefinitionHit { Type = kv.Value, Kind = "jar", File = file, Detail = name, InProgram = program, Remedy = CompiledRemedy });
                }
            }
            catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException) { }   // non è un archivio leggibile: non è una prova
        }

        private static void SearchConfig(string file, List<string> wanted, Action<CodeDefinitionHit> add, bool program)
        {
            try
            {
                if (new FileInfo(file).Length > MaxConfigBytes) return;
                var lines = File.ReadAllLines(file);
                foreach (var t in wanted)
                {
                    var i = Array.FindIndex(lines, l => l.Contains(t, StringComparison.Ordinal));
                    if (i >= 0)
                        add(new CodeDefinitionHit { Type = t, Kind = "config", File = file, Detail = $"{i + 1}: {lines[i].Trim()}", InProgram = program,
                            Remedy = UiText.Say("nominato in configurazione: il legame è dichiarato lì (una regola o un dialetto può leggerlo); la classe va cercata altrove",
                                "named in configuration: the link is declared there (a rule or a dialect can read it); the class itself is elsewhere") });
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }

        private static string SourceRemedy(bool program, bool exact) => program
            ? UiText.Say("il sorgente è dentro una cartella program ma non è nel grafo: il suo linguaggio non è scelto, o il file è escluso; «Ambiente di sviluppo» della cartella",
                "the source is inside a program folder but not in the graph: its language is not chosen, or the file is excluded; «Development environment» of the folder")
            : exact
                ? UiText.Say("il sorgente c'è, fuori dalle cartelle program: marca la cartella program, o collegala come radice esterna (@alias), e rifai «Estrai la struttura»",
                    "the source is there, outside the program folders: mark its folder as program, or link it as an external root (@alias), then «Extract the structure»")
                : UiText.Say("un file con lo stesso nome, in un altro package: forse è lui (copia, package cambiato), da verificare",
                    "a file with the same name, in another package: maybe it is the one (a copy, a moved package), to be checked");

        private static string CompiledRemedy => UiText.Say(
            "c'è solo compilato: il grafo non legge i .class né i jar; serve il sorgente (radice esterna) o un'analisi ulteriore delle firme dal bytecode",
            "only compiled: the graph does not read .class files or jars; it needs the source (external root) or a further analysis of the signatures from the bytecode");
    }
}
