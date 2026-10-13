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
    /// Le chiamate (D38, F11) si risolvono con quello che il codice scrive, prima del nome: il tipo del ricevente
    /// (variabile locale, parametro, campo anche ereditato, <c>new T()</c>, <c>Tipo.x()</c> statica), la gerarchia
    /// della classe (madri e interfacce del perimetro), le classi che la contengono, gli import statici; solo se tutto
    /// questo non dice niente, il nome su tutto il perimetro (<c>inferred</c>/<c>ambiguous</c>). Un ricevente di
    /// piattaforma (<c>java.*</c>) non fa archi, uno di una libreria va al suo nodo external <c>Tipo.metodo</c>, mai al
    /// metodo omonimo del progetto. Una chiamata su un'interfaccia va al metodo dell'interfaccia e, <c>inferred</c>, a
    /// quello delle classi del perimetro che la implementano. <c>new T()</c> è <c>instantiates</c>. Gli stereotipi Spring danno un kind proprio come i decoratori Angular:
    /// <c>@Service</c>/<c>@Repository</c> → service, <c>@Component</c>/<c>@Controller</c>/<c>@RestController</c> → component.
    /// </summary>
    public sealed class TreeSitterJavaExtractor : ICodeExtractor
    {
        public string LanguageId => CodeLanguages.Java;
        public string ExtractorName => "tree-sitter-java";
        public string ExtractorVersion => "3";   // 3: risoluzione delle chiamate per tipo e gerarchia (D38)
        /// <summary>
        /// Oltre questo numero di omonimi un nome non si risolve più «a tutti i candidati»: sarebbero archi a caso (in
        /// PlantUML <c>add</c> ha decine di omonimi). Si emette un solo arco <c>ambiguous</c> verso un nodo external
        /// <c>name</c>, che dice «qui c'è una chiamata che la sola sintassi non sa attribuire».
        /// </summary>
        public const int MaxAmbiguousCandidates = 5;


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
                catch (Exception ex) { result.Warnings.Add($"{f.LogicalPath}: non leggibile ({ex.Message})"); continue; }
                var tree = parser.Parse(text);
                if (tree == null) { result.Warnings.Add($"{f.LogicalPath}: tree-sitter non ha prodotto un albero"); continue; }
                parsed.Add(new ParsedFile(f, text, tree, PackageOf(tree.RootNode)));
            }
            var index = new PerimeterIndex();
            foreach (var pf in parsed) index.Collect(pf);
            index.Link();

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

        /// <summary>
        /// Che cosa sa il file dei nomi di tipo: i suoi tipi, il package, gli import (singoli, <c>.*</c>, statici). È il
        /// contesto in cui si risolve un nome scritto in quel file, anche quando lo si risolve da un altro (i campi e le
        /// madri di una classe si leggono con il contesto del file che la dichiara).
        /// </summary>
        private sealed class FileContext
        {
            public string Package = string.Empty;
            public readonly Dictionary<string, string> FileTypes = new(StringComparer.Ordinal);       // simple → fqn
            public readonly Dictionary<string, string> Imports = new(StringComparer.Ordinal);         // simple → fqn (import a.b.C)
            public readonly List<string> WildcardPackages = new();                                      // import a.b.*
            public readonly Dictionary<string, string> StaticImports = new(StringComparer.Ordinal);   // membro → fqn del tipo (import static a.b.C.m)
            public readonly List<string> StaticWildcards = new();                                       // import static a.b.C.*
            public readonly List<(string Fqn, int Line)> ImportEdges = new();

            public static FileContext Of(Node root, string package)
            {
                var ctx = new FileContext { Package = package };
                foreach (var imp in root.NamedChildren.Where(c => c.Type == "import_declaration"))
                {
                    var scoped = imp.NamedChildren.FirstOrDefault(c => c.Type is "scoped_identifier" or "identifier")?.Text;
                    if (scoped == null) continue;
                    var wildcard = imp.NamedChildren.Any(c => c.Type == "asterisk");
                    var isStatic = imp.Text.StartsWith("import static", StringComparison.Ordinal);
                    if (isStatic)
                    {
                        // import static a.b.C.m → m da C; import static a.b.C.* → tutti i membri statici di C
                        if (wildcard) { ctx.StaticWildcards.Add(scoped); ctx.ImportEdges.Add((scoped, Line(imp))); continue; }
                        var dot = scoped.LastIndexOf('.');
                        if (dot <= 0) continue;
                        ctx.StaticImports[scoped.Substring(dot + 1)] = scoped.Substring(0, dot);
                        ctx.ImportEdges.Add((scoped.Substring(0, dot), Line(imp)));
                        continue;
                    }
                    if (wildcard) { ctx.WildcardPackages.Add(scoped); continue; }
                    ctx.Imports[scoped.Split('.').Last()] = scoped;
                    ctx.ImportEdges.Add((scoped, Line(imp)));
                }
                foreach (var decl in root.NamedChildren.Where(c => TypeDeclarations.Contains(c.Type))) ctx.CollectFileTypes(decl, null);
                return ctx;
            }

            private void CollectFileTypes(Node decl, string outerFqn)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var fqn = outerFqn != null ? outerFqn + "." + name : (Package.Length == 0 ? name : Package + "." + name);
                FileTypes[name] = fqn;
                var body = BodyOf(decl);
                if (body == null) return;
                foreach (var m in body.NamedChildren.Where(c => TypeDeclarations.Contains(c.Type))) CollectFileTypes(m, fqn);
            }
        }

        private enum TypeRefKind { Project, External, Platform, Unknown }

        /// <summary>
        /// Un nome di tipo risolto: tipi del perimetro (più d'uno se omonimi), un external con il suo nome qualificato, la
        /// piattaforma (<c>java.*</c>: nessun nodo), o sconosciuto. La confidenza dice quanto lo dice il codice.
        /// </summary>
        private sealed record TypeRef(TypeRefKind Kind, IReadOnlyList<string> Fqns, string Confidence)
        {
            public static readonly TypeRef Unknown = new(TypeRefKind.Unknown, Array.Empty<string>(), "ambiguous");
            public static readonly TypeRef Platform = new(TypeRefKind.Platform, Array.Empty<string>(), "extracted");
        }

        private sealed record MethodEntry(string Id, int Arity, bool Varargs);

        private sealed class TypeInfo
        {
            public string Fqn, Id;
            public bool IsInterface;
            public FileContext Ctx;
            public readonly List<string> SuperNames = new();                                          // extends + implements, nome di testa
            public readonly Dictionary<string, string> FieldTypeNames = new(StringComparer.Ordinal);  // campo → nome di tipo di testa
            public List<TypeRef> Supers = new();
        }

        private sealed class PerimeterIndex
        {
            public readonly Dictionary<string, string> TypesByFqn = new(StringComparer.Ordinal);                         // fqn → id
            public readonly Dictionary<string, List<(string Id, string Fqn)>> TypesByName = new(StringComparer.Ordinal); // simple name → candidati
            public readonly Dictionary<string, List<string>> MethodsByName = new(StringComparer.Ordinal);                // nome → ids (tutti i tipi)
            public readonly Dictionary<string, Dictionary<string, List<MethodEntry>>> MethodsByOwner = new(StringComparer.Ordinal); // fqn tipo → nome → metodi
            public readonly Dictionary<string, List<MethodEntry>> CtorsByOwner = new(StringComparer.Ordinal);           // fqn tipo → costruttori
            public readonly Dictionary<string, TypeInfo> Types = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<string>> Implementors = new(StringComparer.Ordinal);                // fqn → fqn che lo estendono/implementano
            private readonly Dictionary<ParsedFile, FileContext> _contexts = new();

            public FileContext ContextOf(ParsedFile pf) => _contexts[pf];

            public void Collect(ParsedFile pf)
            {
                var ctx = FileContext.Of(pf.Tree.RootNode, pf.Package);
                _contexts[pf] = ctx;
                foreach (var node in pf.Tree.RootNode.NamedChildren)
                    if (TypeDeclarations.Contains(node.Type)) CollectType(pf, ctx, node, null);
            }

            private void CollectType(ParsedFile pf, FileContext ctx, Node decl, string outerFqn)
            {
                var name = NameOf(decl);
                if (name == null) return;
                var fqn = outerFqn != null ? outerFqn + "." + name : (pf.Package.Length == 0 ? name : pf.Package + "." + name);
                var kind = KindOf(decl);
                var id = StableId(kind, fqn);
                TypesByFqn[fqn] = id;
                if (!TypesByName.TryGetValue(name, out var list)) TypesByName[name] = list = new();
                list.Add((id, fqn));
                var info = new TypeInfo { Fqn = fqn, Id = id, IsInterface = kind == "interface", Ctx = ctx };
                Types[fqn] = info;
                foreach (var t in TypeNames(decl.NamedChildren.FirstOrDefault(c => c.Type == "superclass")).Take(1)) info.SuperNames.Add(t);
                foreach (var clause in decl.NamedChildren.Where(c => c.Type is "super_interfaces" or "extends_interfaces"))
                {
                    var tl = clause.NamedChildren.FirstOrDefault(c => c.Type == "type_list") ?? clause;
                    foreach (var t in tl.NamedChildren.Where(IsTypeNode))
                        if (TypeNames(t).FirstOrDefault() is string head) info.SuperNames.Add(head);
                }
                if (decl.Type == "record_declaration")
                    foreach (var p in decl.NamedChildren.FirstOrDefault(c => c.Type == "formal_parameters")?.NamedChildren.Where(c => c.Type == "formal_parameter") ?? Enumerable.Empty<Node>())
                        if (NameOf(p) is string pn && TypeNames(p.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault() is string pt) info.FieldTypeNames[pn] = pt;
                var body = BodyOf(decl);
                if (body == null) return;
                foreach (var m in body.NamedChildren.SelectMany(c => c.Type == "enum_body_declarations" ? c.NamedChildren : new[] { c }))
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
                        var (arity, varargs) = Arity(m);
                        ol.Add(new MethodEntry(mid, arity, varargs));
                    }
                    else if (m.Type == "constructor_declaration" && NameOf(m) is string cname)
                    {
                        if (!CtorsByOwner.TryGetValue(fqn, out var cl)) CtorsByOwner[fqn] = cl = new();
                        var (arity, varargs) = Arity(m);
                        cl.Add(new MethodEntry(StableId("constructor", fqn + "." + cname + ParameterTypeList(m)), arity, varargs));
                    }
                    else if (m.Type is "field_declaration" or "constant_declaration")
                    {
                        var head = TypeNames(m.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault();
                        if (head == null) continue;
                        foreach (var v in m.NamedChildren.Where(c => c.Type == "variable_declarator"))
                            if (NameOf(v) is string fname) info.FieldTypeNames[fname] = head;
                    }
                    else if (TypeDeclarations.Contains(m.Type)) CollectType(pf, ctx, m, fqn);
                }
            }

            /// <summary>Dopo la raccolta: le madri di ogni tipo risolte nel contesto del suo file, e chi le estende.</summary>
            public void Link()
            {
                foreach (var info in Types.Values)
                {
                    info.Supers = info.SuperNames.Select(n => Resolve(info.Ctx, n)).ToList();
                    foreach (var sup in info.Supers.Where(r => r.Kind == TypeRefKind.Project))
                        foreach (var f in sup.Fqns)
                        {
                            if (!Implementors.TryGetValue(f, out var l)) Implementors[f] = l = new();
                            l.Add(info.Fqn);
                        }
                }
            }

            /// <summary>
            /// Un nome di tipo semplice scritto in un file, nell'ordine in cui lo risolve Java: tipi del file, import singolo
            /// (anche esterno: vince su un omonimo del perimetro), stesso package, <c>java.lang</c>, import <c>.*</c>; poi,
            /// per un perimetro senza import completi, l'omonimo unico (<c>inferred</c>) o tutti gli omonimi
            /// (<c>ambiguous</c>, mai uno a caso); sconosciuto = external nel package più probabile, senza certezza.
            /// </summary>
            public TypeRef Resolve(FileContext ctx, string simple)
            {
                if (string.IsNullOrEmpty(simple)) return TypeRef.Unknown;
                if (ctx.FileTypes.TryGetValue(simple, out var f)) return Project(f, "extracted");
                if (ctx.Imports.TryGetValue(simple, out var imported))
                {
                    if (TypesByFqn.ContainsKey(imported)) return Project(imported, "extracted");
                    return IsPlatform(imported) ? TypeRef.Platform : new TypeRef(TypeRefKind.External, new[] { imported }, "extracted");
                }
                var samePackage = ctx.Package.Length == 0 ? simple : ctx.Package + "." + simple;
                if (TypesByFqn.ContainsKey(samePackage)) return Project(samePackage, "extracted");
                if (JavaLang.Contains(simple)) return TypeRef.Platform;
                var fromWildcards = ctx.WildcardPackages.Select(p => p + "." + simple).Where(TypesByFqn.ContainsKey).ToList();
                if (fromWildcards.Count > 0) return new TypeRef(TypeRefKind.Project, fromWildcards, fromWildcards.Count == 1 ? "extracted" : "ambiguous");
                if (TypesByName.TryGetValue(simple, out var list))
                    return new TypeRef(TypeRefKind.Project, list.Select(x => x.Fqn).ToList(), list.Count == 1 ? "inferred" : "ambiguous");
                var foreign = ctx.WildcardPackages.Where(p => !IsPlatform(p)).ToList();
                if (foreign.Count == 0 && ctx.WildcardPackages.Count > 0) return TypeRef.Platform;   // import java.util.*: è quasi certo un tipo della piattaforma
                if (foreign.Count == 1) return new TypeRef(TypeRefKind.External, new[] { foreign[0] + "." + simple }, "inferred");
                if (foreign.Count > 1) return new TypeRef(TypeRefKind.External, new[] { "?." + simple }, "ambiguous");
                // né import né perimetro: lo stesso package, fuori dal perimetro (o un parametro di tipo): non è certo
                return new TypeRef(TypeRefKind.External, new[] { samePackage }, "inferred");
            }

            private static TypeRef Project(string fqn, string confidence) => new(TypeRefKind.Project, new[] { fqn }, confidence);

            /// <summary>
            /// I metodi <paramref name="name"/> visibili da un tipo: il tipo stesso, poi le madri e le interfacce del
            /// perimetro (il più vicino vince), filtrati per numero di argomenti quando questo ne lascia almeno uno.
            /// <paramref name="externalAncestor"/> è la prima madre fuori dal perimetro, se non lo si trova nel perimetro.
            /// </summary>
            public List<MethodEntry> FindMethod(string fqn, string name, int argc, out string externalAncestor, bool skipSelf = false)
            {
                externalAncestor = null;
                var visited = new HashSet<string>(StringComparer.Ordinal);
                var queue = new Queue<string>();
                if (skipSelf) { visited.Add(fqn); foreach (var s in SupersOf(fqn, ref externalAncestor)) queue.Enqueue(s); }
                else queue.Enqueue(fqn);
                while (queue.Count > 0)
                {
                    var t = queue.Dequeue();
                    if (!visited.Add(t)) continue;
                    if (MethodsByOwner.TryGetValue(t, out var byName) && byName.TryGetValue(name, out var ms) && ms.Count > 0)
                        return ByArity(ms, argc);
                    foreach (var s in SupersOf(t, ref externalAncestor)) queue.Enqueue(s);
                }
                return null;
            }

            private IEnumerable<string> SupersOf(string fqn, ref string externalAncestor)
            {
                if (!Types.TryGetValue(fqn, out var info)) return Enumerable.Empty<string>();
                var ext = info.Supers.FirstOrDefault(r => r.Kind == TypeRefKind.External);
                if (externalAncestor == null && ext != null) externalAncestor = ext.Fqns[0];
                return info.Supers.Where(r => r.Kind == TypeRefKind.Project).SelectMany(r => r.Fqns).ToList();
            }

            public static List<MethodEntry> ByArity(List<MethodEntry> ms, int argc)
            {
                if (argc < 0) return ms;
                var fit = ms.Where(m => m.Arity == argc || (m.Varargs && argc >= m.Arity - 1)).ToList();
                return fit.Count > 0 ? fit : ms;
            }

            /// <summary>Il tipo di un campo visto da un tipo: il suo, poi quelli delle madri del perimetro.</summary>
            public TypeRef FieldType(string fqn, string field)
            {
                var visited = new HashSet<string>(StringComparer.Ordinal);
                var queue = new Queue<string>(new[] { fqn });
                while (queue.Count > 0)
                {
                    var t = queue.Dequeue();
                    if (!visited.Add(t) || !Types.TryGetValue(t, out var info)) continue;
                    if (info.FieldTypeNames.TryGetValue(field, out var head)) return Resolve(info.Ctx, head);
                    foreach (var s in info.Supers.Where(r => r.Kind == TypeRefKind.Project).SelectMany(r => r.Fqns)) queue.Enqueue(s);
                }
                return null;
            }
        }

        private static bool IsPlatform(string fqn) => fqn.StartsWith("java.", StringComparison.Ordinal) || fqn.StartsWith("javax.", StringComparison.Ordinal) || fqn.StartsWith("jakarta.", StringComparison.Ordinal);

        /// <summary>Quanti parametri e se l'ultimo è varargs: per scegliere fra gli overload con il numero di argomenti.</summary>
        private static (int Arity, bool Varargs) Arity(Node decl)
        {
            var prms = decl.NamedChildren.FirstOrDefault(c => c.Type == "formal_parameters");
            if (prms == null) return (0, false);
            var list = prms.NamedChildren.Where(p => p.Type is "formal_parameter" or "spread_parameter").ToList();
            return (list.Count, list.Any(p => p.Type == "spread_parameter"));
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
            private readonly Dictionary<string, ScriptEdge> _edges = new(StringComparer.Ordinal);
            private readonly FileContext _ctx;

            public FileEmitter(TreeSitterJavaExtractor owner, ParsedFile pf, PerimeterIndex index)
            {
                _owner = owner; _pf = pf; _index = index; _ctx = index.ContextOf(pf);
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
                foreach (var decl in root.NamedChildren.Where(c => TypeDeclarations.Contains(c.Type))) EmitType(decl, null, null);
                return _script;
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
                    foreach (var (ifqn, line) in _ctx.ImportEdges)
                    {
                        var target = ResolveImported(ifqn);
                        if (target != null) Edge(typeId, target, "imports", "extracted", line);
                    }
                }

                // extends / implements
                var superclass = decl.NamedChildren.FirstOrDefault(c => c.Type == "superclass");
                foreach (var t in TypeNames(superclass).Take(1)) TypeEdges(typeId, t, "inherits", Line(superclass));
                foreach (var clause in decl.NamedChildren.Where(c => c.Type is "super_interfaces" or "extends_interfaces"))
                {
                    var relation = clause.Type == "super_interfaces" ? "implements" : "inherits";
                    var list = clause.NamedChildren.FirstOrDefault(c => c.Type == "type_list") ?? clause;
                    foreach (var t in list.NamedChildren.Where(IsTypeNode))
                    {
                        var head = TypeNames(t).FirstOrDefault();
                        if (head != null) TypeEdges(typeId, head, relation, Line(t));
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
                    case "static_initializer": case "block": WalkBody(m, typeId, fqn, Locals(m)); break;
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
                    WalkBody(v, fid, fqn, Locals(v));   // inizializzatore: new X(), chiamate
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
                if (body != null) WalkBody(body, id, fqn, Locals(m));
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

            /// <summary>
            /// Le variabili di un metodo (o di un inizializzatore) con il nome del loro tipo: parametri, variabili locali
            /// (<c>var x = new T()</c> compreso), for-each, catch, parametri tipizzati delle lambda. Una mappa piatta per
            /// metodo (Java non lascia ridichiarare una locale nello stesso metodo); un nome dichiarato con due tipi
            /// diversi (in due lambda, in due blocchi) non si sa: resta senza tipo.
            /// </summary>
            private static Dictionary<string, string> Locals(Node scope)
            {
                var locals = new Dictionary<string, string>(StringComparer.Ordinal);
                var clash = new HashSet<string>(StringComparer.Ordinal);
                void Add(string name, string type)
                {
                    if (name == null || type == null || clash.Contains(name)) return;
                    if (locals.TryGetValue(name, out var known) && known != type) { locals.Remove(name); clash.Add(name); return; }
                    locals[name] = type;
                }
                foreach (var n in Walk(scope))
                {
                    switch (n.Type)
                    {
                        case "formal_parameter":
                            Add(NameOf(n), TypeNames(n.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault());
                            break;
                        case "spread_parameter":
                            Add(n.NamedChildren.FirstOrDefault(c => c.Type == "variable_declarator")?.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text,
                                TypeNames(n.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault());
                            break;
                        case "local_variable_declaration":
                        {
                            var declared = TypeNames(n.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault();
                            foreach (var v in n.NamedChildren.Where(c => c.Type == "variable_declarator"))
                            {
                                var type = declared;
                                if (type == "var")   // var x = new T(): il tipo è quello creato; altrimenti non si sa
                                    type = v.NamedChildren.FirstOrDefault(c => c.Type == "object_creation_expression") is Node created
                                        ? TypeNames(created.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault() : null;
                                Add(NameOf(v), type);
                            }
                            break;
                        }
                        case "enhanced_for_statement":
                            Add(n.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text, TypeNames(n.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault());
                            break;
                        case "catch_formal_parameter":
                            Add(n.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text,
                                TypeNames(n.NamedChildren.FirstOrDefault(c => c.Type == "catch_type")).FirstOrDefault());
                            break;
                    }
                }
                return locals;
            }

            private void WalkBody(Node body, string ownerId, string ownerFqn, Dictionary<string, string> locals)
            {
                foreach (var n in Walk(body))
                {
                    if (n.Type == "method_invocation")
                    {
                        var nameNode = n.NamedChildren.LastOrDefault(c => c.Type == "identifier");
                        if (nameNode == null) continue;
                        var obj = n.NamedChildren.FirstOrDefault();
                        var argc = n.NamedChildren.FirstOrDefault(c => c.Type == "argument_list")?.NamedChildren.Count ?? -1;
                        ResolveCall(ownerId, ownerFqn, obj == nameNode ? null : obj, nameNode.Text, argc, locals, Line(n));
                    }
                    else if (n.Type == "explicit_constructor_invocation")
                    {
                        // this(...) / super(...): il costruttore della classe o della madre del perimetro
                        var who = n.NamedChildren.FirstOrDefault();
                        var argc = n.NamedChildren.FirstOrDefault(c => c.Type == "argument_list")?.NamedChildren.Count ?? -1;
                        if (ownerFqn == null || who == null) continue;
                        var targets = who.Type == "this" ? new[] { ownerFqn }
                            : _index.Types.TryGetValue(ownerFqn, out var info) ? info.Supers.Where(r => r.Kind == TypeRefKind.Project && !_index.Types[r.Fqns[0]].IsInterface).SelectMany(r => r.Fqns).Take(1).ToArray() : Array.Empty<string>();
                        foreach (var t in targets)
                            if (_index.CtorsByOwner.TryGetValue(t, out var ctors))
                            {
                                var fit = PerimeterIndex.ByArity(ctors, argc);
                                foreach (var c in fit) Edge(ownerId, c.Id, "calls", fit.Count == 1 ? "extracted" : "inferred", Line(n));
                            }
                    }
                    else if (n.Type == "object_creation_expression")
                    {
                        var head = TypeNames(n.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault();
                        if (head != null) TypeEdges(ownerId, head, "instantiates", Line(n));
                    }
                }
            }

            /// <summary>
            /// Una chiamata: prima il ricevente scritto nel codice (tipo di variabile, campo, <c>new T()</c>, classe per le
            /// statiche, <c>super</c>), poi — per <c>m()</c> senza ricevente — la gerarchia della classe, le classi che la
            /// contengono e gli import statici; il nome su tutto il perimetro solo quando il codice non dice di più.
            /// </summary>
            private void ResolveCall(string ownerId, string ownerFqn, Node obj, string name, int argc, Dictionary<string, string> locals, int line)
            {
                if (obj == null || obj.Type == "this")
                {
                    // la classe e le sue madri, poi le classi che la contengono (annidate, anonime)
                    string inheritedFrom = null;
                    for (var scope = ownerFqn; scope != null; scope = OuterOf(scope))
                    {
                        var own = _index.FindMethod(scope, name, argc, out var extAncestor);
                        if (own != null) { MethodEdges(ownerId, own, "extracted", line); return; }
                        inheritedFrom ??= extAncestor;
                        if (obj != null) break;   // this.m(): solo la classe, non quelle che la contengono
                    }
                    if (obj == null)
                    {
                        if (_ctx.StaticImports.TryGetValue(name, out var holder)) { CallOnType(ownerId, Holder(holder), name, argc, line); return; }
                        foreach (var w in _ctx.StaticWildcards)
                            if (_index.TypesByFqn.ContainsKey(w) && _index.FindMethod(w, name, argc, out _) is List<MethodEntry> st) { MethodEdges(ownerId, st, "extracted", line); return; }
                    }
                    // non dichiarato nel perimetro, ma la classe estende una libreria: è suo, ereditato
                    if (inheritedFrom != null) { Edge(ownerId, External("method", inheritedFrom + "." + name, name), "calls", "inferred", line); return; }
                    CallByName(ownerId, name, line);
                    return;
                }
                if (obj.Type == "super")
                {
                    if (ownerFqn != null && _index.FindMethod(ownerFqn, name, argc, out var ext, skipSelf: true) is List<MethodEntry> sup) { MethodEdges(ownerId, sup, "extracted", line); return; }
                    if (ownerFqn != null && _index.Types.TryGetValue(ownerFqn, out var oi) && oi.Supers.FirstOrDefault(r => r.Kind == TypeRefKind.External) is TypeRef e)
                    { Edge(ownerId, External("method", e.Fqns[0] + "." + name, name), "calls", "inferred", line); return; }
                    if (ownerFqn != null && _index.Types.TryGetValue(ownerFqn, out var oi2) && oi2.Supers.Count == 0) return;   // super di Object: piattaforma
                    CallByName(ownerId, name, line);
                    return;
                }
                var receiver = ReceiverType(obj, ownerFqn, locals);
                if (receiver == null) { CallByName(ownerId, name, line); return; }
                CallOnType(ownerId, receiver, name, argc, line);
            }

            private TypeRef Holder(string fqn)
                => _index.TypesByFqn.ContainsKey(fqn) ? new TypeRef(TypeRefKind.Project, new[] { fqn }, "extracted")
                 : IsPlatform(fqn) ? TypeRef.Platform : new TypeRef(TypeRefKind.External, new[] { fqn }, "extracted");

            /// <summary>Il tipo del ricevente quando il codice lo scrive; null quando non lo dice (catene di chiamate, lambda senza tipo…).</summary>
            private TypeRef ReceiverType(Node obj, string ownerFqn, Dictionary<string, string> locals)
            {
                switch (obj.Type)
                {
                    case "identifier":
                        if (locals.TryGetValue(obj.Text, out var local)) return _index.Resolve(_ctx, local);
                        for (var scope = ownerFqn; scope != null; scope = OuterOf(scope))
                            if (_index.FieldType(scope, obj.Text) is TypeRef field) return field;
                        // non è una variabile: un nome di tipo (chiamata statica), se lo è
                        if (char.IsUpper(obj.Text[0]))
                        {
                            var asType = _index.Resolve(_ctx, obj.Text);
                            return asType.Kind == TypeRefKind.External && asType.Confidence != "extracted" ? null : asType;
                        }
                        return null;
                    case "field_access":
                    {
                        var target = obj.NamedChildren.FirstOrDefault();
                        var field = obj.NamedChildren.LastOrDefault(c => c.Type == "identifier")?.Text;
                        if (target?.Type == "this" && field != null)
                            for (var scope = ownerFqn; scope != null; scope = OuterOf(scope))
                                if (_index.FieldType(scope, field) is TypeRef f) return f;
                        return null;
                    }
                    case "object_creation_expression":
                    {
                        var head = TypeNames(obj.NamedChildren.FirstOrDefault(IsTypeNode)).FirstOrDefault();
                        return head == null ? null : _index.Resolve(_ctx, head);
                    }
                    case "string_literal": return TypeRef.Platform;
                    default: return null;
                }
            }

            /// <summary>Una chiamata su un tipo noto: i suoi metodi (e delle madri), external per le librerie, niente per la piattaforma.</summary>
            private void CallOnType(string ownerId, TypeRef receiver, string name, int argc, int line)
            {
                switch (receiver.Kind)
                {
                    case TypeRefKind.Platform: return;
                    case TypeRefKind.Unknown: CallByName(ownerId, name, line); return;
                    case TypeRefKind.External:
                        Edge(ownerId, External("method", receiver.Fqns[0] + "." + name, name), "calls", receiver.Confidence, line);
                        return;
                }
                var found = false;
                foreach (var fqn in receiver.Fqns)
                {
                    var ms = _index.FindMethod(fqn, name, argc, out var extAncestor);
                    if (ms != null)
                    {
                        found = true;
                        MethodEdges(ownerId, ms, Weaker(receiver.Confidence, ms.Count == 1 ? "extracted" : "inferred"), line);
                        // su un'interfaccia (o una classe astratta) chi la implementa nel perimetro: inferred, ma solo se sono
                        // poche. Oltre MaxAmbiguousCandidates resta l'arco al contratto: in PlantUML drawU() ha centinaia di
                        // implementazioni e faceva 53.000 archi (misurato il 13/10), rumore per chi legge e per le descrizioni.
                        var impls = ImplementorsOf(fqn)
                            .SelectMany(impl => _index.MethodsByOwner.TryGetValue(impl, out var byName) && byName.TryGetValue(name, out var im) ? PerimeterIndex.ByArity(im, argc) : Enumerable.Empty<MethodEntry>())
                            .Take(MaxAmbiguousCandidates + 1).ToList();
                        if (impls.Count <= MaxAmbiguousCandidates)
                            foreach (var im in impls) Edge(ownerId, im.Id, "calls", "inferred", line);
                    }
                    else if (extAncestor != null)
                    {
                        found = true;
                        Edge(ownerId, External("method", extAncestor + "." + name, name), "calls", "inferred", line);
                    }
                }
                // un tipo del perimetro senza quel metodo né madri esterne: è di Object (toString, equals…) → piattaforma
                if (!found && !ObjectMethods.Contains(name)) CallByName(ownerId, name, line);
            }

            private static readonly HashSet<string> ObjectMethods = new(StringComparer.Ordinal)
                { "toString", "equals", "hashCode", "getClass", "notify", "notifyAll", "wait", "clone", "finalize", "name", "ordinal", "values", "valueOf", "compareTo" };

            private IEnumerable<string> ImplementorsOf(string fqn)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                var queue = new Queue<string>(new[] { fqn });
                while (queue.Count > 0)
                {
                    var t = queue.Dequeue();
                    if (!_index.Implementors.TryGetValue(t, out var subs)) continue;
                    foreach (var sub in subs) if (seen.Add(sub)) { yield return sub; queue.Enqueue(sub); }
                }
            }

            private void MethodEdges(string ownerId, List<MethodEntry> ms, string confidence, int line)
            {
                var conf = ms.Count == 1 ? confidence : Weaker(confidence, "inferred");
                foreach (var m in ms) Edge(ownerId, m.Id, "calls", conf, line);
            }

            private static string Weaker(string a, string b) => ConfidenceRank(a) <= ConfidenceRank(b) ? a : b;

            /// <summary>La classe che contiene <paramref name="fqn"/>, se è un tipo del perimetro (annidate); null altrimenti.</summary>
            private string OuterOf(string fqn)
            {
                var dot = fqn.LastIndexOf('.');
                if (dot <= 0) return null;
                var outer = fqn.Substring(0, dot);
                return _index.Types.ContainsKey(outer) ? outer : null;
            }

            /// <summary>L'ultima risorsa: il nome su tutto il perimetro. Mai il testo della chiamata come nome del nodo.</summary>
            private void CallByName(string ownerId, string name, int line)
            {
                if (_index.MethodsByName.TryGetValue(name, out var ms) && ms.Count <= MaxAmbiguousCandidates)
                {
                    var conf = ms.Count == 1 ? "inferred" : "ambiguous";
                    foreach (var id in ms) Edge(ownerId, id, "calls", conf, line);
                }
                else Edge(ownerId, External("method", "?." + name, name), "calls", "ambiguous", line);
            }

            // ---- resolution

            /// <summary>Gli archi verso un nome di tipo: uno per candidato del perimetro, l'external, o niente (piattaforma).</summary>
            private void TypeEdges(string sourceId, string simple, string relation, int line)
            {
                var r = _index.Resolve(_ctx, simple);
                switch (r.Kind)
                {
                    case TypeRefKind.Project:
                        foreach (var fqn in r.Fqns) Edge(sourceId, _index.TypesByFqn[fqn], relation, r.Confidence, line);
                        break;
                    case TypeRefKind.External:
                        Edge(sourceId, External("type", r.Fqns[0], simple), relation, r.Confidence, line);
                        break;
                }
            }

            private string ResolveImported(string fqn)
            {
                if (_index.TypesByFqn.TryGetValue(fqn, out var id)) return id;
                return IsPlatform(fqn) ? null : External("type", fqn, fqn.Split('.').Last());
            }

            private void Reference(string sourceId, string simpleType, int line) => TypeEdges(sourceId, simpleType, "references", line);

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
