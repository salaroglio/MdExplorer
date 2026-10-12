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
    /// L'estrattore Python su tree-sitter (F8, D1, D8): stessa forma di quello TypeScript, due passate (indice del
    /// perimetro, poi uno script per file). Tree-sitter è solo sintassi: le chiamate si risolvono per nome con la
    /// confidenza esplicita (<c>self.x()</c> in una classe che definisce <c>x</c> è <c>extracted</c>, un omonimo solo nel
    /// perimetro <c>inferred</c>, più omonimi <c>ambiguous</c>, nessuno = nodo <c>external</c>).
    /// <para>
    /// Ogni file è un simbolo <c>module</c> (<c>pkg/mod.py</c> → <c>pkg/mod</c>, <c>pkg/__init__.py</c> → <c>pkg</c>);
    /// gli <c>import</c> diventano archi <c>imports</c> verso il modulo del perimetro o verso un modulo <c>external</c>.
    /// Una classe chiamata come funzione è un'istanza (<c>instantiates</c>); <c>__init__</c> è il costruttore; i
    /// <c>self.x = …</c> dentro i metodi e gli assegnamenti nel corpo della classe sono campi; <c>@property</c> è una
    /// property, <c>@staticmethod</c>/<c>@classmethod</c> segnano <c>isStatic</c>; un nome con <c>_</c> davanti è privato.
    /// </para>
    /// </summary>
    public sealed class TreeSitterPythonExtractor : ICodeExtractor
    {
        public string LanguageId => CodeLanguages.Python;
        public string ExtractorName => "tree-sitter-python";
        public string ExtractorVersion => "2";
        /// <summary>
        /// Oltre questo numero di omonimi un nome non si risolve più «a tutti i candidati»: sarebbero archi a caso (in
        /// PlantUML <c>add</c> ha decine di omonimi). Si emette un solo arco <c>ambiguous</c> verso un nodo external
        /// <c>name</c>, che dice «qui c'è una chiamata che la sola sintassi non sa attribuire».
        /// </summary>
        public const int MaxAmbiguousCandidates = 5;


        private readonly GrammarStore _store;
        private readonly ILogger<TreeSitterPythonExtractor> _logger;

        public TreeSitterPythonExtractor(GrammarStore store, ILogger<TreeSitterPythonExtractor> logger)
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

            var parsed = new List<ParsedFile>();
            foreach (var f in request.AllFiles)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = File.ReadAllText(f.AbsolutePath, Encoding.UTF8); }
                catch (Exception ex) { result.Warnings.Add($"{f.LogicalPath}: non leggibile ({ex.Message})"); continue; }
                var tree = parser.Parse(text);
                if (tree == null) { result.Warnings.Add($"{f.LogicalPath}: tree-sitter non ha prodotto un albero"); continue; }
                parsed.Add(new ParsedFile(f, text, tree));
            }
            var index = new PerimeterIndex();
            foreach (var pf in parsed) index.Collect(pf);

            var toEmit = new HashSet<string>(request.FilesToEmit.Select(f => f.LogicalPath), StringComparer.Ordinal);
            foreach (var pf in parsed)
            {
                ct.ThrowIfCancellationRequested();
                if (!toEmit.Contains(pf.File.LogicalPath)) continue;
                try { result.Scripts.Add((pf.File, new FileEmitter(this, pf, index).Emit())); }
                catch (Exception ex)
                {
                    result.Warnings.Add($"{pf.File.LogicalPath}: estrazione fallita ({ex.GetType().Name}: {ex.Message})");
                    _logger?.LogWarning(ex, "[CodeGraph/tree-sitter] estrazione fallita per {File}", pf.File.LogicalPath);
                }
            }
            foreach (var pf in parsed) pf.Tree.Dispose();
            return result;
        }

        // ------------------------------------------------------------------ naming

        /// <summary><c>pkg/mod.py</c> → <c>pkg/mod</c>; <c>pkg/__init__.py</c> → <c>pkg</c>.</summary>
        public static string ModuleName(string projectRelativePath)
        {
            var m = projectRelativePath.EndsWith(".py", StringComparison.OrdinalIgnoreCase) ? projectRelativePath.Substring(0, projectRelativePath.Length - 3) : projectRelativePath;
            if (m.EndsWith("/__init__", StringComparison.Ordinal)) m = m.Substring(0, m.Length - "/__init__".Length);
            else if (m == "__init__") m = string.Empty;
            return m;
        }

        public static string StableId(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId(CodeLanguages.Python, kind, qualifiedName);

        private static string Sha256(string content)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        }

        private sealed record ParsedFile(CodeSourceFile File, string Text, Tree Tree)
        {
            public string Module => ModuleName(File.LogicalPath);
        }

        // ------------------------------------------------------------------ perimeter index

        private sealed class PerimeterIndex
        {
            public readonly Dictionary<string, string> Modules = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<string>> ClassesByName = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<string>> FunctionsByName = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<string>> MethodsByName = new(StringComparer.Ordinal);

            public void Collect(ParsedFile pf)
            {
                Modules[pf.Module] = StableId("module", pf.Module);
                foreach (var node in pf.Tree.RootNode.NamedChildren) CollectTop(pf, Unwrap(node), null);
            }

            private void CollectTop(ParsedFile pf, Node node, string owner)
            {
                if (node.Type == "class_definition")
                {
                    var name = NameOf(node);
                    if (name == null) return;
                    var qn = pf.Module + ":" + (owner == null ? name : owner + "." + name);
                    Add(ClassesByName, name, StableId("class", qn));
                    var body = node.NamedChildren.FirstOrDefault(c => c.Type == "block");
                    if (body == null) return;
                    foreach (var m in body.NamedChildren)
                    {
                        var inner = Unwrap(m);
                        if (inner.Type == "function_definition")
                        {
                            var mname = NameOf(inner);
                            if (mname == null || mname == "__init__") continue;
                            Add(MethodsByName, mname, StableId(MethodKind(m, mname), qn + "." + mname));
                        }
                        else if (inner.Type == "class_definition") CollectTop(pf, inner, owner == null ? name : owner + "." + name);
                    }
                }
                else if (node.Type == "function_definition" && owner == null)
                {
                    var name = NameOf(node);
                    if (name != null) Add(FunctionsByName, name, StableId("function", pf.Module + ":" + name));
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

        /// <summary>Un <c>decorated_definition</c> avvolge la definizione: si guarda dentro.</summary>
        private static Node Unwrap(Node node)
            => node.Type == "decorated_definition" ? node.NamedChildren.LastOrDefault(c => c.Type is "function_definition" or "class_definition") ?? node : node;

        private static IEnumerable<string> Decorators(Node maybeDecorated)
            => maybeDecorated.Type != "decorated_definition" ? Enumerable.Empty<string>()
               : maybeDecorated.NamedChildren.Where(c => c.Type == "decorator").Select(d =>
               {
                   var inner = d.NamedChildren.FirstOrDefault();
                   if (inner == null) return d.Text.TrimStart('@');
                   if (inner.Type == "call") inner = inner.NamedChildren.FirstOrDefault() ?? inner;
                   return inner.Type == "attribute" ? inner.NamedChildren.LastOrDefault()?.Text ?? inner.Text : inner.Text;
               });

        private static string NameOf(Node decl) => decl.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text;

        private static string MethodKind(Node maybeDecorated, string name)
        {
            if (name == "__init__") return "constructor";
            var decs = Decorators(maybeDecorated).ToList();
            return decs.Contains("property") || decs.Any(d => d.EndsWith(".setter", StringComparison.Ordinal)) ? "property" : "method";
        }

        private static int Line(Node n) => (int)n.StartPosition.Row + 1;
        private static int EndLine(Node n) => (int)n.EndPosition.Row + 1;

        /// <summary>La dichiarazione senza il corpo: <c>def load(self, path: str) -> None</c>.</summary>
        private static string Signature(Node decl)
        {
            var body = decl.NamedChildren.FirstOrDefault(c => c.Type == "block");
            var text = body == null ? decl.Text : decl.Text.Substring(0, Math.Max(0, body.StartIndex - decl.StartIndex));
            return string.Join(" ", text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim())).Trim().TrimEnd(':').Trim();
        }

        private static string Visibility(string name)
            => name.StartsWith("__", StringComparison.Ordinal) && name.EndsWith("__", StringComparison.Ordinal) ? "public"
             : name.StartsWith("_", StringComparison.Ordinal) ? "private" : "public";

        /// <summary>Gli identificatori dentro un'annotazione di tipo (<c>Optional[Thing]</c> → Optional, Thing).</summary>
        private static IEnumerable<string> TypeNames(Node typeNode)
            => typeNode == null ? Enumerable.Empty<string>()
               : Walk(typeNode).Where(n => n.Type == "identifier").Select(n => n.Text).Distinct(StringComparer.Ordinal);

        // ------------------------------------------------------------------ emitter

        private sealed class FileEmitter
        {
            private readonly TreeSitterPythonExtractor _owner;
            private readonly ParsedFile _pf;
            private readonly PerimeterIndex _index;
            private readonly CodeScript _script = new();
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly Dictionary<string, ScriptEdge> _edges = new(StringComparer.Ordinal);
            private readonly HashSet<string> _declared = new(StringComparer.Ordinal);
            private readonly string _moduleId;

            public FileEmitter(TreeSitterPythonExtractor owner, ParsedFile pf, PerimeterIndex index)
            {
                _owner = owner; _pf = pf; _index = index;
                _moduleId = StableId("module", pf.Module);
            }

            public CodeScript Emit()
            {
                _script.Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = _pf.File.LogicalPath,
                    Language = _owner.LanguageId, SourceHash = Sha256(_pf.Text), Extractor = _owner.ExtractorName,
                    ExtractorVersion = _owner.ExtractorVersion, GeneratedAt = DateTime.UtcNow.ToString("o"),
                };
                var root = _pf.Tree.RootNode;
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = _moduleId, Kind = "module", Name = string.IsNullOrEmpty(_pf.Module) ? "__init__" : Path.GetFileName(_pf.Module), QualifiedName = _pf.Module,
                    LineStart = 1, LineEnd = EndLine(root), Signature = _pf.File.LogicalPath, Visibility = "public",
                });
                _declared.Add(_moduleId);

                foreach (var node in root.NamedChildren)
                {
                    switch (node.Type)
                    {
                        case "import_statement":
                        case "import_from_statement":
                            EmitImport(node);
                            break;
                        case "decorated_definition":
                        case "class_definition":
                        case "function_definition":
                            EmitDefinition(node, _moduleId, ownerQn: null, ownerClass: null);
                            break;
                        case "expression_statement":
                            foreach (var a in node.NamedChildren.Where(c => c.Type == "assignment")) EmitModuleVariable(a);
                            break;
                        case "assignment":
                            EmitModuleVariable(node);
                            break;
                    }
                }
                return _script;
            }

            // ---- import

            private void EmitImport(Node import)
            {
                var line = Line(import);
                if (import.Type == "import_statement")
                {
                    foreach (var c in import.NamedChildren)
                    {
                        var dotted = c.Type == "aliased_import" ? c.NamedChildren.FirstOrDefault(x => x.Type == "dotted_name") : c;
                        if (dotted?.Type == "dotted_name") Edge(_moduleId, ResolveModule(dotted.Text, relativeDots: 0), "imports", "extracted", line);
                    }
                    return;
                }
                // from X import a, b
                var first = import.NamedChildren.FirstOrDefault();
                if (first == null) return;
                string moduleText; int dots = 0;
                if (first.Type == "relative_import")
                {
                    dots = first.NamedChildren.FirstOrDefault(c => c.Type == "import_prefix")?.Text.Length ?? 1;
                    moduleText = first.NamedChildren.FirstOrDefault(c => c.Type == "dotted_name")?.Text ?? string.Empty;
                }
                else moduleText = first.Text;
                Edge(_moduleId, ResolveModule(moduleText, dots), "imports", "extracted", line);
            }

            /// <summary><c>pkg.mod</c> → <c>pkg/mod</c> nel perimetro (o <c>pkg/mod/__init__</c> = <c>pkg/mod</c>), <c>.util</c> relativo alla cartella del modulo.</summary>
            private string ResolveModule(string dotted, int relativeDots)
            {
                string candidate;
                if (relativeDots > 0)
                {
                    var dir = Path.GetDirectoryName(_pf.File.LogicalPath)?.Replace('\\', '/') ?? string.Empty;
                    var parts = dir.Length == 0 ? new List<string>() : dir.Split('/').ToList();
                    for (var i = 1; i < relativeDots && parts.Count > 0; i++) parts.RemoveAt(parts.Count - 1);
                    if (dotted.Length > 0) parts.AddRange(dotted.Split('.'));
                    candidate = string.Join("/", parts);
                }
                else candidate = dotted.Replace('.', '/');

                if (_index.Modules.TryGetValue(candidate, out var id)) return id;
                // Un modulo di pacchetto cercato per suffisso: «app.util» quando il perimetro è «src/app/util».
                var bySuffix = _index.Modules.Keys.Where(k => k.EndsWith("/" + candidate, StringComparison.Ordinal)).ToList();
                if (bySuffix.Count == 1) return _index.Modules[bySuffix[0]];
                return External("module", dotted.Length == 0 ? candidate : dotted, dotted.Length == 0 ? Path.GetFileName(candidate) : dotted.Split('.').Last());
            }

            // ---- definitions

            private void EmitDefinition(Node node, string parentId, string ownerQn, string ownerClass)
            {
                var decl = Unwrap(node);
                if (decl.Type == "class_definition") EmitClass(node, decl, parentId, ownerQn);
                else if (decl.Type == "function_definition") EmitFunction(node, decl, parentId, ownerQn, ownerClass);
            }

            private void EmitClass(Node wrapper, Node decl, string parentId, string ownerQn)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var qn = _pf.Module + ":" + (ownerQn == null ? name : ownerQn + "." + name);
                var classId = Declare(decl, "class", name, qn, Signature(decl), null, Visibility(name), parentId);
                Edge(parentId, classId, "contains", "extracted", Line(decl));

                var bases = decl.NamedChildren.FirstOrDefault(c => c.Type == "argument_list");
                if (bases != null)
                {
                    foreach (var b in bases.NamedChildren)
                    {
                        if (b.Type == "keyword_argument") continue;          // metaclass=…
                        var bname = b.Type == "attribute" ? b.NamedChildren.LastOrDefault()?.Text : b.Type == "identifier" ? b.Text : null;
                        if (bname != null) Edge(classId, ResolveClass(bname), "inherits", ConfidenceOfClass(bname), Line(b));
                    }
                }

                var body = decl.NamedChildren.FirstOrDefault(c => c.Type == "block");
                if (body == null) return;
                var innerQn = ownerQn == null ? name : ownerQn + "." + name;
                foreach (var m in body.NamedChildren)
                {
                    switch (m.Type)
                    {
                        case "decorated_definition":
                        case "function_definition":
                        case "class_definition":
                            EmitDefinition(m, classId, innerQn, name);
                            break;
                        case "expression_statement":
                            foreach (var a in m.NamedChildren.Where(c => c.Type == "assignment")) EmitField(a, classId, qn);
                            break;
                        case "assignment":   // la grammatica può dare l'assegnamento senza expression_statement intorno
                            EmitField(m, classId, qn);
                            break;
                    }
                }
            }

            private void EmitField(Node assignment, string classId, string classQn)
            {
                var target = assignment.NamedChildren.FirstOrDefault();
                if (target == null) return;
                string fname;
                if (target.Type == "identifier") fname = target.Text;
                else if (target.Type == "attribute" && target.NamedChildren.FirstOrDefault()?.Text == "self") fname = target.NamedChildren.LastOrDefault()?.Text;
                else return;
                if (fname == null) return;
                var typeNode = assignment.NamedChildren.FirstOrDefault(c => c.Type == "type");
                var fqn = classQn + "." + fname;
                var fid = StableId("field", fqn);
                if (_declared.Contains(fid)) return;         // self.x assegnato in più metodi: un campo solo
                Declare(assignment, "field", fname, fqn, Signature(assignment), typeNode?.Text, Visibility(fname), classId);
                Edge(classId, fid, "contains", "extracted", Line(assignment));
                foreach (var t in TypeNames(typeNode)) Edge(fid, ResolveClass(t), "references", ConfidenceOfClass(t), Line(assignment));
            }

            private void EmitModuleVariable(Node assignment)
            {
                var target = assignment.NamedChildren.FirstOrDefault();
                if (target?.Type != "identifier") return;
                var typeNode = assignment.NamedChildren.FirstOrDefault(c => c.Type == "type");
                var fqn = _pf.Module + ":" + target.Text;
                var fid = StableId("field", fqn);
                if (_declared.Contains(fid)) return;
                Declare(assignment, "field", target.Text, fqn, Signature(assignment), typeNode?.Text, Visibility(target.Text), _moduleId);
                Edge(_moduleId, fid, "contains", "extracted", Line(assignment));
                WalkBody(assignment, fid, null, null);
            }

            private void EmitFunction(Node wrapper, Node decl, string parentId, string ownerQn, string ownerClass)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var inClass = ownerClass != null;
                var decorators = Decorators(wrapper).ToList();
                var kind = inClass ? MethodKind(wrapper, name) : "function";
                var qn = _pf.Module + ":" + (ownerQn == null ? name : ownerQn + "." + name);
                var returnNode = decl.NamedChildren.FirstOrDefault(c => c.Type == "type");
                var isAsync = decl.Text.StartsWith("async", StringComparison.Ordinal);
                var isStatic = decorators.Contains("staticmethod") || decorators.Contains("classmethod");
                var id = Declare(decl, kind, name, qn, Signature(decl), kind == "constructor" ? null : returnNode?.Text, Visibility(name), parentId, isAsync, isStatic);
                Edge(parentId, id, "contains", "extracted", Line(decl));
                foreach (var t in TypeNames(returnNode)) Edge(id, ResolveClass(t), "references", ConfidenceOfClass(t), Line(decl));
                EmitParameters(decl, id, skipSelf: inClass && !decorators.Contains("staticmethod"));

                var body = decl.NamedChildren.FirstOrDefault(c => c.Type == "block");
                if (body == null) return;
                if (inClass)
                {
                    // self.x = … dentro i metodi: campi della classe
                    foreach (var a in Walk(body).Where(n => n.Type == "assignment"))
                    {
                        var target = a.NamedChildren.FirstOrDefault();
                        if (target?.Type == "attribute" && target.NamedChildren.FirstOrDefault()?.Text == "self") EmitField(a, parentId, _pf.Module + ":" + ownerQn);
                    }
                }
                WalkBody(body, id, ownerQn, ownerClass);
            }

            // ---- parameters

            private void EmitParameters(Node decl, string ownerId, bool skipSelf)
            {
                var prms = decl.NamedChildren.FirstOrDefault(c => c.Type == "parameters");
                if (prms == null) return;
                var pos = 0;
                var first = true;
                foreach (var p in prms.NamedChildren)
                {
                    if (p.Type is "keyword_separator" or "positional_separator") continue;
                    var isFirst = first; first = false;
                    string name = null, type = null, def = null, direction = "in";
                    Node typeNode = null;
                    switch (p.Type)
                    {
                        case "identifier": name = p.Text; break;
                        case "typed_parameter":
                            name = p.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text ?? p.NamedChildren.FirstOrDefault()?.Text;
                            typeNode = p.NamedChildren.FirstOrDefault(c => c.Type == "type"); type = typeNode?.Text;
                            break;
                        case "default_parameter":
                            name = p.NamedChildren.FirstOrDefault()?.Text; def = p.NamedChildren.LastOrDefault()?.Text;
                            break;
                        case "typed_default_parameter":
                            name = p.NamedChildren.FirstOrDefault()?.Text; typeNode = p.NamedChildren.FirstOrDefault(c => c.Type == "type"); type = typeNode?.Text;
                            def = p.NamedChildren.LastOrDefault()?.Text;
                            break;
                        case "list_splat_pattern": name = "*" + (NameOf(p) ?? p.Text.TrimStart('*')); direction = "params"; break;
                        case "dictionary_splat_pattern": name = "**" + (NameOf(p) ?? p.Text.TrimStart('*')); direction = "params"; break;
                        default: name = p.Text; break;
                    }
                    if (name == null) continue;
                    if (isFirst && skipSelf && name is "self" or "cls") continue;
                    _script.Parameters.Add(new ScriptParameter
                    {
                        SymbolId = ownerId, Position = pos++, Name = name, ParameterType = type, Direction = direction,
                        DefaultValue = def, IsNullable = def == "None" || (type != null && type.StartsWith("Optional", StringComparison.Ordinal)),
                    });
                    foreach (var t in TypeNames(typeNode)) Edge(ownerId, ResolveClass(t), "references", ConfidenceOfClass(t), Line(p));
                }
            }

            // ---- bodies: calls, instances

            private void WalkBody(Node body, string ownerId, string ownerQn, string ownerClass)
            {
                foreach (var n in Walk(body))
                {
                    if (n.Type != "call") continue;
                    var callee = n.NamedChildren.FirstOrDefault();
                    if (callee == null) continue;
                    var line = Line(n);
                    if (callee.Type == "identifier")
                    {
                        var name = callee.Text;
                        if (_index.ClassesByName.TryGetValue(name, out var classes))
                        {
                            var conf = classes.Count == 1 ? "inferred" : "ambiguous";
                            foreach (var id in classes) Edge(ownerId, id, "instantiates", conf, line);
                        }
                        else if (_index.FunctionsByName.TryGetValue(name, out var fns) && fns.Count <= MaxAmbiguousCandidates)
                        {
                            var conf = fns.Count == 1 ? "inferred" : "ambiguous";
                            foreach (var id in fns) Edge(ownerId, id, "calls", conf, line);
                        }
                        else Edge(ownerId, External("function", fns != null ? "?." + name : name, name), "calls", "ambiguous", line);
                    }
                    else if (callee.Type == "attribute")
                    {
                        var obj = callee.NamedChildren.FirstOrDefault();
                        var attr = callee.NamedChildren.LastOrDefault(c => c.Type == "identifier")?.Text;
                        if (attr == null || obj == null) continue;
                        if (obj.Type == "identifier" && obj.Text is "self" or "cls" && ownerQn != null)
                        {
                            var ownMethod = StableId("method", _pf.Module + ":" + ownerQn + "." + attr);
                            var ownProp = StableId("property", _pf.Module + ":" + ownerQn + "." + attr);
                            if (_index.MethodsByName.TryGetValue(attr, out var own) && (own.Contains(ownMethod) || own.Contains(ownProp)))
                            {
                                Edge(ownerId, own.Contains(ownMethod) ? ownMethod : ownProp, "calls", "extracted", line);
                                continue;
                            }
                        }
                        if (obj.Type == "identifier" && _index.ClassesByName.TryGetValue(obj.Text, out var cls) && cls.Count == 1)
                        {
                            // Classe.metodo(): statico o di classe
                            var target = _index.MethodsByName.TryGetValue(attr, out var ms0) ? ms0.FirstOrDefault(id => true) : null;
                            if (ms0 != null && ms0.Count >= 1) { foreach (var id in ms0) Edge(ownerId, id, "calls", ms0.Count == 1 ? "inferred" : "ambiguous", line); continue; }
                        }
                        if (_index.MethodsByName.TryGetValue(attr, out var ms) && ms.Count <= MaxAmbiguousCandidates)
                        {
                            var conf = ms.Count == 1 ? "inferred" : "ambiguous";
                            foreach (var id in ms) Edge(ownerId, id, "calls", conf, line);
                        }
                        else Edge(ownerId, External("method", ms != null ? "?." + attr : callee.Text, attr), "calls", "ambiguous", line);
                    }
                }
            }

            // ---- resolution

            private string ResolveClass(string name)
            {
                if (_index.ClassesByName.TryGetValue(name, out var list) && list.Count >= 1) return list[0];
                return External("type", name, name);
            }

            private string ConfidenceOfClass(string name)
                => _index.ClassesByName.TryGetValue(name, out var list) ? (list.Count == 1 ? "inferred" : "ambiguous") : "extracted";

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
                if (!_declared.Add(id)) return id;
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
                // Un arco per (sorgente, destinazione, relazione): mille chiamate allo stesso metodo nello stesso corpo non
                // sono mille fatti (PlantUML: 29.000 archi in un file, provato il 10/10). Se lo stesso arco torna con una
                // confidenza migliore (this.run() dopo helper.run()), vince la migliore e la sua riga.
                var key = $"{source}|{target}|{relation}";
                if (_edges.TryGetValue(key, out var existing))
                {
                    if (ConfidenceRank(confidence) > ConfidenceRank(existing.Confidence)) { existing.Confidence = confidence; existing.Line = line; }
                    return;
                }
                var edge = new ScriptEdge { SourceId = source, TargetId = target, Relation = relation, Confidence = confidence, Line = line };
                _edges[key] = edge;
                _script.Edges.Add(edge);
            }
            private static int ConfidenceRank(string confidence) => confidence switch { "extracted" => 2, "inferred" => 1, _ => 0 };
        }
    }
}
