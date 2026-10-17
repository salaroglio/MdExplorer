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
    /// Lo script stesso (il <c>module</c>, D36) è il file: il suo <c>summary</c> è la descrizione del file e si correggono i
    /// parametri del suo <c>param()</c>; ritorno e avvertenze non ne ha.
    /// </summary>
    public static class CodeDocEditor
    {
        public static CodeScript EditFile(CodeScript ast, CodeScript existing, string summary, string writtenBy)
        {
            if (string.IsNullOrWhiteSpace(summary))
                throw new CodeDocEditException(UiText.Say("summary è obbligatorio: a che cosa serve il file, in 1-2 frasi.", "summary is required: what the file is for, in 1-2 sentences."));
            var doc = Copy(ast, existing, writtenBy);
            doc.FileDoc = new ScriptFileDoc { Summary = summary.Trim() };
            return doc;
        }

        public static CodeScript EditSymbol(CodeScript ast, CodeScript existing, string symbolId, string summary, string returnsDescription,
            string remarks, IReadOnlyDictionary<string, string> parameters, string writtenBy)
        {
            var symbol = ast.Symbols.FirstOrDefault(s => s.Id == symbolId)
                ?? throw new CodeDocEditException(UiText.Say($"il simbolo '{symbolId}' non è nello script AST di {ast.Header.Source}: rianalizza il codice.", $"symbol '{symbolId}' is not in the AST script of {ast.Header.Source}: analyze the code again."));
            var candidate = CodeDocPlanner.Select(ast, null, force: true).FirstOrDefault(c => c.Symbol.Id == symbolId)
                ?? throw new CodeDocEditException(symbol.Kind == "module"
                    ? UiText.Say($"«{symbol.Name}» è lo script stesso e non ha parametri: per dire a che cosa serve usa la descrizione del file (file = {ast.Header.Source}).",
                        $"«{symbol.Name}» is the script itself and has no parameters: to say what it is for, use the file description (file = {ast.Header.Source}).")
                    : UiText.Say($"«{symbol.Name}» è un {symbol.Kind}: i campi e i nodi esterni non si descrivono.",
                        $"«{symbol.Name}» is a {symbol.Kind}: fields and external nodes are not described."));
            if (summary != null && summary.Trim().Length == 0)
                throw new CodeDocEditException(UiText.Say("summary non può essere vuoto: ometti il campo per lasciarlo com'è.", "summary cannot be empty: leave the field out to keep it as it is."));

            var unknown = (parameters ?? new Dictionary<string, string>()).Keys
                .Where(n => !candidate.Parameters.Any(p => p.Name == n)).ToList();
            if (unknown.Count > 0)
            {
                var known = candidate.Parameters.Count == 0 ? UiText.Say("nessuno", "none") : string.Join(", ", candidate.Parameters.Select(p => p.Name));
                throw new CodeDocEditException(UiText.Say($"parametri che {symbol.QualifiedName} non ha: {string.Join(", ", unknown)}. Parametri: {known}.", $"parameters that {symbol.QualifiedName} does not have: {string.Join(", ", unknown)}. Parameters: {known}."));
            }

            if (candidate.ParametersOnly) return EditScript(ast, existing, candidate, summary, returnsDescription, remarks, parameters, writtenBy);

            var doc = Copy(ast, existing, writtenBy);
            var old = doc.SymbolDocs.FirstOrDefault(d => d.SymbolId == symbolId);
            // una descrizione scritta per una firma vecchia non torna «fresca» correggendo un pezzo: il summary va riscritto
            if (old != null && summary == null && !string.Equals(old.SignatureHash, candidate.SignatureHash, StringComparison.OrdinalIgnoreCase))
                throw new CodeDocEditException(UiText.Say($"la descrizione di {symbol.QualifiedName} è di una firma vecchia: riscrivi anche summary.", $"the description of {symbol.QualifiedName} is for an old signature: rewrite summary too."));
            if (old == null && summary == null)
                throw new CodeDocEditException(UiText.Say($"{symbol.QualifiedName} non ha ancora una descrizione: summary è obbligatorio.", $"{symbol.QualifiedName} has no description yet: summary is required."));

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

            SetParameters(doc, symbolId, parameters);
            return doc;
        }

        /// <summary>Lo script stesso: summary = la descrizione del file, poi i parametri del param(). Nessun symbolDoc.</summary>
        private static CodeScript EditScript(CodeScript ast, CodeScript existing, CodeDocCandidate script, string summary, string returnsDescription,
            string remarks, IReadOnlyDictionary<string, string> parameters, string writtenBy)
        {
            if (returnsDescription != null || remarks != null)
                throw new CodeDocEditException(UiText.Say(
                    $"«{script.Symbol.Name}» è lo script stesso: non ha returnsDescription né remarks. " +
                    "A che cosa serve va in summary (è la descrizione del file), i parametri in parameters.",
                    $"«{script.Symbol.Name}» is the script itself: it has no returnsDescription or remarks. " +
                    "What it is for goes in summary (it is the file description), the parameters in parameters."));
            if (summary == null && (parameters == null || parameters.Count == 0))
                throw new CodeDocEditException(UiText.Say($"niente da scrivere per «{script.Symbol.Name}»: dai summary (a che cosa serve lo script) o parameters.", $"nothing to write for «{script.Symbol.Name}»: give summary (what the script is for) or parameters."));
            var doc = Copy(ast, existing, writtenBy);
            if (summary != null) doc.FileDoc = new ScriptFileDoc { Summary = summary.Trim() };
            SetParameters(doc, script.Symbol.Id, parameters);
            return doc;
        }

        private static void SetParameters(CodeScript doc, string symbolId, IReadOnlyDictionary<string, string> parameters)
        {
            foreach (var (name, text) in parameters ?? new Dictionary<string, string>())
            {
                doc.ParameterDocs.RemoveAll(p => p.SymbolId == symbolId && p.ParameterName == name);
                if (text == null || text.Trim().Length == 0) continue;
                doc.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = symbolId, ParameterName = name, Description = text.Trim() });
            }
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
