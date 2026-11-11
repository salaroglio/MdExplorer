using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// La parte comune degli estrattori Roslyn (C# e Visual Basic .NET): il modello dei simboli è lo stesso per i due
    /// linguaggi, cambia solo la sintassi. Qui stanno simboli, archi, id e reference del runtime; ogni linguaggio dice
    /// come si legge un file, dove stanno le dichiarazioni di tipo e come sono fatte una chiamata e una costruzione.
    /// </summary>
    public abstract class RoslynExtractorBase : ICodeExtractor
    {
        public abstract string LanguageId { get; }
        public string ExtractorName => "roslyn";
        public abstract string ExtractorVersion { get; }
        public string Availability => "ready";

        private readonly ILogger _logger;

        /// <summary>Una chiamata come è scritta: il testo dell'espressione chiamata e, se c'è, il punto di chiamata (D42).</summary>
        protected readonly struct InvocationShape
        {
            public string Text { get; init; }
            /// <summary>Null = nessun punto di chiamata da registrare.</summary>
            public string Callee { get; init; }
            public string Receiver { get; init; }
            public List<string> Arguments { get; init; }
            /// <summary>Gli argomenti come nodi, per posizione: una costante (<c>Const SP = "PKG.X"</c>) vale come la stringa scritta.</summary>
            public IReadOnlyList<SyntaxNode> ArgumentNodes { get; init; }
            /// <summary>Per posizione, il tipo scritto dentro <c>typeof(X)</c> / <c>GetType(X)</c>; null dove l'argomento è altro.</summary>
            public IReadOnlyList<SyntaxNode> TypeNodes { get; init; }
            /// <summary>Gli argomenti generici SCRITTI nella chiamata (<c>AddScoped&lt;IFoo, Foo&gt;()</c>); vuoto se non ce ne sono.</summary>
            public IReadOnlyList<SyntaxNode> GenericNodes { get; init; }
            /// <summary>L'espressione prima del punto, per seguire una catena (<c>RegisterType&lt;Foo&gt;().As&lt;IFoo&gt;()</c>); null senza ricevente.</summary>
            public SyntaxNode ReceiverNode { get; init; }
        }

        /// <summary>Un argomento di un attributo: una stringa o un tipo (<c>typeof(X)</c> / <c>GetType(X)</c>); un vettore dà un argomento per elemento.</summary>
        protected readonly struct AttributeArgumentShape
        {
            /// <summary>Il nome scritto (<c>Name = …</c>, <c>Name:=…</c>); null per un argomento per posizione.</summary>
            public string Element { get; init; }
            /// <summary>La posizione fra gli argomenti senza nome, da 0; -1 per uno con nome.</summary>
            public int Position { get; init; }
            public string Value { get; init; }
            public SyntaxNode TypeNode { get; init; }
        }

        /// <summary>Un attributo come è scritto su una dichiarazione (D43 per .NET): <c>[Export(typeof(IFoo))]</c>, <c>&lt;Export(GetType(IFoo))&gt;</c>.</summary>
        protected readonly struct AttributeShape
        {
            /// <summary>Il nome scritto, per risolvere la classe dell'attributo.</summary>
            public SyntaxNode NameNode { get; init; }
            public IReadOnlyList<AttributeArgumentShape> Arguments { get; init; }
        }

        /// <summary>Gli attributi scritti sulla dichiarazione di un tipo o di un membro, nell'ordine.</summary>
        protected abstract IEnumerable<AttributeShape> AttributesOf(SyntaxNode declaration);

        /// <summary>Gli attributi che non dicono niente al grafo: non si registrano (come <c>@Override</c> in Java).</summary>
        private static readonly HashSet<string> SilentAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Serializable", "NonSerialized", "Obsolete", "Flags", "CompilerGenerated", "GeneratedCode", "DesignerGenerated", "DebuggerStepThrough",
            "DebuggerNonUserCode", "DebuggerDisplay", "DebuggerBrowsable", "DebuggerHidden", "SuppressMessage", "MethodImpl", "StructLayout", "FieldOffset",
            "Extension", "ParamArray", "CLSCompliant", "ComVisible", "Conditional", "EditorBrowsable", "Browsable", "STAThread", "MTAThread", "ThreadStatic",
        };

        /// <summary><c>.csproj</c> o <c>.vbproj</c>: il file di progetto di questo linguaggio.</summary>
        protected abstract string ProjectExtension { get; }
        /// <param name="project">Il progetto del file, o null se non ne ha uno (o i sorgenti non stanno su questo disco).</param>
        protected abstract SyntaxTree Parse(string text, string path, DotNetProject project);
        protected abstract Compilation Compile(string name, IReadOnlyList<SyntaxTree> trees, IReadOnlyList<MetadataReference> references, DotNetProject project);

        /// <summary>I file di un progetto (o quelli senza progetto) e la loro compilazione.</summary>
        private sealed class ProjectGroup
        {
            public DotNetProject Project { get; init; }
            public List<SyntaxTree> Trees { get; } = new();
            public Compilation Compilation { get; set; }
        }

        /// <summary>Prima i progetti riferiti, poi chi li riferisce: così la compilazione di uno vede i tipi dell'altro.</summary>
        private static IEnumerable<ProjectGroup> InReferenceOrder(List<ProjectGroup> groups, Dictionary<string, ProjectGroup> byKey)
        {
            var done = new HashSet<ProjectGroup>();
            var open = new HashSet<ProjectGroup>();
            var ordered = new List<ProjectGroup>();
            void Visit(ProjectGroup g)
            {
                if (done.Contains(g) || !open.Add(g)) return;   // un giro di riferimenti (non dovrebbe esistere) non ferma l'analisi
                foreach (var r in g.Project?.ProjectReferences ?? Array.Empty<string>())
                    if (byKey.TryGetValue(r, out var other)) Visit(other);
                open.Remove(g);
                done.Add(g);
                ordered.Add(g);
            }
            foreach (var g in groups) Visit(g);
            return ordered;
        }

        private static readonly Dictionary<string, MetadataReference> FileReferences = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Una libreria su disco come reference, una volta sola per percorso; null se non si legge.</summary>
        private static MetadataReference FileReference(string path)
        {
            lock (FileReferences)
            {
                if (FileReferences.TryGetValue(path, out var known)) return known;
                MetadataReference reference = null;
                try { reference = MetadataReference.CreateFromFile(path); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is BadImageFormatException) { }
                return FileReferences[path] = reference;
            }
        }
        /// <summary>Le dichiarazioni di tipo del file, col nodo INTERO (le sue righe sono quelle del simbolo).</summary>
        protected abstract IEnumerable<SyntaxNode> TypeDeclarations(SyntaxNode root);
        protected virtual INamedTypeSymbol TypeSymbol(SemanticModel model, SyntaxNode typeDeclaration) => model.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
        /// <summary>Dal nodo che Roslyn dà per un membro al nodo che ne contiene anche il corpo (in VB sono due nodi diversi).</summary>
        protected virtual SyntaxNode Whole(SyntaxNode memberDeclaration) => memberDeclaration;
        protected abstract bool IsInvocation(SyntaxNode node, SemanticModel model, out InvocationShape shape);
        /// <param name="typeName">Il nome del tipo com'è scritto (ultima parola); null se non è scritto (<c>new("x")</c>).</param>
        protected abstract bool IsCreation(SyntaxNode node, out string typeName, out IReadOnlyList<SyntaxNode> arguments);

        /// <summary>Un valore assegnato a qualcosa che ha un nome: <c>cmd.CommandText = "…"</c>, <c>With {.Nome = "…"}</c>.</summary>
        protected readonly struct AssignmentShape
        {
            public string Name { get; init; }
            /// <summary>Il testo prima del punto; null senza ricevente.</summary>
            public string Receiver { get; init; }
            public SyntaxNode Target { get; init; }
            public SyntaxNode Value { get; init; }
        }

        protected abstract bool IsAssignment(SyntaxNode node, out AssignmentShape shape);
        /// <summary>Un evento sollevato: <c>RaiseEvent Click(…)</c>, <c>Click?.Invoke(…)</c>. Il nodo è ciò che nomina l'evento; la base controlla che lo sia.</summary>
        protected abstract bool IsRaise(SyntaxNode node, out SyntaxNode eventNode);
        /// <summary>Un gestore agganciato a un evento nel codice: <c>AddHandler x.Click, AddressOf H</c>, <c>x.Click += H</c>.</summary>
        protected abstract bool IsSubscription(SyntaxNode node, out SyntaxNode eventNode, out SyntaxNode handlerNode, out bool certain);
        /// <summary>
        /// Gli eventi che la dichiarazione di un metodo dice di gestire (VB <c>Handles btnCerca.Click, Me.Load</c>): il nodo che
        /// nomina l'evento e quello che nomina chi lo solleva (<c>btnCerca</c>; null per <c>Me</c>).
        /// </summary>
        protected virtual IEnumerable<(SyntaxNode Event, SyntaxNode Container)> HandledEvents(SyntaxNode methodDeclaration) => Enumerable.Empty<(SyntaxNode, SyntaxNode)>();
        /// <summary>Un nome di metodo usato senza la forma della chiamata (VB: parentesi facoltative, <c>AddressOf</c>).</summary>
        protected virtual bool IsMethodNameOutsideInvocation(SyntaxNode node) => false;

        private static readonly SymbolDisplayFormat QualifiedFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        private static readonly SymbolDisplayFormat SignatureFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
            memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType |
                           SymbolDisplayMemberOptions.IncludeAccessibility | SymbolDisplayMemberOptions.IncludeModifiers,
            kindOptions: SymbolDisplayKindOptions.IncludeMemberKeyword | SymbolDisplayKindOptions.IncludeTypeKeyword,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName |
                              SymbolDisplayParameterOptions.IncludeDefaultValue | SymbolDisplayParameterOptions.IncludeParamsRefOut,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private static readonly SymbolDisplayFormat TypeFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        protected RoslynExtractorBase(ILogger logger) { _logger = logger; }

        public CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var result = new CodeExtractionResult();
            if (request.FilesToEmit.Count == 0) return result;

            // 1) Le compilazioni: una per file di progetto (.csproj/.vbproj), con ciò che il progetto dice — spazio dei nomi di
            //    radice, Imports, framework, librerie, progetti riferiti; i file senza progetto stanno insieme, come prima.
            var local = request.Sources == null || request.Sources is LocalCodeSources;
            var projects = local ? new DotNetProjectMap(ProjectExtension, request.ProjectPath) : null;
            var contents = new Dictionary<string, string>(StringComparer.Ordinal);
            var groups = new List<ProjectGroup>();
            var groupByKey = new Dictionary<string, ProjectGroup>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in request.AllFiles)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = request.ReadText(f); }
                catch (Exception ex) { result.Warnings.Add(UiText.Say($"{f.LogicalPath}: non leggibile ({ex.Message})", $"{f.LogicalPath}: cannot be read ({ex.Message})")); continue; }
                contents[f.LogicalPath] = text;
                var project = projects?.Of(f.AbsolutePath);
                var key = project?.File ?? string.Empty;
                if (!groupByKey.TryGetValue(key, out var group)) { groupByKey[key] = group = new ProjectGroup { Project = project }; groups.Add(group); }
                group.Trees.Add(Parse(text, f.LogicalPath, project));
            }
            if (projects != null) result.Warnings.AddRange(projects.Problems);

            var trees = new List<(SyntaxTree Tree, Compilation Compilation)>();
            var compilations = new List<Compilation>();
            var missingFrameworks = new HashSet<string>(StringComparer.Ordinal);
            foreach (var group in InReferenceOrder(groups, groupByKey))
            {
                ct.ThrowIfCancellationRequested();
                var p = group.Project;
                var references = new List<MetadataReference>();
                var frameworkDirectory = p is { IsNetFramework: true } ? NetFrameworkLibraries.FindDirectory(p.FrameworkVersion) : null;
                if (frameworkDirectory != null) references.AddRange(NetFrameworkLibraries.Assemblies(frameworkDirectory).Select(FileReference).Where(r => r != null));
                else
                {
                    references.AddRange(RuntimeReferences());
                    if (p is { IsNetFramework: true } && missingFrameworks.Add(p.FrameworkVersion))
                        result.Warnings.Add(UiText.Say(
                            $"{Path.GetFileName(p.File)}: il progetto è .NET Framework {p.FrameworkVersion} ma su questo computer non ci sono le sue librerie: ciò che eredita o chiama dal framework (System.Web, Windows Forms…) resta un nome non risolto. Le porta il «Developer Pack» di .NET Framework, o indica una cartella con {NetFrameworkLibraries.OverrideVariable}.",
                            $"{Path.GetFileName(p.File)}: the project targets .NET Framework {p.FrameworkVersion} but its libraries are not on this computer: what it inherits or calls from the framework (System.Web, Windows Forms…) stays an unresolved name. The .NET Framework «Developer Pack» brings them, or point {NetFrameworkLibraries.OverrideVariable} to a folder."));
                }
                if (p != null)
                {
                    references.AddRange(p.Libraries.Select(FileReference).Where(r => r != null));
                    if (p.MissingLibraries.Count > 0)
                        result.Warnings.Add(UiText.Say(
                            $"{Path.GetFileName(p.File)}: {p.MissingLibraries.Count} librerie riferite non ci sono su questo computer ({string.Join(", ", p.MissingLibraries.Take(3).Select(Path.GetFileName))}{(p.MissingLibraries.Count > 3 ? "…" : "")}): ciò che ne usa resta un nome non risolto.",
                            $"{Path.GetFileName(p.File)}: {p.MissingLibraries.Count} referenced libraries are not on this computer ({string.Join(", ", p.MissingLibraries.Take(3).Select(Path.GetFileName))}{(p.MissingLibraries.Count > 3 ? "…" : "")}): what uses them stays an unresolved name."));
                    foreach (var referenced in p.ProjectReferences)
                        if (groupByKey.TryGetValue(referenced, out var other) && other.Compilation != null) references.Add(other.Compilation.ToMetadataReference());
                }
                group.Compilation = Compile("mde-codegraph" + (p == null ? "" : "-" + Path.GetFileNameWithoutExtension(p.File)), group.Trees, references, p);
                compilations.Add(group.Compilation);
                trees.AddRange(group.Trees.Select(t => (t, group.Compilation)));
            }
            var perimeter = new PerimeterIndex(compilations);

            // 2) Un script per file da emettere.
            var toEmit = new HashSet<string>(request.FilesToEmit.Select(f => f.LogicalPath), StringComparer.Ordinal);
            foreach (var (tree, compilation) in trees)
            {
                ct.ThrowIfCancellationRequested();
                if (!toEmit.Contains(tree.FilePath)) continue;
                var file = request.FilesToEmit.First(f => f.LogicalPath == tree.FilePath);
                try
                {
                    var script = ExtractFile(tree, compilation.GetSemanticModel(tree), perimeter, contents[tree.FilePath]);
                    result.Scripts.Add((file, script));
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"{tree.FilePath}: estrazione fallita ({ex.GetType().Name}: {ex.Message})");
                    _logger?.LogWarning(ex, "[CodeGraph/roslyn] estrazione fallita per {File}", tree.FilePath);
                }
            }
            return result;
        }

        // ------------------------------------------------------------------ one file

        private CodeScript ExtractFile(SyntaxTree tree, SemanticModel model, PerimeterIndex perimeter, string content)
        {
            var script = new CodeScript
            {
                Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = tree.FilePath, Language = LanguageId,
                    SourceHash = Sha256(content), Extractor = ExtractorName, ExtractorVersion = ExtractorVersion,
                    GeneratedAt = DateTime.UtcNow.ToString("o"),
                }
            };
            var ctx = new FileContext(this, script, model, perimeter);
            var root = tree.GetRoot();

            foreach (var typeDecl in TypeDeclarations(root))
            {
                var type = TypeSymbol(model, typeDecl);
                if (type == null) continue;
                var typeId = ctx.Declare(type, typeDecl, KindOf(type), signature: type.ToDisplayString(SignatureFormat), returnType: null);
                ctx.Annotate(typeId, typeDecl);

                if (type.ContainingType != null) ctx.Edge(ctx.IdOf(type.ContainingType), typeId, "contains", "extracted", Line(typeDecl));
                if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object && type.BaseType.SpecialType != SpecialType.System_ValueType && type.BaseType.SpecialType != SpecialType.System_Enum)
                    ctx.Edge(typeId, ctx.IdOf(type.BaseType), type.TypeKind == TypeKind.Interface ? "inherits" : "inherits", "extracted", Line(typeDecl));
                foreach (var iface in type.Interfaces)
                    ctx.Edge(typeId, ctx.IdOf(iface), type.TypeKind == TypeKind.Interface ? "inherits" : "implements", "extracted", Line(typeDecl));

                foreach (var member in type.GetMembers())
                {
                    if (member.IsImplicitlyDeclared) continue;
                    var declared = member.DeclaringSyntaxReferences.FirstOrDefault(r => r.SyntaxTree == tree)?.GetSyntax();
                    if (declared == null) continue; // partial: dichiarato in un altro file
                    var decl = Whole(declared);
                    switch (member)
                    {
                        case IMethodSymbol m when m.MethodKind is MethodKind.Ordinary or MethodKind.DeclareMethod or MethodKind.Constructor or MethodKind.StaticConstructor:
                            {
                                var kind = IsPlainMethod(m) ? "method" : "constructor";
                                var id = ctx.Declare(m, decl, kind, m.ToDisplayString(SignatureFormat), IsPlainMethod(m) ? m.ReturnType.ToDisplayString(TypeFormat) : null, typeId);
                                ctx.Edge(typeId, id, "contains", "extracted", Line(decl));
                                ctx.Annotate(id, decl);
                                foreach (var (handled, container) in HandledEvents(decl))
                                {
                                    ctx.Handler(handled, id, Line(handled), certain: true);
                                    // il controllo della pagina (un campo WithEvents del perimetro) porta al suo gestore: l'evento di
                                    // libreria da solo è lo stesso nodo per tutti i pulsanti
                                    if (container != null) ctx.ControlHandler(container, id, Line(handled));
                                }
                                if (IsPlainMethod(m)) ctx.Reference(id, m.ReturnType, Line(decl));
                                var pos = 0;
                                foreach (var p in m.Parameters)
                                {
                                    ctx.Script.Parameters.Add(new ScriptParameter
                                    {
                                        SymbolId = id, Position = pos++, Name = p.Name, ParameterType = p.Type.ToDisplayString(TypeFormat),
                                        Direction = Direction(p), IsNullable = p.NullableAnnotation == NullableAnnotation.Annotated || p.Type.NullableAnnotation == NullableAnnotation.Annotated,
                                        DefaultValue = p.HasExplicitDefaultValue ? (p.ExplicitDefaultValue?.ToString() ?? "null") : null,
                                    });
                                    ctx.Reference(id, p.Type, Line(decl));
                                }
                                ctx.WalkBody(decl, id);
                                break;
                            }
                        case IPropertySymbol prop:
                            {
                                var id = ctx.Declare(prop, decl, PropertyKind(prop), prop.ToDisplayString(SignatureFormat), prop.Type.ToDisplayString(TypeFormat), typeId);
                                ctx.Edge(typeId, id, "contains", "extracted", Line(decl));
                                ctx.Annotate(id, decl);
                                ctx.Reference(id, prop.Type, Line(decl));
                                ctx.WalkBody(decl, id);
                                break;
                            }
                        case IEventSymbol ev:
                            {
                                // un evento è un nodo (kind field: è un campo di tipo delegato): chi lo solleva lo chiama, lui chiama i gestori
                                var id = ctx.Declare(ev, decl, "field", ev.ToDisplayString(SignatureFormat), ev.Type.ToDisplayString(TypeFormat), typeId);
                                ctx.Edge(typeId, id, "contains", "extracted", Line(decl));
                                ctx.Reference(id, ev.Type, Line(decl));
                                break;
                            }
                        case IFieldSymbol field when !field.IsImplicitlyDeclared:
                            {
                                var id = ctx.Declare(field, decl, "field", field.ToDisplayString(SignatureFormat), field.Type.ToDisplayString(TypeFormat), typeId);
                                ctx.Edge(typeId, id, "contains", "extracted", Line(decl));
                                if (type.TypeKind != TypeKind.Enum) ctx.Annotate(id, decl);
                                if (type.TypeKind != TypeKind.Enum) ctx.Reference(id, field.Type, Line(decl));
                                ctx.WalkBody(decl, id);
                                break;
                            }
                    }
                }
            }
            return script;
        }

        private static string KindOf(INamedTypeSymbol t) => t.TypeKind switch
        {
            TypeKind.Interface => "interface",
            TypeKind.Struct => t.IsRecord ? "record" : "struct",
            TypeKind.Enum => "enum",
            TypeKind.Class => t.IsRecord ? "record" : "class",
            TypeKind.Delegate => "class",
            _ => "class",
        };

        /// <summary>Un metodo con un nome: anche <c>Declare Function … Lib</c> di VB (una funzione di una DLL nativa).</summary>
        private static bool IsPlainMethod(IMethodSymbol m) => m.MethodKind is MethodKind.Ordinary or MethodKind.DeclareMethod;

        /// <summary>Un campo <c>WithEvents</c> di VB è per Roslyn una proprietà: nel grafo resta il campo che chi legge ha scritto.</summary>
        private static string PropertyKind(IPropertySymbol p) => p.IsWithEvents ? "field" : "property";

        private static string Direction(IParameterSymbol p)
        {
            if (p.IsParams) return "params";
            if (p.IsThis) return "this";
            return p.RefKind switch { RefKind.Out => "out", RefKind.Ref => "ref", RefKind.In => "in", RefKind.RefReadOnlyParameter => "ref", _ => "in" };
        }

        private static int Line(SyntaxNode node) => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        // ------------------------------------------------------------------ ids

        private static string Sha256(string content)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        }

        // ------------------------------------------------------------------ references

        private static IReadOnlyList<MetadataReference> _runtimeReferences;
        private static readonly object RefLock = new();

        /// <summary>Le reference del runtime in cui gira MdExplorer (trusted platform assemblies): senza SDK, senza NuGet.</summary>
        private static IReadOnlyList<MetadataReference> RuntimeReferences()
        {
            if (_runtimeReferences != null) return _runtimeReferences;
            lock (RefLock)
            {
                if (_runtimeReferences != null) return _runtimeReferences;
                var list = new List<MetadataReference>();
                var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
                var paths = string.IsNullOrEmpty(tpa)
                    ? Directory.GetFiles(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "*.dll")
                    : tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in paths)
                {
                    try { list.Add(MetadataReference.CreateFromFile(p)); }
                    catch { /* un assembly non leggibile come metadata (native, risorse) non è una reference */ }
                }
                _runtimeReferences = list;
                return list;
            }
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>
        /// I tipi dichiarati nei sorgenti, di tutte le compilazioni: per sapere chi è in perimetro e per il dispatch per
        /// interfaccia. L'implementazione può stare in un progetto che chi chiama non riferisce, quindi le interfacce si
        /// riconoscono per nome qualificato, non per identità del simbolo.
        /// </summary>
        private sealed class PerimeterIndex
        {
            private readonly Dictionary<string, List<INamedTypeSymbol>> _implementers = new(StringComparer.Ordinal);

            public PerimeterIndex(IEnumerable<Compilation> compilations)
            {
                foreach (var compilation in compilations)
                {
                    var stack = new Stack<INamespaceOrTypeSymbol>();
                    stack.Push(compilation.Assembly.GlobalNamespace);
                    while (stack.Count > 0)
                    {
                        var s = stack.Pop();
                        foreach (var m in s.GetMembers())
                        {
                            if (m is INamespaceSymbol ns) stack.Push(ns);
                            else if (m is INamedTypeSymbol t)
                            {
                                stack.Push(t);
                                if (t.TypeKind == TypeKind.Interface) continue;
                                foreach (var key in t.AllInterfaces.Select(Key).Distinct(StringComparer.Ordinal))
                                {
                                    if (!_implementers.TryGetValue(key, out var list)) _implementers[key] = list = new List<INamedTypeSymbol>();
                                    list.Add(t);
                                }
                            }
                        }
                    }
                }
            }

            private static string Key(ISymbol symbol) => symbol.OriginalDefinition.ToDisplayString(QualifiedFormat);

            public bool IsInPerimeter(ISymbol s) => s.Locations.Any(l => l.IsInSource);

            public IEnumerable<ISymbol> ImplementationsOf(ISymbol interfaceMember)
            {
                if (!_implementers.TryGetValue(Key(interfaceMember.ContainingType), out var types)) yield break;
                var interfaceKey = Key(interfaceMember.ContainingType);
                var memberKey = Key(interfaceMember);
                foreach (var t in types)
                {
                    // l'interfaccia e il membro come li vede la compilazione del tipo che implementa
                    foreach (var iface in t.AllInterfaces.Where(i => Key(i) == interfaceKey))
                    {
                        var member = iface.GetMembers(interfaceMember.Name).FirstOrDefault(m => Key(m) == memberKey);
                        var impl = member == null ? null : t.FindImplementationForInterfaceMember(member);
                        if (impl != null && IsInPerimeter(impl)) { yield return impl; break; }
                    }
                }
            }
        }

        private sealed class FileContext
        {
            public CodeScript Script { get; }
            private readonly SemanticModel _model;
            private readonly PerimeterIndex _perimeter;
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly HashSet<string> _edges = new(StringComparer.Ordinal);
            private readonly CodeCallSites _callSites;
            private readonly RoslynExtractorBase _x;

            public FileContext(RoslynExtractorBase extractor, CodeScript script, SemanticModel model, PerimeterIndex perimeter)
            {
                _x = extractor; Script = script; _model = model; _perimeter = perimeter;
                _callSites = new CodeCallSites(script);
            }

            public string IdOf(ISymbol symbol)
            {
                var original = symbol.OriginalDefinition ?? symbol;
                var kind = original switch
                {
                    INamedTypeSymbol t when _perimeter.IsInPerimeter(t) => KindOf(t),
                    IMethodSymbol m when _perimeter.IsInPerimeter(m) => IsPlainMethod(m) ? "method" : "constructor",
                    IPropertySymbol p when _perimeter.IsInPerimeter(original) => PropertyKind(p),
                    IFieldSymbol when _perimeter.IsInPerimeter(original) => "field",
                    IEventSymbol when _perimeter.IsInPerimeter(original) => "field",
                    _ => "external",
                };
                var qualified = original.ToDisplayString(QualifiedFormat);
                var id = RoslynCSharpExtractor.StableId(_x.LanguageId, kind, qualified);
                if (kind == "external" && _externals.Add(id))
                {
                    Script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = original.Name, QualifiedName = qualified });
                }
                return id;
            }

            public string Declare(ISymbol symbol, SyntaxNode decl, string kind, string signature, string returnType, string parentId = null)
            {
                var span = decl.GetLocation().GetLineSpan();
                var id = RoslynCSharpExtractor.StableId(_x.LanguageId, kind, symbol.ToDisplayString(QualifiedFormat));
                Script.Symbols.Add(new ScriptSymbol
                {
                    Id = id, Kind = kind, Name = symbol.Name, QualifiedName = symbol.ToDisplayString(QualifiedFormat),
                    LineStart = span.StartLinePosition.Line + 1, LineEnd = span.EndLinePosition.Line + 1,
                    Signature = signature, ReturnType = returnType,
                    Visibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant(),
                    IsStatic = symbol.IsStatic || symbol is INamedTypeSymbol { TypeKind: TypeKind.Module }, IsAsync = symbol is IMethodSymbol { IsAsync: true },
                    ParentId = parentId ?? (symbol.ContainingType != null ? IdOf(symbol.ContainingType) : null),
                    // D43: il tipo restituito col nome che ha nel grafo, per le regole «il metodo annotato fornisce ciò che restituisce»
                    ReturnTypeName = symbol is IMethodSymbol { ReturnsVoid: false } method && IsPlainMethod(method) ? GraphTypeName(method.ReturnType) : null,
                });
                return id;
            }

            /// <summary>Il nome qualificato di un tipo com'è nel grafo; null per i tipi del linguaggio, i parametri di tipo, i vettori e ciò che non si risolve.</summary>
            private static string GraphTypeName(ITypeSymbol type)
                => type is INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: not TypeKind.Error } named ? named.OriginalDefinition.ToDisplayString(QualifiedFormat) : null;

            /// <summary>
            /// Gli attributi della dichiarazione (D43 per .NET), letti da come sono scritti: i pacchetti dei contenitori non sono
            /// fra le reference, quindi la classe dell'attributo spesso non si risolve e <c>typeName</c> resta vuoto. Il nome è
            /// l'ultima parola senza <c>Attribute</c> (<c>Export</c>); l'argomento per posizione 0 è l'elemento <c>value</c>, i
            /// successivi <c>arg1</c>, <c>arg2</c>; quelli con nome hanno il loro.
            /// </summary>
            public void Annotate(string symbolId, SyntaxNode declaration)
            {
                var ordinal = 0;
                foreach (var a in _x.AttributesOf(declaration))
                {
                    var written = a.NameNode.ToString();
                    var name = written[(written.LastIndexOf('.') + 1)..];
                    if (name.EndsWith("Attribute", StringComparison.Ordinal) && name.Length > "Attribute".Length) name = name[..^"Attribute".Length];
                    if (SilentAttributes.Contains(name)) continue;
                    var attributeClass = (_model.GetSymbolInfo(a.NameNode).Symbol as IMethodSymbol)?.ContainingType ?? _model.GetTypeInfo(a.NameNode).Type as INamedTypeSymbol;
                    var annotation = new ScriptAnnotation { SymbolId = symbolId, Ordinal = ordinal++, Name = name, Line = Line(a.NameNode), TypeName = attributeClass == null ? null : GraphTypeName(attributeClass) };
                    var index = 0;
                    foreach (var arg in a.Arguments)
                    {
                        var text = CodeCallSites.IsNameLike(arg.Value) ? arg.Value : null;
                        var type = arg.TypeNode == null ? null : TypeArgument(arg.TypeNode, 0);
                        if (text == null && type == null) continue;
                        var element = arg.Element ?? (arg.Position == 0 ? "value" : "arg" + arg.Position);
                        annotation.Arguments.Add(new ScriptAnnotationArgument { Element = element, Index = index++, Value = text, TypeName = type?.TypeName });
                    }
                    Script.Annotations.Add(annotation);
                }
            }

            public void Edge(string source, string target, string relation, string confidence, int? line)
            {
                if (source == null || target == null || source == target) return;
                if (!_edges.Add($"{source}|{target}|{relation}|{line}")) return;
                Script.Edges.Add(new ScriptEdge { SourceId = source, TargetId = target, Relation = relation, Confidence = confidence, Line = line });
            }

            /// <summary>Un uso di tipo (campo, proprietà, parametro, ritorno): arco <c>references</c> verso il tipo, anche generico (ai suoi argomenti).</summary>
            public void Reference(string from, ITypeSymbol type, int line)
            {
                if (type == null || type.SpecialType != SpecialType.None || type.TypeKind == TypeKind.TypeParameter) return;
                if (type is IArrayTypeSymbol arr) { Reference(from, arr.ElementType, line); return; }
                if (type is INamedTypeSymbol named)
                {
                    Edge(from, IdOf(named), "references", "extracted", line);
                    foreach (var arg in named.TypeArguments) Reference(from, arg, line);
                }
            }

            /// <summary>Dentro un corpo: chiamate (calls) e costruzioni (instantiates).</summary>
            public void WalkBody(SyntaxNode decl, string ownerId)
            {
                // i punti registrati di questo corpo, per nodo: una catena lega una chiamata a quella che le fa da ricevente (D43)
                var sites = new Dictionary<SyntaxNode, ScriptCallSite>();
                var chained = new List<(ScriptCallSite Site, SyntaxNode Receiver)>();
                foreach (var node in decl.DescendantNodes())
                {
                    if (_x.IsInvocation(node, _model, out var shape))
                    {
                        var info = _model.GetSymbolInfo(node);
                        var line = Line(node);
                        if (_x.IsRaise(node, out var raised) && _model.GetSymbolInfo(raised).Symbol is IEventSymbol raisedEvent)
                            Edge(ownerId, IdOf(raisedEvent), "calls", "extracted", line);
                        if (shape.Callee != null && _callSites.Add(ownerId, line, shape.Callee, shape.Receiver, Strings(shape.Arguments, shape.ArgumentNodes), types: TypesOf(shape)) is { } site)
                        {
                            sites[node] = site;
                            if (shape.ReceiverNode != null) chained.Add((site, shape.ReceiverNode));
                        }
                        if (info.Symbol is IMethodSymbol m) Call(ownerId, m, line);
                        else if (info.CandidateSymbols.Length > 0)
                        {
                            foreach (var c in info.CandidateSymbols) Edge(ownerId, IdOf(c), "calls", "ambiguous", line);
                        }
                        else if (info.Symbol == null)
                        {
                            var text = shape.Text;
                            var id = RoslynCSharpExtractor.StableId(_x.LanguageId, "external", text);
                            if (_externals.Add(id)) Script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = text.Split('.').Last(), QualifiedName = text });
                            Edge(ownerId, id, "calls", "ambiguous", line);
                        }
                    }
                    else if (_x.IsCreation(node, out var createdName, out var createdArguments))
                    {
                        var info = _model.GetSymbolInfo(node);
                        var line = Line(node);
                        // New OleDbCommand("PKG.PROC", cn): il nome passato al costruttore è un punto di chiamata come gli altri
                        createdName ??= (_model.GetTypeInfo(node).Type as INamedTypeSymbol)?.Name;
                        if (createdName != null && createdArguments.Count > 0)
                            _callSites.Add(ownerId, line, createdName, null, Strings(null, createdArguments));
                        if (info.Symbol is IMethodSymbol ctor && ctor.ContainingType != null)
                            Edge(ownerId, IdOf(ctor.ContainingType), "instantiates", "extracted", line);
                        else if (_model.GetTypeInfo(node).Type is INamedTypeSymbol t && t.TypeKind != TypeKind.Error)
                            Edge(ownerId, IdOf(t), "instantiates", "extracted", line);
                    }
                    else if (_x.IsRaise(node, out var raisedNode))
                    {
                        if (_model.GetSymbolInfo(raisedNode).Symbol is IEventSymbol e) Edge(ownerId, IdOf(e), "calls", "extracted", Line(node));
                    }
                    else if (_x.IsSubscription(node, out var eventNode, out var handlerNode, out var certain))
                    {
                        if (_model.GetSymbolInfo(handlerNode).Symbol is IMethodSymbol { MethodKind: MethodKind.Ordinary } handler)
                            Handler(eventNode, IdOf(handler.OriginalDefinition), Line(node), certain);
                    }
                    else if (_x.IsAssignment(node, out var assigned))
                    {
                        // cmd.CommandText = "PKG.PROC": un nome assegnato a una proprietà o a un campo si registra come una chiamata
                        // con quel nome e un argomento, così le regole sulle stringhe lo leggono (11/11/2026)
                        var target = _model.GetSymbolInfo(assigned.Target).Symbol;
                        var named = target is IPropertySymbol or IFieldSymbol || (target == null && assigned.Receiver != null);
                        if (named && ConstantString(assigned.Value) is { } value)
                            _callSites.Add(ownerId, Line(node), assigned.Name, assigned.Receiver, new[] { value });
                    }
                    else if (_x.IsMethodNameOutsideInvocation(node))
                    {
                        if (_model.GetSymbolInfo(node).Symbol is IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.ReducedExtension } m) Call(ownerId, m, Line(node));
                    }
                }
                foreach (var (site, receiver) in chained)
                {
                    // scendendo la catena, la prima chiamata registrata: quelle in mezzo senza tipi né nomi si saltano
                    for (var x = receiver; x != null;)
                    {
                        if (sites.TryGetValue(x, out var down)) { site.ReceiverLine = down.Line; site.ReceiverOrdinal = down.Ordinal; break; }
                        x = _x.IsInvocation(x, _model, out var inner) ? inner.ReceiverNode : null;
                    }
                }
            }

            /// <summary>
            /// Un gestore legato a un evento: l'evento «chiama» il gestore, dedotto (chi solleva l'evento arriva lì). Se l'evento
            /// non si risolve (libreria che manca) resta un nodo esterno col nome scritto, e il legame è ambiguo.
            /// </summary>
            public void Handler(SyntaxNode eventNode, string handlerId, int line, bool certain)
            {
                if (_model.GetSymbolInfo(eventNode).Symbol is IEventSymbol ev) { Edge(IdOf(ev), handlerId, "calls", "inferred", line); return; }
                if (!certain) return;   // x += y senza un evento riconosciuto può essere una somma
                var text = eventNode.Parent is { } p && p.ToString().Length <= CodeCallSites.MaxReceiverLength && p.ToString().EndsWith(eventNode.ToString(), StringComparison.Ordinal) && p.ToString().Contains('.') ? p.ToString() : eventNode.ToString();
                var id = RoslynCSharpExtractor.StableId(_x.LanguageId, "external", text);
                if (_externals.Add(id)) Script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = text.Split('.').Last(), QualifiedName = text });
                Edge(id, handlerId, "calls", "ambiguous", line);
            }

            public void ControlHandler(SyntaxNode containerNode, string handlerId, int line)
            {
                if (_model.GetSymbolInfo(containerNode).Symbol is { } control and (IPropertySymbol or IFieldSymbol) && _perimeter.IsInPerimeter(control))
                    Edge(IdOf(control), handlerId, "calls", "inferred", line);
            }

            /// <summary>Una stringa scritta o una costante che vale una stringa; null per tutto il resto.</summary>
            private string ConstantString(SyntaxNode node)
            {
                if (node == null) return null;
                var constant = _model.GetConstantValue(node);
                return constant.HasValue ? constant.Value as string : null;
            }

            /// <summary>Le stringhe degli argomenti per posizione: quelle che il linguaggio ha letto, e dove non ne ha lette le costanti.</summary>
            private List<string> Strings(IReadOnlyList<string> written, IReadOnlyList<SyntaxNode> nodes)
            {
                var count = Math.Max(written?.Count ?? 0, nodes?.Count ?? 0);
                var list = new List<string>(count);
                for (var i = 0; i < count; i++)
                    list.Add((written != null && i < written.Count ? written[i] : null) ?? (nodes != null && i < nodes.Count ? ConstantString(nodes[i]) : null));
                return list;
            }

            /// <summary>I fatti per le regole di registrazione (D43): i tipi fra le parentesi per posizione, poi quelli generici.</summary>
            private List<ScriptTypeArgument> TypesOf(InvocationShape shape)
            {
                var list = new List<ScriptTypeArgument>();
                for (var i = 0; i < (shape.TypeNodes?.Count ?? 0); i++)
                    if (shape.TypeNodes[i] != null && TypeArgument(shape.TypeNodes[i], i) is { } t) list.Add(t);
                for (var i = 0; i < (shape.GenericNodes?.Count ?? 0); i++)
                    if (TypeArgument(shape.GenericNodes[i], ScriptTypeArgument.GenericPositionBase + i) is { } t) list.Add(t);
                return list;
            }

            /// <summary>Il tipo scritto, col nome qualificato che ha nel grafo; null se non si risolve o è un parametro di tipo.</summary>
            private ScriptTypeArgument TypeArgument(SyntaxNode typeNode, int position)
            {
                var type = _model.GetSymbolInfo(typeNode).Symbol as ITypeSymbol ?? _model.GetTypeInfo(typeNode).Type;
                if (type == null || type.TypeKind is TypeKind.Error or TypeKind.TypeParameter) return null;
                if (type is IArrayTypeSymbol or IPointerTypeSymbol) return null;
                return new ScriptTypeArgument
                {
                    Position = position, TypeName = type.OriginalDefinition.ToDisplayString(QualifiedFormat),
                    Confidence = type.SpecialType != SpecialType.None ? "platform" : "extracted",
                };
            }

            /// <summary>La chiamata risolta; su un'interfaccia, in più, ogni implementazione nel perimetro (inferred).</summary>
            private void Call(string ownerId, IMethodSymbol m, int line)
            {
                var target = m.ReducedFrom ?? m.OriginalDefinition ?? m;
                Edge(ownerId, IdOf(target), "calls", "extracted", line);
                if (target.ContainingType?.TypeKind == TypeKind.Interface)
                    foreach (var impl in _perimeter.ImplementationsOf(target)) Edge(ownerId, IdOf(impl), "calls", "inferred", line);
            }
        }
    }
}
