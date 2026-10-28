using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'identità di un simbolo nasce dal FILE che lo dichiara (G4 del piano 2026-10-27-Grafo-Di-Progetti-Enormi): l'id
    /// dell'estrattore (<c>hash(linguaggio|tipo|nome qualificato)</c>, la «chiave») diventa <c>hash(file|percorso logico|chiave)</c>.
    /// Prima (punto 3a del 14/10) nella chiave entrava la cartella <c>program</c>: con i contesti marcati e smarcati sopra
    /// una radice enorme lo stesso file avrebbe avuto id diversi. Il percorso logico (<c>program/…</c>, <c>@alias/…</c>) è
    /// uguale su ogni macchina: gli script vanno su Git. Utente (27/10): «così non c'è modo di sbagliarsi».
    /// <para>
    /// Una chiave dichiarata in PIÙ file (classe <c>partial</c>, duplicato nello stesso contesto) e gli esterni restano legati
    /// al nome (la chiave così com'è): un nodo solo. Gli estrattori non cambiano: tutto si decide qui, dopo l'estrazione.
    /// </para>
    /// </summary>
    public static class CodeFileScope
    {
        /// <summary>
        /// In coda alla versione dell'emettitore (<see cref="CodeBodyHash.EmitterVersion"/>): gli script senza hanno gli id
        /// della cartella program (<see cref="LegacyProgramId"/>), risultano «di un'altra versione» e si rifanno.
        /// </summary>
        public const string EmitterSuffix = "+file1";

        public static string FileId(string logicalPath, string key)
        {
            if (string.IsNullOrWhiteSpace(logicalPath))
                throw new InvalidOperationException(UiText.Say("L'id di un simbolo richiede il file che lo dichiara: logicalPath è vuoto.", "A symbol id needs the file declaring it: logicalPath is empty."));
            return Hash($"file|{logicalPath}|{key}");
        }

        /// <summary>L'id di prima del 27/10 (cartella program + chiave): serve solo a migrare le descrizioni.</summary>
        public static string LegacyProgramId(string programFolder, string key) => Hash($"program|{programFolder}|{key}");

        /// <summary>Uno script AST scritto prima degli id per file (versione dell'emettitore senza <see cref="EmitterSuffix"/>).</summary>
        public static bool IsLegacy(string emitterVersion) => emitterVersion == null || !emitterVersion.EndsWith(EmitterSuffix, StringComparison.Ordinal);

        /// <summary>
        /// Riscrive tutti gli id dello script (simboli, genitori, parametri, archi, punti di chiamata, descrizioni) con quelli
        /// dati da <paramref name="declarations"/>. Con <paramref name="legacyProgramFolder"/> (lo script su disco ha gli id
        /// vecchi) annota in <paramref name="renames"/> id vecchio → nuovo di ogni simbolo dello script, per le descrizioni.
        /// Torna quante destinazioni non sono né dichiarate in un file né esterne: archi verso nodi che non esistono.
        /// </summary>
        public static int Apply(CodeScript script, CodeDeclarations declarations, string legacyProgramFolder = null, IDictionary<string, string> renames = null)
        {
            if (legacyProgramFolder != null && renames != null)
                foreach (var s in script.Symbols.Where(s => s.Id != null))
                    renames[LegacyProgramId(legacyProgramFolder, s.Id)] = declarations.Resolve(s.Id);

            var local = new HashSet<string>(script.Symbols.Where(s => s.Id != null).Select(s => s.Id), StringComparer.Ordinal);
            var cache = new Dictionary<string, string>(StringComparer.Ordinal);
            var unresolved = new HashSet<string>(StringComparer.Ordinal);
            string Map(string id)
            {
                if (id == null) return null;
                if (!cache.TryGetValue(id, out var mapped))
                {
                    cache[id] = mapped = declarations.Resolve(id);
                    if (!local.Contains(id) && !declarations.IsDeclared(id)) unresolved.Add(id);
                }
                return mapped;
            }
            foreach (var s in script.Symbols) { s.Id = Map(s.Id); s.ParentId = Map(s.ParentId); }
            foreach (var p in script.Parameters) p.SymbolId = Map(p.SymbolId);
            foreach (var e in script.Edges) { e.SourceId = Map(e.SourceId); e.TargetId = Map(e.TargetId); }
            foreach (var c in script.CallSites) c.SourceId = Map(c.SourceId);
            foreach (var d in script.SymbolDocs) d.SymbolId = Map(d.SymbolId);
            foreach (var d in script.ParameterDocs) d.SymbolId = Map(d.SymbolId);
            return unresolved.Count;
        }

        private static string Hash(string text)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(bytes, 0, 12).ToLowerInvariant(); // 24 hex, come StableId
        }
    }

    /// <summary>
    /// Quale file dichiara ogni chiave, da TUTTI gli script emessi in un'analisi (i file del programma e quelli dei ponti):
    /// un riferimento verso un altro file va all'id del file che dichiara la destinazione.
    /// </summary>
    public sealed class CodeDeclarations
    {
        private readonly Dictionary<string, SortedSet<string>> _files = new(StringComparer.Ordinal);

        public void Add(string logicalPath, CodeScript script)
        {
            foreach (var s in script.Symbols.Where(s => s.Id != null && s.Kind != "external"))
            {
                if (!_files.TryGetValue(s.Id, out var set)) _files[s.Id] = set = new SortedSet<string>(StringComparer.Ordinal);
                set.Add(logicalPath);
            }
        }

        public bool IsDeclared(string key) => key != null && _files.ContainsKey(key);

        /// <summary>Le chiavi dichiarate in un file solo, con quel file: le uniche con un id per file.</summary>
        public IEnumerable<(string Key, string File)> Single
            => _files.Where(kv => kv.Value.Count == 1).Select(kv => (kv.Key, kv.Value.Min));

        /// <summary>Dichiarata in un file solo → l'id di quel file; in più file, esterna o sconosciuta → la chiave stessa.</summary>
        public string Resolve(string key)
        {
            if (key == null) return null;
            return _files.TryGetValue(key, out var set) && set.Count == 1 ? CodeFileScope.FileId(set.Min, key) : key;
        }

        /// <summary>Le chiavi dichiarate in più file, con i file: un nodo solo per nome (partial, duplicati).</summary>
        public IReadOnlyList<(string Key, IReadOnlyCollection<string> Files)> Shared
            => _files.Where(kv => kv.Value.Count > 1).Select(kv => (kv.Key, (IReadOnlyCollection<string>)kv.Value)).ToList();
    }
}
