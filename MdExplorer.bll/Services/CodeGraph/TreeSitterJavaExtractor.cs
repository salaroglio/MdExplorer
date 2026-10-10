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
    /// L'estrattore Java su tree-sitter (F8, D1, D8). Come per C#, i tipi hanno il nome qualificato dal package
    /// (<c>com.acme.app.Foo</c>, annidati <c>com.acme.app.Foo.Inner</c>) e i metodi portano i tipi dei parametri
    /// (<c>com.acme.app.Foo.bar(String, int...)</c>), così gli overload sono simboli distinti. Tree-sitter è solo
    /// sintassi: un nome di tipo si risolve nell'ordine tipi del file, import, stesso package, omonimo unico nel
    /// perimetro (<c>inferred</c>), più omonimi (<c>ambiguous</c>); <c>java.lang</c> e gli import <c>java.*</c>/
    /// <c>javax.*</c> non fanno nodi (sarebbero rumore: String, List, Override…); il resto è <c>external</c>.
    /// Le chiamate: <c>x()</c> e <c>this.x()</c> su un metodo della classe sono <c>extracted</c>, <c>Tipo.x()</c>
    /// statiche su un tipo del perimetro <c>extracted</c>, <c>obj.x()</c> per nome con la confidenza, <c>new T()</c>
    /// è <c>instantiates</c>. Gli stereotipi Spring danno un kind proprio come i decoratori Angular:
    /// <c>@Service</c>/<c>@Repository</c> → service, <c>@Component</c>/<c>@Controller</c>/<c>@RestController</c> → component.
    /// </summary>
    public sealed class TreeSitterJavaExtractor : ICodeExtractor
    {
        public string LanguageId => CodeLanguages.Java;
        public string ExtractorName => "tree-sitter-java";
        public string ExtractorVersion => "1";

        private readonly GrammarStore _store;
        private readonly ILogger<TreeSitterJavaExtractor> _logger;

        public TreeSitterJavaExtractor(GrammarStore store, ILogger<TreeSitterJavaExtractor> logger)
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
                catch (Exception ex) { result.Warnings.Add($"{f.ProjectRelativePath}: non leggibile ({ex.Message})"); continue; }
                var tree = parser.Parse(text);
                if (tree == null) { result.Warnings.Add($"{f.ProjectRelativePath}: tree-sitter non ha prodotto un albero"); continue; }
                parsed.Add(new ParsedFile(f, text, tree, PackageOf(tree.RootNode)));
            }
            var index = new PerimeterIndex();
            foreach (var pf in parsed) index.Collect(pf);

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

        public static string StableId(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId(CodeLanguages.Java, kind, qualifiedName);

        private static string Sha256(string content)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        }

        private sealed record ParsedFile(CodeSourceFile File, string Text, Tree Tree, string Package);

        private static string PackageOf(Node root)
            => root.NamedChildren.FirstOrDefault(c => c.Type == "package_declaration")?.NamedChildren.FirstOrDefault(c => c.Type is "scoped_identifier" or "identifier")?.Text ?? string.Empty;

        /// <summary>I tipi di <c>java.lang</c> che non valgono un nodo: ci sono in ogni file.</summary>
        private static readonly HashSet<string> JavaLang = new(StringComparer.Ordinal)
        {
            "String", "Object", "Integer", "Long", "Double", "Float", "Boolean", "Character", "Byte", "Short", "Void", "Number", "Math", "System",
            "Exception", "RuntimeException", "Throwable", "Error", "IllegalArgumentException", "IllegalStateException", "NullPointerException",
            "UnsupportedOperationException", "Runnable", "Thread", "Iterable", "Comparable", "CharSequence", "StringBuilder", "StringBuffer",
            "Class", "Enum", "Record", "Override", "Deprecated", "SuppressWarnings", "FunctionalInterface", "SafeVarargs", "AutoCloseable", "Cloneable",
        };

        private static readonly string[] TypeDeclarations = { "class_declaration", "interface_declaration", "enum_declaration", "record_declaration", "annotation_type_declaration" };

        // ------------------------------------------------------------------ perimeter index

        private sealed class PerimeterIndex
        {
            public readonly Dictionary<string, string> TypesByFqn = new(StringComparer.Ordinal);                         // fqn → id
            public readonly Dictionary<string, List<(string Id, string Fqn)>> TypesByName = new(StringComparer.Ordinal); // simple name → candidati
            public readonly Dictionary<string, List<string>> MethodsByName = new(StringComparer.Ordinal);                // nome → ids (tutti i tipi)
            public readonly Dictionary<string, Dictionary<string, List<string>>> MethodsByOwner = new(StringComparer.Ordinal); // fqn tipo → nome → ids

            public void Collect(ParsedFile pf)
            {
                foreach (var node in pf.Tree.RootNode.NamedChildren)
                    if (TypeDeclarations.Contains(node.Type)) CollectType(pf, node, null);
            }

            private void CollectType(ParsedFile pf, Node decl, string outerFqn)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var fqn = outerFqn != null ? outerFqn + "." + name : (pf.Package.Length == 0 ? name : pf.Package + "." + name);
                var id = StableId(KindOf(decl), fqn);
                TypesByFqn[fqn] = id;
                if (!TypesByName.TryGetValue(name, out var list)) TypesByName[name] = list = new();
                list.Add((id, fqn));
                var body = BodyOf(decl);
                if (body == null) return;
                foreach (var m in body.NamedChildren)
                {
                    if (m.Type == "method_declaration")
                    {
                        var mname = NameOf(m);
                        if (mname == null) continue;
                        var mid = StableId("method", fqn + "." + mname + ParameterTypeList(m));
                        if (!MethodsByName.TryGetValue(mname, out var ml)) MethodsByName[mname] = ml = new();
                        ml.Add(mid);
                        if (!MethodsByOwner.TryGetValue(fqn, out var byName)) MethodsByOwner[fqn] = byName = new(StringComparer.Ordinal);
                        if (!byName.TryGetValue(mname, out var ol)) byName[mname] = ol = new();
                        ol.Add(mid);
                    }
                    else if (TypeDeclarations.Contains(m.Type)) CollectType(pf, m, fqn);
                }
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

        private static string NameOf(Node decl) => decl.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text;

        private static Node BodyOf(Node decl)
            => decl.NamedChildren.FirstOrDefault(c => c.Type is "class_body" or "interface_body" or "enum_body" or "annotation_type_body" or "block" or "constructor_body");

        private static IEnumerable<string> Annotations(Node decl)
            => decl.NamedChildren.FirstOrDefault(c => c.Type == "modifiers")?.NamedChildren
                   .Where(c => c.Type is "marker_annotation" or "annotation")
                   .Select(a => a.NamedChildren.FirstOrDefault(c => c.Type is "identifier" or "scoped_identifier")?.Text.Split('.').Last())
                   .Where(a => a != null) ?? Enumerable.Empty<string>();

        private static string KindOf(Node decl)
        {
            switch (decl.Type)
            {
                case "interface_declaration": case "annotation_type_declaration": return "interface";
                case "enum_declaration": return "enum";
                case "record_declaration": return "record";
            }
            foreach (var a in Annotations(decl))
            {
                switch (a)
                {
                    case "Service": case "Repository": return "service";
                    case "Component": case "Controller": case "RestController": return "component";
                }
            }
            return "class";
        }

        private static string Visibility(Node decl)
        {
            var mods = decl.NamedChildren.FirstOrDefault(c => c.Type == "modifiers")?.Text ?? string.Empty;
            var tokens = mods.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Contains("public")) return "public";
            if (tokens.Contains("private")) return "private";
            if (tokens.Contains("protected")) return "protected";
            return decl.Parent?.Type == "interface_body" ? "public" : "package";
        }

        private static bool HasModifier(Node decl, string modifier)
            => (decl.NamedChildren.FirstOrDefault(c => c.Type == "modifiers")?.Text ?? string.Empty)
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Contains(modifier);

        private static int Line(Node n) => (int)n.StartPosition.Row + 1;
        private static int EndLine(Node n) => (int)n.EndPosition.Row + 1;

        /// <summary>La dichiarazione senza corpo, annotazioni comprese, su una riga.</summary>
        private static string Signature(Node decl)
        {
            var body = BodyOf(decl);
            var text = body == null ? decl.Text : decl.Text.Substring(0, Math.Max(0, body.StartIndex - decl.StartIndex));
            return string.Join(" ", text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim())).Trim().TrimEnd('{', ';').Trim();
        }

        private static bool IsTypeNode(Node n)
            => n.Type is "type_identifier" or "generic_type" or "integral_type" or "floating_point_type" or "boolean_type" or "void_type" or "array_type" or "scoped_type_identifier";

        /// <summary>Il tipo di ritorno di un metodo: il figlio «tipo» prima del nome.</summary>
        private static Node ReturnTypeOf(Node method)
        {
            var name = method.NamedChildren.FirstOrDefault(c => c.Type == "identifier");
            return method.NamedChildren.FirstOrDefault(c => IsTypeNode(c) && (name == null || c.StartIndex < name.StartIndex));
        }

        /// <summary><c>(String, int...)</c>: i tipi dei parametri come scritti, per distinguere gli overload.</summary>
        private static string ParameterTypeList(Node decl)
        {
            var prms = decl.NamedChildren.FirstOrDefault(c => c.Type == "formal_parameters");
            if (prms == null) return "()";
            var types = prms.NamedChildren.Where(p => p.Type is "formal_parameter" or "spread_parameter")
                .Select(p => (p.NamedChildren.FirstOrDefault(IsTypeNode)?.Text ?? "?") + (p.Type == "spread_parameter" ? "..." : string.Empty));
            return "(" + string.Join(", ", types) + ")";
        }

        /// <summary>I nomi di tipo dentro un nodo tipo, generici e array compresi (<c>Map&lt;String, Hit[]&gt;</c> → Map, String, Hit); mai i primitivi.</summary>
        private static IEnumerable<string> TypeNames(Node typeNode)
            => typeNode == null ? Enumerable.Empty<string>()
               : Walk(typeNode).Where(n => n.Type == "type_identifier").Select(n => n.Text).Distinct(StringComparer.Ordinal);

        // ------------------------------------------------------------------ emitter

        private sealed class FileEmitter
        {
            private readonly TreeSitterJavaExtractor _owner;
            private readonly ParsedFile _pf;
            private readonly PerimeterIndex _index;
            private readonly CodeScript _script = new();
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly HashSet<string> _edges = new(StringComparer.Ordinal);
            private readonly Dictionary<string, string> _imports = new(StringComparer.Ordinal);   // simple name → fqn
            private readonly Dictionary<string, string> _fileTypes = new(StringComparer.Ordinal); // simple name → fqn dei tipi di questo file
            private readonly List<(string Fqn, int Line)> _importEdges = new();

            public FileEmitter(TreeSitterJavaExtractor owner, ParsedFile pf, PerimeterIndex index)
            {
                _owner = owner; _pf = pf; _index = index;
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
                foreach (var imp in root.NamedChildren.Where(c => c.Type == "import_declaration")) CollectImport(imp);
                foreach (var decl in root.NamedChildren.Where(c => TypeDeclarations.Contains(c.Type))) CollectFileTypes(decl, null);
                foreach (var decl in root.NamedChildren.Where(c => TypeDeclarations.Contains(c.Type))) EmitType(decl, null, null);
                return _script;
            }

            private void CollectFileTypes(Node decl, string outerFqn)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var fqn = outerFqn != null ? outerFqn + "." + name : (_pf.Package.Length == 0 ? name : _pf.Package + "." + name);
                _fileTypes[name] = fqn;
                var body = BodyOf(decl);
                if (body == null) return;
                foreach (var m in body.NamedChildren.Where(c => TypeDeclarations.Contains(c.Type))) CollectFileTypes(m, fqn);
            }

            // ---- import

            private void CollectImport(Node imp)
            {
                if (imp.NamedChildren.Any(c => c.Type == "asterisk")) return;                         // import a.b.*: niente da agganciare
                var scoped = imp.NamedChildren.FirstOrDefault(c => c.Type is "scoped_identifier" or "identifier")?.Text;
                if (scoped == null) return;
                var isStatic = imp.Text.StartsWith("import static", StringComparison.Ordinal);
                var fqn = isStatic ? scoped.Substring(0, Math.Max(0, scoped.LastIndexOf('.'))) : scoped;
                if (fqn.Length == 0) return;
                var simple = fqn.Split('.').Last();
                _imports[simple] = fqn;
                _importEdges.Add((fqn, Line(imp)));
            }

            // ---- types

            private void EmitType(Node decl, string parentId, string outerFqn)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var fqn = outerFqn != null ? outerFqn + "." + name : (_pf.Package.Length == 0 ? name : _pf.Package + "." + name);
                var kind = KindOf(decl);
                var typeId = Declare(decl, kind, name, fqn, Signature(decl), null, Visibility(decl), parentId, isStatic: HasModifier(decl, "static"));
                if (parentId != null) Edge(parentId, typeId, "contains", "extracted", Line(decl));
                else
                {
                    // Gli import del file si agganciano ai tipi di primo livello (in Java il file non è un modulo).
                    foreach (var (ifqn, line) in _importEdges)
                    {
                        var target = ResolveImported(ifqn);
                        if (target != null) Edge(typeId, target, "imports", "extracted", line);
                    }
                }

                // extends / implements
                var superclass = decl.NamedChildren.FirstOrDefault(c => c.Type == "superclass");
                foreach (var t in TypeNames(superclass).Take(1)) Edge(typeId, ResolveType(t), "inherits", ConfidenceOfType(t), Line(superclass));
                foreach (var clause in decl.NamedChildren.Where(c => c.Type is "super_interfaces" or "extends_interfaces"))
                {
                    var relation = clause.Type == "super_interfaces" ? "implements" : "inherits";
                    var list = clause.NamedChildren.FirstOrDefault(c => c.Type == "type_list") ?? clause;
                    foreach (var t in list.NamedChildren.Where(IsTypeNode))
                    {
                        var head = TypeNames(t).FirstOrDefault();
                        if (head != null) Edge(typeId, ResolveType(head), relation, ConfidenceOfType(head), Line(t));
                    }
                }

                // record: i componenti sono campi
                if (decl.Type == "record_declaration")
                {
                    var prms = decl.NamedChildren.FirstOrDefault(c => c.Type == "formal_parameters");
                    if (prms != null)
                        foreach (var p in prms.NamedChildren.Where(c => c.Type == "formal_parameter"))
                        {
                            var pname = NameOf(p); var ptype = p.NamedChildren.FirstOrDefault(IsTypeNode);
                            if (pname == null) continue;
                            var fid = Declare(p, "field", pname, fqn + "." + pname, p.Text, ptype?.Text, "private", typeId);
                            Edge(typeId, fid, "contains", "extracted", Line(p));
                            foreach (var t in TypeNames(ptype)) Reference(fid, t, Line(p));
                        }
                }

                var body = BodyOf(decl);
                if (body == null) return;
                foreach (var m in body.NamedChildren)
                {
                    switch (m.Type)
                    {
                        case "field_declaration": case "constant_declaration": EmitField(m, typeId, fqn); break;
                        case "enum_constant":
                            {
                                var cname = NameOf(m);
                                if (cname == null) break;
                                var cid = Declare(m, "field", cname, fqn + "." + cname, cname, name, "public", typeId, isStatic: true);
                                Edge(typeId, cid, "contains", "extracted", Line(m));
                                break;
                            }
                        case "enum_body_declarations":
                            foreach (var inner in m.NamedChildren) EmitMember(inner, typeId, fqn, name);
                            break;
                        default: EmitMember(m, typeId, fqn, name); break;
                    }
                }
            }

            private void EmitMember(Node m, string typeId, string fqn, string typeName)
            {
                switch (m.Type)
                {
                    case "method_declaration": EmitMethod(m, typeId, fqn, "method"); break;
                    case "constructor_declaration": EmitMethod(m, typeId, fqn, "constructor"); break;
                    case "field_declaration": case "constant_declaration": EmitField(m, typeId, fqn); break;
                    case "class_declaration": case "interface_declaration": case "enum_declaration": case "record_declaration": case "annotation_type_declaration":
                        EmitType(m, typeId, fqn); break;
                    case "static_initializer": case "block": WalkBody(m, typeId, fqn); break;
                }
            }

            private void EmitField(Node field, string typeId, string fqn)
            {
                var typeNode = field.NamedChildren.FirstOrDefault(IsTypeNode);
                foreach (var v in field.NamedChildren.Where(c => c.Type == "variable_declarator"))
                {
                    var name = NameOf(v);
                    if (name == null) continue;
                    var fid = Declare(v, "field", name, fqn + "." + name, Signature(field), typeNode?.Text, Visibility(field), typeId, isStatic: HasModifier(field, "static"));
                    Edge(typeId, fid, "contains", "extracted", Line(v));
                    foreach (var t in TypeNames(typeNode)) Reference(fid, t, Line(field));
                    WalkBody(v, fid, fqn);   // inizializzatore: new X(), chiamate
                }
            }

            private void EmitMethod(Node m, string typeId, string fqn, string kind)
            {
                var name = NameOf(m);
                if (name == null) return;
                var returnNode = kind == "constructor" ? null : ReturnTypeOf(m);
                var qn = fqn + "." + name + ParameterTypeList(m);
                var id = Declare(m, kind, name, qn, Signature(m), returnNode?.Text, Visibility(m), typeId, isStatic: HasModifier(m, "static"));
                Edge(typeId, id, "contains", "extracted", Line(m));
                foreach (var t in TypeNames(returnNode)) Reference(id, t, Line(m));
                EmitParameters(m, id);
                var throws = m.NamedChildren.FirstOrDefault(c => c.Type == "throws");
                foreach (var t in TypeNames(throws)) Reference(id, t, Line(m));
                var body = BodyOf(m);
                if (body != null) WalkBody(body, id, fqn);
            }

            private void EmitParameters(Node decl, string ownerId)
            {
                var prms = decl.NamedChildren.FirstOrDefault(c => c.Type == "formal_parameters");
                if (prms == null) return;
                var pos = 0;
                foreach (var p in prms.NamedChildren)
                {
                    if (p.Type is not ("formal_parameter" or "spread_parameter")) continue;
                    var typeNode = p.NamedChildren.FirstOrDefault(IsTypeNode);
                    var nameNode = p.Type == "spread_parameter"
                        ? p.NamedChildren.FirstOrDefault(c => c.Type == "variable_declarator")?.NamedChildren.FirstOrDefault(c => c.Type == "identifier")
                        : p.NamedChildren.FirstOrDefault(c => c.Type == "identifier");
                    _script.Parameters.Add(new ScriptParameter
                    {
                        SymbolId = ownerId, Position = pos++, Name = nameNode?.Text ?? p.Text,
                        ParameterType = typeNode == null ? null : typeNode.Text + (p.Type == "spread_parameter" ? "..." : string.Empty),
                        Direction = p.Type == "spread_parameter" ? "params" : "in", IsNullable = false,
                    });
                    foreach (var t in TypeNames(typeNode)) Reference(ownerId, t, Line(p));
                }
            }

            // ---- bodies: calls, new

            private void WalkBody(Node body, string ownerId, string ownerFqn)
            {
                foreach (var n in Walk(body))
                {
                    if (n.Type == "method_invocation")
                    {
                        var nameNode = n.NamedChildren.LastOrDefault(c => c.Type == "identifier");
                        if (nameNode == null) continue;
                        var mname = nameNode.Text;
                        var obj = n.NamedChildren.FirstOrDefault();
                        var line = Line(n);
                        var implicitThis = obj == null || obj == nameNode || obj.Type == "this";
                        if (implicitThis)
                        {
                            if (OwnMethods(ownerFqn, mname, out var own)) { foreach (var id in own) Edge(ownerId, id, "calls", own.Count == 1 ? "extracted" : "inferred", line); continue; }
                            CallByName(ownerId, mname, "this." + mname, line);
                            continue;
                        }
                        if (obj.Type == "identifier" && ResolveTypeFqn(obj.Text) is string staticFqn && _index.MethodsByOwner.TryGetValue(staticFqn, out var ofStatic) && ofStatic.TryGetValue(mname, out var statics))
                        {
                            foreach (var id in statics) Edge(ownerId, id, "calls", statics.Count == 1 ? "extracted" : "inferred", line);
                            continue;
                        }
                        CallByName(ownerId, mname, obj.Text + "." + mname, line);
                    }
                    else if (n.Type == "object_creation_expression")
                    {
                        var typeNode = n.NamedChildren.FirstOrDefault(IsTypeNode);
                        var head = TypeNames(typeNode).FirstOrDefault();
                        if (head == null) continue;
                        var target = ResolveType(head);
                        if (target != null) Edge(ownerId, target, "instantiates", ConfidenceOfType(head), Line(n));
                    }
                }
            }

            private bool OwnMethods(string ownerFqn, string name, out List<string> ids)
            {
                ids = null;
                return ownerFqn != null && _index.MethodsByOwner.TryGetValue(ownerFqn, out var byName) && byName.TryGetValue(name, out ids) && ids.Count > 0;
            }

            private void CallByName(string ownerId, string name, string externalName, int line)
            {
                if (_index.MethodsByName.TryGetValue(name, out var ms))
                {
                    var conf = ms.Count == 1 ? "inferred" : "ambiguous";
                    foreach (var id in ms) Edge(ownerId, id, "calls", conf, line);
                }
                else Edge(ownerId, External("method", externalName, name), "calls", "ambiguous", line);
            }

            // ---- resolution

            /// <summary>Il fqn di un nome semplice: tipi del file, import, stesso package, omonimo unico nel perimetro; null se sconosciuto.</summary>
            private string ResolveTypeFqn(string simple)
            {
                if (_fileTypes.TryGetValue(simple, out var f)) return f;
                if (_imports.TryGetValue(simple, out var i) && _index.TypesByFqn.ContainsKey(i)) return i;
                var samePackage = _pf.Package.Length == 0 ? simple : _pf.Package + "." + simple;
                if (_index.TypesByFqn.ContainsKey(samePackage)) return samePackage;
                if (_index.TypesByName.TryGetValue(simple, out var list) && list.Count == 1) return list[0].Fqn;
                return null;
            }

            /// <summary>L'id del tipo, oppure external; null per java.lang e per gli import java.*/javax.* (niente nodo).</summary>
            private string ResolveType(string simple)
            {
                var fqn = ResolveTypeFqn(simple);
                if (fqn != null && _index.TypesByFqn.TryGetValue(fqn, out var id)) return id;
                if (_index.TypesByName.TryGetValue(simple, out var list) && list.Count > 1) return list[0].Id;
                if (_imports.TryGetValue(simple, out var imported))
                    return IsPlatform(imported) ? null : External("type", imported, simple);
                if (JavaLang.Contains(simple)) return null;
                return External("type", _pf.Package.Length == 0 ? simple : _pf.Package + "." + simple, simple);
            }

            private string ConfidenceOfType(string simple)
            {
                if (ResolveTypeFqn(simple) is string fqn && _index.TypesByFqn.ContainsKey(fqn))
                    return _fileTypes.ContainsKey(simple) || _imports.ContainsKey(simple) || (_index.TypesByName.TryGetValue(simple, out var l) && l.Count == 1 && l[0].Fqn == fqn && fqn == (_pf.Package.Length == 0 ? simple : _pf.Package + "." + simple)) ? "extracted" : "inferred";
                return _index.TypesByName.TryGetValue(simple, out var list) && list.Count > 1 ? "ambiguous" : "extracted";
            }

            private string ResolveImported(string fqn)
            {
                if (_index.TypesByFqn.TryGetValue(fqn, out var id)) return id;
                return IsPlatform(fqn) ? null : External("type", fqn, fqn.Split('.').Last());
            }

            private static bool IsPlatform(string fqn) => fqn.StartsWith("java.", StringComparison.Ordinal) || fqn.StartsWith("javax.", StringComparison.Ordinal) || fqn.StartsWith("jakarta.", StringComparison.Ordinal);

            private void Reference(string sourceId, string simpleType, int line)
            {
                var target = ResolveType(simpleType);
                if (target != null) Edge(sourceId, target, "references", ConfidenceOfType(simpleType), line);
            }

            private string External(string what, string qualifiedName, string name)
            {
                var id = StableId("external", what + ":" + qualifiedName);
                if (_externals.Add(id)) _script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = name, QualifiedName = qualifiedName });
                return id;
            }

            // ---- helpers

            private string Declare(Node node, string kind, string name, string qualifiedName, string signature, string returnType, string visibility, string parentId, bool isStatic = false)
            {
                var id = StableId(kind, qualifiedName);
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = id, Kind = kind, Name = name, QualifiedName = qualifiedName, LineStart = Line(node), LineEnd = EndLine(node),
                    Signature = signature, ReturnType = returnType, Visibility = visibility, IsStatic = isStatic, ParentId = parentId,
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
