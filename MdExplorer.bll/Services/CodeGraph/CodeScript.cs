using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Uno script non rispetta il formato: il messaggio dice file, riga e che cosa non va. Mai ignorato in silenzio.</summary>
    public sealed class CodeScriptFormatException : Exception
    {
        public CodeScriptFormatException(string scriptPath, int line, string why)
            : base(UiText.Say($"'{scriptPath}', riga {line}: {why}", $"'{scriptPath}', line {line}: {why}")) { ScriptPath = scriptPath; Line = line; }
        public string ScriptPath { get; }
        public int Line { get; }
    }

    /// <summary><c>Rule</c>: gli archi prodotti da una regola di arricchimento (D42), in <c>.mde-code/_rules/</c>.</summary>
    public enum CodeScriptKind { Ast, Doc, Rule }

    /// <summary>La prima riga di uno script: chi l'ha scritto, da quale sorgente, con quale schema.</summary>
    public sealed class CodeScriptHeader
    {
        public int Schema { get; set; }
        public CodeScriptKind Kind { get; set; }
        /// <summary>Il file sorgente, relativo alla radice del progetto, con <c>/</c>; per uno script di regola, l'id della regola.</summary>
        public string Source { get; set; }
        public string Language { get; set; }
        // ast
        public string SourceHash { get; set; }
        public string Extractor { get; set; }
        public string ExtractorVersion { get; set; }
        public string GeneratedAt { get; set; }
        // doc
        public string WrittenBy { get; set; }
        public string WrittenAt { get; set; }
    }

    public sealed class ScriptSymbol
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
        public string QualifiedName { get; set; }
        public int? LineStart { get; set; }
        public int? LineEnd { get; set; }
        public string Signature { get; set; }
        public string ReturnType { get; set; }
        public string Visibility { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAsync { get; set; }
        public string ParentId { get; set; }
        /// <summary>
        /// Un metodo «finto» (D41, 21/10/2026): <c>getter</c>, <c>setter</c>, <c>constant</c> o <c>empty</c>, riconosciuto
        /// dall'estrattore dal corpo di un'istruzione sola. Non va all'LLM e non conta nei livelli della descrizione dal
        /// basso: la sua descrizione la scrive MdExplorer. Null = un metodo vero.
        /// </summary>
        public string Trivial { get; set; }
        /// <summary>
        /// L'impronta del corpo (F12b): hash delle righe del simbolo con gli spazi compattati, calcolata dopo l'estrazione
        /// per ogni linguaggio (<see cref="CodeBodyHash"/>), solo per chi ha un corpo che si esegue (metodi, funzioni,
        /// costruttori, proprietà). Null = non calcolata (script di prima del 21/10/2026, o un tipo).
        /// </summary>
        public string BodyHash { get; set; }
        /// <summary>Il campo letto o scritto, o la costante restituita (com'è scritta nel codice); null per <c>empty</c>.</summary>
        public string TrivialOf { get; set; }
        /// <summary><c>external</c> = fuori perimetro: nessun file, nessuna riga, condiviso fra gli script.</summary>
        public bool IsExternal => string.Equals(Kind, "external", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class ScriptParameter
    {
        public string SymbolId { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public string ParameterType { get; set; }
        public string Direction { get; set; } = "in";
        public string DefaultValue { get; set; }
        public bool IsNullable { get; set; }
    }

    public sealed class ScriptEdge
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string Relation { get; set; }
        public string Confidence { get; set; }
        public int? Line { get; set; }
        /// <summary>Solo negli script di regola: il file del chiamante (negli ast è il sorgente dello script).</summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// Una chiamata che passa almeno una stringa letterale «a forma di nome» (D42, 23/10/2026): il nome di un metodo, di
    /// un template, un <c>alias.metodo</c>. Per il parser è testo, non una chiamata: le regole di arricchimento la leggono
    /// e aggiungono gli archi. Le frasi (con spazi) non si registrano: sono messaggi di log ed eccezioni
    /// (<see cref="CodeCallSites.IsNameLike"/>).
    /// </summary>
    public sealed class ScriptCallSite
    {
        /// <summary>Il simbolo che contiene la chiamata.</summary>
        public string SourceId { get; set; }
        public int Line { get; set; }
        /// <summary>L'ordine fra le chiamate con stringhe dello stesso chiamante sulla stessa riga (0, 1…): rende l'IRI unico.</summary>
        public int Ordinal { get; set; }
        /// <summary>Il nome chiamato com'è scritto: <c>setTarget</c>.</summary>
        public string Callee { get; set; }
        /// <summary>Il ricevente com'è scritto (<c>svc</c>, <c>this.svc</c>, <c>factory.create("a")</c>); null senza ricevente o se troppo lungo.</summary>
        public string Receiver { get; set; }
        public int ArgumentCount { get; set; }
        public List<ScriptStringArgument> Arguments { get; } = new();
    }

    public sealed class ScriptStringArgument
    {
        /// <summary>Da 0, come i parametri.</summary>
        public int Position { get; set; }
        /// <summary>Il contenuto, senza virgolette.</summary>
        public string Value { get; set; }
    }

    public sealed class ScriptSymbolDoc
    {
        public string SymbolId { get; set; }
        public string Summary { get; set; }
        public string ReturnsDescription { get; set; }
        public string Remarks { get; set; }
        public string SignatureHash { get; set; }
        /// <summary>
        /// Chi ha scritto QUESTA descrizione, se non è chi ha scritto lo script (intestazione): le descrizioni fisse dei
        /// metodi banali (D41) le scrive MdExplorer, quelle rimaste da un giro precedente il loro LLM. Null = l'intestazione.
        /// </summary>
        public string WrittenBy { get; set; }
        /// <summary>L'impronta del corpo quando la descrizione è stata scritta (F12b): diversa da quella di oggi = da rifare.</summary>
        public string BodyHash { get; set; }
        /// <summary>
        /// L'impronta di quello che la descrizione sapeva intorno al simbolo (F12b): le sintesi delle funzioni chiamate e,
        /// per un tipo, dei membri, come erano nel prompt. Diversa da quella di oggi = «da rivedere» (D39 b), non rifatta.
        /// </summary>
        public string ContextHash { get; set; }
        /// <summary>La versione del prompt mde-code-doc che l'ha scritta: più vecchia di quella di oggi = «da rivedere».</summary>
        public string PromptVersion { get; set; }
    }

    /// <summary>
    /// La scelta dell'LLM su una chiamata ambigua (F12c, D39 «llm-resolved»): in quella riga il chiamante chiama davvero
    /// <see cref="TargetId"/>, non le altre candidate (<see cref="Rejected"/>). Sta nello script delle descrizioni, mai in
    /// quello AST: il fatto del parser («ambigua») resta com'è; le viste mostrano la scelta come <c>llm-resolved</c> e
    /// nascondono le scartate (utente, 22/10/2026).
    /// </summary>
    public sealed class ScriptCallResolution
    {
        public string SourceId { get; set; }
        public int? Line { get; set; }
        public string TargetId { get; set; }
        public List<string> Rejected { get; } = new();
    }

    public sealed class ScriptParameterDoc
    {
        public string SymbolId { get; set; }
        public string ParameterName { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Il FILE nel suo insieme (12/10/2026): a che cosa serve, in una o due frasi, scritto dall'LLM nello stesso giro
    /// delle descrizioni dei simboli. In Fuseki è un <c>code:FileDescription</c> che descrive il <c>code:File</c>: vale
    /// per ogni linguaggio (anche dove non c'è un simbolo module, come in C#).
    /// </summary>
    public sealed class ScriptFileDoc
    {
        public string Summary { get; set; }
    }

    /// <summary>Uno script letto e validato: l'intestazione più i record, già tipizzati.</summary>
    public sealed class CodeScript
    {
        public CodeScriptHeader Header { get; set; }
        public List<ScriptSymbol> Symbols { get; } = new();
        public List<ScriptParameter> Parameters { get; } = new();
        public List<ScriptEdge> Edges { get; } = new();
        public List<ScriptCallSite> CallSites { get; } = new();
        public List<ScriptSymbolDoc> SymbolDocs { get; } = new();
        public List<ScriptParameterDoc> ParameterDocs { get; } = new();
        public List<ScriptCallResolution> CallResolutions { get; } = new();
        /// <summary>Solo negli script di regola: che cosa fa la regola, in parole semplici (D42).</summary>
        public string RuleDescription { get; set; }
        /// <summary>Solo negli script di descrizioni: la descrizione del file, se l'LLM l'ha scritta.</summary>
        public ScriptFileDoc FileDoc { get; set; }
        public int RecordCount => Symbols.Count + Parameters.Count + Edges.Count + CallSites.Count + SymbolDocs.Count + ParameterDocs.Count + (FileDoc != null ? 1 : 0);
    }

    /// <summary>
    /// Il formato degli script (D12): JSON Lines, prima riga l'intestazione, poi un record per riga con il campo
    /// <c>type</c>. Lettura <b>rigida</b>: un campo che il formato non conosce, un campo obbligatorio assente, un
    /// <c>type</c> ignoto o uno <c>schema</c> più nuovo di quelli che il programma sa leggere sono errori con riga
    /// e nome del campo. Uno script che non si capisce non si carica a metà.
    /// </summary>
    public static class CodeScriptParser
    {
        /// <summary>
        /// Lo schema che questo MdExplorer scrive: 3 = Turtle con il programma nell'id di ogni simbolo
        /// (<see cref="CodeProgramScope"/>, 14/10/2026). Il 2 (Turtle, 12/10) e l'1 (JSON Lines) sono ritirati: gli script
        /// si cancellano e si rifanno (<see cref="ICodeGraphStore.RetireOldScriptsAsync"/>). Il JSON Lines resta solo
        /// come formato delle risposte dell'LLM (<see cref="CodeDocPlanner"/>).
        /// </summary>
        public const int CurrentSchema = 3;
        public const int LegacyJsonSchema = 1;

        private static readonly HashSet<string> HeaderFields = new(StringComparer.Ordinal)
            { "schema", "kind", "source", "language", "sourceHash", "extractor", "extractorVersion", "generatedAt", "writtenBy", "writtenAt" };
        private static readonly HashSet<string> SymbolFields = new(StringComparer.Ordinal)
            { "type", "id", "kind", "name", "qualifiedName", "lineStart", "lineEnd", "signature", "returnType", "visibility", "isStatic", "isAsync", "parentId" };
        private static readonly HashSet<string> ParameterFields = new(StringComparer.Ordinal)
            { "type", "symbolId", "position", "name", "parameterType", "direction", "defaultValue", "isNullable" };
        private static readonly HashSet<string> EdgeFields = new(StringComparer.Ordinal)
            { "type", "sourceId", "targetId", "relation", "confidence", "line" };
        private static readonly HashSet<string> SymbolDocFields = new(StringComparer.Ordinal)
            { "type", "symbolId", "summary", "returnsDescription", "remarks", "signatureHash" };
        private static readonly HashSet<string> ParameterDocFields = new(StringComparer.Ordinal)
            { "type", "symbolId", "parameterName", "description" };
        private static readonly HashSet<string> FileDocFields = new(StringComparer.Ordinal)
            { "type", "summary" };
        private static readonly HashSet<string> CallResolutionFields = new(StringComparer.Ordinal)
            { "type", "symbolId", "line", "target" };

        public static readonly string[] SymbolKinds =
            { "namespace", "module", "class", "interface", "record", "struct", "enum", "method", "constructor", "property", "field", "function", "type",
              "component", "service", "directive", "pipe", "ngmodule", "external",
              // un pezzo di un blocco troppo lungo, dalla sua etichetta alla successiva (linguaggi dichiarativi)
              "fragment" };
        public static readonly string[] Relations =
            { "contains", "imports", "inherits", "implements", "references", "calls", "instantiates",
              // il codice passa da un frammento all'altro: GOTO, o prosegue nel pezzo che segue
              "jumps",
              // smistamento per stringa: il nome del metodo viaggia in una stringa (D42, solo dalle regole)
              "dispatches" };
        /// <summary><c>rule</c>: l'arco l'ha prodotto una regola di arricchimento (D42), mai il parser.</summary>
        public static readonly string[] Confidences = { "extracted", "inferred", "ambiguous", "rule" };
        public static readonly string[] Directions = { "in", "out", "ref", "params", "this" };
        public static readonly string[] TrivialShapes = { "getter", "setter", "constant", "empty" };

        /// <summary>Uno script Turtle, o il JSON Lines delle risposte dell'LLM: il formato si riconosce dalla prima riga.</summary>
        public static CodeScript Parse(string content, string scriptPath)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (CodeScriptTurtle.IsTurtle(content)) return CodeScriptTurtle.Read(content, scriptPath);
            return ParseLegacyJson(content, scriptPath);
        }

        private static CodeScript ParseLegacyJson(string content, string scriptPath)
        {
            var script = new CodeScript();
            var lineNo = 0;
            using var reader = new StringReader(content);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lineNo++;
                if (string.IsNullOrWhiteSpace(line)) continue;
                JsonDocument doc;
                try { doc = JsonDocument.Parse(line); }
                catch (JsonException ex) { throw new CodeScriptFormatException(scriptPath, lineNo, UiText.Say($"JSON non valido: {ex.Message}", $"Invalid JSON: {ex.Message}")); }
                using (doc)
                {
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Object)
                        throw new CodeScriptFormatException(scriptPath, lineNo, UiText.Say("ogni riga deve essere un oggetto JSON", "every line must be a JSON object"));

                    if (script.Header == null)
                    {
                        script.Header = ReadHeader(root, scriptPath, lineNo);
                        continue;
                    }
                    ReadRecord(root, script, scriptPath, lineNo);
                }
            }
            if (script.Header == null)
                throw new CodeScriptFormatException(scriptPath, 1, UiText.Say("manca l'intestazione (prima riga con schema, kind, source)", "header missing (first line with schema, kind, source)"));
            return script;
        }

        private static CodeScriptHeader ReadHeader(JsonElement root, string path, int line)
        {
            RejectUnknown(root, HeaderFields, path, line, "intestazione");
            var h = new CodeScriptHeader
            {
                Schema = RequireInt(root, "schema", path, line),
                Source = RequireString(root, "source", path, line),
                Language = OptString(root, "language"),
                SourceHash = OptString(root, "sourceHash"),
                Extractor = OptString(root, "extractor"),
                ExtractorVersion = OptString(root, "extractorVersion"),
                GeneratedAt = OptString(root, "generatedAt"),
                WrittenBy = OptString(root, "writtenBy"),
                WrittenAt = OptString(root, "writtenAt"),
            };
            if (h.Schema != LegacyJsonSchema)
                throw new CodeScriptFormatException(path, line, UiText.Say($"schema {h.Schema} in JSON Lines non supportato: il JSON Lines è solo lo schema {LegacyJsonSchema}, gli script nuovi sono Turtle.", $"schema {h.Schema} in JSON Lines not supported: JSON Lines is only schema {LegacyJsonSchema}, new scripts are Turtle."));
            var kind = RequireString(root, "kind", path, line);
            switch (kind)
            {
                case "ast":
                    h.Kind = CodeScriptKind.Ast;
                    if (h.SourceHash == null) throw Missing(path, line, "sourceHash", "intestazione ast");
                    if (h.Extractor == null) throw Missing(path, line, "extractor", "intestazione ast");
                    if (h.ExtractorVersion == null) throw Missing(path, line, "extractorVersion", "intestazione ast");
                    if (h.Language == null) throw Missing(path, line, "language", "intestazione ast");
                    break;
                case "doc":
                    h.Kind = CodeScriptKind.Doc;
                    if (h.WrittenBy == null) throw Missing(path, line, "writtenBy", "intestazione doc");
                    if (h.WrittenAt == null) throw Missing(path, line, "writtenAt", "intestazione doc");
                    break;
                default:
                    throw new CodeScriptFormatException(path, line, UiText.Say($"kind '{kind}' sconosciuto: ammessi 'ast' e 'doc'", $"unknown kind '{kind}': allowed 'ast' and 'doc'"));
            }
            return h;
        }

        private static void ReadRecord(JsonElement root, CodeScript script, string path, int line)
        {
            var type = RequireString(root, "type", path, line);
            var isAst = script.Header.Kind == CodeScriptKind.Ast;
            switch (type)
            {
                case "symbol":
                    if (!isAst) throw WrongKind(path, line, type, "ast");
                    RejectUnknown(root, SymbolFields, path, line, type);
                    var s = new ScriptSymbol
                    {
                        Id = RequireString(root, "id", path, line),
                        Kind = RequireEnum(root, "kind", SymbolKinds, path, line),
                        Name = RequireString(root, "name", path, line),
                        QualifiedName = RequireString(root, "qualifiedName", path, line),
                        LineStart = OptInt(root, "lineStart", path, line),
                        LineEnd = OptInt(root, "lineEnd", path, line),
                        Signature = OptString(root, "signature"),
                        ReturnType = OptString(root, "returnType"),
                        Visibility = OptString(root, "visibility"),
                        IsStatic = OptBool(root, "isStatic", path, line),
                        IsAsync = OptBool(root, "isAsync", path, line),
                        ParentId = OptString(root, "parentId"),
                    };
                    if (!s.IsExternal && s.LineStart == null)
                        throw Missing(path, line, "lineStart", UiText.Say($"symbol '{s.QualifiedName}' (solo 'external' può non avere righe)", $"symbol '{s.QualifiedName}' (only 'external' may have no lines)"));
                    script.Symbols.Add(s);
                    break;
                case "parameter":
                    if (!isAst) throw WrongKind(path, line, type, "ast");
                    RejectUnknown(root, ParameterFields, path, line, type);
                    script.Parameters.Add(new ScriptParameter
                    {
                        SymbolId = RequireString(root, "symbolId", path, line),
                        Position = RequireInt(root, "position", path, line),
                        Name = RequireString(root, "name", path, line),
                        ParameterType = OptString(root, "parameterType"),
                        Direction = root.TryGetProperty("direction", out _) ? RequireEnum(root, "direction", Directions, path, line) : "in",
                        DefaultValue = OptString(root, "defaultValue"),
                        IsNullable = OptBool(root, "isNullable", path, line),
                    });
                    break;
                case "edge":
                    if (!isAst) throw WrongKind(path, line, type, "ast");
                    RejectUnknown(root, EdgeFields, path, line, type);
                    script.Edges.Add(new ScriptEdge
                    {
                        SourceId = RequireString(root, "sourceId", path, line),
                        TargetId = RequireString(root, "targetId", path, line),
                        Relation = RequireEnum(root, "relation", Relations, path, line),
                        Confidence = RequireEnum(root, "confidence", Confidences, path, line),
                        Line = OptInt(root, "line", path, line),
                    });
                    break;
                case "fileDoc":
                    if (isAst) throw WrongKind(path, line, type, "doc");
                    RejectUnknown(root, FileDocFields, path, line, type);
                    var fd = new ScriptFileDoc { Summary = OptString(root, "summary") };
                    if (string.IsNullOrWhiteSpace(fd.Summary)) throw Missing(path, line, "summary", "fileDoc");
                    if (script.FileDoc != null) throw new CodeScriptFormatException(path, line, UiText.Say("un solo fileDoc per script", "only one fileDoc per script"));
                    script.FileDoc = fd;
                    break;
                case "callResolution":
                    // solo dalle risposte dell'LLM (le candidate scartate le aggiunge CodeDocPlanner, che conosce l'AST)
                    if (isAst) throw WrongKind(path, line, type, "doc");
                    RejectUnknown(root, CallResolutionFields, path, line, type);
                    script.CallResolutions.Add(new ScriptCallResolution
                    {
                        SourceId = RequireString(root, "symbolId", path, line),
                        Line = OptInt(root, "line", path, line) ?? throw Missing(path, line, "line", "callResolution"),
                        TargetId = RequireString(root, "target", path, line),
                    });
                    break;
                case "symbolDoc":
                    if (isAst) throw WrongKind(path, line, type, "doc");
                    RejectUnknown(root, SymbolDocFields, path, line, type);
                    var d = new ScriptSymbolDoc
                    {
                        SymbolId = RequireString(root, "symbolId", path, line),
                        Summary = OptString(root, "summary"),
                        ReturnsDescription = OptString(root, "returnsDescription"),
                        Remarks = OptString(root, "remarks"),
                        SignatureHash = OptString(root, "signatureHash"),
                    };
                    if (string.IsNullOrWhiteSpace(d.Summary)) throw Missing(path, line, "summary", "symbolDoc");
                    script.SymbolDocs.Add(d);
                    break;
                case "parameterDoc":
                    if (isAst) throw WrongKind(path, line, type, "doc");
                    RejectUnknown(root, ParameterDocFields, path, line, type);
                    script.ParameterDocs.Add(new ScriptParameterDoc
                    {
                        SymbolId = RequireString(root, "symbolId", path, line),
                        ParameterName = RequireString(root, "parameterName", path, line),
                        Description = RequireString(root, "description", path, line),
                    });
                    break;
                default:
                    throw new CodeScriptFormatException(path, line,
                        UiText.Say($"type '{type}' sconosciuto: ammessi symbol, parameter, edge (ast) e fileDoc, symbolDoc, parameterDoc (doc)", $"unknown type '{type}': allowed symbol, parameter, edge (ast) and fileDoc, symbolDoc, parameterDoc (doc)"));
            }
        }

        // ---- helpers ----

        private static void RejectUnknown(JsonElement root, HashSet<string> allowed, string path, int line, string what)
        {
            foreach (var prop in root.EnumerateObject())
            {
                if (!allowed.Contains(prop.Name))
                    throw new CodeScriptFormatException(path, line,
                        UiText.Say($"campo '{prop.Name}' sconosciuto in '{what}'. Ammessi: {string.Join(", ", allowed.OrderBy(a => a))}", $"unknown field '{prop.Name}' in '{what}'. Allowed: {string.Join(", ", allowed.OrderBy(a => a))}"));
            }
        }

        private static CodeScriptFormatException Missing(string path, int line, string field, string what)
            => new(path, line, UiText.Say($"manca il campo obbligatorio '{field}' in '{what}'", $"required field '{field}' missing in '{what}'"));

        private static CodeScriptFormatException WrongKind(string path, int line, string type, string expectedKind)
            => new(path, line, UiText.Say($"il record '{type}' sta solo in uno script '{expectedKind}'", $"the record '{type}' belongs only in a '{expectedKind}' script"));

        private static string RequireString(JsonElement root, string name, string path, int line)
        {
            if (!root.TryGetProperty(name, out var el) || el.ValueKind == JsonValueKind.Null) throw Missing(path, line, name, "record");
            if (el.ValueKind != JsonValueKind.String) throw new CodeScriptFormatException(path, line, UiText.Say($"'{name}' deve essere una stringa", $"'{name}' must be a string"));
            var v = el.GetString();
            if (string.IsNullOrWhiteSpace(v)) throw new CodeScriptFormatException(path, line, UiText.Say($"'{name}' è vuoto", $"'{name}' is empty"));
            return v;
        }

        private static string OptString(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var el) || el.ValueKind == JsonValueKind.Null) return null;
            return el.ValueKind == JsonValueKind.String ? el.GetString() : el.GetRawText();
        }

        private static int RequireInt(JsonElement root, string name, string path, int line)
        {
            var v = OptInt(root, name, path, line);
            return v ?? throw Missing(path, line, name, "record");
        }

        private static int? OptInt(JsonElement root, string name, string path, int line)
        {
            if (!root.TryGetProperty(name, out var el) || el.ValueKind == JsonValueKind.Null) return null;
            if (el.ValueKind != JsonValueKind.Number || !el.TryGetInt32(out var v))
                throw new CodeScriptFormatException(path, line, UiText.Say($"'{name}' deve essere un numero intero", $"'{name}' must be an integer"));
            return v;
        }

        private static bool OptBool(JsonElement root, string name, string path, int line)
        {
            if (!root.TryGetProperty(name, out var el) || el.ValueKind == JsonValueKind.Null) return false;
            if (el.ValueKind != JsonValueKind.True && el.ValueKind != JsonValueKind.False)
                throw new CodeScriptFormatException(path, line, UiText.Say($"'{name}' deve essere true o false", $"'{name}' must be true or false"));
            return el.GetBoolean();
        }

        private static string RequireEnum(JsonElement root, string name, string[] allowed, string path, int line)
        {
            var v = RequireString(root, name, path, line);
            if (!allowed.Contains(v, StringComparer.Ordinal))
                throw new CodeScriptFormatException(path, line, UiText.Say($"'{name}' = '{v}' non ammesso. Ammessi: {string.Join(", ", allowed)}", $"'{name}' = '{v}' not allowed. Allowed: {string.Join(", ", allowed)}"));
            return v;
        }
    }
}
