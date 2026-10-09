using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging;
using TreeSitter;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'estrattore TypeScript su tree-sitter (D1, D8): la grammatica si carica per percorso assoluto dallo store delle
    /// grammatiche scaricate; senza grammatica l'estrattore si dichiara non pronto e i file restano contati.
    /// <para>
    /// Due passate: la prima legge TUTTI i file del perimetro e indicizza le dichiarazioni (serve per risolvere i nomi);
    /// la seconda emette uno script per ogni file richiesto. Tree-sitter è solo sintassi: le chiamate si risolvono per
    /// nome, quindi <c>this.x()</c> dentro una classe che definisce <c>x</c> è <c>extracted</c>, una funzione con un
    /// solo omonimo nel perimetro è <c>inferred</c>, più omonimi sono <c>ambiguous</c>, nessuno è un nodo <c>external</c>.
    /// </para>
    /// <para>
    /// Ogni file è un simbolo <c>module</c> (nome qualificato = percorso senza estensione); gli <c>import</c> relativi
    /// diventano archi <c>imports</c> fra moduli, quelli di pacchetto (<c>@angular/core</c>) un modulo <c>external</c>.
    /// Le classi con decoratore Angular hanno un kind proprio: component, service (Injectable), directive, pipe, ngmodule.
    /// </para>
    /// </summary>
    public sealed class TreeSitterTypeScriptExtractor : ICodeExtractor
    {
        public string LanguageId => CodeLanguages.TypeScript;
        public string ExtractorName => "tree-sitter-typescript";
        public string ExtractorVersion => "1";

        private readonly GrammarStore _store;
        private readonly ILogger<TreeSitterTypeScriptExtractor> _logger;
        private static readonly string[] CodeExtensions = { ".ts", ".tsx", ".mts", ".cts" };

        public TreeSitterTypeScriptExtractor(GrammarStore store, ILogger<TreeSitterTypeScriptExtractor> logger)
        {
            _store = store; _logger = logger;
        }

        public string Availability
        {
            get
            {
                var g = GrammarCatalog.ForLanguage(LanguageId);
                return _store.IsInstalled(g) ? "ready" : $"grammatica da scaricare ({g.ApproxSizeKb / 1024.0:0.#} MB)";
            }
        }

        public CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var result = new CodeExtractionResult();
            if (request.FilesToEmit.Count == 0) return result;
            var g = GrammarCatalog.ForLanguage(LanguageId);
            if (!_store.IsInstalled(g)) throw new InvalidOperationException($"Grammatica '{g.Id}' non installata in {_store.Folder}: scaricala dalla finestra «Ambiente di sviluppo».");

            using var language = new Language(_store.PathOf(g), g.Symbol);
            using var parser = new Parser(language);

            // Passata 1: parsing di tutto il perimetro e indice delle dichiarazioni.
            var parsed = new List<ParsedFile>();
            foreach (var f in request.AllFiles)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = File.ReadAllText(f.AbsolutePath, Encoding.UTF8); }
                catch (Exception ex) { result.Warnings.Add($"{f.ProjectRelativePath}: non leggibile ({ex.Message})"); continue; }
                var tree = parser.Parse(text);
                if (tree == null) { result.Warnings.Add($"{f.ProjectRelativePath}: tree-sitter non ha prodotto un albero"); continue; }
                parsed.Add(new ParsedFile(f, text, tree));
            }
            var index = new PerimeterIndex();
            foreach (var pf in parsed) index.Collect(pf);

            // Passata 2: uno script per file richiesto.
            var toEmit = new HashSet<string>(request.FilesToEmit.Select(f => f.ProjectRelativePath), StringComparer.Ordinal);
            foreach (var pf in parsed)
            {
                ct.ThrowIfCancellationRequested();
                if (!toEmit.Contains(pf.File.ProjectRelativePath)) continue;
                try { result.Scripts.Add((pf.File, new FileEmitter(this, pf, index).Emit())); }
                catch (Exception ex)
                {
                    result.Warnings.Add($"{pf.File.ProjectRelativePath}: estrazione fallita ({ex.GetType().Name}: {ex.Message})");
                    _logger?.LogWarning(ex, "[CodeGraph/tree-sitter] estrazione fallita per {File}", pf.File.ProjectRelativePath);
                }
            }
            foreach (var pf in parsed) pf.Tree.Dispose();
            return result;
        }

        // ------------------------------------------------------------------ naming

        /// <summary><c>src/app/x.component.ts</c> → <c>src/app/x.component</c>: il nome del modulo.</summary>
        public static string ModuleName(string projectRelativePath)
        {
            foreach (var ext in CodeExtensions)
                if (projectRelativePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) return projectRelativePath.Substring(0, projectRelativePath.Length - ext.Length);
            return projectRelativePath;
        }

        public static string StableId(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId(CodeLanguages.TypeScript, kind, qualifiedName);

        private static string Sha256(string content)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        }

        private sealed record ParsedFile(CodeSourceFile File, string Text, Tree Tree)
        {
            public string Module => ModuleName(File.ProjectRelativePath);
        }

        // ------------------------------------------------------------------ perimeter index

        /// <summary>Dichiarazioni di tutto il perimetro: per nome (funzioni e tipi) e per classe (membri), più i moduli.</summary>
        private sealed class PerimeterIndex
        {
            public readonly Dictionary<string, string> Modules = new(StringComparer.Ordinal);                 // module name → id
            public readonly Dictionary<string, List<(string Id, string Kind)>> TypesByName = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<string>> FunctionsByName = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<string>> MethodsByName = new(StringComparer.Ordinal);    // name → ids (tutte le classi)

            public void Collect(ParsedFile pf)
            {
                Modules[pf.Module] = StableId("module", pf.Module);
                foreach (var node in Walk(pf.Tree.RootNode))
                {
                    switch (node.Type)
                    {
                        case "class_declaration":
                        case "abstract_class_declaration":
                        case "interface_declaration":
                        case "enum_declaration":
                        case "type_alias_declaration":
                            {
                                var name = NameOf(node);
                                if (name == null) break;
                                var kind = KindOfType(node);
                                Add(TypesByName, name, (StableId(kind, pf.Module + ":" + name), kind));
                                if (node.Type is "class_declaration" or "abstract_class_declaration")
                                {
                                    var body = node.NamedChildren.FirstOrDefault(c => c.Type == "class_body");
                                    if (body == null) break;
                                    foreach (var m in body.NamedChildren)
                                    {
                                        if (m.Type is not ("method_definition" or "abstract_method_signature" or "method_signature")) continue;
                                        var mname = NameOf(m);
                                        if (mname == null || mname == "constructor") continue;
                                        var mkind = IsAccessor(m) ? "property" : "method";
                                        Add(MethodsByName, mname, StableId(mkind, pf.Module + ":" + name + "." + mname));
                                    }
                                }
                                break;
                            }
                        case "function_declaration":
                            {
                                var name = NameOf(node);
                                if (name != null) Add(FunctionsByName, name, StableId("function", pf.Module + ":" + name));
                                break;
                            }
                    }
                }
            }

            private static void Add<T>(Dictionary<string, List<T>> map, string key, T value)
            {
                if (!map.TryGetValue(key, out var list)) map[key] = list = new List<T>();
                list.Add(value);
            }
        }

        // ------------------------------------------------------------------ shared syntax helpers

        private static IEnumerable<Node> Walk(Node root)
        {
            var stack = new Stack<Node>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var n = stack.Pop();
                yield return n;
                var children = n.NamedChildren;
                for (var i = children.Count - 1; i >= 0; i--) stack.Push(children[i]);
            }
        }

        private static string NameOf(Node decl)
            => decl.NamedChildren.FirstOrDefault(c => c.Type is "identifier" or "type_identifier" or "property_identifier")?.Text;

        private static bool IsAccessor(Node method)
        {
            // get/set sono token anonimi figli di method_definition: si guarda il testo prima del nome.
            var name = method.NamedChildren.FirstOrDefault(c => c.Type == "property_identifier");
            if (name == null) return false;
            var head = method.Text.Substring(0, Math.Max(0, name.StartIndex - method.StartIndex));
            return head.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(t => t is "get" or "set");
        }

        private static string KindOfType(Node decl) => decl.Type switch
        {
            "interface_declaration" => "interface",
            "enum_declaration" => "enum",
            "type_alias_declaration" => "type",
            _ => AngularKind(decl) ?? "class",
        };

        /// <summary>Il decoratore Angular, se c'è: figlio della classe o fratello sotto l'export_statement.</summary>
        private static string AngularKind(Node classDecl)
        {
            IEnumerable<Node> decorators = classDecl.NamedChildren.Where(c => c.Type == "decorator");
            if (classDecl.Parent != null && classDecl.Parent.Type == "export_statement")
                decorators = decorators.Concat(classDecl.Parent.NamedChildren.Where(c => c.Type == "decorator"));
            foreach (var d in decorators)
            {
                var call = d.NamedChildren.FirstOrDefault(c => c.Type == "call_expression");
                var name = call?.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text ?? d.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text;
                switch (name)
                {
                    case "Component": return "component";
                    case "Injectable": return "service";
                    case "Directive": return "directive";
                    case "Pipe": return "pipe";
                    case "NgModule": return "ngmodule";
                }
            }
            return null;
        }

        private static int Line(Node n) => (int)n.StartPosition.Row + 1;
        private static int EndLine(Node n) => (int)n.EndPosition.Row + 1;

        /// <summary>Il testo della dichiarazione senza il corpo: fino al figlio «corpo» (statement_block, class_body, …).</summary>
        private static string Signature(Node decl)
        {
            var body = decl.NamedChildren.FirstOrDefault(c => c.Type is "statement_block" or "class_body" or "interface_body" or "enum_body" or "object_type");
            var text = body == null ? decl.Text : decl.Text.Substring(0, Math.Max(0, body.StartIndex - decl.StartIndex));
            return string.Join(" ", text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim())).Trim().TrimEnd('{').Trim();
        }

        private static string Visibility(Node member)
        {
            var mod = member.NamedChildren.FirstOrDefault(c => c.Type == "accessibility_modifier")?.Text;
            return mod ?? "public";
        }

        /// <summary>I type_identifier dentro un'annotazione di tipo, generici compresi (Promise&lt;Hit[]&gt; → Promise, Hit).</summary>
        private static IEnumerable<string> TypeNames(Node typeAnnotationOrType)
            => typeAnnotationOrType == null ? Enumerable.Empty<string>()
               : Walk(typeAnnotationOrType).Where(n => n.Type == "type_identifier").Select(n => n.Text).Distinct(StringComparer.Ordinal);

        // ------------------------------------------------------------------ emitter

        private sealed class FileEmitter
        {
            private readonly TreeSitterTypeScriptExtractor _owner;
            private readonly ParsedFile _pf;
            private readonly PerimeterIndex _index;
            private readonly CodeScript _script = new();
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly HashSet<string> _edges = new(StringComparer.Ordinal);
            private readonly string _moduleId;

            public FileEmitter(TreeSitterTypeScriptExtractor owner, ParsedFile pf, PerimeterIndex index)
            {
                _owner = owner; _pf = pf; _index = index;
                _moduleId = StableId("module", pf.Module);
            }

            public CodeScript Emit()
            {
                _script.Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = _pf.File.ProjectRelativePath,
                    Language = _owner.LanguageId, SourceHash = Sha256(_pf.Text), Extractor = _owner.ExtractorName,
                    ExtractorVersion = _owner.ExtractorVersion, GeneratedAt = DateTime.UtcNow.ToString("o"),
                };
                var root = _pf.Tree.RootNode;
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = _moduleId, Kind = "module", Name = Path.GetFileName(_pf.Module), QualifiedName = _pf.Module,
                    LineStart = 1, LineEnd = EndLine(root), Signature = _pf.File.ProjectRelativePath, Visibility = "public",
                });

                foreach (var node in root.NamedChildren) EmitTopLevel(node, exported: false);
                return _script;
            }

            private void EmitTopLevel(Node node, bool exported)
            {
                switch (node.Type)
                {
                    case "export_statement":
                        foreach (var c in node.NamedChildren) EmitTopLevel(c, exported: true);
                        break;
                    case "import_statement":
                        EmitImport(node);
                        break;
                    case "class_declaration":
                    case "abstract_class_declaration":
                        EmitClass(node, exported);
                        break;
                    case "interface_declaration":
                    case "enum_declaration":
                    case "type_alias_declaration":
                        EmitTypeLike(node, exported);
                        break;
                    case "function_declaration":
                        EmitFunction(node, exported);
                        break;
                }
            }

            // ---- import

            private void EmitImport(Node import)
            {
                var spec = import.NamedChildren.FirstOrDefault(c => c.Type == "string")?.NamedChildren.FirstOrDefault(c => c.Type == "string_fragment")?.Text;
                if (string.IsNullOrEmpty(spec)) return;
                string targetId;
                if (spec.StartsWith('.'))
                {
                    var dir = Path.GetDirectoryName(_pf.Module)?.Replace('\\', '/') ?? string.Empty;
                    var candidate = NormalizeRelative(string.IsNullOrEmpty(dir) ? spec : dir + "/" + spec);
                    if (_index.Modules.TryGetValue(candidate, out var id) || _index.Modules.TryGetValue(candidate + "/index", out id))
                        targetId = id;
                    else
                        targetId = External("module", candidate, Path.GetFileName(candidate));
                }
                else
                {
                    targetId = External("module", spec, spec);
                }
                Edge(_moduleId, targetId, "imports", "extracted", Line(import));
            }

            private static string NormalizeRelative(string path)
            {
                var parts = new List<string>();
                foreach (var p in path.Split('/'))
                {
                    if (p == "." || p.Length == 0) continue;
                    if (p == "..") { if (parts.Count > 0) parts.RemoveAt(parts.Count - 1); continue; }
                    parts.Add(p);
                }
                return string.Join("/", parts);
            }

            // ---- class

            private void EmitClass(Node decl, bool exported)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var kind = KindOfType(decl);
                var qn = _pf.Module + ":" + name;
                var classId = Declare(decl, kind, name, qn, Signature(decl), returnType: null, visibility: exported ? "public" : "internal", parentId: _moduleId);
                Edge(_moduleId, classId, "contains", "extracted", Line(decl));

                var heritage = decl.NamedChildren.FirstOrDefault(c => c.Type == "class_heritage");
                if (heritage != null)
                {
                    foreach (var ext in heritage.NamedChildren.Where(c => c.Type == "extends_clause"))
                        foreach (var t in TypeNames(ext).Concat(ext.NamedChildren.Where(c => c.Type == "identifier").Select(c => c.Text)).Distinct())
                            Edge(classId, ResolveType(t), "inherits", ConfidenceOfType(t), Line(ext));
                    foreach (var impl in heritage.NamedChildren.Where(c => c.Type == "implements_clause"))
                        foreach (var t in TypeNames(impl))
                            Edge(classId, ResolveType(t), "implements", ConfidenceOfType(t), Line(impl));
                }

                var body = decl.NamedChildren.FirstOrDefault(c => c.Type == "class_body");
                if (body == null) return;
                foreach (var m in body.NamedChildren)
                {
                    switch (m.Type)
                    {
                        case "method_definition":
                        case "abstract_method_signature":
                        case "method_signature":
                            EmitMethod(m, classId, name);
                            break;
                        case "public_field_definition":
                        case "property_signature":
                            {
                                var fname = NameOf(m);
                                if (fname == null) break;
                                var typeNode = m.NamedChildren.FirstOrDefault(c => c.Type == "type_annotation");
                                var fid = Declare(m, "field", fname, qn + "." + fname, Signature(m), typeNode?.Text.TrimStart(':').Trim(), Visibility(m), classId);
                                Edge(classId, fid, "contains", "extracted", Line(m));
                                foreach (var t in TypeNames(typeNode)) Edge(fid, ResolveType(t), "references", ConfidenceOfType(t), Line(m));
                                WalkBody(m, fid, name);
                                break;
                            }
                    }
                }
            }

            private void EmitMethod(Node m, string classId, string className)
            {
                var mname = NameOf(m);
                if (mname == null) return;
                var isCtor = mname == "constructor";
                var kind = isCtor ? "constructor" : IsAccessor(m) ? "property" : "method";
                var qn = _pf.Module + ":" + className + "." + mname;
                var returnNode = m.NamedChildren.FirstOrDefault(c => c.Type == "type_annotation");
                var isAsync = m.Text.Substring(0, Math.Max(0, (m.NamedChildren.FirstOrDefault(c => c.Type == "property_identifier")?.StartIndex ?? m.StartIndex) - m.StartIndex)).Contains("async");
                var isStatic = m.Text.StartsWith("static ", StringComparison.Ordinal);
                var id = Declare(m, kind, mname, qn, Signature(m), isCtor ? null : returnNode?.Text.TrimStart(':').Trim(), Visibility(m), classId, isAsync, isStatic);
                Edge(classId, id, "contains", "extracted", Line(m));
                foreach (var t in TypeNames(returnNode)) Edge(id, ResolveType(t), "references", ConfidenceOfType(t), Line(m));
                EmitParameters(m, id);
                WalkBody(m, id, className);
            }

            private void EmitFunction(Node fn, bool exported)
            {
                var name = NameOf(fn);
                if (name == null) return;
                var qn = _pf.Module + ":" + name;
                var returnNode = fn.NamedChildren.FirstOrDefault(c => c.Type == "type_annotation");
                var id = Declare(fn, "function", name, qn, Signature(fn), returnNode?.Text.TrimStart(':').Trim(), exported ? "public" : "internal", _moduleId,
                    isAsync: fn.Text.StartsWith("async ", StringComparison.Ordinal));
                Edge(_moduleId, id, "contains", "extracted", Line(fn));
                foreach (var t in TypeNames(returnNode)) Edge(id, ResolveType(t), "references", ConfidenceOfType(t), Line(fn));
                EmitParameters(fn, id);
                WalkBody(fn, id, null);
            }

            private void EmitTypeLike(Node decl, bool exported)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var kind = KindOfType(decl);
                var qn = _pf.Module + ":" + name;
                var id = Declare(decl, kind, name, qn, Signature(decl), null, exported ? "public" : "internal", _moduleId);
                Edge(_moduleId, id, "contains", "extracted", Line(decl));
                if (decl.Type == "interface_declaration")
                {
                    foreach (var ext in decl.NamedChildren.Where(c => c.Type == "extends_type_clause"))
                        foreach (var t in TypeNames(ext)) Edge(id, ResolveType(t), "inherits", ConfidenceOfType(t), Line(ext));
                    var body = decl.NamedChildren.FirstOrDefault(c => c.Type is "interface_body" or "object_type");
                    if (body == null) return;
                    foreach (var m in body.NamedChildren)
                    {
                        var mname = NameOf(m);
                        if (mname == null) continue;
                        if (m.Type == "method_signature")
                        {
                            var mid = Declare(m, "method", mname, qn + "." + mname, Signature(m), m.NamedChildren.FirstOrDefault(c => c.Type == "type_annotation")?.Text.TrimStart(':').Trim(), "public", id);
                            Edge(id, mid, "contains", "extracted", Line(m));
                            EmitParameters(m, mid);
                        }
                        else if (m.Type == "property_signature")
                        {
                            var typeNode = m.NamedChildren.FirstOrDefault(c => c.Type == "type_annotation");
                            var pid = Declare(m, "property", mname, qn + "." + mname, Signature(m), typeNode?.Text.TrimStart(':').Trim(), "public", id);
                            Edge(id, pid, "contains", "extracted", Line(m));
                            foreach (var t in TypeNames(typeNode)) Edge(pid, ResolveType(t), "references", ConfidenceOfType(t), Line(m));
                        }
                    }
                }
            }

            // ---- parameters

            private void EmitParameters(Node decl, string ownerId)
            {
                var prms = decl.NamedChildren.FirstOrDefault(c => c.Type == "formal_parameters");
                if (prms == null) return;
                var pos = 0;
                foreach (var p in prms.NamedChildren)
                {
                    if (p.Type is not ("required_parameter" or "optional_parameter")) continue;
                    var rest = p.NamedChildren.FirstOrDefault(c => c.Type == "rest_pattern");
                    var nameNode = (rest ?? p).NamedChildren.FirstOrDefault(c => c.Type == "identifier");
                    var typeNode = p.NamedChildren.FirstOrDefault(c => c.Type == "type_annotation");
                    var defaultNode = p.NamedChildren.LastOrDefault(c => c.Type is not ("identifier" or "type_annotation" or "accessibility_modifier" or "rest_pattern" or "decorator" or "override_modifier" or "readonly"));
                    _script.Parameters.Add(new ScriptParameter
                    {
                        SymbolId = ownerId, Position = pos++, Name = nameNode?.Text ?? p.Text, ParameterType = typeNode?.Text.TrimStart(':').Trim(),
                        Direction = rest != null ? "params" : "in", IsNullable = p.Type == "optional_parameter",
                        DefaultValue = defaultNode != null && defaultNode.EndIndex > (typeNode?.EndIndex ?? nameNode?.EndIndex ?? 0) ? defaultNode.Text : null,
                    });
                    foreach (var t in TypeNames(typeNode)) Edge(ownerId, ResolveType(t), "references", ConfidenceOfType(t), Line(p));
                }
            }

            // ---- bodies: calls, new

            private void WalkBody(Node decl, string ownerId, string className)
            {
                foreach (var n in Walk(decl))
                {
                    if (n.Type == "call_expression")
                    {
                        var callee = n.NamedChildren.FirstOrDefault();
                        if (callee == null) continue;
                        var line = Line(n);
                        if (callee.Type == "identifier")
                        {
                            var fname = callee.Text;
                            if (_index.FunctionsByName.TryGetValue(fname, out var fns))
                            {
                                var conf = fns.Count == 1 ? "inferred" : "ambiguous";
                                foreach (var id in fns) Edge(ownerId, id, "calls", conf, line);
                            }
                            else Edge(ownerId, External("function", fname, fname), "calls", "ambiguous", line);
                        }
                        else if (callee.Type == "member_expression")
                        {
                            var obj = callee.NamedChildren.FirstOrDefault();
                            var prop = callee.NamedChildren.LastOrDefault(c => c.Type == "property_identifier")?.Text;
                            if (prop == null) continue;
                            if (obj?.Type == "this" && className != null)
                            {
                                var ownId = StableId("method", _pf.Module + ":" + className + "." + prop);
                                if (_index.MethodsByName.TryGetValue(prop, out var own) && own.Contains(ownId)) { Edge(ownerId, ownId, "calls", "extracted", line); continue; }
                            }
                            if (_index.MethodsByName.TryGetValue(prop, out var ms))
                            {
                                var conf = ms.Count == 1 ? "inferred" : "ambiguous";
                                foreach (var id in ms) Edge(ownerId, id, "calls", conf, line);
                            }
                            else Edge(ownerId, External("method", callee.Text, prop), "calls", "ambiguous", line);
                        }
                    }
                    else if (n.Type == "new_expression")
                    {
                        var ctor = n.NamedChildren.FirstOrDefault(c => c.Type is "identifier" or "member_expression");
                        if (ctor == null) continue;
                        var tname = ctor.Type == "identifier" ? ctor.Text : ctor.NamedChildren.LastOrDefault()?.Text ?? ctor.Text;
                        Edge(ownerId, ResolveType(tname), "instantiates", ConfidenceOfType(tname), Line(n));
                    }
                }
            }

            // ---- resolution

            private string ResolveType(string name)
            {
                if (_index.TypesByName.TryGetValue(name, out var list) && list.Count >= 1) return list[0].Id;
                return External("type", name, name);
            }

            private string ConfidenceOfType(string name)
                => _index.TypesByName.TryGetValue(name, out var list) ? (list.Count == 1 ? "inferred" : "ambiguous") : "extracted";

            private string External(string what, string qualifiedName, string name)
            {
                var id = StableId("external", what + ":" + qualifiedName);
                if (_externals.Add(id)) _script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = name, QualifiedName = qualifiedName });
                return id;
            }

            // ---- helpers

            private string Declare(Node node, string kind, string name, string qualifiedName, string signature, string returnType, string visibility, string parentId, bool isAsync = false, bool isStatic = false)
            {
                var id = StableId(kind, qualifiedName);
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = id, Kind = kind, Name = name, QualifiedName = qualifiedName, LineStart = Line(node), LineEnd = EndLine(node),
                    Signature = signature, ReturnType = returnType, Visibility = visibility, IsAsync = isAsync, IsStatic = isStatic, ParentId = parentId,
                });
                return id;
            }

            private void Edge(string source, string target, string relation, string confidence, int? line)
            {
                if (source == null || target == null || source == target) return;
                if (!_edges.Add($"{source}|{target}|{relation}|{line}")) return;
                _script.Edges.Add(new ScriptEdge { SourceId = source, TargetId = target, Relation = relation, Confidence = confidence, Line = line });
            }
        }
    }
}
