using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il programma entra nell'identità di ogni simbolo (punto 3a del 14/10/2026). L'estrattore dà
    /// <c>hash(linguaggio|tipo|nome qualificato)</c>; due cartelle <c>program</c> dello stesso progetto con lo stesso nome
    /// qualificato (<c>com.acme.Main</c>) davano lo stesso IRI e nel grafo, che è uno per progetto, le triple si fondevano.
    /// Qui, in un posto solo e dopo l'estrazione, ogni id dello script diventa <c>hash(cartella program|id)</c>: gli
    /// estrattori non cambiano e la regola vale per tutti i linguaggi. La cartella è il percorso LOGICO
    /// (<c>program</c>, <c>@backend/src</c>), uguale su ogni macchina: gli script vanno su Git.
    /// </summary>
    public static class CodeProgramScope
    {
        public static string ScopedId(string programFolder, string extractorId)
        {
            if (string.IsNullOrWhiteSpace(programFolder))
                throw new InvalidOperationException(UiText.Say("L'id di un simbolo richiede la cartella program: programFolder è vuoto.", "A symbol id needs the program folder: programFolder is empty."));
            if (string.IsNullOrEmpty(extractorId)) return extractorId;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"program|{programFolder}|{extractorId}"));
            return Convert.ToHexString(bytes, 0, 12).ToLowerInvariant(); // 24 hex, come StableId
        }

        /// <summary>
        /// Riscrive tutti gli id dello script (simboli, genitori, parametri, archi, descrizioni) per la cartella data.
        /// <paramref name="foreign"/>: gli id dell'estrattore dei simboli di un ALTRO programma da cui questo dipende (ponti
        /// del build, punto 3b) con il loro id nel grafo: un arco verso di loro va al simbolo vero dell'altro programma.
        /// </summary>
        public static void Apply(CodeScript script, string programFolder, IReadOnlyDictionary<string, string> foreign = null)
        {
            var cache = new Dictionary<string, string>(StringComparer.Ordinal);
            string Map(string id)
            {
                if (id == null) return null;
                if (!cache.TryGetValue(id, out var scoped))
                    cache[id] = scoped = foreign != null && foreign.TryGetValue(id, out var other) ? other : ScopedId(programFolder, id);
                return scoped;
            }
            foreach (var s in script.Symbols) { s.Id = Map(s.Id); s.ParentId = Map(s.ParentId); }
            foreach (var p in script.Parameters) p.SymbolId = Map(p.SymbolId);
            foreach (var e in script.Edges) { e.SourceId = Map(e.SourceId); e.TargetId = Map(e.TargetId); }
            foreach (var c in script.CallSites) c.SourceId = Map(c.SourceId);
            foreach (var d in script.SymbolDocs) d.SymbolId = Map(d.SymbolId);
            foreach (var d in script.ParameterDocs) d.SymbolId = Map(d.SymbolId);
        }

        /// <summary>
        /// Gli id dell'estrattore dei simboli dichiarati da un programma, ricavati dal grafo: ogni estrattore dà a un simbolo
        /// dichiarato <c>StableId(linguaggio, tipo, nome qualificato)</c>, quindi dal nome si ritrova l'id di prima del 3a e
        /// la verifica <c>ScopedId(cartella, quello) == id nel grafo</c> dice se è davvero così. Un solo simbolo che non torna
        /// e il ponte per quel linguaggio NON si fa (<paramref name="mismatches"/> &gt; 0): un ponte a metà lascerebbe archi
        /// verso nodi che non esistono.
        /// </summary>
        public static Dictionary<string, string> ForeignIds(IEnumerable<(string Id, string Kind, string QualifiedName, string Language)> declared,
            string programFolder, string language, out int mismatches)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            mismatches = 0;
            foreach (var s in declared)
            {
                if (s.Language != language || s.Kind == "external") continue;
                var extractorId = RoslynCSharpExtractor.StableId(language, s.Kind, s.QualifiedName);
                if (ScopedId(programFolder, extractorId) != s.Id) { mismatches++; continue; }
                map[extractorId] = s.Id;
            }
            return map;
        }
    }
}
