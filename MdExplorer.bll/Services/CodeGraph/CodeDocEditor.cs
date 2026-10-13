using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Una correzione che non si può applicare: il messaggio dice a chi la chiede che cosa cambiare.</summary>
    public sealed class CodeDocEditException : Exception
    {
        public CodeDocEditException(string message) : base(message) { }
    }

    /// <summary>
    /// Correzione di una descrizione dal MarkAgent (D34, strumento MCP <c>code_set_description</c>): il file nel suo
    /// insieme, oppure un simbolo e i suoi parametri. Parte DETERMINISTICA, senza LLM: chi scrive il testo è l'agente che
    /// parla con l'utente. Per ogni campo <c>null</c> = lascia com'è, stringa vuota = togli (non per <c>summary</c>, che è
    /// obbligatorio). Il risultato è lo script <c>.doc.ttl</c> completo da scrivere, con l'hash della firma di OGGI.
    /// </summary>
    public static class CodeDocEditor
    {
        public static CodeScript EditFile(CodeScript ast, CodeScript existing, string summary, string writtenBy)
        {
            if (string.IsNullOrWhiteSpace(summary))
                throw new CodeDocEditException("summary è obbligatorio: a che cosa serve il file, in 1-2 frasi.");
            var doc = Copy(ast, existing, writtenBy);
            doc.FileDoc = new ScriptFileDoc { Summary = summary.Trim() };
            return doc;
        }

        public static CodeScript EditSymbol(CodeScript ast, CodeScript existing, string symbolId, string summary, string returnsDescription,
            string remarks, IReadOnlyDictionary<string, string> parameters, string writtenBy)
        {
            var symbol = ast.Symbols.FirstOrDefault(s => s.Id == symbolId)
                ?? throw new CodeDocEditException($"il simbolo '{symbolId}' non è nello script AST di {ast.Header.Source}: rianalizza il codice.");
            var candidate = CodeDocPlanner.Select(ast, null, force: true).FirstOrDefault(c => c.Symbol.Id == symbolId)
                ?? throw new CodeDocEditException($"«{symbol.Name}» è un {symbol.Kind}: i moduli, i campi e i nodi esterni non si descrivono. " +
                                                  "Per dire a che cosa serve il file usa la descrizione del file.");
            if (summary != null && summary.Trim().Length == 0)
                throw new CodeDocEditException("summary non può essere vuoto: ometti il campo per lasciarlo com'è.");

            var unknown = (parameters ?? new Dictionary<string, string>()).Keys
                .Where(n => !candidate.Parameters.Any(p => p.Name == n)).ToList();
            if (unknown.Count > 0)
            {
                var known = candidate.Parameters.Count == 0 ? "nessuno" : string.Join(", ", candidate.Parameters.Select(p => p.Name));
                throw new CodeDocEditException($"parametri che {symbol.QualifiedName} non ha: {string.Join(", ", unknown)}. Parametri: {known}.");
            }

            var doc = Copy(ast, existing, writtenBy);
            var old = doc.SymbolDocs.FirstOrDefault(d => d.SymbolId == symbolId);
            // una descrizione scritta per una firma vecchia non torna «fresca» correggendo un pezzo: il summary va riscritto
            if (old != null && summary == null && !string.Equals(old.SignatureHash, candidate.SignatureHash, StringComparison.OrdinalIgnoreCase))
                throw new CodeDocEditException($"la descrizione di {symbol.QualifiedName} è di una firma vecchia: riscrivi anche summary.");
            if (old == null && summary == null)
                throw new CodeDocEditException($"{symbol.QualifiedName} non ha ancora una descrizione: summary è obbligatorio.");

            var updated = new ScriptSymbolDoc
            {
                SymbolId = symbolId,
                Summary = summary?.Trim() ?? old.Summary,
                ReturnsDescription = Field(returnsDescription, old?.ReturnsDescription),
                Remarks = Field(remarks, old?.Remarks),
                SignatureHash = candidate.SignatureHash,
            };
            doc.SymbolDocs.RemoveAll(d => d.SymbolId == symbolId);
            doc.SymbolDocs.Add(updated);

            foreach (var (name, text) in parameters ?? new Dictionary<string, string>())
            {
                doc.ParameterDocs.RemoveAll(p => p.SymbolId == symbolId && p.ParameterName == name);
                if (text == null || text.Trim().Length == 0) continue;
                doc.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = symbolId, ParameterName = name, Description = text.Trim() });
            }
            return doc;
        }

        private static string Field(string given, string old)
            => given == null ? old : (given.Trim().Length == 0 ? null : given.Trim());

        /// <summary>Lo script esistente rifatto con la nuova intestazione; le descrizioni di simboli spariti si tolgono come in «Descrivi».</summary>
        private static CodeScript Copy(CodeScript ast, CodeScript existing, string writtenBy)
        {
            var doc = new CodeScript
            {
                Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = ast.Header.Source,
                    WrittenBy = writtenBy, WrittenAt = DateTime.UtcNow.ToString("o"),
                },
                FileDoc = existing?.FileDoc,
            };
            if (existing != null)
            {
                doc.SymbolDocs.AddRange(existing.SymbolDocs);
                doc.ParameterDocs.AddRange(existing.ParameterDocs);
            }
            CodeDocPlanner.DropOrphans(doc, ast);
            return doc;
        }
    }
}
