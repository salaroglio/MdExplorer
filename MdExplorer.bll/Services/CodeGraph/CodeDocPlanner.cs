using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un simbolo da far descrivere all'AI, con quello che le serve sapere.</summary>
    public sealed class CodeDocCandidate
    {
        public ScriptSymbol Symbol { get; init; }
        public IReadOnlyList<ScriptParameter> Parameters { get; init; }
        public string SignatureHash { get; init; }
    }

    /// <summary>
    /// La parte DETERMINISTICA della skill «descrivi il codice» (D11, [[feedback_parser_vs_llm_split]]): quali simboli
    /// hanno bisogno di una descrizione, che cosa chiedere, come leggere la risposta e come fonderla con lo script
    /// <c>.doc.jsonl</c> esistente. L'AI ragiona sul significato; tutto il resto è qui ed è testabile senza AI.
    /// </summary>
    public static class CodeDocPlanner
    {
        /// <summary>I kind che hanno senso descrivere: non i moduli, non i nodi external.</summary>
        private static readonly HashSet<string> Describable = new(StringComparer.Ordinal)
        {
            "class", "interface", "record", "struct", "enum", "method", "constructor", "property", "function", "type",
            "component", "service", "directive", "pipe", "ngmodule",
        };

        /// <summary>L'hash della firma: cambia se cambiano firma, tipo di ritorno o parametri — non se il metodo si sposta.</summary>
        public static string SignatureHash(ScriptSymbol s, IEnumerable<ScriptParameter> parameters)
        {
            var sb = new StringBuilder();
            sb.Append(s.Kind).Append('|').Append(s.QualifiedName).Append('|').Append(s.Signature).Append('|').Append(s.ReturnType);
            foreach (var p in parameters.OrderBy(p => p.Position))
                sb.Append('|').Append(p.Name).Append(':').Append(p.ParameterType).Append(':').Append(p.Direction).Append(':').Append(p.DefaultValue);
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).ToLowerInvariant();
        }

        /// <summary>
        /// Chi va descritto: i simboli descrivibili dello script AST che non hanno una descrizione, o la cui firma è
        /// cambiata da quando sono stati descritti (hash diverso). Con <paramref name="force"/> tutti.
        /// </summary>
        public static List<CodeDocCandidate> Select(CodeScript ast, CodeScript existingDoc, bool force)
        {
            var docs = existingDoc?.SymbolDocs.ToDictionary(d => d.SymbolId, d => d, StringComparer.Ordinal) ?? new Dictionary<string, ScriptSymbolDoc>();
            var list = new List<CodeDocCandidate>();
            foreach (var s in ast.Symbols)
            {
                if (!Describable.Contains(s.Kind)) continue;
                var prms = ast.Parameters.Where(p => p.SymbolId == s.Id).OrderBy(p => p.Position).ToList();
                var hash = SignatureHash(s, prms);
                if (!force && docs.TryGetValue(s.Id, out var d) && string.Equals(d.SignatureHash, hash, StringComparison.OrdinalIgnoreCase)) continue;
                list.Add(new CodeDocCandidate { Symbol = s, Parameters = prms, SignatureHash = hash });
            }
            return list;
        }

        public const int MaxSourceChars = 60_000;
        /// <summary>Quanti simboli per domanda quando il file è troppo lungo per andare intero.</summary>
        public const int ExcerptBatchSize = 12;

        /// <summary>
        /// Che cosa mandare all'LLM. Un file entro <see cref="MaxSourceChars"/> va intero con tutti i candidati. Uno più
        /// lungo NON si tronca: prima (fino al 12/10/2026) il troncamento lasciava le funzioni oltre il taglio senza
        /// descrizione, perché l'LLM non le vedeva. Si mandano i candidati a gruppi, ognuno con il pezzo di sorgente
        /// delle righe dei suoi simboli (intestate «righe a-b»).
        /// </summary>
        public static List<(IReadOnlyList<CodeDocCandidate> Candidates, string Source)> Batches(string sourceText, IReadOnlyList<CodeDocCandidate> candidates)
        {
            var list = new List<(IReadOnlyList<CodeDocCandidate>, string)>();
            if (candidates.Count == 0) return list;
            if (sourceText.Length <= MaxSourceChars) { list.Add((candidates, sourceText)); return list; }
            var lines = sourceText.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < candidates.Count; i += ExcerptBatchSize)
            {
                var batch = candidates.Skip(i).Take(ExcerptBatchSize).ToList();
                var sb = new StringBuilder();
                foreach (var c in batch.OrderBy(c => c.Symbol.LineStart ?? 0))
                {
                    var from = Math.Max(1, c.Symbol.LineStart ?? 1);
                    var to = Math.Min(lines.Length, Math.Max(from, c.Symbol.LineEnd ?? from));
                    sb.Append("// ---- righe ").Append(from).Append('-').Append(to).Append(" ----\n");
                    for (var l = from; l <= to; l++) sb.Append(lines[l - 1]).Append('\n');
                    sb.Append('\n');
                }
                list.Add((batch, sb.ToString()));
            }
            return list;
        }

        /// <summary>Il prompt: la skill, il sorgente, i simboli da descrivere (JSON Lines) e il formato della risposta.</summary>
        public static string BuildPrompt(string skillBody, string sourceRelativePath, string sourceText, IReadOnlyList<CodeDocCandidate> candidates)
        {
            if (sourceText.Length > MaxSourceChars) sourceText = sourceText.Substring(0, MaxSourceChars) + "\n/* … troncato … */";
            var sb = new StringBuilder();
            sb.Append(skillBody.Trim()).Append("\n\n");
            sb.Append("---- FILE ----\n").Append(sourceRelativePath).Append('\n').Append(sourceText).Append("\n\n");
            sb.Append("---- SIMBOLI DA DESCRIVERE (uno per riga) ----\n");
            foreach (var c in candidates)
            {
                sb.Append(JsonSerializer.Serialize(new
                {
                    symbolId = c.Symbol.Id, kind = c.Symbol.Kind, qualifiedName = c.Symbol.QualifiedName, signature = c.Symbol.Signature,
                    returnType = c.Symbol.ReturnType, lineStart = c.Symbol.LineStart,
                    parameters = c.Parameters.Select(p => new { name = p.Name, type = p.ParameterType, direction = p.Direction, defaultValue = p.DefaultValue }),
                }, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping })).Append('\n');
            }
            sb.Append("\n---- RISPOSTA ----\n");
            return sb.ToString();
        }

        /// <summary>
        /// Legge la risposta dell'AI (JSON Lines di <c>symbolDoc</c>/<c>parameterDoc</c>): si tengono solo i record dei
        /// simboli chiesti, con il <c>signatureHash</c> calcolato da noi; righe non JSON o fuori formato finiscono in
        /// <paramref name="rejected"/> con il motivo, mai ignorate in silenzio.
        /// </summary>
        public static CodeScript ParseAnswer(string answer, string sourceRelativePath, IReadOnlyList<CodeDocCandidate> candidates, string writtenBy, out List<string> rejected)
        {
            rejected = new List<string>();
            var byId = candidates.ToDictionary(c => c.Symbol.Id, c => c, StringComparer.Ordinal);
            var result = new CodeScript
            {
                Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = sourceRelativePath, WrittenBy = writtenBy, WrittenAt = DateTime.UtcNow.ToString("o") }
            };
            var text = StripFences(answer ?? string.Empty);
            var lineNo = 0;
            foreach (var raw in text.Split('\n'))
            {
                lineNo++;
                var line = raw.Trim();
                if (line.Length == 0 || !line.StartsWith('{')) { if (line.Length > 0) rejected.Add($"riga {lineNo}: non è un record JSON: {Shorten(line)}"); continue; }
                // la risposta dell'LLM è JSON Lines (formato di scambio, non degli script): si rilegge come schema 1
                var header = JsonSerializer.Serialize(new { schema = CodeScriptParser.LegacyJsonSchema, kind = "doc", source = sourceRelativePath, writtenBy, writtenAt = result.Header.WrittenAt });
                CodeScript one;
                try { one = CodeScriptParser.Parse(header + "\n" + line, sourceRelativePath); }
                catch (CodeScriptFormatException ex) { rejected.Add($"riga {lineNo}: {ex.Message}"); continue; }
                foreach (var d in one.SymbolDocs)
                {
                    if (!byId.TryGetValue(d.SymbolId, out var c)) { rejected.Add($"riga {lineNo}: symbolId '{d.SymbolId}' non richiesto"); continue; }
                    d.SignatureHash = c.SignatureHash;
                    result.SymbolDocs.RemoveAll(x => x.SymbolId == d.SymbolId);
                    result.SymbolDocs.Add(d);
                }
                foreach (var p in one.ParameterDocs)
                {
                    if (!byId.TryGetValue(p.SymbolId, out var c)) { rejected.Add($"riga {lineNo}: symbolId '{p.SymbolId}' non richiesto"); continue; }
                    if (!c.Parameters.Any(x => x.Name == p.ParameterName)) { rejected.Add($"riga {lineNo}: parametro '{p.ParameterName}' non esiste in {c.Symbol.QualifiedName}"); continue; }
                    result.ParameterDocs.RemoveAll(x => x.SymbolId == p.SymbolId && x.ParameterName == p.ParameterName);
                    result.ParameterDocs.Add(p);
                }
            }
            return result;
        }

        /// <summary>Fonde le descrizioni nuove in quelle esistenti: i simboli non toccati restano, quelli descritti ora si sostituiscono.</summary>
        public static CodeScript Merge(CodeScript existing, CodeScript fresh)
        {
            var merged = new CodeScript { Header = fresh.Header };
            var touched = new HashSet<string>(fresh.SymbolDocs.Select(d => d.SymbolId).Concat(fresh.ParameterDocs.Select(p => p.SymbolId)), StringComparer.Ordinal);
            if (existing != null)
            {
                merged.SymbolDocs.AddRange(existing.SymbolDocs.Where(d => !touched.Contains(d.SymbolId)));
                merged.ParameterDocs.AddRange(existing.ParameterDocs.Where(p => !touched.Contains(p.SymbolId)));
            }
            merged.SymbolDocs.AddRange(fresh.SymbolDocs);
            merged.ParameterDocs.AddRange(fresh.ParameterDocs);
            return merged;
        }

        /// <summary>Toglie le descrizioni di simboli che l'AST non ha più (metodi cancellati): si segnalano, non si tengono per sempre.</summary>
        public static int DropOrphans(CodeScript doc, CodeScript ast)
        {
            var alive = new HashSet<string>(ast.Symbols.Select(s => s.Id), StringComparer.Ordinal);
            var n = doc.SymbolDocs.RemoveAll(d => !alive.Contains(d.SymbolId));
            n += doc.ParameterDocs.RemoveAll(p => !alive.Contains(p.SymbolId));
            return n;
        }

        public static CodeScript ReadScriptOrNull(string path, string relativeName)
            => File.Exists(path) ? CodeScriptParser.Parse(File.ReadAllText(path, Encoding.UTF8), relativeName) : null;

        private static string StripFences(string t)
        {
            t = t.Trim();
            if (!t.StartsWith("```", StringComparison.Ordinal)) return t;
            var nl = t.IndexOf('\n'); if (nl >= 0) t = t.Substring(nl + 1);
            var last = t.LastIndexOf("```", StringComparison.Ordinal); if (last >= 0) t = t.Substring(0, last);
            return t.Trim();
        }

        private static string Shorten(string s) => s.Length > 80 ? s.Substring(0, 80) + "…" : s;
    }
}
