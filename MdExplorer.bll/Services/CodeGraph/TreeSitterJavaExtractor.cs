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
            "Runtime", "Process", "ProcessBuilder", "ClassLoader", "StrictMath", "ThreadLocal", "InheritableThreadLocal", "ThreadGroup", "Appendable",
            "Readable", "Package", "Module", "ModuleLayer", "StackTraceElement", "StackWalker", "SecurityManager", "ScopedValue",
            "InterruptedException", "CloneNotSupportedException", "ArithmeticException", "ArrayIndexOutOfBoundsException", "IndexOutOfBoundsException",
            "StringIndexOutOfBoundsException", "ClassCastException", "NumberFormatException", "SecurityException", "ClassNotFoundException",
            "NoSuchFieldException", "NoSuchMethodException", "IllegalAccessException", "InstantiationException", "ReflectiveOperationException",
            "NegativeArraySizeException", "ArrayStoreException", "IllegalMonitorStateException", "EnumConstantNotPresentException", "TypeNotPresentException",
            "StackOverflowError", "OutOfMemoryError", "AssertionError", "LinkageError", "VirtualMachineError", "InternalError", "NoClassDefFoundError",
            "ExceptionInInitializerError", "UnsatisfiedLinkError", "IncompatibleClassChangeError",
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
        /// <para><see cref="Args"/> sono gli argomenti di tipo come scritti (<c>List&lt;Foo&gt;</c> → <c>Foo</c>), da risolvere
        /// in <see cref="ArgsCtx"/>: servono a dire che cosa restituiscono <c>get</c>, <c>next</c>… <see cref="IsArray"/>:
        /// un array (i suoi elementi sono del tipo). Per la piattaforma <see cref="Fqns"/> porta il nome semplice.</para>
        /// </summary>
        private sealed record TypeRef(TypeRefKind Kind, IReadOnlyList<string> Fqns, string Confidence,
            IReadOnlyList<string> Args = null, FileContext ArgsCtx = null, bool IsArray = false)
        {
            public static readonly TypeRef Unknown = new(TypeRefKind.Unknown, Array.Empty<string>(), "ambiguous");
            public static readonly TypeRef Platform = new(TypeRefKind.Platform, Array.Empty<string>(), "extracted");
            /// <summary>Un valore restituito da una libreria: il tipo non si sa, ma non è del perimetro.</summary>
            public static readonly TypeRef LibraryValue = new(TypeRefKind.External, new[] { "?" }, "inferred");
            public string PlatformName => Kind == TypeRefKind.Platform && Fqns.Count > 0 ? Fqns[0] : null;
        }

        /// <summary>Un metodo o costruttore del perimetro: arità per gli overload, tipo di ritorno come scritto e dove leggerlo.</summary>
        private sealed record MethodEntry(string Id, int Arity, bool Varargs, string ReturnText = null, string OwnerFqn = null, IReadOnlyCollection<string> TypeParams = null);

        private sealed class TypeInfo
        {
            public string Fqn, Id;
            public bool IsInterface;
            public FileContext Ctx;
            public readonly List<string> SuperTexts = new();                                          // extends + implements, come scritti (Base<Foo>)
            public readonly Dictionary<string, string> FieldTypes = new(StringComparer.Ordinal);      // campo → tipo come scritto (List<Foo>)
            public List<TypeRef> Supers = new();
            /// <summary>I parametri di tipo della classe in ordine (<c>class Box&lt;T&gt;</c> → T), anche quelli delle classi che la contengono.</summary>
            public List<string> TypeParams = new();
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

            /// <summary>La «madre esterna» quando è della piattaforma: niente nodo, niente arco.</summary>
            public const string PlatformAncestor = "<piattaforma>";

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
                info.TypeParams = TypeParamsOf(decl).ToList();
                if (outerFqn != null && Types.TryGetValue(outerFqn, out var outerInfo)) info.TypeParams.AddRange(outerInfo.TypeParams.Where(t => !info.TypeParams.Contains(t)));
                Types[fqn] = info;
                if (decl.NamedChildren.FirstOrDefault(c => c.Type == "superclass")?.NamedChildren.FirstOrDefault(IsTypeNode)?.Text is string superText) info.SuperTexts.Add(superText);
                foreach (var clause in decl.NamedChildren.Where(c => c.Type is "super_interfaces" or "extends_interfaces"))
                {
                    var tl = clause.NamedChildren.FirstOrDefault(c => c.Type == "type_list") ?? clause;
                    foreach (var t in tl.NamedChildren.Where(IsTypeNode)) info.SuperTexts.Add(t.Text);
                }
                if (decl.Type == "record_declaration")
                    foreach (var p in decl.NamedChildren.FirstOrDefault(c => c.Type == "formal_parameters")?.NamedChildren.Where(c => c.Type == "formal_parameter") ?? Enumerable.Empty<Node>())
                        if (NameOf(p) is string pn && p.NamedChildren.FirstOrDefault(IsTypeNode)?.Text is string pt) info.FieldTypes[pn] = pt;
                var body = BodyOf(decl);
                if (body == null) return;
                foreach (var m in body.NamedChildren.SelectMany(c => c.Type == "enum_body_declarations" ? c.NamedChildren : new[] { c }))
                {
                    if (m.Type == "enum_constant" && NameOf(m) is string constant) { info.FieldTypes[constant] = name; continue; }
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
                        ol.Add(new MethodEntry(mid, arity, varargs, ReturnTypeOf(m)?.Text, fqn, info.TypeParams.Concat(TypeParamsOf(m)).ToHashSet(StringComparer.Ordinal)));
                    }
                    else if (m.Type == "constructor_declaration" && NameOf(m) is string cname)
                    {
                        if (!CtorsByOwner.TryGetValue(fqn, out var cl)) CtorsByOwner[fqn] = cl = new();
                        var (arity, varargs) = Arity(m);
                        cl.Add(new MethodEntry(StableId("constructor", fqn + "." + cname + ParameterTypeList(m)), arity, varargs, null, fqn));
                    }
                    else if (m.Type is "field_declaration" or "constant_declaration")
                    {
                        var typeText = m.NamedChildren.FirstOrDefault(IsTypeNode)?.Text;
                        if (typeText == null) continue;
                        foreach (var v in m.NamedChildren.Where(c => c.Type == "variable_declarator"))
                            if (NameOf(v) is string fname) info.FieldTypes[fname] = typeText + (v.NamedChildren.Any(c => c.Type == "dimensions") ? "[]" : "");
                    }
                    else if (TypeDeclarations.Contains(m.Type)) CollectType(pf, ctx, m, fqn);
                }
            }

            /// <summary>Dopo la raccolta: le madri di ogni tipo risolte nel contesto del suo file, e chi le estende.</summary>
            public void Link()
            {
                foreach (var info in Types.Values)
                {
                    info.Supers = info.SuperTexts.Select(t => ResolveText(info.Ctx, t, info.TypeParams)).Where(r => r.Kind != TypeRefKind.Unknown).ToList();
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
            /// Un tipo come scritto (<c>List&lt;Foo&gt;</c>, <c>Foo[]</c>, <c>Map.Entry&lt;K, V&gt;</c>, <c>a.b.Foo</c>): la
            /// testa risolta come <see cref="Resolve"/>, gli argomenti tenuti per dopo, l'array segnato. Un parametro di tipo
            /// (<c>T</c>) non è un tipo: sconosciuto.
            /// </summary>
            public TypeRef ResolveText(FileContext ctx, string text, IEnumerable<string> typeParams = null)
            {
                if (string.IsNullOrWhiteSpace(text)) return TypeRef.Unknown;
                var t = text.Trim();
                var isArray = false;
                while (t.EndsWith("[]", StringComparison.Ordinal)) { t = t.Substring(0, t.Length - 2).TrimEnd(); isArray = true; }
                if (t.EndsWith("...", StringComparison.Ordinal)) { t = t.Substring(0, t.Length - 3).TrimEnd(); isArray = true; }
                var lt = t.IndexOf('<');
                var head = (lt < 0 ? t : t.Substring(0, lt)).Trim();
                var args = lt < 0 ? Array.Empty<string>() : SplitTypeArgs(t.Substring(lt));
                // le annotazioni sul tipo (@NonNull Foo) non contano
                head = head.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? head;
                if (typeParams != null && typeParams.Contains(head)) return TypeRef.Unknown;
                if (head is "var" or "void") return TypeRef.Unknown;
                if (head.Length > 0 && char.IsLower(head[0]) && !head.Contains('.')) return new TypeRef(TypeRefKind.Platform, new[] { head }, "extracted", IsArray: isArray);   // primitivi
                TypeRef r;
                if (head.Contains('.'))
                {
                    // Map.Entry, Outer.Inner, oppure a.b.Foo scritto per intero
                    if (TypesByFqn.ContainsKey(head)) r = Project(head, "extracted");
                    else
                    {
                        var first = head.Substring(0, head.IndexOf('.'));
                        var firstRef = Resolve(ctx, first);
                        var nested = firstRef.Kind == TypeRefKind.Project ? firstRef.Fqns.Select(f => f + head.Substring(first.Length)).Where(TypesByFqn.ContainsKey).ToList() : null;
                        r = nested is { Count: > 0 } ? new TypeRef(TypeRefKind.Project, nested, firstRef.Confidence)
                          : firstRef.Kind == TypeRefKind.Platform || IsPlatform(head) ? new TypeRef(TypeRefKind.Platform, new[] { head }, "extracted")
                          : firstRef.Kind == TypeRefKind.External ? new TypeRef(TypeRefKind.External, new[] { firstRef.Fqns[0] + head.Substring(first.Length) }, firstRef.Confidence)
                          : TypeRef.Unknown;
                    }
                }
                else
                {
                    r = Resolve(ctx, head);
                    if (r.Kind == TypeRefKind.Platform) r = new TypeRef(TypeRefKind.Platform, new[] { head }, "extracted");
                }
                return r with { Args = args, ArgsCtx = ctx, IsArray = isArray };
            }

            /// <summary><c>&lt;String, List&lt;Foo&gt;&gt;</c> → [String, List&lt;Foo&gt;]; <c>? extends Foo</c> → Foo; <c>?</c> → Object.</summary>
            private static IReadOnlyList<string> SplitTypeArgs(string angle)
            {
                var list = new List<string>();
                var depth = 0; var start = 1;
                for (var i = 0; i < angle.Length; i++)
                {
                    var c = angle[i];
                    if (c == '<') { depth++; continue; }
                    if (c == '>') { depth--; if (depth == 0) { list.Add(angle.Substring(start, i - start)); break; } continue; }
                    if (c == ',' && depth == 1) { list.Add(angle.Substring(start, i - start)); start = i + 1; }
                }
                return list.Select(a =>
                {
                    a = a.Trim();
                    if (!a.StartsWith("?", StringComparison.Ordinal)) return a;
                    var bound = a.IndexOf("extends ", StringComparison.Ordinal);   // ? super Foo, ?: si legge un Object
                    return bound >= 0 ? a.Substring(bound + "extends ".Length).Trim() : "Object";
                }).ToList();
            }

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
                // estende o implementa la piattaforma (ArrayList, Iterator, JFrame…): un metodo non trovato è suo
                if (externalAncestor == null && info.Supers.Any(r => r.Kind == TypeRefKind.Platform)) externalAncestor = PlatformAncestor;
                return info.Supers.Where(r => r.Kind == TypeRefKind.Project).SelectMany(r => r.Fqns).ToList();
            }

            public static List<MethodEntry> ByArity(List<MethodEntry> ms, int argc)
            {
                if (argc < 0) return ms;
                var fit = ms.Where(m => m.Arity == argc || (m.Varargs && argc >= m.Arity - 1)).ToList();
                return fit.Count > 0 ? fit : ms;
            }

            /// <summary>
            /// Gli argomenti di tipo con cui <paramref name="from"/> estende (anche alla lontana) <paramref name="target"/>:
            /// <c>class Tikz extends AbstractUGraphic&lt;TikzGraphics&gt;</c> → [TikzGraphics] letti nel file di Tikz. Un
            /// argomento che è a sua volta un parametro di tipo di una classe intermedia si sostituisce risalendo.
            /// </summary>
            public (IReadOnlyList<string> Args, FileContext Ctx, IReadOnlyCollection<string> TypeParams)? ArgsFor(string from, string target)
            {
                if (from == target || !Types.TryGetValue(from, out var info)) return null;
                foreach (var sup in info.Supers.Where(r => r.Kind == TypeRefKind.Project))
                    foreach (var f in sup.Fqns)
                    {
                        if (f == target) return sup.Args is { Count: > 0 } ? (sup.Args, sup.ArgsCtx, info.TypeParams) : null;
                        if (ArgsFor(f, target) is var (args, ctx, tps) && Types.TryGetValue(f, out var mid))
                        {
                            // l'argomento è un parametro della classe intermedia (class Mid<X> extends Base<X>): lo si sostituisce
                            var mapped = args.Select(a => mid.TypeParams.IndexOf(a.Trim()) is var i && i >= 0 && sup.Args != null && i < sup.Args.Count ? sup.Args[i] : a).ToList();
                            return mapped.SequenceEqual(args) ? (args, ctx, tps) : (mapped, sup.ArgsCtx, info.TypeParams);
                        }
                    }
                return null;
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
                    if (info.FieldTypes.TryGetValue(field, out var text)) return ResolveText(info.Ctx, text, info.TypeParams);
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
        /// <summary>I parametri di tipo dichiarati (<c>class Box&lt;T, U&gt;</c>, <c>&lt;R&gt; R map()</c>) in ordine.</summary>
        private static IEnumerable<string> TypeParamsOf(Node decl)
            => decl.NamedChildren.FirstOrDefault(c => c.Type == "type_parameters")?.NamedChildren
                   .Where(c => c.Type == "type_parameter")
                   .Select(c => c.NamedChildren.FirstOrDefault(x => x.Type is "type_identifier" or "identifier")?.Text)
                   .Where(n => n != null) ?? Enumerable.Empty<string>();

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
                if (body != null) WalkBody(body, id, fqn, Locals(m), m);
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
            /// Le variabili visibili in un punto, con il loro tipo come scritto: ognuna vale nel blocco in cui è dichiarata
            /// e dalla sua dichiarazione in poi. Due variabili con lo stesso nome in blocchi diversi (<c>if</c>/<c>else</c>)
            /// restano distinte: per una chiamata vale la dichiarazione più interna che la precede.
            /// </summary>
            private sealed class LocalScope
            {
                private readonly Dictionary<string, List<(int From, int To, int Decl, string Type)>> _byName = new(StringComparer.Ordinal);

                public void Add(string name, string type, Node scope, Node decl)
                {
                    if (name == null || string.IsNullOrWhiteSpace(type) || scope == null) return;
                    if (!_byName.TryGetValue(name, out var list)) _byName[name] = list = new();
                    list.Add(((int)scope.StartIndex, (int)scope.EndIndex, (int)decl.StartIndex, type));
                }

                public bool TryGet(string name, Node at, out string type)
                {
                    type = null;
                    if (!_byName.TryGetValue(name, out var list)) return false;
                    var pos = (int)at.StartIndex;
                    var best = list.Where(d => d.From <= pos && pos <= d.To && d.Decl <= pos).OrderByDescending(d => d.From).ThenByDescending(d => d.Decl).FirstOrDefault();
                    type = best.Type;
                    return type != null;
                }

                public IEnumerable<string> Names => _byName.Keys;
            }

            private static LocalScope Locals(Node scope)
            {
                var locals = new LocalScope();
                static string Dims(Node n) => n.NamedChildren.Any(c => c.Type == "dimensions") ? "[]" : "";
                static string Created(Node v) => v.NamedChildren.FirstOrDefault(c => c.Type == "object_creation_expression") is Node created ? created.NamedChildren.FirstOrDefault(IsTypeNode)?.Text : null;
                foreach (var n in Walk(scope))
                {
                    switch (n.Type)
                    {
                        case "formal_parameter":   // di un metodo o di una lambda: vale nel metodo / nella lambda
                            locals.Add(NameOf(n), n.NamedChildren.FirstOrDefault(IsTypeNode)?.Text + Dims(n), n.Parent?.Parent, n.Parent);
                            break;
                        case "spread_parameter":
                            locals.Add(n.NamedChildren.FirstOrDefault(c => c.Type == "variable_declarator")?.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text,
                                n.NamedChildren.FirstOrDefault(IsTypeNode)?.Text + "[]", n.Parent?.Parent, n.Parent);
                            break;
                        case "local_variable_declaration":
                        {
                            var declared = n.NamedChildren.FirstOrDefault(IsTypeNode)?.Text;
                            foreach (var v in n.NamedChildren.Where(c => c.Type == "variable_declarator"))
                                // var x = new T(): il tipo è quello creato; altrimenti non si sa
                                locals.Add(NameOf(v), declared == "var" ? Created(v) : declared == null ? null : declared + Dims(v), n.Parent, n);
                            break;
                        }
                        case "enhanced_for_statement":
                        {
                            var t = n.NamedChildren.FirstOrDefault(IsTypeNode)?.Text;
                            if (t != "var") locals.Add(n.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text, t, n, n);
                            break;
                        }
                        case "resource":   // try (InputStream is = …): vale nel try
                        {
                            var t = n.NamedChildren.FirstOrDefault(IsTypeNode)?.Text;
                            locals.Add(n.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text, t == "var" ? Created(n) : t, n.Parent?.Parent, n);
                            break;
                        }
                        case "catch_formal_parameter":   // catch (A | B e): il primo tipo, vale nel catch
                            locals.Add(n.NamedChildren.FirstOrDefault(c => c.Type == "identifier")?.Text,
                                TypeNames(n.NamedChildren.FirstOrDefault(c => c.Type == "catch_type")).FirstOrDefault(), n.Parent, n);
                            break;
                    }
                }
                return locals;
            }

            // I parametri di tipo visibili dal corpo che si sta leggendo (della classe e del metodo): T non è un tipo.
            private IReadOnlyCollection<string> _typeParams = Array.Empty<string>();

            private IReadOnlyCollection<string> TypeParamsFor(string ownerFqn, Node member)
                => (ownerFqn != null && _index.Types.TryGetValue(ownerFqn, out var info) ? info.TypeParams : new List<string>())
                   .Concat(member == null ? Enumerable.Empty<string>() : TypeParamsOf(member)).ToHashSet(StringComparer.Ordinal);

            private void WalkBody(Node body, string ownerId, string ownerFqn, LocalScope locals, Node member = null)
            {
                _typeParams = TypeParamsFor(ownerFqn, member);
                foreach (var n in Walk(body))
                {
                    if (n.Type == "method_invocation")
                    {
                        var nameNode = n.NamedChildren.LastOrDefault(c => c.Type == "identifier");
                        if (nameNode == null) continue;
                        var obj = n.NamedChildren.FirstOrDefault();
                        var argc = n.NamedChildren.FirstOrDefault(c => c.Type == "argument_list")?.NamedChildren.Count ?? -1;
                        ResolveCall(ownerId, ownerFqn, obj == nameNode ? null : obj, nameNode.Text, argc, locals, Line(n), n);
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
            /// Una chiamata: prima il ricevente scritto nel codice (tipo di variabile, campo, risultato di un'altra chiamata,
            /// cast, <c>new T()</c>, classe per le statiche, <c>super</c>), poi — per <c>m()</c> senza ricevente — la gerarchia
            /// della classe, le classi che la contengono e gli import statici; il nome su tutto il perimetro solo quando il
            /// codice non dice di più.
            /// </summary>
            private void ResolveCall(string ownerId, string ownerFqn, Node obj, string name, int argc, LocalScope locals, int line, Node at)
            {
                if (obj == null || obj.Type == "this")
                {
                    var implicitTargets = ImplicitTargets(ownerFqn, obj, name, argc, out var holder, out var inheritedFrom, at);
                    if (implicitTargets != null) { MethodEdges(ownerId, implicitTargets, "extracted", line); return; }
                    if (holder != null) { CallOnType(ownerId, holder, name, argc, line); return; }
                    if (inheritedFrom == PerimeterIndex.PlatformAncestor || ObjectMethods.Contains(name)) return;   // ereditato dalla piattaforma, o di Object
                    if (inheritedFrom != null) { Edge(ownerId, External("method", inheritedFrom + "." + name, name), "calls", "inferred", line); return; }
                    CallByName(ownerId, name, line);
                    return;
                }
                if (obj.Type == "super")
                {
                    string ext = null;
                    if (ownerFqn != null && _index.FindMethod(ownerFqn, name, argc, out ext, skipSelf: true) is List<MethodEntry> sup) { MethodEdges(ownerId, sup, "extracted", line); return; }
                    if (ext != null && ext != PerimeterIndex.PlatformAncestor) { Edge(ownerId, External("method", ext + "." + name, name), "calls", "inferred", line); return; }
                    return;   // super di Object (toString, equals…): piattaforma
                }
                var receiver = ReceiverType(obj, ownerFqn, locals, 0);
                if (receiver == null) { CallByName(ownerId, name, line); return; }
                CallOnType(ownerId, receiver, name, argc, line);
            }

            /// <summary>
            /// <c>m()</c> o <c>this.m()</c>: la classe e le madri, poi (solo senza <c>this</c>) le classi che la contengono e
            /// gli import statici. Restituisce i metodi trovati nel perimetro, oppure il tipo che li porta (<paramref name="holder"/>,
            /// import statico di una libreria) o la madre di libreria da cui è ereditato.
            /// </summary>
            private List<MethodEntry> ImplicitTargets(string ownerFqn, Node obj, string name, int argc, out TypeRef holder, out string inheritedFrom, Node at = null)
            {
                holder = null; inheritedFrom = null;
                // dentro una classe anonima (new TextBlock() { … }) m() è prima di tutto suo o del tipo che crea
                for (var p = at?.Parent; p != null; p = p.Parent)
                {
                    if (TypeDeclarations.Contains(p.Type) || p.Type is "method_declaration" && p.Parent?.Parent?.Type != "object_creation_expression") break;
                    if (p.Type != "class_body" || p.Parent?.Type != "object_creation_expression") continue;
                    // un metodo dichiarato nel corpo anonimo non è un simbolo del grafo: niente arco, ma nemmeno un'attribuzione per nome
                    if (p.NamedChildren.Any(c => c.Type == "method_declaration" && NameOf(c) == name)) { holder = TypeRef.Platform; return null; }
                    var created = Known(_index.ResolveText(_ctx, p.Parent.NamedChildren.FirstOrDefault(IsTypeNode)?.Text, _typeParams));
                    if (created == null) break;
                    if (created.Kind == TypeRefKind.Project)
                        foreach (var fqn in created.Fqns)
                        {
                            var found = _index.FindMethod(fqn, name, argc, out var anonExt);
                            if (found != null) return found;
                            if (anonExt != null) { inheritedFrom = anonExt; return null; }
                        }
                    else if (created.Kind == TypeRefKind.Platform) { if (obj != null) { holder = created; return null; } }
                    else { if (obj != null) { holder = created; return null; } }
                    if (obj != null) return null;
                    break;
                }
                for (var scope = ownerFqn; scope != null; scope = OuterOf(scope))
                {
                    var own = _index.FindMethod(scope, name, argc, out var extAncestor);
                    if (own != null) return own;
                    inheritedFrom ??= extAncestor;
                    if (obj != null) break;   // this.m(): solo la classe, non quelle che la contengono
                }
                if (obj != null) return null;
                if (_ctx.StaticImports.TryGetValue(name, out var h))
                {
                    var r = Holder(h);
                    if (r.Kind == TypeRefKind.Project && _index.FindMethod(r.Fqns[0], name, argc, out _) is List<MethodEntry> st) return st;
                    holder = r; return null;
                }
                foreach (var w in _ctx.StaticWildcards)
                    if (_index.TypesByFqn.ContainsKey(w) && _index.FindMethod(w, name, argc, out _) is List<MethodEntry> st) return st;
                if (inheritedFrom == null)
                {
                    // import static org.junit.Assert.*: un metodo che non è del perimetro viene da lì
                    var foreign = _ctx.StaticWildcards.Where(w => !_index.TypesByFqn.ContainsKey(w)).ToList();
                    if (foreign.Count == 1) holder = Holder(foreign[0]);
                    else if (foreign.Count > 1 && foreign.All(IsPlatform)) holder = TypeRef.Platform;
                }
                return null;
            }

            private TypeRef Holder(string fqn)
                => _index.TypesByFqn.ContainsKey(fqn) ? new TypeRef(TypeRefKind.Project, new[] { fqn }, "extracted")
                 : IsPlatform(fqn) ? new TypeRef(TypeRefKind.Platform, new[] { fqn.Split('.').Last() }, "extracted")
                 : new TypeRef(TypeRefKind.External, new[] { fqn }, "extracted");

            /// <summary>
            /// Il tipo di un'espressione usata come ricevente, quando il codice lo dice: variabile, campo (anche di un altro
            /// oggetto o di una classe), risultato di una chiamata (il tipo di ritorno), cast, elemento di array,
            /// <c>new T()</c>, letterali. Null quando non lo dice (lambda senza tipo, parametri di tipo, catene oltre <see cref="MaxChainDepth"/>).
            /// </summary>
            private TypeRef ReceiverType(Node obj, string ownerFqn, LocalScope locals, int depth)
            {
                if (obj == null || depth > MaxChainDepth) return null;
                switch (obj.Type)
                {
                    case "this":
                        return ownerFqn != null ? new TypeRef(TypeRefKind.Project, new[] { ownerFqn }, "extracted") : null;
                    case "identifier":
                    {
                        if (locals.TryGet(obj.Text, obj, out var local)) return Known(_index.ResolveText(_ctx, local, _typeParams));
                        for (var scope = ownerFqn; scope != null; scope = OuterOf(scope))
                            if (_index.FieldType(scope, obj.Text) is TypeRef field) return Known(field);
                        if (_ctx.StaticImports.TryGetValue(obj.Text, out var holderOfField))   // import static a.B.CAMPO
                        {
                            var hr = Holder(holderOfField);
                            return hr.Kind == TypeRefKind.Project ? Known(_index.FieldType(hr.Fqns[0], obj.Text)) : hr.Kind == TypeRefKind.Platform ? TypeRef.Platform : TypeRef.LibraryValue;
                        }
                        // non è una variabile: un nome di tipo (chiamata statica), se lo è
                        if (char.IsUpper(obj.Text[0]))
                        {
                            var asType = _index.Resolve(_ctx, obj.Text);
                            if (asType.Kind == TypeRefKind.Platform) return new TypeRef(TypeRefKind.Platform, new[] { obj.Text }, "extracted");
                            return asType.Kind == TypeRefKind.External && asType.Confidence != "extracted" ? null : asType;
                        }
                        return null;
                    }
                    case "field_access":
                    {
                        var target = obj.NamedChildren.FirstOrDefault();
                        var field = obj.NamedChildren.LastOrDefault(c => c.Type == "identifier")?.Text;
                        if (field == null || target == null) return null;
                        var baseType = ReceiverType(target, ownerFqn, locals, depth + 1);
                        if (baseType != null) return FieldOf(baseType, field);
                        // a.b.Foo scritto per intero (javax.imageio.ImageIO.read(…), net.x.Debug.LOG)
                        var dotted = obj.Text.Replace(" ", string.Empty);
                        if (_index.TypesByFqn.ContainsKey(dotted)) return new TypeRef(TypeRefKind.Project, new[] { dotted }, "extracted");
                        if (IsPlatform(dotted)) return new TypeRef(TypeRefKind.Platform, new[] { dotted.Split('.').Last() }, "extracted");
                        return null;
                    }
                    case "method_invocation":
                        return TypeOfInvocation(obj, ownerFqn, locals, depth + 1);
                    case "object_creation_expression":
                        return Known(_index.ResolveText(_ctx, obj.NamedChildren.FirstOrDefault(IsTypeNode)?.Text, _typeParams));
                    case "array_creation_expression":
                        return TypeRef.Platform;
                    case "cast_expression":
                        return Known(_index.ResolveText(_ctx, obj.NamedChildren.FirstOrDefault(IsTypeNode)?.Text, _typeParams));
                    case "parenthesized_expression":
                        return ReceiverType(obj.NamedChildren.FirstOrDefault(), ownerFqn, locals, depth + 1);
                    case "array_access":
                    {
                        var arr = ReceiverType(obj.NamedChildren.FirstOrDefault(), ownerFqn, locals, depth + 1);
                        return arr != null && arr.IsArray ? arr with { IsArray = false } : null;
                    }
                    case "string_literal": case "class_literal": case "character_literal": case "text_block":
                        return TypeRef.Platform;
                    default: return null;
                }
            }

            /// <summary>
            /// Quanto si risale una catena (<c>a.b().c().d()</c>): ogni anello conta due livelli. 100 = 50 anelli; il limite
            /// c'è solo contro un'espressione patologica (8, la prima stesura, fermava i <c>sb.append(…)</c> da dieci anelli).
            /// </summary>
            private const int MaxChainDepth = 100;

            /// <summary>Uno sconosciuto (parametro di tipo, <c>var</c> senza <c>new</c>) è «il codice non lo dice»: null.</summary>
            private static TypeRef Known(TypeRef r) => r == null || r.Kind == TypeRefKind.Unknown ? null : r;

            /// <summary>Il campo <paramref name="field"/> di un valore di tipo noto.</summary>
            private TypeRef FieldOf(TypeRef owner, string field)
            {
                if (owner.IsArray) return field == "length" ? TypeRef.Platform : null;
                switch (owner.Kind)
                {
                    case TypeRefKind.Platform: return TypeRef.Platform;        // System.out, Color.RED…
                    case TypeRefKind.External: return TypeRef.LibraryValue;
                    case TypeRefKind.Project:
                        foreach (var fqn in owner.Fqns)
                            if (_index.FieldType(fqn, field) is TypeRef f) return Known(f);
                        // un nome di classe annidata (Outer.Inner.metodo()) o una costante di enum
                        foreach (var fqn in owner.Fqns)
                            if (_index.TypesByFqn.ContainsKey(fqn + "." + field)) return new TypeRef(TypeRefKind.Project, new[] { fqn + "." + field }, owner.Confidence);
                        return null;
                }
                return null;
            }

            /// <summary>
            /// Il tipo del risultato di <c>a.b(…)</c>: si risolve la chiamata come per gli archi (senza scriverne) e si prende
            /// il tipo di ritorno dei metodi trovati, letto nel file che li dichiara; un parametro di tipo della classe si
            /// sostituisce con l'argomento del ricevente (<c>Box&lt;Foo&gt;.get()</c> → Foo). Per la piattaforma, i metodi
            /// che restituiscono un elemento (<c>List&lt;Foo&gt;.get</c>, <c>Map&lt;K,V&gt;.get</c>, <c>next</c>,
            /// <c>orElse</c>…) danno l'argomento; tutti gli altri restano piattaforma. Null quando non si sa.
            /// </summary>
            private TypeRef TypeOfInvocation(Node inv, string ownerFqn, LocalScope locals, int depth)
            {
                var nameNode = inv.NamedChildren.LastOrDefault(c => c.Type == "identifier");
                if (nameNode == null) return null;
                var name = nameNode.Text;
                var obj = inv.NamedChildren.FirstOrDefault();
                if (obj == nameNode) obj = null;
                var argc = inv.NamedChildren.FirstOrDefault(c => c.Type == "argument_list")?.NamedChildren.Count ?? -1;
                List<MethodEntry> ms;
                TypeRef receiver = null;
                if (obj == null || obj.Type == "this")
                {
                    ms = ImplicitTargets(ownerFqn, obj, name, argc, out var holder, out var inheritedFrom);
                    if (ms == null)
                    {
                        if (holder != null) return holder.Kind == TypeRefKind.Platform ? TypeRef.Platform : holder.Kind == TypeRefKind.External ? TypeRef.LibraryValue : null;
                        if (inheritedFrom == PerimeterIndex.PlatformAncestor || ObjectMethods.Contains(name)) return TypeRef.Platform;
                        return inheritedFrom != null ? TypeRef.LibraryValue : null;
                    }
                }
                else if (obj.Type == "super")
                {
                    string ext = null;
                    ms = ownerFqn == null ? null : _index.FindMethod(ownerFqn, name, argc, out ext, skipSelf: true);
                    if (ms == null) return ext == PerimeterIndex.PlatformAncestor || ObjectMethods.Contains(name) ? TypeRef.Platform : ext != null ? TypeRef.LibraryValue : null;
                }
                else
                {
                    receiver = ReceiverType(obj, ownerFqn, locals, depth + 1);
                    if (receiver == null) return null;
                    switch (receiver.Kind)
                    {
                        case TypeRefKind.Platform: return PlatformResult(receiver, name);
                        case TypeRefKind.External: return TypeRef.LibraryValue;
                        case TypeRefKind.Unknown: return null;
                    }
                    if (receiver.IsArray) return name == "clone" ? receiver : TypeRef.Platform;
                    ms = new List<MethodEntry>();
                    var fromLibrary = false;
                    foreach (var fqn in receiver.Fqns)
                    {
                        var found = _index.FindMethod(fqn, name, argc, out var ext);
                        if (found != null) ms.AddRange(found);
                        else if (ext == PerimeterIndex.PlatformAncestor || ObjectMethods.Contains(name)) return TypeRef.Platform;
                        else if (ext != null) fromLibrary = true;
                    }
                    if (ms.Count == 0) return fromLibrary ? TypeRef.LibraryValue : null;
                }
                // i tipi di ritorno: devono dire la stessa cosa, sennò non si sa
                TypeRef result = null;
                foreach (var m in ms)
                {
                    var r = ReturnOf(m, receiver, receiver?.Fqns.FirstOrDefault() ?? ownerFqn);
                    if (r == null) return null;
                    if (result == null) result = r;
                    else if (result.Kind != r.Kind || !result.Fqns.SequenceEqual(r.Fqns)) return null;
                }
                return result == null ? null : result with { Confidence = Weaker(result.Confidence, receiver?.Confidence ?? "extracted") };
            }

            private TypeRef ReturnOf(MethodEntry m, TypeRef receiver, string fromFqn)
            {
                if (m.ReturnText == null || m.OwnerFqn == null || !_index.Types.TryGetValue(m.OwnerFqn, out var owner)) return null;
                var text = m.ReturnText.Trim();
                var head = text.Split('<')[0].Split('[')[0].Trim();
                var idx = owner.TypeParams.IndexOf(head);
                if (idx < 0) return Known(_index.ResolveText(owner.Ctx, text, m.TypeParams));
                var array = text.EndsWith("[]", StringComparison.Ordinal) ? "[]" : "";
                // T dichiarato dalla classe del metodo. Box<Foo>.get(): l'argomento del ricevente, letto dove è scritto
                if (fromFqn == m.OwnerFqn)
                    return receiver?.Args != null && idx < receiver.Args.Count && receiver.ArgsCtx != null
                        ? Known(_index.ResolveText(receiver.ArgsCtx, receiver.Args[idx] + array, _typeParams)) : null;
                // ereditato: class Tikz extends AbstractUGraphic<TikzGraphics> e O getGraphicObject() → TikzGraphics
                if (fromFqn != null && _index.ArgsFor(fromFqn, m.OwnerFqn) is var (args, ctx, tps) && idx < args.Count)
                    return Known(_index.ResolveText(ctx, args[idx] + array, tps));
                return null;
            }

            private static readonly HashSet<string> MapLike = new(StringComparer.Ordinal)
                { "Map", "HashMap", "TreeMap", "LinkedHashMap", "ConcurrentHashMap", "ConcurrentMap", "SortedMap", "NavigableMap", "EnumMap", "WeakHashMap", "IdentityHashMap", "Hashtable" };
            private static readonly HashSet<string> ElementOfFirst = new(StringComparer.Ordinal)
                { "get", "getFirst", "getLast", "next", "previous", "peek", "peekFirst", "peekLast", "poll", "pollFirst", "pollLast", "pop", "element",
                  "first", "last", "orElse", "orElseThrow", "orElseGet", "take", "join", "getNow", "firstElement", "lastElement", "elementAt", "floor", "ceiling", "higher", "lower", "removeFirst", "removeLast" };
            private static readonly HashSet<string> KeepsArgs = new(StringComparer.Ordinal)
                { "iterator", "listIterator", "descendingIterator", "subList", "headSet", "tailSet", "subSet", "unmodifiableList", "reversed" };

            /// <summary>Il risultato di un metodo della piattaforma: l'elemento per gli accessor dei contenitori, altrimenti piattaforma.</summary>
            private TypeRef PlatformResult(TypeRef receiver, string name)
            {
                var type = receiver.PlatformName ?? string.Empty;
                var args = receiver.Args ?? Array.Empty<string>();
                TypeRef Arg(int i) => i < args.Count && receiver.ArgsCtx != null ? Known(_index.ResolveText(receiver.ArgsCtx, args[i], _typeParams)) : null;
                if (receiver.IsArray) return TypeRef.Platform;
                if (MapLike.Contains(type) || type.EndsWith("Map", StringComparison.Ordinal) && args.Count == 2)
                {
                    switch (name)
                    {
                        case "get": case "getOrDefault": case "remove": case "put": case "putIfAbsent": case "computeIfAbsent": case "computeIfPresent": case "compute": case "merge": case "replace":
                            return Arg(1);
                        case "values": return args.Count == 2 ? new TypeRef(TypeRefKind.Platform, new[] { "Collection" }, "extracted", new[] { args[1] }, receiver.ArgsCtx) : TypeRef.Platform;
                        case "keySet": case "navigableKeySet": return args.Count >= 1 ? new TypeRef(TypeRefKind.Platform, new[] { "Set" }, "extracted", new[] { args[0] }, receiver.ArgsCtx) : TypeRef.Platform;
                        case "firstKey": case "lastKey": case "ceilingKey": case "floorKey": case "higherKey": case "lowerKey": return Arg(0);
                    }
                    return TypeRef.Platform;
                }
                if (type is "Map.Entry" or "Entry")
                    return name == "getKey" ? Arg(0) : name == "getValue" ? Arg(1) : TypeRef.Platform;
                if (args.Count >= 1 && ElementOfFirst.Contains(name)) return Arg(0);
                if (args.Count >= 1 && KeepsArgs.Contains(name)) return receiver with { Fqns = new[] { name.Contains("terator") ? "Iterator" : type } };
                return TypeRef.Platform;
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
                if (receiver.IsArray) return;   // clone(), length: dell'array, piattaforma
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
                        if (extAncestor != PerimeterIndex.PlatformAncestor) Edge(ownerId, External("method", extAncestor + "." + name, name), "calls", "inferred", line);
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
