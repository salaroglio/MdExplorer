using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
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
        private static IEnumerable<ProjectGroup> InReferenceOrder(List<ProjectGroup> groups, Func<ProjectGroup, IEnumerable<ProjectGroup>> referenced)
        {
            var done = new HashSet<ProjectGroup>();
            var open = new HashSet<ProjectGroup>();
            var ordered = new List<ProjectGroup>();
            void Visit(ProjectGroup g)
            {
                if (done.Contains(g) || !open.Add(g)) return;   // un giro di riferimenti (non dovrebbe esistere) non ferma l'analisi
                foreach (var other in referenced(g)) Visit(other);
                open.Remove(g);
                done.Add(g);
                ordered.Add(g);
            }
            foreach (var g in groups) Visit(g);
            return ordered;
        }

        /// <summary>
        /// I progetti che ogni progetto riferisce, fra quelli dei sorgenti: per percorso (<c>ProjectReference</c>) e, nei progetti
        /// di Visual Studio 2002/2003, per identità (<c>Reference Project="{GUID}"</c>, 12/11/2026). Un GUID che non è di nessun
        /// progetto dei sorgenti, o che è di più d'uno (una copia del progetto in un archivio), si dice: i suoi tipi restano
        /// nomi non risolti.
        /// </summary>
        private static Dictionary<ProjectGroup, List<ProjectGroup>> ReferencedGroups(List<ProjectGroup> groups, Dictionary<string, ProjectGroup> byKey, List<string> warnings,
            Dictionary<ProjectGroup, List<string>> borrowed)
        {
            var byGuid = groups.Where(g => g.Project?.ProjectGuid != null).GroupBy(g => g.Project.ProjectGuid, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<ProjectGroup, List<ProjectGroup>>();
            foreach (var g in groups)
            {
                var list = new List<ProjectGroup>();
                foreach (var r in g.Project?.ProjectReferences ?? Array.Empty<string>())
                    if (byKey.TryGetValue(r, out var other) && !list.Contains(other)) list.Add(other);
                foreach (var (guid, name) in g.Project?.ProjectReferenceGuids ?? Array.Empty<(string, string)>())
                {
                    if (!byGuid.TryGetValue(guid, out var found))
                    {
                        // il progetto non c'è (la libreria di un altro, compilata): la sua DLL, se un altro progetto la riferisce
                        var lib = BorrowLibrary(name, g, groups, out var from, out var problem);
                        if (lib != null)
                        {
                            Add(borrowed, g, lib);
                            warnings.Add(UiText.Say(
                                $"{Path.GetFileName(g.Project.File)}: riferisce il progetto {name} ({{{guid}}}), che non è fra i sorgenti analizzati: si usa la libreria {Path.GetFileName(lib)} con lo stesso nome di assembly, riferita da {Path.GetFileName(from)}.",
                                $"{Path.GetFileName(g.Project.File)}: references the project {name} ({{{guid}}}), which is not among the analysed sources: the library {Path.GetFileName(lib)} with the same assembly name, referenced by {Path.GetFileName(from)}, is used."));
                        }
                        else
                            warnings.Add(UiText.Say(
                                $"{Path.GetFileName(g.Project.File)}: riferisce il progetto {name} ({{{guid}}}), che non è fra i sorgenti analizzati{problem ?? ""}: ciò che ne usa resta un nome non risolto.",
                                $"{Path.GetFileName(g.Project.File)}: references the project {name} ({{{guid}}}), which is not among the analysed sources{problem ?? ""}: what uses it stays an unresolved name."));
                    }
                    else if (found.Count > 1)
                        warnings.Add(UiText.Say(
                            $"{Path.GetFileName(g.Project.File)}: il progetto {name} ({{{guid}}}) che riferisce è in {found.Count} file di progetto ({string.Join(", ", found.Select(f => f.Project.File))}): non si sa quale, ciò che ne usa resta un nome non risolto.",
                            $"{Path.GetFileName(g.Project.File)}: the project {name} ({{{guid}}}) it references is in {found.Count} project files ({string.Join(", ", found.Select(f => f.Project.File))}): which one is unknown, what uses it stays an unresolved name."));
                    else if (found[0] != g && !list.Contains(found[0])) list.Add(found[0]);
                }
                result[g] = list;
            }
            return result;
        }

        private static void Add(Dictionary<ProjectGroup, List<string>> map, ProjectGroup g, string path)
        {
            if (!map.TryGetValue(g, out var list)) map[g] = list = new List<string>();
            if (!list.Contains(path, StringComparer.OrdinalIgnoreCase)) list.Add(path);
        }

        private static readonly Dictionary<string, AssemblyName> AssemblyNames = new(StringComparer.OrdinalIgnoreCase);

        private static AssemblyName AssemblyNameOf(string path)
        {
            lock (AssemblyNames)
            {
                if (AssemblyNames.TryGetValue(path, out var known)) return known;
                AssemblyName name = null;
                try { name = AssemblyName.GetAssemblyName(path); }
                catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException or ArgumentException) { }
                return AssemblyNames[path] = name;
            }
        }

        /// <summary>
        /// Una libreria che questo progetto vuole e non trova (un progetto riferito che non c'è, un HintPath che non esiste), presa
        /// da un ALTRO progetto dei sorgenti che riferisce una DLL con lo stesso nome di assembly (12/11/2026, rilevazione dal cliente:
        /// il DL riferiva come progetto un blocco applicativo la cui DLL stava nelle librerie delle schermate). Una deduzione: chi
        /// la usa lo dice con un avviso. Più DLL con quel nome ma identità diverse (versione, chiave) = non si sceglie, e
        /// <paramref name="problem"/> lo dice.
        /// </summary>
        private static string BorrowLibrary(string assemblyName, ProjectGroup g, List<ProjectGroup> groups, out string fromProject, out string problem)
        {
            fromProject = null; problem = null;
            if (string.IsNullOrWhiteSpace(assemblyName)) return null;
            var found = groups.Where(o => o != g && o.Project != null)
                .SelectMany(o => o.Project.Libraries.Select(l => (Project: o.Project.File, Path: l, Name: AssemblyNameOf(l))))
                .Where(x => x.Name != null && string.Equals(x.Name.Name, assemblyName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase).ToList();
            if (found.Count == 0) return null;
            var identities = found.Select(x => x.Name.FullName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (identities.Count > 1)
            {
                problem = UiText.Say($" (ci sono {identities.Count} librerie {assemblyName} diverse fra gli altri progetti: {string.Join("; ", identities)})",
                    $" (there are {identities.Count} different {assemblyName} libraries among the other projects: {string.Join("; ", identities)})");
                return null;
            }
            fromProject = found[0].Project;
            return found[0].Path;
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

        /// <summary>Il linguaggio dei nodi esterni che l'SQL nelle stringhe nomina: lo stesso per tutti gli estrattori, così la tabella è un nodo solo.</summary>
        public const string SqlLanguage = "sql";

        /// <summary>Un pezzo di una stringa scritta nel codice: il testo, o il nodo di una parte non scritta (una variabile concatenata).</summary>
        protected readonly struct StringPart
        {
            public string Text { get; init; }
            public SyntaxNode Hole { get; init; }
        }

        /// <summary>
        /// La stringa intera che comincia in questo nodo — un letterale, una stringa interpolata, o tutta una concatenazione
        /// (<c>"SELECT … " &amp; filtro &amp; " ORDER BY …"</c>) — con i suoi pezzi in ordine. Falso per un nodo che è solo una
        /// parte di una concatenazione più grande (la si legge una volta, dall'alto) e per ciò che non è una stringa.
        /// </summary>
        protected abstract bool IsWholeString(SyntaxNode node, out IReadOnlyList<StringPart> parts);
        /// <summary>Un evento sollevato: <c>RaiseEvent Click(…)</c>, <c>Click?.Invoke(…)</c>. Il nodo è ciò che nomina l'evento; la base controlla che lo sia.</summary>
        protected abstract bool IsRaise(SyntaxNode node, out SyntaxNode eventNode);
        /// <summary>Un gestore agganciato a un evento nel codice: <c>AddHandler x.Click, AddressOf H</c>, <c>x.Click += H</c>.</summary>
        protected abstract bool IsSubscription(SyntaxNode node, out SyntaxNode eventNode, out SyntaxNode handlerNode, out bool certain);
        /// <summary>
        /// Gli eventi che la dichiarazione di un metodo dice di gestire (VB <c>Handles btnCerca.Click, Me.Load</c>): il nodo che
        /// nomina l'evento e quello che nomina chi lo solleva (<c>btnCerca</c>; null per <c>Me</c>).
        /// </summary>
        protected virtual IEnumerable<(SyntaxNode Event, SyntaxNode Container)> HandledEvents(SyntaxNode methodDeclaration) => Enumerable.Empty<(SyntaxNode, SyntaxNode)>();
        /// <summary>VB: l'oggetto del blocco <c>With</c> che contiene il nodo, com'è scritto (<c>.CommandType = …</c> dentro <c>With cmd</c>); null fuori da un With.</summary>
        protected virtual string WithObject(SyntaxNode node) => null;
        /// <summary>VB: il nodo è l'espressione di un <c>With</c> (<c>With New OracleCommand(…)</c>).</summary>
        protected virtual bool IsWithExpression(SyntaxNode node) => false;
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
            var borrowed = new Dictionary<ProjectGroup, List<string>>();
            var referencedGroups = ReferencedGroups(groups, groupByKey, result.Warnings, borrowed);

            var trees = new List<(SyntaxTree Tree, Compilation Compilation)>();
            var compilations = new List<Compilation>();
            var missingFrameworks = new HashSet<string>(StringComparer.Ordinal);
            foreach (var group in InReferenceOrder(groups, g => referencedGroups[g]))
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
                    var missing = new List<string>();
                    foreach (var m in p.MissingLibraries)
                    {
                        var lib = BorrowLibrary(Path.GetFileNameWithoutExtension(m), group, groups, out var from, out var problem);
                        if (lib == null) { missing.Add(Path.GetFileName(m) + (problem ?? "")); continue; }
                        Add(borrowed, group, lib);
                        result.Warnings.Add(UiText.Say(
                            $"{Path.GetFileName(p.File)}: la libreria {Path.GetFileName(m)} non c'è dove la indica (HintPath): si usa quella con lo stesso nome di assembly riferita da {Path.GetFileName(from)}.",
                            $"{Path.GetFileName(p.File)}: the library {Path.GetFileName(m)} is not where its HintPath says: the one with the same assembly name referenced by {Path.GetFileName(from)} is used."));
                    }
                    if (borrowed.TryGetValue(group, out var extra)) references.AddRange(extra.Select(FileReference).Where(r => r != null));
                    if (missing.Count > 0)
                        result.Warnings.Add(UiText.Say(
                            $"{Path.GetFileName(p.File)}: {missing.Count} librerie riferite non ci sono su questo computer ({string.Join(", ", missing)}): ciò che ne usa resta un nome non risolto.",
                            $"{Path.GetFileName(p.File)}: {missing.Count} referenced libraries are not on this computer ({string.Join(", ", missing)}): what uses them stays an unresolved name."));
                    foreach (var other in referencedGroups[group])
                        if (other.Compilation != null) references.Add(other.Compilation.ToMetadataReference());
                }
                group.Compilation = Compile("mde-codegraph" + (p == null ? "" : "-" + Path.GetFileNameWithoutExtension(p.File)), group.Trees, references, p);
                compilations.Add(group.Compilation);
                trees.AddRange(group.Trees.Select(t => (t, group.Compilation)));
            }
            var perimeter = new PerimeterIndex(compilations);
            var compilationOf = trees.ToDictionary(t => t.Tree, t => t.Compilation);
            var models = new Dictionary<SyntaxTree, SemanticModel>();
            SemanticModel ModelOf(SyntaxTree t)
            {
                if (models.TryGetValue(t, out var m)) return m;
                return compilationOf.TryGetValue(t, out var c) ? models[t] = c.GetSemanticModel(t) : null;
            }
            var flow = new RoslynStringFlow(ModelOf, trees.Select(t => t.Tree).ToList());
            var procedureParameters = ProcedureParameters(trees.Select(t => t.Tree).ToList(), ModelOf, flow, ct);

            // 2) Un script per file da emettere.
            var toEmit = new HashSet<string>(request.FilesToEmit.Select(f => f.LogicalPath), StringComparer.Ordinal);
            foreach (var (tree, compilation) in trees)
            {
                ct.ThrowIfCancellationRequested();
                if (!toEmit.Contains(tree.FilePath)) continue;
                var file = request.FilesToEmit.First(f => f.LogicalPath == tree.FilePath);
                try
                {
                    var script = ExtractFile(tree, ModelOf(tree), perimeter, contents[tree.FilePath], flow, procedureParameters);
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

        private CodeScript ExtractFile(SyntaxTree tree, SemanticModel model, PerimeterIndex perimeter, string content, RoslynStringFlow flow, IReadOnlyDictionary<string, ProcedureParameter> procedureParameters)
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
            var ctx = new FileContext(this, script, model, perimeter, flow, procedureParameters);
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

        // ------------------------------------------------------------------ procedure chiamate per nome

        /// <summary>Il nome di una procedura come si scrive in un comando: <c>PROC</c>, <c>PKG.PROC</c>, <c>SCHEMA.PKG.PROC</c>.</summary>
        private static readonly Regex ProcedureName = new(@"^[A-Za-z_][\w$#]*(?:\.[A-Za-z_][\w$#]*){0,2}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        /// <summary>Il nome di un parametro di libreria che riceve il nome della procedura (<c>commandText</c>, <c>spName</c>, <c>sql</c> di Dapper, i nomi italiani…).</summary>
        private static readonly Regex ProcedureParameterName = new(@"^(command|commandText|cmdText|text|testo|sql|storedProcedure|storedProcedureName|procedure|procedureName|procName|spName|sp|procedura|nomeProcedura|nomeProc|nomeSp|comando|testoComando)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        private static string MethodKey(IMethodSymbol m) => m.OriginalDefinition.ToDisplayString(QualifiedFormat);

        /// <summary>Lo stato del tipo di comando per una coppia: <see cref="TypeStoredProcedure"/>, <see cref="TypeMaybe"/>, o la posizione del parametro che lo dà.</summary>
        private const int TypeStoredProcedure = -1;
        private const int TypeMaybe = -2;

        /// <summary>
        /// Un metodo che passa un suo parametro come nome di una procedura (l'helper <c>Esegui(testo, tipo)</c> con
        /// <c>cmd.CommandText = testo</c> e <c>cmd.CommandType = tipo</c>): le coppie (posizione del nome, stato del tipo).
        /// Le frecce le fanno i chiamanti, che scrivono il nome e il tipo.
        /// </summary>
        private sealed class ProcedureParameter
        {
            public string Name;
            public HashSet<(int Name, int Type)> Pairs = new();
        }

        /// <summary>Un punto in cui un corpo chiama una procedura per nome.</summary>
        private sealed class ProcedureSite
        {
            public SyntaxNode At;
            /// <summary><c>StoredProcedure</c> fra gli argomenti di una chiamata (o di un <c>New</c>): gli argomenti, il metodo chiamato.</summary>
            public IReadOnlyList<SyntaxNode> Args;
            public IMethodSymbol Callee;
            /// <summary>Un comando del corpo: l'espressione del nome (<c>CommandText = …</c>, <c>New …Command(…)</c>) e lo stato del tipo.</summary>
            public SyntaxNode Name;
            public int Type = TypeStoredProcedure;
        }

        /// <summary>I metodi con un corpo, di un file: quelli che l'estrazione percorre.</summary>
        private IEnumerable<(IMethodSymbol Method, SyntaxNode Decl)> MethodBodies(SyntaxTree tree, SemanticModel model)
        {
            foreach (var typeDecl in TypeDeclarations(tree.GetRoot()))
            {
                var type = TypeSymbol(model, typeDecl);
                if (type == null) continue;
                foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
                {
                    if (member.IsImplicitlyDeclared) continue;
                    var declared = member.DeclaringSyntaxReferences.FirstOrDefault(r => r.SyntaxTree == tree)?.GetSyntax();
                    if (declared != null) yield return (member, Whole(declared));
                }
            }
        }

        /// <summary><c>CommandType.StoredProcedure</c> scritto (anche qualificato), o la costante 4 di un enum <c>CommandType</c>.</summary>
        private static bool IsStoredProcedure(IOperation op)
        {
            while (op is IConversionOperation c) op = c.Operand;
            if (op == null) return false;
            if (op.Syntax.ToString().Replace(" ", "").EndsWith("CommandType.StoredProcedure", StringComparison.OrdinalIgnoreCase)) return true;
            return op.Type?.Name == "CommandType" && op.ConstantValue.HasValue && Convert.ToInt32(op.ConstantValue.Value) == 4;
        }

        private static int? ParameterOrdinal(IOperation op)
        {
            while (op is IConversionOperation c) op = c.Operand;
            return op is IParameterReferenceOperation p ? p.Parameter.Ordinal : null;
        }

        /// <summary>
        /// Dove un corpo chiama una procedura per nome (12/11/2026):
        /// <list type="bullet">
        /// <item><c>CommandType.StoredProcedure</c> fra gli argomenti di una chiamata o di un <c>New</c>: il nome è fra gli argomenti;</item>
        /// <item><c>….CommandType = CommandType.StoredProcedure</c>: il nome è quello di <c>CommandText</c> o del <c>New …Command(…)</c> dello stesso corpo;</item>
        /// <item><c>….CommandType = tipo</c> con <c>tipo</c> un parametro: come sopra, se i chiamanti passano <c>StoredProcedure</c>.</item>
        /// </list>
        /// </summary>
        private List<ProcedureSite> ProcedureSites(SyntaxNode decl, SemanticModel model, RoslynStringFlow flow)
        {
            var sites = new List<ProcedureSite>();
            foreach (var token in decl.DescendantTokens())
            {
                if (!string.Equals(token.ValueText, "StoredProcedure", StringComparison.OrdinalIgnoreCase)) continue;
                var dot = token.GetPreviousToken();
                if (dot.ValueText != "." || !string.Equals(dot.GetPreviousToken().ValueText, "CommandType", StringComparison.OrdinalIgnoreCase)) continue;
                for (var a = token.Parent; a != null && a != decl; a = a.Parent)
                {
                    IReadOnlyList<SyntaxNode> args = null;
                    if (IsInvocation(a, model, out var shape)) args = shape.ArgumentNodes;
                    else if (IsCreation(a, out _, out var created)) args = created;
                    if (args != null && args.Any(n => n != null && n.Span.Contains(token.Span)))
                    {
                        var callee = model.GetOperation(a) switch { IInvocationOperation i => i.TargetMethod, IObjectCreationOperation o => o.Constructor, _ => null };
                        sites.Add(new ProcedureSite { At = a, Args = args, Callee = callee });
                        break;
                    }
                    if (IsAssignment(a, out var assigned) && assigned.Value != null && assigned.Value.Span.Contains(token.Span)) break;
                }
            }
            // i comandi del corpo, oggetto per oggetto: il CommandType e il CommandText (o il New …Command) dello stesso oggetto —
            // «cmd», il blocco With (nessun nome), la variabile a cui si assegna il New. Un comando senza CommandType è testo SQL.
            var types = new Dictionary<string, SortedSet<int>>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in decl.DescendantNodes())
            {
                if (!IsAssignment(node, out var assigned) || !string.Equals(assigned.Name, "CommandType", StringComparison.OrdinalIgnoreCase) || assigned.Value == null) continue;
                var value = model.GetOperation(assigned.Value);
                int? state = IsStoredProcedure(value) || assigned.Value.ToString().Replace(" ", "").EndsWith("CommandType.StoredProcedure", StringComparison.OrdinalIgnoreCase)
                    ? TypeStoredProcedure : ParameterOrdinal(value);
                if (state == null) continue;   // un altro tipo, o uno che non si sa: quel comando non si dà per procedura
                var key = CommandObject(assigned.Receiver ?? WithObject(node) ?? InitializedObject(node, model));
                if (!types.TryGetValue(key, out var set)) types[key] = set = new SortedSet<int>();
                set.Add(state.Value);
            }
            if (types.Count == 0) return sites;
            foreach (var node in decl.DescendantNodes())
            {
                SyntaxNode name = null;
                string target = null;
                if (IsAssignment(node, out var assigned) && string.Equals(assigned.Name, "CommandText", StringComparison.OrdinalIgnoreCase))
                {
                    name = assigned.Value;
                    target = assigned.Receiver ?? WithObject(node) ?? InitializedObject(node, model);
                }
                else if (IsCreation(node, out var createdName, out var createdArguments) && createdArguments.Count > 0)
                {
                    // il testo del comando: il parametro del costruttore che diventa CommandText (CommandTextParameter); senza
                    // costruttore noto, il primo argomento di un tipo che si chiama …Command
                    if (model.GetOperation(node) is IObjectCreationOperation { Constructor: not null } created)
                    {
                        var ordinal = CommandTextParameter(created.Constructor, model, flow, 0);
                        name = ordinal == null ? null : created.Arguments.FirstOrDefault(x => x.Parameter?.Ordinal == ordinal && x.ArgumentKind != ArgumentKind.DefaultValue)?.Value.Syntax;
                    }
                    else if ((createdName ?? (model.GetTypeInfo(node).Type as INamedTypeSymbol)?.Name)?.EndsWith("Command", StringComparison.OrdinalIgnoreCase) == true)
                        name = createdArguments[0];
                    target = AssignedTo(node, model) ?? (IsWithExpression(node) ? node.ToString() : null);
                }
                if (name == null || !types.TryGetValue(CommandObject(target), out var states)) continue;
                foreach (var type in states) sites.Add(new ProcedureSite { At = node, Name = name, Type = type });
            }
            return sites;
        }

        /// <summary>
        /// Il parametro di un costruttore che diventa il testo del comando. Nel codice analizzato si legge il corpo: un
        /// <c>CommandText = parametro</c>, o un <c>MyBase.New(…)</c>/<c>: base(…)</c> che lo passa al costruttore di sopra. In una
        /// libreria, il parametro stringa che si chiama come tale (<c>cmdText</c> di <c>SqlCommand</c>, <c>OracleCommand</c>,
        /// <c>OleDbCommand</c>). Null se nessun parametro lo diventa.
        /// </summary>
        private int? CommandTextParameter(IMethodSymbol ctor, SemanticModel model, RoslynStringFlow flow, int depth)
        {
            if (ctor == null || depth > 8) return null;
            var syntax = ctor.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (syntax == null)
            {
                var named = ctor.Parameters.Where(p => p.Type?.SpecialType == SpecialType.System_String && ProcedureParameterName.IsMatch(p.Name)).ToList();
                return named.Count == 1 ? named[0].Ordinal : null;
            }
            var ctorModel = syntax.SyntaxTree == model.SyntaxTree ? model : flow.ModelOf(syntax.SyntaxTree);
            if (ctorModel == null) return null;
            var body = Whole(syntax);
            foreach (var node in body.DescendantNodesAndSelf())
            {
                if (IsAssignment(node, out var assigned) && string.Equals(assigned.Name, "CommandText", StringComparison.OrdinalIgnoreCase) && assigned.Value != null
                    && flow.Of(assigned.Value, ctorModel) is { Parameters.Count: 1 } v && v.Known.Count == 0 && !v.Unknown)
                    return v.Parameters.Min;
                if (ctorModel.GetOperation(node) is IInvocationOperation { TargetMethod.MethodKind: MethodKind.Constructor } chained && !ReferenceEquals(chained.TargetMethod, ctor)
                    && CommandTextParameter(chained.TargetMethod, ctorModel, flow, depth + 1) is { } up)
                {
                    var arg = chained.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == up);
                    if (arg != null && ParameterOrdinal(arg.Value) is { } mine) return mine;
                }
            }
            return null;
        }

        /// <summary>Il nome dell'oggetto comando com'è scritto, senza <c>Me.</c>/<c>this.</c>; "" per il blocco <c>With</c> o un inizializzatore.</summary>
        private static string CommandObject(string receiver)
        {
            if (string.IsNullOrWhiteSpace(receiver)) return string.Empty;
            var r = receiver.Replace(" ", "");
            foreach (var prefix in new[] { "Me.", "this.", "MyBase.", "base." })
                if (r.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) r = r[prefix.Length..];
            return r;
        }

        /// <summary>Per un'assegnazione dentro un inizializzatore (<c>New X With {.CommandType = …}</c>, <c>new X { CommandType = … }</c>): la variabile che lo riceve.</summary>
        private static string InitializedObject(SyntaxNode assignment, SemanticModel model)
        {
            for (var n = assignment.Parent; n != null && n.Span.Length < assignment.Span.Length + 2000; n = n.Parent)
                if (model.GetOperation(n) is IObjectCreationOperation) return AssignedTo(n, model);
            return null;
        }

        /// <summary>La variabile (o il campo) a cui si assegna un <c>New</c>: <c>Dim cmd As New …</c>, <c>var cmd = new …</c>, <c>cmd = New …</c>; null per un <c>With New …</c>.</summary>
        private static string AssignedTo(SyntaxNode creation, SemanticModel model)
        {
            for (var op = model.GetOperation(creation)?.Parent; op != null; op = op.Parent)
            {
                switch (op)
                {
                    case IConversionOperation: case IVariableInitializerOperation: continue;
                    case IVariableDeclaratorOperation d: return d.Symbol.Name;
                    case IVariableDeclarationOperation vd when vd.Declarators.Length == 1: return vd.Declarators[0].Symbol.Name;
                    case ISimpleAssignmentOperation a: return a.Target.Syntax.ToString();
                    default: return null;
                }
            }
            return null;
        }

        /// <summary>
        /// L'argomento col nome della procedura, per una chiamata a un metodo che NON è nel codice analizzato (una libreria): il
        /// parametro che si chiama come un nome di procedura (<c>commandText</c>, <c>spName</c>…), se è uno solo; altrimenti il
        /// primo argomento che fra i suoi valori ha un nome col punto (<c>PKG.PROC</c>), altrimenti il primo argomento stringa
        /// — una deduzione. Null se nessun argomento è una stringa.
        /// </summary>
        /// <summary>
        /// Quale argomento porta il nome della procedura in una chiamata con <c>CommandType.StoredProcedure</c> a un metodo di cui non
        /// si legge il corpo. <see cref="Argument"/> null = non si sa, e <see cref="Unknown"/> dice perché; <see cref="ByShape"/> =
        /// scelto dalla forma del valore, non dal parametro (il metodo non è risolto).
        /// </summary>
        private sealed record LibraryChoice(SyntaxNode Argument, bool ByShape, string Unknown);

        /// <summary>
        /// L'argomento col nome della procedura, per una chiamata a un metodo che NON è nel codice analizzato (una libreria):
        /// <list type="bullet">
        /// <item>metodo risolto: il parametro stringa che si chiama come un nome di procedura (<c>commandText</c>, <c>spName</c>…),
        /// se è uno solo; nessuno o più d'uno = non si sa;</item>
        /// <item>metodo non risolto (la sua libreria non è fra i riferimenti): i nomi dei parametri non si conoscono. Se fra gli
        /// argomenti che possono essere una stringa uno solo ha valori tutti a forma di nome di procedura (<c>PROC</c>,
        /// <c>PKG.PROC</c>), è lui, dalla forma (12/11/2026, rilevazione dal cliente: prima si prendeva il primo argomento
        /// stringa, che era la connessione); altrimenti non si sa.</item>
        /// </list>
        /// </summary>
        private static LibraryChoice LibraryProcedureArgument(ProcedureSite site, SemanticModel model, RoslynStringFlow flow)
        {
            var op = model.GetOperation(site.At);
            var args = op switch { IInvocationOperation i => i.Arguments, IObjectCreationOperation o => o.Arguments, _ => default };
            var method = op switch { IInvocationOperation i => i.TargetMethod, IObjectCreationOperation o => o.Constructor, _ => null };
            if (!args.IsDefault && method != null)
            {
                var named = args.Where(a => a.Parameter != null && a.Parameter.Type?.SpecialType == SpecialType.System_String && ProcedureParameterName.IsMatch(a.Parameter.Name)).ToList();
                if (named.Count == 1) return new LibraryChoice(named[0].Value.Syntax, false, null);
                var strings = method.Parameters.Where(p => p.Type?.SpecialType == SpecialType.System_String).Select(p => p.Name).ToList();
                return new LibraryChoice(null, false, named.Count == 0
                    ? $"{method.Name}: none of its string parameters ({string.Join(", ", strings)}) is named like a procedure name"
                    : $"{method.Name}: more than one string parameter is named like a procedure name ({string.Join(", ", named.Select(a => a.Parameter.Name))})");
            }
            var shaped = new List<SyntaxNode>();
            foreach (var a in site.Args)
            {
                if (a == null || IsStoredProcedure(model.GetOperation(a))) continue;
                var type = model.GetTypeInfo(a).Type;
                if (type != null && type.TypeKind != TypeKind.Error && type.SpecialType is not (SpecialType.System_String or SpecialType.System_Object)) continue;
                var v = flow.Of(a, model);
                if (v.Known.Count > 0 && !v.Unknown && v.Parameters.Count == 0 && !v.Combined && v.Known.All(k => ProcedureName.IsMatch(k.Trim()))) shaped.Add(a);
            }
            var callee = site.At is { } at ? CalleeText(at) : "the method";
            if (shaped.Count == 1) return new LibraryChoice(shaped[0], true, null);
            return new LibraryChoice(null, false, shaped.Count == 0
                ? $"{callee} is not resolved (its library is not among the references): which argument is the procedure name is not known"
                : $"{callee} is not resolved (its library is not among the references) and {shaped.Count} arguments look like a procedure name");
        }

        /// <summary>Il metodo chiamato com'è scritto, senza argomenti: <c>OleDbHelper.ExecuteDataset</c>.</summary>
        private static string CalleeText(SyntaxNode call)
        {
            var text = call.ToString();
            var paren = text.IndexOf('(');
            var head = (paren > 0 ? text[..paren] : text).Trim();
            if (head.StartsWith("New ", StringComparison.OrdinalIgnoreCase) || head.StartsWith("new ", StringComparison.Ordinal)) head = head[4..].Trim();
            return head.Length > 80 ? head[..80] + "…" : head;
        }

        /// <summary>
        /// Il giro preliminare, su tutti i file: i metodi che passano un loro parametro come nome di una procedura, anche
        /// attraverso altri metodi (<c>Apri(n)</c> → <c>Esegui(n, CommandType.StoredProcedure)</c>), fino al punto fisso.
        /// </summary>
        private Dictionary<string, ProcedureParameter> ProcedureParameters(IReadOnlyList<SyntaxTree> trees, Func<SyntaxTree, SemanticModel> modelOf, RoslynStringFlow flow, CancellationToken ct)
        {
            var result = new Dictionary<string, ProcedureParameter>(StringComparer.Ordinal);
            bool Add(IMethodSymbol m, int name, int type)
            {
                var key = MethodKey(m);
                if (!result.TryGetValue(key, out var p)) result[key] = p = new ProcedureParameter { Name = m.Name };
                return p.Pairs.Add((name, type));
            }
            var withMarker = trees.Where(t => t.GetText().ToString().IndexOf("CommandType", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            // 1) i comandi dei corpi: il nome viene da un parametro
            var callSites = new List<(IMethodSymbol Method, ProcedureSite Site, SemanticModel Model)>();
            foreach (var tree in withMarker)
            {
                ct.ThrowIfCancellationRequested();
                var model = modelOf(tree);
                foreach (var (method, decl) in MethodBodies(tree, model))
                    foreach (var site in ProcedureSites(decl, model, flow))
                    {
                        if (site.Name == null) { callSites.Add((method, site, model)); continue; }
                        foreach (var p in flow.Of(site.Name, model).Parameters) Add(method, p, site.Type);
                    }
            }
            // 2) StoredProcedure passato a un metodo di libreria: il nome viene da un parametro
            foreach (var (method, site, model) in callSites)
            {
                if (site.Callee != null && result.ContainsKey(MethodKey(site.Callee))) continue;   // lo fa il punto 3
                if (LibraryProcedureArgument(site, model, flow) is { Argument: { } arg, ByShape: false })
                    foreach (var p in flow.Of(arg, model).Parameters) Add(method, p, TypeStoredProcedure);
            }
            // 3) le chiamate a quei metodi, fino al punto fisso
            for (var round = 0; round < 32 && result.Count > 0; round++)
            {
                var names = new HashSet<string>(result.Values.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
                var changed = false;
                foreach (var tree in trees)
                {
                    ct.ThrowIfCancellationRequested();
                    var model = modelOf(tree);
                    foreach (var token in tree.GetRoot().DescendantTokens())
                    {
                        if (!names.Contains(token.ValueText)) continue;
                        var call = token.Parent?.AncestorsAndSelf().Take(4).Select(n => model.GetOperation(n)).OfType<IInvocationOperation>().FirstOrDefault();
                        if (call == null || !result.TryGetValue(MethodKey(call.TargetMethod), out var target)) continue;
                        if (model.GetEnclosingSymbol(token.SpanStart) is not IMethodSymbol caller) continue;
                        foreach (var (name, type) in target.Pairs.ToList())
                        {
                            var nameArg = call.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == name);
                            if (nameArg == null) continue;
                            var state = TypeState(call, type);
                            if (state == null) continue;   // il chiamante passa un altro tipo: non è una procedura
                            foreach (var p in flow.Of(nameArg.Value).Parameters) changed |= Add(caller, p, state.Value);
                        }
                    }
                }
                if (!changed) break;
            }
            return result;
        }

        /// <summary>
        /// Lo stato del tipo di comando in una chiamata, per una coppia: <see cref="TypeStoredProcedure"/> (scritto, o la coppia non
        /// ha condizione), la posizione di un parametro del chiamante che lo passa, <see cref="TypeMaybe"/> se non si sa; null se il
        /// chiamante passa un altro tipo (<c>CommandType.Text</c>).
        /// </summary>
        private static int? TypeState(IInvocationOperation call, int type)
        {
            if (type == TypeStoredProcedure || type == TypeMaybe) return type;
            var typeArg = call.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == type);
            if (typeArg == null) return TypeMaybe;
            if (IsStoredProcedure(typeArg.Value)) return TypeStoredProcedure;
            if (ParameterOrdinal(typeArg.Value) is { } ordinal) return ordinal;
            var v = typeArg.Value; while (v is IConversionOperation c) v = c.Operand;
            return v.ConstantValue.HasValue ? null : TypeMaybe;
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
            private readonly RoslynStringFlow _flow;
            private readonly IReadOnlyDictionary<string, ProcedureParameter> _procedureParameters;

            public FileContext(RoslynExtractorBase extractor, CodeScript script, SemanticModel model, PerimeterIndex perimeter,
                RoslynStringFlow flow, IReadOnlyDictionary<string, ProcedureParameter> procedureParameters)
            {
                _x = extractor; Script = script; _model = model; _perimeter = perimeter; _flow = flow; _procedureParameters = procedureParameters;
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

            public void Edge(string source, string target, string relation, string confidence, int? line, string guard = null)
            {
                if (source == null || target == null || source == target) return;
                if (!_edges.Add($"{source}|{target}|{relation}|{line}")) return;
                Script.Edges.Add(new ScriptEdge { SourceId = source, TargetId = target, Relation = relation, Confidence = confidence, Line = line, Guard = guard });
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
                    if (_x.IsWholeString(node, out var parts)) Sql(ownerId, node, parts);
                    if (_x.IsInvocation(node, _model, out var shape))
                    {
                        var info = _model.GetSymbolInfo(node);
                        var line = Line(node);
                        if (_x.IsRaise(node, out var raised) && _model.GetSymbolInfo(raised).Symbol is IEventSymbol raisedEvent)
                            Edge(ownerId, IdOf(raisedEvent), "calls", "extracted", line);
                        if (shape.Callee != null && AddCallSites(ownerId, line, shape.Callee, shape.Receiver, shape.Arguments, shape.ArgumentNodes, TypesOf(shape)) is { } site)
                        {
                            sites[node] = site;
                            if (shape.ReceiverNode != null) chained.Add((site, shape.ReceiverNode));
                        }
                        if (info.Symbol is IMethodSymbol m)
                        {
                            Call(ownerId, m, line);
                            ProceduresThrough(ownerId, node, m, line);
                        }
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
                            AddCallSites(ownerId, line, createdName, null, null, createdArguments, null);
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
                        if (named && assigned.Value != null)
                            foreach (var value in NameValues(_flow.Of(assigned.Value, _model)))
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
                StoredProcedures(decl, ownerId);
            }

            /// <summary>
            /// La condizione scritta sulla freccia verso una procedura: attraverso quale variabile o parametro passa il nome, e se è
            /// uno di più valori (<c>(1 of 8)</c>) o uno dei valori noti mentre altri non si sanno.
            /// </summary>
            private static string NameGuard(string core, int count, StringValues v, string condition)
            {
                var text = core;
                if (!v.IsComplete) text = (text == null ? "" : text + " ") + $"(one of {count} known values; others: {v.Reason ?? "from the callers"})";
                else if (count > 1) text = (text == null ? "" : text + " ") + $"(1 of {count})";
                if (condition != null) text = (text == null ? "" : text + ", ") + condition;
                return text;
            }

            /// <summary>
            /// Le frecce verso le procedure di un nome con i suoi valori: una per ogni valore a forma di nome (certa o, se il tipo
            /// non si sa, <c>ambiguous</c>), e il nodo <c>?</c> con il motivo per quello che il codice non dice. Un valore che non
            /// è un nome di procedura lo dice anche lui.
            /// </summary>
            private void ProcedureEdges(string ownerId, int line, StringValues v, Func<string, string> core, bool certain, string condition, bool fromCallersHere)
            {
                var names = v.Known.Select(k => k.Trim()).Where(k => ProcedureName.IsMatch(k)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var combined = v.Combined ? "combination of pieces not verified" : null;
                foreach (var name in names)
                    SqlEdge(ownerId, name, SqlAccess.Call, false, line, NameGuard(core(name), names.Count, v, combined == null ? condition : condition == null ? combined : condition + ", " + combined),
                        certain && !v.Combined ? "inferred" : "ambiguous");
                if (v.Known.Any(k => !ProcedureName.IsMatch(k.Trim())))
                    SqlEdge(ownerId, null, SqlAccess.Call, true, line, NameGuard(null, 0, StringValues.Of(""), $"a value that is not a procedure name{(condition == null ? "" : ", " + condition)}"));
                if (v.Unknown)
                    SqlEdge(ownerId, null, SqlAccess.Call, true, line, (v.Via == null ? "" : v.Via + ": ") + "the name is decided at run time (" + v.Reason + ")" + (condition == null ? "" : ", " + condition));
                if (v.Parameters.Count > 0 && fromCallersHere)
                    SqlEdge(ownerId, null, SqlAccess.Call, true, line, (v.Via == null ? "" : v.Via + ": ") + "the name comes from the callers" + (condition == null ? "" : ", " + condition));
            }

            /// <summary>
            /// Le procedure chiamate per nome in questo corpo (<see cref="ProcedureSites"/>), con tutti i valori che il nome può avere
            /// in quel punto (<see cref="RoslynStringFlow"/>). Una chiamata a un metodo del codice analizzato che passa il nome a un
            /// comando la fa <see cref="ProceduresThrough"/>; qui restano i comandi del corpo e i metodi di libreria.
            /// </summary>
            private void StoredProcedures(SyntaxNode decl, string ownerId)
            {
                foreach (var site in _x.ProcedureSites(decl, _model, _flow))
                {
                    var line = Line(site.At);
                    if (site.Name != null)
                    {
                        var v = _flow.Of(site.Name, _model);
                        var condition = site.Type >= 0 ? $"when the caller passes CommandType.StoredProcedure" : null;
                        ProcedureEdges(ownerId, line, v, name => v.Via == null ? null : $"{v.Via} = \"{name}\"", site.Type != TypeMaybe, condition, fromCallersHere: true);
                        continue;
                    }
                    if (site.Callee != null && _procedureParameters.ContainsKey(MethodKey(site.Callee))) continue;
                    var choice = LibraryProcedureArgument(site, _model, _flow);
                    if (choice.Argument == null)
                    {
                        SqlEdge(ownerId, null, SqlAccess.Call, true, line, choice.Unknown);
                        continue;
                    }
                    var values = _flow.Of(choice.Argument, _model);
                    var shape = choice.ByShape ? "library not resolved: the only argument shaped like a procedure name" : null;
                    ProcedureEdges(ownerId, line, values, name => values.Via == null ? null : $"{values.Via} = \"{name}\"", certain: !choice.ByShape, condition: shape, fromCallersHere: true);
                }
            }

            /// <summary>
            /// La chiamata a un metodo del codice analizzato che passa un suo parametro come nome di una procedura
            /// (<see cref="ProcedureParameters"/>): la freccia va da chi chiama, che scrive il nome — <c>Esegui(testo = "PKG.X")</c>.
            /// Se il tipo del comando lo passa il chiamante, conta quello che passa: <c>StoredProcedure</c> sì, un altro tipo no, un
            /// valore che non si sa dà una freccia <c>ambiguous</c>. Un nome che arriva a sua volta da un parametro lo dicono i
            /// chiamanti di questo metodo.
            /// </summary>
            private void ProceduresThrough(string ownerId, SyntaxNode node, IMethodSymbol method, int line)
            {
                if (_procedureParameters.Count == 0 || !_procedureParameters.TryGetValue(MethodKey(method), out var target)) return;
                if (_model.GetOperation(node) is not IInvocationOperation call) return;
                foreach (var (nameOrdinal, type) in target.Pairs)
                {
                    var arg = call.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == nameOrdinal);
                    if (arg == null) continue;
                    var state = TypeState(call, type);
                    if (state == null) continue;
                    var v = _flow.Of(arg.Value);
                    var parameter = arg.Parameter.Name;
                    var condition = state == TypeMaybe ? "if the command type is StoredProcedure (decided at run time)"
                        : state >= 0 ? "when the caller passes CommandType.StoredProcedure" : null;
                    ProcedureEdges(ownerId, line, v, name => $"{method.Name}({parameter} = \"{name}\")", state != TypeMaybe, condition, fromCallersHere: false);
                }
            }

            /// <summary>
            /// Un'espressione che può valere una stringa: di tipo stringa o Object, o di un tipo che Roslyn non conosce (una libreria
            /// che manca). Solo un tipo conosciuto che non è una stringa (un numero, una data, un enum) si scarta.
            /// </summary>
            private bool MayBeString(SyntaxNode node)
            {
                var type = _model.GetTypeInfo(node).Type;
                return type == null || type.TypeKind == TypeKind.Error || type.SpecialType is SpecialType.System_String or SpecialType.System_Object;
            }

            /// <summary>I valori a forma di nome (senza spazi) di una stringa, per i punti di chiamata letti dalle regole.</summary>
            private static IEnumerable<string> NameValues(StringValues v) => v.Combined ? Enumerable.Empty<string>() : v.Known.Where(CodeCallSites.IsNameLike);

            /// <summary>
            /// I punti di chiamata con le stringhe degli argomenti, anche quelle passate per una variabile o una costante: un argomento
            /// che vale una di più stringhe (rami diversi) dà un punto per ciascuna, così le regole le leggono tutte.
            /// </summary>
            private ScriptCallSite AddCallSites(string ownerId, int line, string callee, string receiver, IReadOnlyList<string> written, IReadOnlyList<SyntaxNode> nodes, List<ScriptTypeArgument> types)
            {
                var count = Math.Max(written?.Count ?? 0, nodes?.Count ?? 0);
                var single = new List<string>(count);
                int multi = -1; List<string> alternatives = null;
                for (var i = 0; i < count; i++)
                {
                    var text = written != null && i < written.Count ? written[i] : null;
                    if (text == null && nodes != null && i < nodes.Count && nodes[i] != null && MayBeString(nodes[i]))
                    {
                        var values = NameValues(_flow.Of(nodes[i], _model)).ToList();
                        if (values.Count == 1) text = values[0];
                        else if (values.Count > 1 && multi < 0) { multi = i; alternatives = values; }
                    }
                    single.Add(text);
                }
                var first = _callSites.Add(ownerId, line, callee, receiver, single, types: types);
                if (multi >= 0)
                    foreach (var alternative in alternatives)
                    {
                        var args = single.ToList(); args[multi] = alternative;
                        var site = _callSites.Add(ownerId, line, callee, receiver, args, types: types);
                        first ??= site;
                    }
                return first;
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

            /// <summary>
            /// L'SQL scritto in una stringa (11/11/2026): il simbolo che la contiene legge o scrive le tabelle che nomina e chiama
            /// le procedure, dovunque la stringa vada a finire (una variabile, un parametro, un <c>StringBuilder</c>). Archi
            /// <c>inferred</c> verso nodi esterni del linguaggio <see cref="SqlLanguage"/>, gli stessi per C# e Visual Basic; un nome
            /// che arriva da una variabile è il nodo <c>?</c>, <c>ambiguous</c>.
            /// </summary>
            private void Sql(string ownerId, SyntaxNode node, IReadOnlyList<StringPart> parts)
            {
                // un pezzo che viene da una variabile dà un testo per ogni suo valore, più uno col buco se non si sanno tutti (12/11/2026)
                const int MaxVariants = 64;
                var variants = new List<(StringBuilder Text, string Guard)> { (new StringBuilder(), null) };
                var chosen = new List<List<string>> { new() };   // per variante, i valori scelti dei pezzi con più valori
                foreach (var p in parts)
                {
                    if (p.Text != null) { foreach (var x in variants) x.Text.Append(p.Text); }
                    else
                    {
                        // un pezzo che non è una stringa (un numero, una data) non dà nomi: resta un buco, senza calcoli
                        var v = p.Hole == null || !MayBeString(p.Hole) ? StringValues.Dynamic("not a string") : _flow.Of(p.Hole, _model);
                        var options = v.Known.Count + (v.IsComplete ? 0 : 1);
                        if (v.Known.Count > 0 && variants.Count * options <= MaxVariants)
                        {
                            var expanded = new List<(StringBuilder, string)>();
                            var expandedChosen = new List<List<string>>();
                            for (var i = 0; i < variants.Count; i++)
                            {
                                var (text, guard) = variants[i];
                                foreach (var k in v.Known)
                                {
                                    expanded.Add((new StringBuilder(text.ToString()).Append(k),
                                        guard ?? (v.Via != null && options > 1 ? NameGuard($"{v.Via} = \"{k}\"", v.Known.Count, v, null) : null)));
                                    expandedChosen.Add(options > 1 ? chosen[i].Append(k).ToList() : chosen[i]);
                                }
                                if (!v.IsComplete) { expanded.Add((new StringBuilder(text.ToString()).Append(EmbeddedSql.Hole), guard)); expandedChosen.Add(chosen[i]); }
                            }
                            variants = expanded;
                            chosen = expandedChosen;
                        }
                        else foreach (var x in variants) x.Text.Append(EmbeddedSql.Hole);
                    }
                    if (variants[0].Text.Length > EmbeddedSql.MaxLength) return;
                }
                var line = Line(node);
                for (var i = 0; i < variants.Count; i++)
                {
                    var (text, guard) = variants[i];
                    foreach (var o in EmbeddedSql.Analyze(text.ToString()))
                    {
                        // un nome fatto da due o più pezzi con più valori: la combinazione può non succedere mai
                        var combined = !o.Dynamic && chosen[i].Count(k => k.Length > 0 && o.Name.Contains(k.Trim(), StringComparison.OrdinalIgnoreCase)) >= 2;
                        SqlEdge(ownerId, o.Name, o.Access, o.Dynamic, line, o.Dynamic ? null : combined ? (guard == null ? "" : guard + ", ") + "combination of pieces not verified" : guard,
                            combined ? "ambiguous" : null);
                    }
                }
            }

            /// <summary>L'arco verso il nodo esterno SQL di una tabella o di una procedura (o verso «?» se il nome è una variabile).</summary>
            private void SqlEdge(string ownerId, string name, SqlAccess access, bool dynamic, int line, string guard = null, string confidence = null)
            {
                if (dynamic) name = "?";
                var key = dynamic ? "dynamic:?" : (access == SqlAccess.Call ? "symbol:" : "table:") + name.ToUpperInvariant();
                var id = RoslynCSharpExtractor.StableId(SqlLanguage, "external", key);
                if (_externals.Add(id)) Script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = name.Split('.').Last(), QualifiedName = name, Language = SqlLanguage });
                Edge(ownerId, id, access switch { SqlAccess.Write => "writes", SqlAccess.Call => "calls", _ => "reads" }, dynamic ? "ambiguous" : confidence ?? "inferred", line, guard);
            }

            /// <summary>Una stringa scritta o una costante che vale una stringa; null per tutto il resto.</summary>
            private string ConstantString(SyntaxNode node)
            {
                if (node == null) return null;
                var constant = _model.GetConstantValue(node);
                return constant.HasValue ? constant.Value as string : null;
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
