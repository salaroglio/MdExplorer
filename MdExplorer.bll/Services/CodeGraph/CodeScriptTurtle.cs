using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VDS.RDF;
using VDS.RDF.Parsing;
using static MdExplorer.Features.Services.CodeGraph.CodeGraphIris;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Gli script del grafo del codice come ABox Turtle (schema 2; D21 del grafo, R4 delle radici esterne, 12/10/2026):
    /// chi fa <c>git pull</c> ha le triple vere, le legge con Jena, le valida con SHACL, le carica dove vuole.
    /// <list type="bullet">
    /// <item>Prima riga: <c># mde-code-script: {json}</c>, l'intestazione per i controlli veloci («va rifatto?») senza
    /// leggere tutto il file. La stessa intestazione sta anche nelle triple (nodo file o insieme di descrizioni).</item>
    /// <item>Le triple sono quelle di <see cref="CodeTripleWriter"/>, una riga per nodo, senza <c>code:fromScript</c>
    /// (che è del caricatore): una fonte sola, il file e Fuseki non possono divergere.</item>
    /// <item>La rilettura è rigida come il JSON Lines: una classe o un predicato che il formato non conosce, un campo
    /// obbligatorio assente, uno schema più nuovo sono errori che nominano il nodo e il predicato.</item>
    /// </list>
    /// Letto con dotNetRDF (già dipendenza, per la memoria degli agenti): il pianificatore delle descrizioni e i
    /// controlli lavorano anche senza Fuseki. Scostamento dichiarato dalla D21 («niente parser RDF in C#»).
    /// </summary>
    public static class CodeScriptTurtle
    {
        public const string HeaderMarker = "# mde-code-script: ";

        private static readonly JsonSerializerOptions HeaderJson = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public static bool IsTurtle(string content) => content != null && content.StartsWith(HeaderMarker, StringComparison.Ordinal);

        // ------------------------------------------------------------------ scrittura

        public static string Write(CodeScript script)
        {
            var h = script.Header ?? throw new ArgumentException("Header mancante", nameof(script));
            var sb = new StringBuilder();
            sb.Append(HeaderMarker).Append(JsonSerializer.Serialize(HeaderOf(h), HeaderJson)).Append('\n');
            sb.Append("@prefix code: <").Append(CodeOntology.Namespace).Append("> .\n\n");
            if (h.Kind == CodeScriptKind.Ast) CodeTripleWriter.AppendAstTriples(sb, script, null);
            else CodeTripleWriter.AppendDocTriples(sb, script, null);
            return sb.ToString();
        }

        private static object HeaderOf(CodeScriptHeader h) => h.Kind == CodeScriptKind.Ast
            ? new { schema = CodeScriptParser.CurrentSchema, kind = "ast", source = h.Source, language = h.Language, sourceHash = h.SourceHash, extractor = h.Extractor, extractorVersion = h.ExtractorVersion, generatedAt = h.GeneratedAt }
            : new { schema = CodeScriptParser.CurrentSchema, kind = "doc", source = h.Source, writtenBy = h.WrittenBy, writtenAt = h.WrittenAt };

        // ------------------------------------------------------------------ intestazione veloce

        /// <summary>
        /// L'intestazione dalla prima riga, Turtle o JSON Lines, senza leggere il resto: per «va riparsato?», «di quale
        /// estrattore è?». Null se il file non c'è, è vuoto o la prima riga non si capisce.
        /// </summary>
        public static CodeScriptHeader ReadHeaderLine(string scriptPath)
        {
            if (!System.IO.File.Exists(scriptPath)) return null;
            string first;
            using (var reader = new StreamReader(scriptPath)) first = reader.ReadLine();
            return ParseHeaderLine(first);
        }

        /// <summary>L'intestazione da una prima riga già letta: <c># mde-code-script: {…}</c> o la riga JSON del vecchio formato.</summary>
        public static CodeScriptHeader ParseHeaderLine(string first)
        {
            if (string.IsNullOrWhiteSpace(first)) return null;
            var json = first.StartsWith(HeaderMarker, StringComparison.Ordinal) ? first.Substring(HeaderMarker.Length) : first;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var r = doc.RootElement;
                string S(string n) => r.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                return new CodeScriptHeader
                {
                    Schema = r.TryGetProperty("schema", out var sc) && sc.ValueKind == JsonValueKind.Number ? sc.GetInt32() : 0,
                    Kind = S("kind") == "doc" ? CodeScriptKind.Doc : CodeScriptKind.Ast,
                    Source = S("source"), Language = S("language"), SourceHash = S("sourceHash"), Extractor = S("extractor"),
                    ExtractorVersion = S("extractorVersion"), GeneratedAt = S("generatedAt"), WrittenBy = S("writtenBy"), WrittenAt = S("writtenAt"),
                };
            }
            catch (JsonException) { return null; }
        }

        // ------------------------------------------------------------------ lettura

        private static readonly string Ns = CodeOntology.Namespace;
        private const string RdfType = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";

        private static readonly Dictionary<string, HashSet<string>> Allowed = new(StringComparer.Ordinal)
        {
            ["File"] = Set("path", "language", "contentHash", "extractor", "extractorVersion", "parsedAt", "schema", "symbolCount"),
            ["Symbol"] = Set(new[] { "kind", "name", "qualifiedName", "language", "inFile", "path", "isStatic", "isAsync", "lineStart", "lineEnd", "signature", "returnType", "visibility", "parent", "signatureHash" }
                             .Concat(CodeScriptParser.Relations).ToArray()),
            ["Parameter"] = Set("parameterOf", "position", "name", "direction", "isNullable", "type", "defaultValue"),
            ["Edge"] = Set("source", "target", "relation", "relationIri", "confidence", "path", "line"),
            ["DescriptionSet"] = Set("path", "writtenBy", "writtenAt", "schema"),
            ["Description"] = Set("describes", "summary", "writtenBy", "writtenAt", "returnsDescription", "remarks", "signatureHash"),
            ["ParameterDescription"] = Set("describes", "parameterName", "description"),
        };

        private static HashSet<string> Set(params string[] names) => new(names, StringComparer.Ordinal);

        public static CodeScript Read(string content, string scriptPath)
        {
            var headerLine = ReadHeaderFromContent(content, scriptPath);
            if (headerLine.Schema != CodeScriptParser.CurrentSchema)
                throw new CodeScriptFormatException(scriptPath, 1, headerLine.Schema > CodeScriptParser.CurrentSchema
                    ? $"schema {headerLine.Schema} più nuovo di quello che questo MdExplorer sa leggere ({CodeScriptParser.CurrentSchema}). Aggiorna MdExplorer."
                    : $"schema {headerLine.Schema} non valido per uno script Turtle (atteso {CodeScriptParser.CurrentSchema}).");

            var graph = new Graph();
            try { new TurtleParser().Load(graph, new StringReader(content)); }
            catch (RdfParseException ex) { throw new CodeScriptFormatException(scriptPath, ex.StartLine, "Turtle non valido: " + ex.Message); }

            // nodo → (classe, predicato → valori); l'ordine è quello della prima comparsa nel testo (un grafo non ne ha)
            var order = SubjectOrder(content);
            var nodes = new Dictionary<string, Node>(StringComparer.Ordinal);
            foreach (var t in graph.Triples)
            {
                var subject = IriOf(t.Subject, scriptPath, "soggetto");
                if (!nodes.TryGetValue(subject, out var node)) nodes[subject] = node = new Node(subject);
                var predicate = IriOf(t.Predicate, scriptPath, "predicato");
                if (predicate == RdfType)
                {
                    var cls = IriOf(t.Object, scriptPath, "classe");
                    if (!cls.StartsWith(Ns, StringComparison.Ordinal) || !Allowed.ContainsKey(cls.Substring(Ns.Length)))
                        throw new CodeScriptFormatException(scriptPath, 0, $"<{subject}>: classe <{cls}> sconosciuta al formato");
                    node.Class = cls.Substring(Ns.Length);
                    continue;
                }
                if (!predicate.StartsWith(Ns, StringComparison.Ordinal))
                    throw new CodeScriptFormatException(scriptPath, 0, $"<{subject}>: predicato <{predicate}> fuori dall'ontologia mde-code");
                node.Add(predicate.Substring(Ns.Length), t.Object);
            }
            foreach (var n in nodes.Values)
            {
                if (n.Class == null) throw new CodeScriptFormatException(scriptPath, 0, $"<{n.Iri}>: nodo senza classe (manca «a code:…»)");
                foreach (var p in n.Values.Keys)
                    if (!Allowed[n.Class].Contains(p))
                        throw new CodeScriptFormatException(scriptPath, 0, $"<{n.Iri}>: predicato code:{p} non ammesso su code:{n.Class}");
            }
            var ordered = nodes.Values.OrderBy(n => order.TryGetValue(n.Iri, out var i) ? i : int.MaxValue).ToList();

            var script = new CodeScript();
            if (headerLine.Kind == CodeScriptKind.Ast) ReadAst(script, ordered, scriptPath);
            else ReadDoc(script, ordered, scriptPath);
            return script;
        }

        private static void ReadAst(CodeScript script, List<Node> nodes, string path)
        {
            var file = Single(nodes, "File", path);
            script.Header = new CodeScriptHeader
            {
                Schema = file.Int("schema", path), Kind = CodeScriptKind.Ast, Source = file.Str("path", path), Language = file.Str("language", path),
                SourceHash = file.Str("contentHash", path), Extractor = file.Str("extractor", path), ExtractorVersion = file.Str("extractorVersion", path),
                GeneratedAt = file.Str("parsedAt", path),
            };
            foreach (var n in nodes.Where(n => n.Class == "Symbol"))
            {
                var kind = n.Enum("kind", CodeScriptParser.SymbolKinds, path);
                script.Symbols.Add(new ScriptSymbol
                {
                    Id = SymbolId(n.Iri), Kind = kind, Name = n.Str("name", path), QualifiedName = n.Str("qualifiedName", path),
                    LineStart = n.OptInt("lineStart", path), LineEnd = n.OptInt("lineEnd", path), Signature = n.OptStr("signature"),
                    ReturnType = n.OptStr("returnType"), Visibility = n.OptStr("visibility"),
                    IsStatic = n.OptBool("isStatic", path), IsAsync = n.OptBool("isAsync", path),
                    ParentId = n.OptIri("parent") is { } parent ? SymbolId(parent) : null,
                });
            }
            foreach (var n in nodes.Where(n => n.Class == "Parameter"))
                script.Parameters.Add(new ScriptParameter
                {
                    SymbolId = SymbolId(n.IriValue("parameterOf", path)), Position = n.Int("position", path), Name = n.Str("name", path),
                    ParameterType = n.OptStr("type"), Direction = n.OptStr("direction") is { } d ? n.Enum("direction", CodeScriptParser.Directions, path) : "in",
                    DefaultValue = n.OptStr("defaultValue"), IsNullable = n.OptBool("isNullable", path),
                });
            foreach (var n in nodes.Where(n => n.Class == "Edge"))
                script.Edges.Add(new ScriptEdge
                {
                    SourceId = SymbolId(n.IriValue("source", path)), TargetId = SymbolId(n.IriValue("target", path)),
                    Relation = n.Enum("relation", CodeScriptParser.Relations, path), Confidence = n.Enum("confidence", CodeScriptParser.Confidences, path),
                    Line = n.OptInt("line", path),
                });
        }

        private static void ReadDoc(CodeScript script, List<Node> nodes, string path)
        {
            var set = Single(nodes, "DescriptionSet", path);
            script.Header = new CodeScriptHeader
            {
                Schema = set.Int("schema", path), Kind = CodeScriptKind.Doc, Source = set.Str("path", path),
                WrittenBy = set.OptStr("writtenBy"), WrittenAt = set.OptStr("writtenAt"),
            };
            foreach (var n in nodes.Where(n => n.Class == "Description"))
                script.SymbolDocs.Add(new ScriptSymbolDoc
                {
                    SymbolId = SymbolId(n.IriValue("describes", path)), Summary = n.Str("summary", path),
                    ReturnsDescription = n.OptStr("returnsDescription"), Remarks = n.OptStr("remarks"), SignatureHash = n.OptStr("signatureHash"),
                });
            foreach (var n in nodes.Where(n => n.Class == "ParameterDescription"))
                script.ParameterDocs.Add(new ScriptParameterDoc
                {
                    SymbolId = SymbolId(n.IriValue("describes", path)), ParameterName = n.Str("parameterName", path), Description = n.Str("description", path),
                });
        }

        // ------------------------------------------------------------------ appoggi

        private static CodeScriptHeader ReadHeaderFromContent(string content, string path)
        {
            var end = content.IndexOf('\n');
            return ParseHeaderLine(end < 0 ? content : content.Substring(0, end))
                ?? throw new CodeScriptFormatException(path, 1, "intestazione «# mde-code-script: {…}» illeggibile");
        }

        /// <summary>L'indice della prima riga in cui ogni IRI compare come soggetto (le righe che cominciano con «&lt;»).</summary>
        private static Dictionary<string, int> SubjectOrder(string content)
        {
            var order = new Dictionary<string, int>(StringComparer.Ordinal);
            var i = 0;
            foreach (var line in content.Split('\n'))
            {
                if (line.Length < 2 || line[0] != '<') continue;
                var close = line.IndexOf('>');
                if (close < 0) continue;
                var iri = Uri.UnescapeDataString(line.Substring(1, close - 1));
                order.TryAdd(line.Substring(1, close - 1), i);
                order.TryAdd(iri, i);
                i++;
            }
            return order;
        }

        private static string IriOf(INode node, string path, string role)
            => node is IUriNode u ? u.Uri.OriginalString
             : throw new CodeScriptFormatException(path, 0, $"{role} non è un IRI ({node}): il formato usa solo IRI e literal");

        private static Node Single(List<Node> nodes, string cls, string path)
        {
            var found = nodes.Where(n => n.Class == cls).ToList();
            if (found.Count != 1) throw new CodeScriptFormatException(path, 0, $"atteso un solo nodo code:{cls}, trovati {found.Count}");
            return found[0];
        }

        private sealed class Node
        {
            public Node(string iri) { Iri = iri; }
            public string Iri { get; }
            public string Class { get; set; }
            public Dictionary<string, List<INode>> Values { get; } = new(StringComparer.Ordinal);

            public void Add(string p, INode o)
            {
                if (!Values.TryGetValue(p, out var list)) Values[p] = list = new List<INode>();
                list.Add(o);
            }

            private INode One(string p, string path, bool required)
            {
                if (!Values.TryGetValue(p, out var list) || list.Count == 0)
                    return required ? throw new CodeScriptFormatException(path, 0, $"<{Iri}>: manca code:{p}") : null;
                // le proprietà dirette degli archi (code:calls…) sono molte per simbolo; tutte le altre sono uniche
                if (list.Count > 1) throw new CodeScriptFormatException(path, 0, $"<{Iri}>: code:{p} ripetuto {list.Count} volte");
                return list[0];
            }

            private static string Literal(INode n, string iri, string p, string path)
                => n is ILiteralNode l ? l.Value : throw new CodeScriptFormatException(path, 0, $"<{iri}>: code:{p} deve essere un literal");

            public string Str(string p, string path) => Literal(One(p, path, true), Iri, p, path);
            public string OptStr(string p) => Values.TryGetValue(p, out var l) && l.Count == 1 && l[0] is ILiteralNode lit ? lit.Value : null;
            public int Int(string p, string path) => ParseInt(Str(p, path), p, path);
            public int? OptInt(string p, string path) => One(p, path, false) is { } n ? ParseInt(Literal(n, Iri, p, path), p, path) : null;
            public bool OptBool(string p, string path)
            {
                var n = One(p, path, false);
                if (n == null) return false;
                var v = Literal(n, Iri, p, path);
                return v == "true" ? true : v == "false" ? false : throw new CodeScriptFormatException(path, 0, $"<{Iri}>: code:{p} = «{v}» non è un booleano");
            }
            public string IriValue(string p, string path) => One(p, path, true) is IUriNode u ? u.Uri.OriginalString : throw new CodeScriptFormatException(path, 0, $"<{Iri}>: code:{p} deve essere un IRI");
            public string OptIri(string p) => Values.TryGetValue(p, out var l) && l.Count == 1 && l[0] is IUriNode u ? u.Uri.OriginalString : null;
            public string Enum(string p, string[] allowed, string path)
            {
                var v = Str(p, path);
                return allowed.Contains(v) ? v : throw new CodeScriptFormatException(path, 0, $"<{Iri}>: code:{p} = «{v}» non ammesso (ammessi: {string.Join(", ", allowed)})");
            }

            private int ParseInt(string v, string p, string path)
                => int.TryParse(v, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var i) ? i
                 : throw new CodeScriptFormatException(path, 0, $"<{Iri}>: code:{p} = «{v}» non è un intero");
        }
    }
}
