using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'estrattore C# (D1): una <see cref="CSharpCompilation"/> sui soli file della cartella più le reference del runtime
    /// che MdExplorer si porta dietro — niente <c>.csproj</c>, MSBuild né SDK sulla macchina dell'utente (provato il
    /// 09/10/2026). Il <see cref="SemanticModel"/> risolve davvero: tipi, overload, dispatch per interfaccia.
    /// <list type="bullet">
    /// <item>Simboli: class, interface, record, struct, enum, method, constructor, property, field; parametri con tipo e
    /// direzione; firma per esteso da <see cref="SymbolDisplayFormat"/>.</item>
    /// <item>Archi <c>extracted</c>: contains, inherits, implements, references (tipi di campi, proprietà, parametri,
    /// ritorni), instantiates, calls (risolte). <c>inferred</c>: la chiamata su un'interfaccia verso ogni implementazione
    /// nel perimetro. <c>ambiguous</c>: candidati di Roslyn quando non sa scegliere, o il testo della chiamata come
    /// nodo external quando non trova niente (reference NuGet assenti).</item>
    /// <item>Un simbolo fuori perimetro (runtime, pacchetti) è un nodo <c>external</c> col suo nome qualificato.</item>
    /// </list>
    /// Gli id sono stabili rispetto alle righe: hash di linguaggio, kind e nome qualificato (con i tipi dei parametri per i
    /// metodi); spostare un metodo non cambia il suo id, rinominarlo o cambiarne i parametri sì.
    /// </summary>
    public sealed class RoslynCSharpExtractor : ICodeExtractor
    {
        public string LanguageId => CodeLanguages.CSharp;
        public string ExtractorName => "roslyn";
        public string ExtractorVersion => "1";
        public string Availability => "ready";

        private readonly ILogger<RoslynCSharpExtractor> _logger;

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

        public RoslynCSharpExtractor(ILogger<RoslynCSharpExtractor> logger) { _logger = logger; }

        public CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var result = new CodeExtractionResult();
            if (request.FilesToEmit.Count == 0) return result;

            // 1) La compilazione: tutti i file della cartella, così i nomi si risolvono anche fra file diversi.
            var contents = new Dictionary<string, string>(StringComparer.Ordinal);
            var trees = new List<SyntaxTree>(request.AllFiles.Count);
            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None);
            foreach (var f in request.AllFiles)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = File.ReadAllText(f.AbsolutePath, Encoding.UTF8); }
                catch (Exception ex) { result.Warnings.Add(UiText.Say($"{f.LogicalPath}: non leggibile ({ex.Message})", $"{f.LogicalPath}: cannot be read ({ex.Message})")); continue; }
                contents[f.LogicalPath] = text;
                trees.Add(CSharpSyntaxTree.ParseText(text, parseOptions, path: f.LogicalPath));
            }
            var compilation = CSharpCompilation.Create("mde-codegraph", trees, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
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
            var ctx = new FileContext(script, model, perimeter);
            var root = tree.GetRoot();

            foreach (var typeDecl in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                var type = model.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (type == null) continue;
                var typeId = ctx.Declare(type, typeDecl, KindOf(type), signature: type.ToDisplayString(SignatureFormat), returnType: null);

                if (type.ContainingType != null) ctx.Edge(ctx.IdOf(type.ContainingType), typeId, "contains", "extracted", Line(typeDecl));
                if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object && type.BaseType.SpecialType != SpecialType.System_ValueType && type.BaseType.SpecialType != SpecialType.System_Enum)
                    ctx.Edge(typeId, ctx.IdOf(type.BaseType), type.TypeKind == TypeKind.Interface ? "inherits" : "inherits", "extracted", Line(typeDecl));
                foreach (var iface in type.Interfaces)
                    ctx.Edge(typeId, ctx.IdOf(iface), type.TypeKind == TypeKind.Interface ? "inherits" : "implements", "extracted", Line(typeDecl));

                foreach (var member in type.GetMembers())
                {
                    if (member.IsImplicitlyDeclared) continue;
                    var decl = member.DeclaringSyntaxReferences.FirstOrDefault(r => r.SyntaxTree == tree)?.GetSyntax();
                    if (decl == null) continue; // partial: dichiarato in un altro file
                    switch (member)
                    {
                        case IMethodSymbol m when m.MethodKind is MethodKind.Ordinary or MethodKind.Constructor or MethodKind.StaticConstructor:
                            {
                                var kind = m.MethodKind == MethodKind.Ordinary ? "method" : "constructor";
                                var id = ctx.Declare(m, decl, kind, m.ToDisplayString(SignatureFormat), m.MethodKind == MethodKind.Ordinary ? m.ReturnType.ToDisplayString(TypeFormat) : null, typeId);
                                ctx.Edge(typeId, id, "contains", "extracted", Line(decl));
                                if (m.MethodKind == MethodKind.Ordinary) ctx.Reference(id, m.ReturnType, Line(decl));
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
                                var id = ctx.Declare(prop, decl, "property", prop.ToDisplayString(SignatureFormat), prop.Type.ToDisplayString(TypeFormat), typeId);
                                ctx.Edge(typeId, id, "contains", "extracted", Line(decl));
                                ctx.Reference(id, prop.Type, Line(decl));
                                ctx.WalkBody(decl, id);
                                break;
                            }
                        case IFieldSymbol field when !field.IsImplicitlyDeclared:
                            {
                                var id = ctx.Declare(field, decl, "field", field.ToDisplayString(SignatureFormat), field.Type.ToDisplayString(TypeFormat), typeId);
                                ctx.Edge(typeId, id, "contains", "extracted", Line(decl));
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

        private static string Direction(IParameterSymbol p)
        {
            if (p.IsParams) return "params";
            if (p.IsThis) return "this";
            return p.RefKind switch { RefKind.Out => "out", RefKind.Ref => "ref", RefKind.In => "in", RefKind.RefReadOnlyParameter => "ref", _ => "in" };
        }

        private static int Line(SyntaxNode node) => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        // ------------------------------------------------------------------ ids

        public static string StableId(string languageId, string kind, string qualifiedName)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{languageId}|{kind}|{qualifiedName}"));
            return Convert.ToHexString(bytes, 0, 12).ToLowerInvariant(); // 24 hex
        }

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

            public FileContext(CodeScript script, SemanticModel model, PerimeterIndex perimeter)
            {
                Script = script; _model = model; _perimeter = perimeter;
            }

            public string IdOf(ISymbol symbol)
            {
                var original = symbol.OriginalDefinition ?? symbol;
                var kind = original switch
                {
                    INamedTypeSymbol t when _perimeter.IsInPerimeter(t) => KindOf(t),
                    IMethodSymbol m when _perimeter.IsInPerimeter(m) => m.MethodKind == MethodKind.Ordinary ? "method" : "constructor",
                    IPropertySymbol when _perimeter.IsInPerimeter(original) => "property",
                    IFieldSymbol when _perimeter.IsInPerimeter(original) => "field",
                    _ => "external",
                };
                var qualified = original.ToDisplayString(QualifiedFormat);
                var id = StableId(CodeLanguages.CSharp, kind, qualified);
                if (kind == "external" && _externals.Add(id))
                {
                    Script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = original.Name, QualifiedName = qualified });
                }
                return id;
            }

            public string Declare(ISymbol symbol, SyntaxNode decl, string kind, string signature, string returnType, string parentId = null)
            {
                var span = decl.GetLocation().GetLineSpan();
                var id = StableId(CodeLanguages.CSharp, kind, symbol.ToDisplayString(QualifiedFormat));
                Script.Symbols.Add(new ScriptSymbol
                {
                    Id = id, Kind = kind, Name = symbol.Name, QualifiedName = symbol.ToDisplayString(QualifiedFormat),
                    LineStart = span.StartLinePosition.Line + 1, LineEnd = span.EndLinePosition.Line + 1,
                    Signature = signature, ReturnType = returnType,
                    Visibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant(),
                    IsStatic = symbol.IsStatic, IsAsync = symbol is IMethodSymbol { IsAsync: true },
                    ParentId = parentId ?? (symbol.ContainingType != null ? IdOf(symbol.ContainingType) : null),
                });
                return id;
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
                foreach (var node in decl.DescendantNodes())
                {
                    switch (node)
                    {
                        case InvocationExpressionSyntax inv:
                            {
                                var info = _model.GetSymbolInfo(inv);
                                var line = Line(inv);
                                if (info.Symbol is IMethodSymbol m)
                                {
                                    var target = m.ReducedFrom ?? m.OriginalDefinition ?? m;
                                    Edge(ownerId, IdOf(target), "calls", "extracted", line);
                                    if (target.ContainingType?.TypeKind == TypeKind.Interface)
                                        foreach (var impl in _perimeter.ImplementationsOf(target)) Edge(ownerId, IdOf(impl), "calls", "inferred", line);
                                }
                                else if (info.CandidateSymbols.Length > 0)
                                {
                                    foreach (var c in info.CandidateSymbols) Edge(ownerId, IdOf(c), "calls", "ambiguous", line);
                                }
                                else
                                {
                                    var text = inv.Expression.ToString();
                                    var id = StableId(CodeLanguages.CSharp, "external", text);
                                    if (_externals.Add(id)) Script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = text.Split('.').Last(), QualifiedName = text });
                                    Edge(ownerId, id, "calls", "ambiguous", line);
                                }
                                break;
                            }
                        case BaseObjectCreationExpressionSyntax creation:
                            {
                                var info = _model.GetSymbolInfo(creation);
                                var line = Line(creation);
                                if (info.Symbol is IMethodSymbol ctor && ctor.ContainingType != null)
                                    Edge(ownerId, IdOf(ctor.ContainingType), "instantiates", "extracted", line);
                                else if (_model.GetTypeInfo(creation).Type is INamedTypeSymbol t && t.TypeKind != TypeKind.Error)
                                    Edge(ownerId, IdOf(t), "instantiates", "extracted", line);
                                break;
                            }
                    }
                }
            }
        }
    }
}
