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

        protected abstract SyntaxTree Parse(string text, string path);
        protected abstract Compilation Compile(IReadOnlyList<SyntaxTree> trees, IReadOnlyList<MetadataReference> references);
        /// <summary>Le dichiarazioni di tipo del file, col nodo INTERO (le sue righe sono quelle del simbolo).</summary>
        protected abstract IEnumerable<SyntaxNode> TypeDeclarations(SyntaxNode root);
        protected virtual INamedTypeSymbol TypeSymbol(SemanticModel model, SyntaxNode typeDeclaration) => model.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
        /// <summary>Dal nodo che Roslyn dà per un membro al nodo che ne contiene anche il corpo (in VB sono due nodi diversi).</summary>
        protected virtual SyntaxNode Whole(SyntaxNode memberDeclaration) => memberDeclaration;
        protected abstract bool IsInvocation(SyntaxNode node, SemanticModel model, out InvocationShape shape);
        protected abstract bool IsCreation(SyntaxNode node);
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

            // 1) La compilazione: tutti i file della cartella, così i nomi si risolvono anche fra file diversi.
            var contents = new Dictionary<string, string>(StringComparer.Ordinal);
            var trees = new List<SyntaxTree>(request.AllFiles.Count);
            foreach (var f in request.AllFiles)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = request.ReadText(f); }
                catch (Exception ex) { result.Warnings.Add(UiText.Say($"{f.LogicalPath}: non leggibile ({ex.Message})", $"{f.LogicalPath}: cannot be read ({ex.Message})")); continue; }
                contents[f.LogicalPath] = text;
                trees.Add(Parse(text, f.LogicalPath));
            }
            var compilation = Compile(trees, RuntimeReferences());
            var perimeter = new PerimeterIndex(compilation);

            // 2) Un script per file da emettere.
            var toEmit = new HashSet<string>(request.FilesToEmit.Select(f => f.LogicalPath), StringComparer.Ordinal);
            foreach (var tree in trees)
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

        /// <summary>I tipi dichiarati nella compilazione, per il dispatch per interfaccia e per sapere chi è in perimetro.</summary>
        private sealed class PerimeterIndex
        {
            private readonly List<INamedTypeSymbol> _types = new();
            public PerimeterIndex(Compilation compilation)
            {
                var stack = new Stack<INamespaceOrTypeSymbol>();
                stack.Push(compilation.Assembly.GlobalNamespace);
                while (stack.Count > 0)
                {
                    var s = stack.Pop();
                    foreach (var m in s.GetMembers())
                    {
                        if (m is INamespaceSymbol ns) stack.Push(ns);
                        else if (m is INamedTypeSymbol t) { _types.Add(t); stack.Push(t); }
                    }
                }
            }
            public bool IsInPerimeter(ISymbol s) => s.Locations.Any(l => l.IsInSource);
            public IEnumerable<ISymbol> ImplementationsOf(ISymbol interfaceMember)
            {
                var iface = interfaceMember.ContainingType;
                foreach (var t in _types)
                {
                    if (t.TypeKind == TypeKind.Interface || !t.AllInterfaces.Contains(iface, SymbolEqualityComparer.Default)) continue;
                    var impl = t.FindImplementationForInterfaceMember(interfaceMember);
                    if (impl != null && IsInPerimeter(impl)) yield return impl;
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
                        if (shape.Callee != null && _callSites.Add(ownerId, line, shape.Callee, shape.Receiver, shape.Arguments, types: TypesOf(shape)) is { } site)
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
                    else if (_x.IsCreation(node))
                    {
                        var info = _model.GetSymbolInfo(node);
                        var line = Line(node);
                        if (info.Symbol is IMethodSymbol ctor && ctor.ContainingType != null)
                            Edge(ownerId, IdOf(ctor.ContainingType), "instantiates", "extracted", line);
                        else if (_model.GetTypeInfo(node).Type is INamedTypeSymbol t && t.TypeKind != TypeKind.Error)
                            Edge(ownerId, IdOf(t), "instantiates", "extracted", line);
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
