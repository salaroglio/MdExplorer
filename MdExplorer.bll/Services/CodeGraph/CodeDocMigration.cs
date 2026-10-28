using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le descrizioni scritte da Mark seguono i simboli quando cambia il loro id (G4, 27/10/2026: id per file invece che
    /// per cartella program). Il passaggio è esatto, non per somiglianza: chi rifà lo script AST conosce per ogni simbolo
    /// l'id vecchio e quello nuovo (<see cref="CodeFileScope.Apply"/>). Si riscrivono tutti gli script <c>.doc.ttl</c> del
    /// progetto, perché una scelta su una chiamata (F12c) può puntare a un simbolo di un altro file o di un altro programma.
    /// Utente: le descrizioni di un progetto Java «sono costate un bel po' di token».
    /// </summary>
    public static class CodeDocMigration
    {
        /// <summary>Riscrive gli script doc che citano un id di <paramref name="renames"/>; torna i percorsi scritti.</summary>
        public static List<string> Rewrite(string projectPath, IReadOnlyDictionary<string, string> renames)
        {
            var written = new List<string>();
            var root = CodeGraphPaths.GetScriptsRoot(projectPath);
            if (renames.Count == 0 || !Directory.Exists(root)) return written;
            foreach (var path in Directory.EnumerateFiles(root, "*" + CodeGraphPaths.DocScriptSuffix, SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(path, Encoding.UTF8);
                var header = CodeScriptTurtle.ParseHeaderLine(content.Split('\n', 2)[0]);
                if (header == null || header.Schema != CodeScriptParser.CurrentSchema) continue; // lo ritira RetireOldScripts, o lo dice il caricamento
                var script = CodeScriptTurtle.Read(content, CodeGraphPaths.ToProjectRelative(projectPath, path));
                if (!Apply(script, renames)) continue;
                if (CodeScriptWriter.WriteIfChanged(path, CodeScriptWriter.Serialize(script))) written.Add(path);
            }
            return written;
        }

        /// <summary>
        /// File spostato o rinominato (SVN <c>move</c>): i suoi simboli hanno id nuovi e il suo script doc resta orfano, sotto
        /// il percorso vecchio. Lo stesso nome qualificato dà la stessa chiave, quindi il recupero è esatto: per ogni chiave
        /// dichiarata adesso, <c>FileId(percorso vecchio, chiave)</c> è l'id vecchio. Le descrizioni ritrovate passano allo
        /// script doc del file nuovo (senza sovrascrivere quelle che ci sono già); l'orfano si cancella se resta vuoto.
        /// Una classe RINOMINATA cambia chiave e non si ritrova: resta nell'orfano, contata in <paramref name="left"/>.
        /// </summary>
        public static (List<string> Written, List<string> Removed) Relocate(string projectPath, string folderRelative, ISet<string> alive,
            IReadOnlyList<CodeDeclarations> declarations, out int left)
        {
            left = 0;
            var written = new List<string>();
            var removed = new List<string>();
            var folder = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), folderRelative.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(folder)) return (written, removed);
            var keys = declarations.SelectMany(d => d.Single).ToList();
            foreach (var path in Directory.EnumerateFiles(folder, "*" + CodeGraphPaths.DocScriptSuffix, SearchOption.AllDirectories).ToList())
            {
                var content = File.ReadAllText(path, Encoding.UTF8);
                var header = CodeScriptTurtle.ParseHeaderLine(content.Split('\n', 2)[0]);
                if (header == null || header.Schema != CodeScriptParser.CurrentSchema || header.Source == null || alive.Contains(header.Source)) continue;
                var orphan = CodeScriptTurtle.Read(content, CodeGraphPaths.ToProjectRelative(projectPath, path));
                var cited = new HashSet<string>(orphan.SymbolDocs.Select(d => d.SymbolId).Concat(orphan.ParameterDocs.Select(d => d.SymbolId))
                    .Concat(orphan.CallResolutions.Select(r => r.SourceId)), StringComparer.Ordinal);
                var moves = new Dictionary<string, (string Id, string File)>(StringComparer.Ordinal);
                foreach (var (key, file) in keys)
                {
                    var old = CodeFileScope.FileId(orphan.Header.Source, key);
                    if (cited.Contains(old)) moves[old] = (CodeFileScope.FileId(file, key), file);
                }
                if (moves.Count == 0) { left += cited.Count; continue; }

                var renames = moves.ToDictionary(kv => kv.Key, kv => kv.Value.Id, StringComparer.Ordinal);
                var targets = new Dictionary<string, CodeScript>(StringComparer.Ordinal);
                CodeScript TargetFor(string file)
                {
                    if (targets.TryGetValue(file, out var t)) return t;
                    var targetPath = CodeScriptWriter.ScriptPathFor(projectPath, file, CodeScriptKind.Doc);
                    t = File.Exists(targetPath)
                        ? CodeScriptTurtle.Read(File.ReadAllText(targetPath, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(projectPath, targetPath))
                        : new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = file, WrittenBy = orphan.Header.WrittenBy, WrittenAt = orphan.Header.WrittenAt } };
                    return targets[file] = t;
                }
                string FileOf(string id) => moves.TryGetValue(id, out var m) ? m.File : null;

                foreach (var d in orphan.SymbolDocs.Where(d => FileOf(d.SymbolId) != null).ToList())
                {
                    var t = TargetFor(FileOf(d.SymbolId));
                    d.WrittenBy ??= orphan.Header.WrittenBy;
                    d.SymbolId = renames[d.SymbolId];
                    if (!t.SymbolDocs.Any(x => x.SymbolId == d.SymbolId)) t.SymbolDocs.Add(d);
                    orphan.SymbolDocs.Remove(d);
                }
                foreach (var d in orphan.ParameterDocs.Where(d => FileOf(d.SymbolId) != null).ToList())
                {
                    var t = TargetFor(FileOf(d.SymbolId));
                    d.SymbolId = renames[d.SymbolId];
                    if (!t.ParameterDocs.Any(x => x.SymbolId == d.SymbolId && x.ParameterName == d.ParameterName)) t.ParameterDocs.Add(d);
                    orphan.ParameterDocs.Remove(d);
                }
                foreach (var r in orphan.CallResolutions.Where(r => FileOf(r.SourceId) != null).ToList())
                {
                    var t = TargetFor(FileOf(r.SourceId));
                    Apply(new CodeScript { CallResolutions = { r } }, renames);
                    if (!t.CallResolutions.Any(x => x.SourceId == r.SourceId && x.Line == r.Line)) t.CallResolutions.Add(r);
                    orphan.CallResolutions.Remove(r);
                }
                // la descrizione del file va con lui se tutto è andato in un file solo
                if (orphan.FileDoc != null && targets.Count == 1 && targets.Values.Single().FileDoc == null && orphan.SymbolDocs.Count == 0)
                {
                    targets.Values.Single().FileDoc = orphan.FileDoc;
                    orphan.FileDoc = null;
                }
                foreach (var (file, t) in targets)
                {
                    var targetPath = CodeScriptWriter.ScriptPathFor(projectPath, file, CodeScriptKind.Doc);
                    if (CodeScriptWriter.WriteIfChanged(targetPath, CodeScriptWriter.Serialize(t))) written.Add(targetPath);
                }
                var rest = orphan.SymbolDocs.Count + orphan.ParameterDocs.Count + orphan.CallResolutions.Count;
                left += rest;
                if (rest == 0 && orphan.FileDoc == null) { File.Delete(path); removed.Add(path); }
                else if (CodeScriptWriter.WriteIfChanged(path, CodeScriptWriter.Serialize(orphan))) written.Add(path);
            }
            return (written, removed);
        }

        /// <summary>Sostituisce gli id rinominati in descrizioni, parametri e scelte sulle chiamate; true se ne ha cambiato uno.</summary>
        public static bool Apply(CodeScript script, IReadOnlyDictionary<string, string> renames)
        {
            var changed = false;
            string Map(string id)
            {
                if (id == null || !renames.TryGetValue(id, out var to) || to == id) return id;
                changed = true;
                return to;
            }
            foreach (var d in script.SymbolDocs) d.SymbolId = Map(d.SymbolId);
            foreach (var d in script.ParameterDocs) d.SymbolId = Map(d.SymbolId);
            foreach (var r in script.CallResolutions)
            {
                r.SourceId = Map(r.SourceId);
                r.TargetId = Map(r.TargetId);
                for (var i = 0; i < r.Rejected.Count; i++) r.Rejected[i] = Map(r.Rejected[i]);
            }
            return changed;
        }
    }
}
