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
                throw new InvalidOperationException("L'id di un simbolo richiede la cartella program: programFolder è vuoto.");
            if (string.IsNullOrEmpty(extractorId)) return extractorId;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"program|{programFolder}|{extractorId}"));
            return Convert.ToHexString(bytes, 0, 12).ToLowerInvariant(); // 24 hex, come StableId
        }

        /// <summary>Riscrive tutti gli id dello script (simboli, genitori, parametri, archi, descrizioni) per la cartella data.</summary>
        public static void Apply(CodeScript script, string programFolder)
        {
            var cache = new Dictionary<string, string>(StringComparer.Ordinal);
            string Map(string id)
            {
                if (id == null) return null;
                if (!cache.TryGetValue(id, out var scoped)) cache[id] = scoped = ScopedId(programFolder, id);
                return scoped;
            }
            foreach (var s in script.Symbols) { s.Id = Map(s.Id); s.ParentId = Map(s.ParentId); }
            foreach (var p in script.Parameters) p.SymbolId = Map(p.SymbolId);
            foreach (var e in script.Edges) { e.SourceId = Map(e.SourceId); e.TargetId = Map(e.TargetId); }
            foreach (var d in script.SymbolDocs) d.SymbolId = Map(d.SymbolId);
            foreach (var d in script.ParameterDocs) d.SymbolId = Map(d.SymbolId);
        }
    }
}
