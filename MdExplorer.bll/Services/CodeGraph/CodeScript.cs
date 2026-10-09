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
            : base($"'{scriptPath}', riga {line}: {why}") { ScriptPath = scriptPath; Line = line; }
        public string ScriptPath { get; }
        public int Line { get; }
    }

    public enum CodeScriptKind { Ast, Doc }

    /// <summary>La prima riga di uno script: chi l'ha scritto, da quale sorgente, con quale schema.</summary>
    public sealed class CodeScriptHeader
    {
        public int Schema { get; set; }
        public CodeScriptKind Kind { get; set; }
        /// <summary>Il file sorgente, relativo alla radice del progetto, con <c>/</c>.</summary>
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
    }

    public sealed class ScriptSymbolDoc
    {
        public string SymbolId { get; set; }
        public string Summary { get; set; }
        public string ReturnsDescription { get; set; }
        public string Remarks { get; set; }
        public string SignatureHash { get; set; }
    }

    public sealed class ScriptParameterDoc
    {
        public string SymbolId { get; set; }
        public string ParameterName { get; set; }
        public string Description { get; set; }
    }

    /// <summary>Uno script letto e validato: l'intestazione più i record, già tipizzati.</summary>
    public sealed class CodeScript
    {
        public CodeScriptHeader Header { get; set; }
        public List<ScriptSymbol> Symbols { get; } = new();
        public List<ScriptParameter> Parameters { get; } = new();
        public List<ScriptEdge> Edges { get; } = new();
        public List<ScriptSymbolDoc> SymbolDocs { get; } = new();
        public List<ScriptParameterDoc> ParameterDocs { get; } = new();
        public int RecordCount => Symbols.Count + Parameters.Count + Edges.Count + SymbolDocs.Count + ParameterDocs.Count;
    }

    /// <summary>
    /// Il formato degli script (D12): JSON Lines, prima riga l'intestazione, poi un record per riga con il campo
    /// <c>type</c>. Lettura <b>rigida</b>: un campo che il formato non conosce, un campo obbligatorio assente, un
    /// <c>type</c> ignoto o uno <c>schema</c> più nuovo di quelli che il programma sa leggere sono errori con riga
    /// e nome del campo. Uno script che non si capisce non si carica a metà.
    /// </summary>
    public static class CodeScriptParser
    {
        public const int CurrentSchema = 1;

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

        public static readonly string[] SymbolKinds =
            { "namespace", "module", "class", "interface", "record", "struct", "enum", "method", "constructor", "property", "field", "function", "type",
              "component", "service", "directive", "pipe", "ngmodule", "external" };
        public static readonly string[] Relations =
            { "contains", "imports", "inherits", "implements", "references", "calls", "instantiates" };
        public static readonly string[] Confidences = { "extracted", "inferred", "ambiguous" };
        public static readonly string[] Directions = { "in", "out", "ref", "params", "this" };

        public static CodeScript Parse(string content, string scriptPath)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
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
                catch (JsonException ex) { throw new CodeScriptFormatException(scriptPath, lineNo, $"JSON non valido: {ex.Message}"); }
                using (doc)
                {
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Object)
                        throw new CodeScriptFormatException(scriptPath, lineNo, "ogni riga deve essere un oggetto JSON");

                    if (script.Header == null)
                    {
                        script.Header = ReadHeader(root, scriptPath, lineNo);
                        continue;
                    }
                    ReadRecord(root, script, scriptPath, lineNo);
                }
            }
            if (script.Header == null)
                throw new CodeScriptFormatException(scriptPath, 1, "manca l'intestazione (prima riga con schema, kind, source)");
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
            if (h.Schema < 1 || h.Schema > CurrentSchema)
                throw new CodeScriptFormatException(path, line, $"schema {h.Schema} non supportato: questo MdExplorer legge fino allo schema {CurrentSchema}. Aggiorna MdExplorer.");
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
                    throw new CodeScriptFormatException(path, line, $"kind '{kind}' sconosciuto: ammessi 'ast' e 'doc'");
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
                        throw Missing(path, line, "lineStart", $"symbol '{s.QualifiedName}' (solo 'external' può non avere righe)");
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
                        $"type '{type}' sconosciuto: ammessi symbol, parameter, edge (ast) e symbolDoc, parameterDoc (doc)");
            }
        }

        // ---- helpers ----

        private static void RejectUnknown(JsonElement root, HashSet<string> allowed, string path, int line, string what)
        {
            foreach (var prop in root.EnumerateObject())
            {
                if (!allowed.Contains(prop.Name))
                    throw new CodeScriptFormatException(path, line,
                        $"campo '{prop.Name}' sconosciuto in '{what}'. Ammessi: {string.Join(", ", allowed.OrderBy(a => a))}");
            }
        }

        private static CodeScriptFormatException Missing(string path, int line, string field, string what)
            => new(path, line, $"manca il campo obbligatorio '{field}' in '{what}'");

        private static CodeScriptFormatException WrongKind(string path, int line, string type, string expectedKind)
            => new(path, line, $"il record '{type}' sta solo in uno script '{expectedKind}'");

        private static string RequireString(JsonElement root, string name, string path, int line)
        {
            if (!root.TryGetProperty(name, out var el) || el.ValueKind == JsonValueKind.Null) throw Missing(path, line, name, "record");
            if (el.ValueKind != JsonValueKind.String) throw new CodeScriptFormatException(path, line, $"'{name}' deve essere una stringa");
            var v = el.GetString();
            if (string.IsNullOrWhiteSpace(v)) throw new CodeScriptFormatException(path, line, $"'{name}' è vuoto");
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
                throw new CodeScriptFormatException(path, line, $"'{name}' deve essere un numero intero");
            return v;
        }

        private static bool OptBool(JsonElement root, string name, string path, int line)
        {
            if (!root.TryGetProperty(name, out var el) || el.ValueKind == JsonValueKind.Null) return false;
            if (el.ValueKind != JsonValueKind.True && el.ValueKind != JsonValueKind.False)
                throw new CodeScriptFormatException(path, line, $"'{name}' deve essere true o false");
            return el.GetBoolean();
        }

        private static string RequireEnum(JsonElement root, string name, string[] allowed, string path, int line)
        {
            var v = RequireString(root, name, path, line);
            if (!allowed.Contains(v, StringComparer.Ordinal))
                throw new CodeScriptFormatException(path, line, $"'{name}' = '{v}' non ammesso. Ammessi: {string.Join(", ", allowed)}");
            return v;
        }
    }
}
