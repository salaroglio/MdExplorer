using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>L'inverso di <see cref="CodeScriptParser"/>: scrive uno <see cref="CodeScript"/> in JSON Lines, schema 1.</summary>
    public static class CodeScriptWriter
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public static string Serialize(CodeScript script)
        {
            var h = script.Header ?? throw new ArgumentException("Header mancante", nameof(script));
            var sb = new StringBuilder();
            if (h.Kind == CodeScriptKind.Ast)
            {
                sb.Append(JsonSerializer.Serialize(new
                {
                    schema = CodeScriptParser.CurrentSchema, kind = "ast", source = h.Source, language = h.Language,
                    sourceHash = h.SourceHash, extractor = h.Extractor, extractorVersion = h.ExtractorVersion, generatedAt = h.GeneratedAt
                }, Options)).Append('\n');
                foreach (var s in script.Symbols)
                    sb.Append(JsonSerializer.Serialize(new
                    {
                        type = "symbol", id = s.Id, kind = s.Kind, name = s.Name, qualifiedName = s.QualifiedName,
                        lineStart = s.LineStart, lineEnd = s.LineEnd, signature = s.Signature, returnType = s.ReturnType,
                        visibility = s.Visibility, isStatic = s.IsStatic ? true : (bool?)null, isAsync = s.IsAsync ? true : (bool?)null, parentId = s.ParentId
                    }, Options)).Append('\n');
                foreach (var p in script.Parameters)
                    sb.Append(JsonSerializer.Serialize(new
                    {
                        type = "parameter", symbolId = p.SymbolId, position = p.Position, name = p.Name, parameterType = p.ParameterType,
                        direction = p.Direction == "in" ? null : p.Direction, defaultValue = p.DefaultValue, isNullable = p.IsNullable ? true : (bool?)null
                    }, Options)).Append('\n');
                foreach (var e in script.Edges)
                    sb.Append(JsonSerializer.Serialize(new
                    {
                        type = "edge", sourceId = e.SourceId, targetId = e.TargetId, relation = e.Relation, confidence = e.Confidence, line = e.Line
                    }, Options)).Append('\n');
            }
            else
            {
                sb.Append(JsonSerializer.Serialize(new
                {
                    schema = CodeScriptParser.CurrentSchema, kind = "doc", source = h.Source, writtenBy = h.WrittenBy, writtenAt = h.WrittenAt
                }, Options)).Append('\n');
                foreach (var d in script.SymbolDocs)
                    sb.Append(JsonSerializer.Serialize(new
                    {
                        type = "symbolDoc", symbolId = d.SymbolId, summary = d.Summary, returnsDescription = d.ReturnsDescription, remarks = d.Remarks, signatureHash = d.SignatureHash
                    }, Options)).Append('\n');
                foreach (var d in script.ParameterDocs)
                    sb.Append(JsonSerializer.Serialize(new { type = "parameterDoc", symbolId = d.SymbolId, parameterName = d.ParameterName, description = d.Description }, Options)).Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>Scrive lo script solo se il contenuto cambia: così il FileSystemWatcher non vede salvataggi a vuoto.</summary>
        public static bool WriteIfChanged(string absolutePath, string content)
        {
            if (File.Exists(absolutePath) && string.Equals(File.ReadAllText(absolutePath, Encoding.UTF8), content, StringComparison.Ordinal))
                return false;
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
            File.WriteAllText(absolutePath, content, new UTF8Encoding(false));
            return true;
        }

        /// <summary>Dove sta lo script AST (o doc) di un sorgente: <c>.mde-code/&lt;relativo del sorgente&gt;.ast.jsonl</c>.</summary>
        public static string ScriptPathFor(string projectPath, string sourceProjectRelative, CodeScriptKind kind)
            => Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath),
                sourceProjectRelative.Replace('/', Path.DirectorySeparatorChar) + (kind == CodeScriptKind.Ast ? CodeGraphPaths.AstScriptSuffix : CodeGraphPaths.DocScriptSuffix));
    }
}
