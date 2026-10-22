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
    /// Com'è la descrizione di un simbolo (F12b). Le prime quattro si rifanno da sole; le «Review…» sono «da rivedere»
    /// (D39 b): si dicono e si rifanno solo quando l'utente chiede di rivederle.
    /// </summary>
    public enum CodeDocVerdict
    {
        Fresh,
        Missing,
        SignatureChanged,
        BodyChanged,
        /// <summary>
        /// Manca il significato di un parametro o del valore restituito (utente, 22/10/2026: «si deve segnare bene la firma del
        /// metodo, i parametri, il tipo ed il suo significato»): si rifà da sola.
        /// </summary>
        Incomplete,
        /// <summary>Le sintesi delle funzioni che chiama (o dei membri) sono cambiate da quando è stata scritta.</summary>
        ReviewContext,
        /// <summary>Scritta con un'altra versione del prompt mde-code-doc.</summary>
        ReviewPrompt,
        /// <summary>Senza impronta del contesto o del prompt: scritta prima del 21/10/2026, o da «Descrivi» su un solo simbolo.</summary>
        ReviewNoFingerprint,
    }

    /// <summary>Chi descrivere in un giro e perché, con i conti per la finestra.</summary>
    public sealed class CodeDocAssessment
    {
        public List<CodeDocCandidate> Candidates { get; } = new();
        public Dictionary<CodeDocVerdict, int> Counts { get; } = new();
        /// <summary>Simboli con un corpo ma senza impronta nello script AST (script di prima del 21/10): il corpo cambiato non si vede finché non si rifà «Analizza il codice».</summary>
        public int BodyUnknown { get; set; }
        public int Count(CodeDocVerdict v) => Counts.TryGetValue(v, out var n) ? n : 0;
        public int ToReview => Count(CodeDocVerdict.ReviewContext) + Count(CodeDocVerdict.ReviewPrompt) + Count(CodeDocVerdict.ReviewNoFingerprint);
    }

    /// <summary>Un simbolo da far descrivere all'AI, con quello che le serve sapere.</summary>
    public sealed class CodeDocCandidate
    {
        public ScriptSymbol Symbol { get; init; }
        public IReadOnlyList<ScriptParameter> Parameters { get; init; }
        public string SignatureHash { get; init; }
        /// <summary>
        /// Lo script stesso (il <c>module</c>) con il suo <c>param()</c>: a che cosa serve lo dice la descrizione del file
        /// (D31), qui si chiedono SOLO i parametri con cui lo si lancia (D36). Niente <c>symbolDoc</c>, quindi niente hash
        /// della firma: è da rifare quando un parametro non ha descrizione.
        /// </summary>
        public bool ParametersOnly { get; init; }
        /// <summary>Il percorso logico del sorgente (program/x.java): in una domanda su più file (un ciclo) dice a quale appartiene.</summary>
        public string Source { get; init; }
        /// <summary>Perché è nel giro (F12b).</summary>
        public CodeDocVerdict Verdict { get; init; } = CodeDocVerdict.Missing;
    }

    /// <summary>
    /// La parte DETERMINISTICA della skill «descrivi il codice» (D11, [[feedback_parser_vs_llm_split]]): quali simboli
    /// hanno bisogno di una descrizione, che cosa chiedere, come leggere la risposta e come fonderla con lo script
    /// <c>.doc.jsonl</c> esistente. L'AI ragiona sul significato; tutto il resto è qui ed è testabile senza AI.
    /// </summary>
    public static class CodeDocPlanner
    {
        /// <summary>
        /// I kind che hanno senso descrivere: non i moduli, non i nodi external. Il file nel suo insieme ha la sua voce,
        /// <see cref="ScriptFileDoc"/> (vedi <see cref="NeedsFileDoc"/>), per ogni linguaggio. Il <c>module</c> è il file
        /// stesso: se ha parametri (il <c>param()</c> di uno script PowerShell) si descrivono quelli, e basta (D36).
        /// </summary>
        private static readonly HashSet<string> Describable = new(StringComparer.Ordinal)
        {
            "class", "interface", "record", "struct", "enum", "method", "constructor", "property", "function", "type",
            "component", "service", "directive", "pipe", "ngmodule",
        };

        /// <summary>Un kind che si descrive (non i campi, non i moduli, non gli esterni).</summary>
        public static bool IsDescribable(string kind) => Describable.Contains(kind);

        /// <summary>Chi scrive le descrizioni fisse dei metodi banali (D41): MdExplorer, non un LLM.</summary>
        public const string TrivialAuthor = "mdexplorer (metodo banale)";

        /// <summary>
        /// La «versione del prompt» di una descrizione corretta a mano o da MarkAgent (<c>code_set_description</c>): non
        /// l'ha scritta il prompt, quindi non diventa mai «da rivedere» (né per il prompt né per il contesto); si rifà
        /// solo se cambiano firma o corpo.
        /// </summary>
        public const string EditedPromptVersion = "edit";

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
        /// Chi va descritto: i simboli descrivibili senza descrizione, con la firma o il corpo cambiati; il module con
        /// parametri quando un parametro non ha descrizione (<see cref="CodeDocCandidate.ParametersOnly"/>). Con
        /// <paramref name="force"/> tutti. Senza le impronte di contesto e prompt: per «Descrivi» su un simbolo e i test.
        /// </summary>
        public static List<CodeDocCandidate> Select(CodeScript ast, CodeScript existingDoc, bool force)
            => Assess(ast, existingDoc, force, review: false, contextHashOf: null, promptVersion: null).Candidates;

        /// <summary>
        /// Il giudizio di ogni descrizione (F12b, <see cref="Verdict"/>) e chi entra nel giro: quelle da fare o da rifare e,
        /// con <paramref name="review"/>, anche quelle «da rivedere». <paramref name="contextHashOf"/> e
        /// <paramref name="promptVersion"/> null = le «da rivedere» non si cercano.
        /// </summary>
        public static CodeDocAssessment Assess(CodeScript ast, CodeScript existingDoc, bool force, bool review, Func<string, string> contextHashOf, string promptVersion)
        {
            var docs = existingDoc?.SymbolDocs.ToDictionary(d => d.SymbolId, d => d, StringComparer.Ordinal) ?? new Dictionary<string, ScriptSymbolDoc>();
            var result = new CodeDocAssessment();
            foreach (var s in ast.Symbols)
            {
                var prms = ast.Parameters.Where(p => p.SymbolId == s.Id).OrderBy(p => p.Position).ToList();
                if (s.Kind == "module")
                {
                    if (prms.Count == 0) continue;
                    var missing = prms.Any(p => existingDoc == null || !existingDoc.ParameterDocs.Any(d => d.SymbolId == s.Id && d.ParameterName == p.Name));
                    if (force || missing) result.Candidates.Add(new CodeDocCandidate { Symbol = s, Parameters = prms, SignatureHash = SignatureHash(s, prms), ParametersOnly = true, Source = ast.Header.Source });
                    continue;
                }
                if (!Describable.Contains(s.Kind)) continue;
                // i metodi banali li descrive MdExplorer (TrivialDocs), mai l'LLM (D41)
                if (s.Trivial != null) continue;
                var hash = SignatureHash(s, prms);
                if (CodeBodyHash.HasBody(s.Kind) && s.BodyHash == null) result.BodyUnknown++;
                docs.TryGetValue(s.Id, out var d);
                var verdict = Verdict(s, hash, d, contextHashOf == null ? null : () => contextHashOf(s.Id), promptVersion);
                if (verdict is CodeDocVerdict.Fresh or CodeDocVerdict.ReviewContext or CodeDocVerdict.ReviewPrompt or CodeDocVerdict.ReviewNoFingerprint
                    && d.PromptVersion != EditedPromptVersion && WhatIsMissing(s, prms, d, existingDoc).Count > 0)
                    verdict = CodeDocVerdict.Incomplete;
                result.Counts[verdict] = result.Count(verdict) + 1;
                var take = force || verdict is CodeDocVerdict.Missing or CodeDocVerdict.SignatureChanged or CodeDocVerdict.BodyChanged or CodeDocVerdict.Incomplete
                           || review && verdict is CodeDocVerdict.ReviewContext or CodeDocVerdict.ReviewPrompt or CodeDocVerdict.ReviewNoFingerprint;
                if (take) result.Candidates.Add(new CodeDocCandidate { Symbol = s, Parameters = prms, SignatureHash = hash, Source = ast.Header.Source, Verdict = verdict });
            }
            return result;
        }

        private static readonly HashSet<string> WithReturn = new(StringComparer.Ordinal) { "method", "function", "property" };
        private static readonly HashSet<string> NoValue = new(StringComparer.OrdinalIgnoreCase) { "void", "Void", "Unit", "None", "never", "Task", "System.Void" };

        /// <summary>
        /// Che cosa manca alla descrizione <paramref name="d"/> del simbolo: i parametri senza significato e, per chi restituisce
        /// un valore (tipo di ritorno scritto e non void), il significato di quel valore. Vuota = completa.
        /// </summary>
        public static List<string> WhatIsMissing(ScriptSymbol s, IReadOnlyList<ScriptParameter> parameters, ScriptSymbolDoc d, CodeScript doc)
        {
            var missing = new List<string>();
            foreach (var p in parameters)
                if (doc == null || !doc.ParameterDocs.Any(x => x.SymbolId == s.Id && x.ParameterName == p.Name && !string.IsNullOrWhiteSpace(x.Description)))
                    missing.Add(p.Name);
            if (WithReturn.Contains(s.Kind) && !string.IsNullOrWhiteSpace(s.ReturnType) && !NoValue.Contains(s.ReturnType.Trim())
                && string.IsNullOrWhiteSpace(d?.ReturnsDescription))
                missing.Add("return");
            return missing;
        }

        /// <summary>
        /// Com'è la descrizione <paramref name="d"/> del simbolo oggi. Rifare: manca, firma o corpo cambiati, oppure è la
        /// frase fissa di un metodo che non è più banale. Da rivedere: senza impronte, prompt diverso, contesto cambiato.
        /// <paramref name="contextHash"/> null = non si guarda (né il prompt).
        /// </summary>
        public static CodeDocVerdict Verdict(ScriptSymbol s, string signatureHash, ScriptSymbolDoc d, Func<string> contextHash, string promptVersion)
        {
            if (d == null) return CodeDocVerdict.Missing;
            if (!string.Equals(d.SignatureHash, signatureHash, StringComparison.OrdinalIgnoreCase)) return CodeDocVerdict.SignatureChanged;
            if (s.BodyHash != null && d.BodyHash != null && !string.Equals(s.BodyHash, d.BodyHash, StringComparison.Ordinal)) return CodeDocVerdict.BodyChanged;
            if (d.WrittenBy == TrivialAuthor && s.Trivial == null) return CodeDocVerdict.Missing;
            if (contextHash == null || d.PromptVersion == EditedPromptVersion) return CodeDocVerdict.Fresh;
            if (d.ContextHash == null || d.PromptVersion == null) return CodeDocVerdict.ReviewNoFingerprint;
            if (promptVersion != null && d.PromptVersion != promptVersion) return CodeDocVerdict.ReviewPrompt;
            return d.ContextHash == contextHash() ? CodeDocVerdict.Fresh : CodeDocVerdict.ReviewContext;
        }

        /// <summary>
        /// Le descrizioni fisse dei metodi banali (D41) che mancano o hanno la firma cambiata (con <paramref name="force"/>
        /// tutte): una frase dalla forma del corpo, nella lingua dell'interfaccia, scritte da <see cref="TrivialAuthor"/>.
        /// </summary>
        public static List<ScriptSymbolDoc> TrivialDocs(CodeScript ast, CodeScript existingDoc, string language, bool force)
        {
            var docs = existingDoc?.SymbolDocs.ToDictionary(d => d.SymbolId, d => d, StringComparer.Ordinal) ?? new Dictionary<string, ScriptSymbolDoc>();
            var list = new List<ScriptSymbolDoc>();
            foreach (var s in ast.Symbols.Where(s => s.Trivial != null))
            {
                var hash = SignatureHash(s, ast.Parameters.Where(p => p.SymbolId == s.Id));
                if (!force && docs.TryGetValue(s.Id, out var d) && string.Equals(d.SignatureHash, hash, StringComparison.OrdinalIgnoreCase)
                    && (s.BodyHash == null || d.BodyHash == null || s.BodyHash == d.BodyHash)) continue;
                list.Add(new ScriptSymbolDoc { SymbolId = s.Id, Summary = TrivialSummary(s, language), SignatureHash = hash, BodyHash = s.BodyHash, WrittenBy = TrivialAuthor });
            }
            return list;
        }

        public static string TrivialSummary(ScriptSymbol s, string language)
        {
            var en = language switch
            {
                "it" => false,
                "en" => true,
                _ => throw new ArgumentException(UiText.Say($"Lingua delle descrizioni '{language}' non prevista: it oppure en.", $"Description language '{language}' not supported: it or en."), nameof(language)),
            };
            return s.Trivial switch
            {
                "getter" => en ? $"Returns the field `{s.TrivialOf}`." : $"Restituisce il campo `{s.TrivialOf}`.",
                "setter" => en ? $"Sets the field `{s.TrivialOf}`." : $"Assegna il campo `{s.TrivialOf}`.",
                "constant" => en ? $"Always returns `{s.TrivialOf}`." : $"Restituisce sempre `{s.TrivialOf}`.",
                "empty" => en ? "Does nothing (empty body)." : "Non fa niente (corpo vuoto).",
                _ => throw new InvalidOperationException($"forma di metodo banale '{s.Trivial}' sconosciuta"),
            };
        }

        /// <summary>
        /// La descrizione del file si chiede quando manca (o con <paramref name="force"/>). Non si rifà a ogni modifica del
        /// sorgente: a che cosa serve un file cambia di rado, e ogni giro costa una chiamata all'LLM; «Riscrivi la
        /// descrizione del file» dal tasto destro la rifà.
        /// </summary>
        public static bool NeedsFileDoc(CodeScript existingDoc, bool force) => force || existingDoc?.FileDoc == null;

        public const int MaxSourceChars = 60_000;
        /// <summary>Quanti simboli per domanda quando il file è troppo lungo per andare intero.</summary>
        public const int ExcerptBatchSize = 12;

        /// <summary>
        /// Il sorgente di una domanda su un ciclo che attraversa più file (F12a): per ogni file, sotto la sua intestazione
        /// <c>// ==== file ====</c>, solo le righe dei suoi simboli (<see cref="CodeSourceExcerpt"/>).
        /// </summary>
        public static string CycleSource(IReadOnlyDictionary<string, string> sources, IReadOnlyList<CodeDocCandidate> candidates, IReadOnlyDictionary<string, CodeScript> asts)
        {
            var sb = new StringBuilder();
            foreach (var file in candidates.GroupBy(c => c.Source).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                sb.Append("// ==== ").Append(file.Key).Append(" ====\n");
                sb.Append(CodeSourceExcerpt.For(sources[file.Key], file.Select(c => c.Symbol), asts[file.Key]));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Che cosa mandare all'LLM per una domanda su UN file (22/10/2026): mai più il file intero, solo le righe dei simboli
        /// da descrivere (<see cref="CodeSourceExcerpt.For"/>); se non ce ne sono (la sola descrizione del file),
        /// l'intestazione. Se gli estratti superano <see cref="MaxSourceChars"/>, a gruppi di <see cref="ExcerptBatchSize"/> simboli.
        /// </summary>
        public static List<(IReadOnlyList<CodeDocCandidate> Candidates, string Source)> Pieces(string sourceText, IReadOnlyList<CodeDocCandidate> candidates, CodeScript ast, bool askFile = false)
        {
            var list = new List<(IReadOnlyList<CodeDocCandidate>, string)>();
            if (candidates.Count == 0) { if (askFile) list.Add((candidates, CodeSourceExcerpt.Header(sourceText, ast))); return list; }
            var all = CodeSourceExcerpt.For(sourceText, candidates.Select(c => c.Symbol), ast);
            if (all.Length <= MaxSourceChars) { list.Add((candidates, all)); return list; }
            var ordered = candidates.OrderBy(c => c.Symbol.LineStart ?? 0).ToList();
            for (var i = 0; i < ordered.Count; i += ExcerptBatchSize)
            {
                var batch = ordered.Skip(i).Take(ExcerptBatchSize).ToList();
                list.Add((batch, CodeSourceExcerpt.For(sourceText, batch.Select(c => c.Symbol), ast)));
            }
            return list;
        }

        /// <summary>Quello che si sa intorno a un simbolo quando lo si descrive dal basso (F12a): chi chiama e, per un tipo, i membri.</summary>
        public sealed class SymbolContext
        {
            public IReadOnlyList<CodeDocCall> Calls { get; init; }
            public IReadOnlyList<CodeDocCall> Members { get; init; }
        }

        private static object CallJson(CodeDocCall c) => c.Candidates != null
            ? new { line = c.Line, ambiguous = c.Candidates.Select(CallJson).ToList() }
            : new
            {
                id = c.Id, line = c.Line, name = c.QualifiedName, signature = c.Signature,
                external = c.External ? true : (bool?)null,
                summary = c.External ? null : c.Summary,
                notYetDescribed = !c.External && c.Summary == null ? true : (bool?)null,
            };

        /// <summary>
        /// Il prompt: la skill, il sorgente, i simboli da descrivere (JSON Lines), la lingua e il formato della risposta. La
        /// lingua la decide chi chiama (quella dell'interfaccia): «la lingua dei commenti» lasciata al modello dava descrizioni
        /// in italiano su codice commentato in inglese. Con <paramref name="context"/> (descrizione dal basso, F12a) ogni
        /// simbolo porta le funzioni che chiama (<c>calls</c>) e, se è un tipo, i suoi membri (<c>members</c>), con le
        /// sintesi già scritte.
        /// </summary>
        public static string BuildPrompt(string skillBody, string sourceRelativePath, string sourceText, IReadOnlyList<CodeDocCandidate> candidates, string language, bool askFile = false,
            IReadOnlyDictionary<string, SymbolContext> context = null, IReadOnlyList<CodeDocCall> fileContents = null)
        {
            var languageName = language switch
            {
                "it" => "italiano",
                "en" => "inglese (English)",
                _ => throw new ArgumentException(UiText.Say($"Lingua delle descrizioni '{language}' non prevista: it oppure en.", $"Description language '{language}' not supported: it or en."), nameof(language)),
            };
            if (sourceText.Length > MaxSourceChars) sourceText = sourceText.Substring(0, MaxSourceChars) + "\n/* … troncato … */";
            var sb = new StringBuilder();
            sb.Append(skillBody.Trim()).Append("\n\n");
            sb.Append("---- FILE ----\n").Append(sourceRelativePath).Append('\n').Append(sourceText).Append("\n\n");
            if (askFile)
            {
                sb.Append("---- DESCRIVI ANCHE IL FILE ----\nScrivi una riga {\"type\":\"fileDoc\",\"summary\":\"…\"} con a che cosa serve il file.\n");
                // il file non arriva intero (22/10/2026): a che cosa serve si ricava dai suoi simboli già descritti
                if (fileContents is { Count: > 0 })
                {
                    sb.Append("I simboli del file, con le descrizioni già scritte (uno per riga):\n");
                    foreach (var f in fileContents)
                        sb.Append(JsonSerializer.Serialize(new { name = f.QualifiedName, signature = f.Signature, summary = f.Summary },
                            new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping })).Append('\n');
                }
                sb.Append('\n');
            }
            sb.Append("---- SIMBOLI DA DESCRIVERE (uno per riga) ----\n");
            foreach (var c in candidates)
            {
                SymbolContext ctx = null;
                context?.TryGetValue(c.Symbol.Id, out ctx);
                sb.Append(JsonSerializer.Serialize(new
                {
                    symbolId = c.Symbol.Id, kind = c.Symbol.Kind, qualifiedName = c.Symbol.QualifiedName, signature = c.Symbol.Signature,
                    returnType = c.Symbol.ReturnType, file = c.Source != sourceRelativePath ? c.Source : null, lineStart = c.Symbol.LineStart,
                    onlyParameters = c.ParametersOnly ? true : (bool?)null,
                    parameters = c.Parameters.Select(p => new { name = p.Name, type = p.ParameterType, direction = p.Direction, defaultValue = p.DefaultValue }),
                    calls = ctx?.Calls is { Count: > 0 } calls ? calls.Select(CallJson).ToList() : null,
                    members = ctx?.Members is { Count: > 0 } members ? members.Select(CallJson).ToList() : null,
                }, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping })).Append('\n');
            }
            sb.Append("\n---- LINGUA ----\nScrivi summary, returnsDescription, remarks, description dei parametri e la descrizione del file in ").Append(languageName).Append(".\n");
            sb.Append("\n---- RISPOSTA ----\n");
            return sb.ToString();
        }

        /// <summary>
        /// Legge la risposta dell'AI (JSON Lines di <c>symbolDoc</c>/<c>parameterDoc</c>): si tengono solo i record dei
        /// simboli chiesti, con il <c>signatureHash</c> calcolato da noi; righe non JSON o fuori formato finiscono in
        /// <paramref name="rejected"/> con il motivo, mai ignorate in silenzio.
        /// </summary>
        public static CodeScript ParseAnswer(string answer, string sourceRelativePath, IReadOnlyList<CodeDocCandidate> candidates, string writtenBy, out List<string> rejected, bool askedFile = false,
            Func<string, int?, List<string>> ambiguousAt = null)
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
                if (line.Length == 0 || !line.StartsWith('{')) { if (line.Length > 0) rejected.Add(UiText.Say($"riga {lineNo}: non è un record JSON: {Shorten(line)}", $"line {lineNo}: not a JSON record: {Shorten(line)}")); continue; }
                // la risposta dell'LLM è JSON Lines (formato di scambio, non degli script): si rilegge come schema 1
                var header = JsonSerializer.Serialize(new { schema = CodeScriptParser.LegacyJsonSchema, kind = "doc", source = sourceRelativePath, writtenBy, writtenAt = result.Header.WrittenAt });
                CodeScript one;
                try { one = CodeScriptParser.Parse(header + "\n" + line, sourceRelativePath); }
                catch (CodeScriptFormatException ex) { rejected.Add(UiText.Say($"riga {lineNo}: {ex.Message}", $"line {lineNo}: {ex.Message}")); continue; }
                if (one.FileDoc != null)
                {
                    if (!askedFile) rejected.Add(UiText.Say($"riga {lineNo}: fileDoc non richiesto", $"line {lineNo}: fileDoc not requested"));
                    else result.FileDoc = one.FileDoc;
                }
                foreach (var d in one.SymbolDocs)
                {
                    if (!byId.TryGetValue(d.SymbolId, out var c)) { rejected.Add(UiText.Say($"riga {lineNo}: symbolId '{d.SymbolId}' non richiesto", $"line {lineNo}: symbolId '{d.SymbolId}' not requested")); continue; }
                    if (c.ParametersOnly) { rejected.Add(UiText.Say($"riga {lineNo}: symbolDoc per lo script stesso ('{d.SymbolId}'): a che cosa serve lo dice fileDoc, qui solo i parameterDoc", $"line {lineNo}: symbolDoc for the script itself ('{d.SymbolId}'): fileDoc says what it is for, here only parameterDoc")); continue; }
                    d.SignatureHash = c.SignatureHash;
                    result.SymbolDocs.RemoveAll(x => x.SymbolId == d.SymbolId);
                    result.SymbolDocs.Add(d);
                }
                // la scelta su una chiamata ambigua (F12c): solo fra le candidate di quella riga; le scartate le dice l'AST
                foreach (var r in one.CallResolutions)
                {
                    if (!byId.ContainsKey(r.SourceId)) { rejected.Add(UiText.Say($"riga {lineNo}: callResolution per '{r.SourceId}', simbolo non richiesto", $"line {lineNo}: callResolution for '{r.SourceId}', symbol not requested")); continue; }
                    var options = ambiguousAt?.Invoke(r.SourceId, r.Line) ?? new List<string>();
                    if (!options.Contains(r.TargetId)) { rejected.Add(UiText.Say($"riga {lineNo}: '{r.TargetId}' non è una candidata della chiamata ambigua alla riga {r.Line} di {byId[r.SourceId].Symbol.QualifiedName}", $"line {lineNo}: '{r.TargetId}' is not a candidate of the ambiguous call at line {r.Line} of {byId[r.SourceId].Symbol.QualifiedName}")); continue; }
                    r.Rejected.AddRange(options.Where(o => o != r.TargetId));
                    result.CallResolutions.RemoveAll(x => x.SourceId == r.SourceId && x.Line == r.Line && x.TargetId == r.TargetId);
                    result.CallResolutions.Add(r);
                }
                foreach (var p in one.ParameterDocs)
                {
                    if (!byId.TryGetValue(p.SymbolId, out var c)) { rejected.Add(UiText.Say($"riga {lineNo}: symbolId '{p.SymbolId}' non richiesto", $"line {lineNo}: symbolId '{p.SymbolId}' not requested")); continue; }
                    if (!c.Parameters.Any(x => x.Name == p.ParameterName)) { rejected.Add(UiText.Say($"riga {lineNo}: parametro '{p.ParameterName}' non esiste in {c.Symbol.QualifiedName}", $"line {lineNo}: parameter '{p.ParameterName}' does not exist in {c.Symbol.QualifiedName}")); continue; }
                    result.ParameterDocs.RemoveAll(x => x.SymbolId == p.SymbolId && x.ParameterName == p.ParameterName);
                    result.ParameterDocs.Add(p);
                }
            }
            return result;
        }

        /// <summary>Fonde le descrizioni nuove in quelle esistenti: i simboli non toccati restano, quelli descritti ora si sostituiscono.</summary>
        public static CodeScript Merge(CodeScript existing, CodeScript fresh)
        {
            var merged = new CodeScript { Header = fresh.Header, FileDoc = fresh.FileDoc ?? existing?.FileDoc };
            var touched = new HashSet<string>(fresh.SymbolDocs.Select(d => d.SymbolId).Concat(fresh.ParameterDocs.Select(p => p.SymbolId)), StringComparer.Ordinal);
            if (existing != null)
            {
                // chi resta tiene il suo autore: prima prendeva quello del giro nuovo (D34, autore per descrizione)
                foreach (var d in existing.SymbolDocs.Where(d => !touched.Contains(d.SymbolId)))
                {
                    d.WrittenBy ??= existing.Header?.WrittenBy;
                    if (d.WrittenBy == fresh.Header?.WrittenBy) d.WrittenBy = null;
                    merged.SymbolDocs.Add(d);
                }
                merged.ParameterDocs.AddRange(existing.ParameterDocs.Where(p => !touched.Contains(p.SymbolId)));
            }
            merged.SymbolDocs.AddRange(fresh.SymbolDocs);
            merged.ParameterDocs.AddRange(fresh.ParameterDocs);
            // le scelte sulle chiamate ambigue: la nuova su (chiamante, riga) sostituisce la vecchia, le altre restano
            var resolved = new HashSet<(string, int?)>(fresh.CallResolutions.Select(r => (r.SourceId, r.Line)));
            if (existing != null) merged.CallResolutions.AddRange(existing.CallResolutions.Where(r => !resolved.Contains((r.SourceId, r.Line))));
            merged.CallResolutions.AddRange(fresh.CallResolutions);
            return merged;
        }

        /// <summary>
        /// Toglie le descrizioni di simboli che l'AST non ha più (metodi cancellati) e di parametri che il simbolo non ha
        /// più (rinominati, tolti): si segnalano, non si tengono per sempre.
        /// </summary>
        public static int DropOrphans(CodeScript doc, CodeScript ast)
        {
            var alive = new HashSet<string>(ast.Symbols.Select(s => s.Id), StringComparer.Ordinal);
            var aliveParams = new HashSet<(string, string)>(ast.Parameters.Select(p => (p.SymbolId, p.Name)));
            var n = doc.SymbolDocs.RemoveAll(d => !alive.Contains(d.SymbolId));
            n += doc.ParameterDocs.RemoveAll(p => !aliveParams.Contains((p.SymbolId, p.ParameterName)));
            // una scelta su una chiamata ambigua vale finché le candidate di quella riga sono le stesse (F12c): se la chiamata
            // è sparita, o il parser ora la risolve, o è arrivata una candidata nuova, la scelta non regge più
            var ambiguous = ast.Edges.Where(e => e.Relation == "calls" && e.Confidence == "ambiguous")
                .GroupBy(e => (e.SourceId, e.Line)).ToDictionary(g => g.Key, g => g.Select(e => e.TargetId).ToHashSet(StringComparer.Ordinal));
            n += doc.CallResolutions.RemoveAll(r => !ambiguous.TryGetValue((r.SourceId, r.Line), out var set)
                || !set.SetEquals(r.Rejected.Append(r.TargetId)));
            return n;
        }

        /// <summary>
        /// Quante voci ha scritto una risposta, per i conti della finestra: i simboli descritti, lo script di cui sono
        /// arrivati i parametri (non ha un symbolDoc) e il file.
        /// </summary>
        public static int CountDescribed(CodeScript fresh, IEnumerable<CodeDocCandidate> asked)
        {
            var paramsOnly = new HashSet<string>(asked.Where(c => c.ParametersOnly).Select(c => c.Symbol.Id), StringComparer.Ordinal);
            return fresh.SymbolDocs.Count
                   + fresh.ParameterDocs.Select(p => p.SymbolId).Distinct(StringComparer.Ordinal).Count(paramsOnly.Contains)
                   + (fresh.FileDoc != null ? 1 : 0);
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
