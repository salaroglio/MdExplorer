using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'estrattore PowerShell: il parser ufficiale del linguaggio (<see cref="Parser"/> di System.Management.Automation),
    /// in-process come Roslyn, niente grammatica da scaricare né <c>pwsh</c> sulla macchina. È solo sintassi: come per
    /// Python le chiamate si risolvono per nome (senza distinguere maiuscole, come PowerShell) con la confidenza esplicita.
    /// <para>
    /// Ogni file (<c>.ps1</c>, <c>.psm1</c>) è un <c>module</c> (<c>tools/build.ps1</c> → <c>tools/build</c>). Le
    /// <c>function</c>/<c>filter</c> sono <c>function</c> (annidate: <c>Esterna.Interna</c>), con i parametri del
    /// <c>param()</c> o inline, il tipo dal <c>[tipo]</c>, <c>[ref]</c> → <c>ref</c>, <c>ValueFromRemainingArguments</c>
    /// → <c>params</c>, il ritorno da <c>[OutputType()]</c>. Le <c>class</c> danno costruttori, metodi, property
    /// (<c>hidden</c> → privato); la prima base è <c>inherits</c>, le altre <c>implements</c> (la sintassi non le
    /// distingue: è la convenzione). Le <c>enum</c> danno campi. In un <c>.psm1</c> con <c>Export-ModuleMember
    /// -Function</c> a nomi letterali, le funzioni non esportate sono private.
    /// </para>
    /// <para>
    /// <c>imports</c>: dot-sourcing (<c>. $PSScriptRoot\lib.ps1</c>), <c>Import-Module</c>, <c>using module</c>,
    /// <c>#Requires -Modules</c>, verso il modulo del perimetro (per percorso, o per nome del file) o un <c>external</c>.
    /// <c>calls</c>: i comandi (una funzione del perimetro, o un cmdlet <c>external</c>) e i metodi (<c>$this.X()</c> della
    /// propria classe = <c>extracted</c>, <c>[Classe]::X()</c>, omonimi). <c>instantiates</c>: <c>[Classe]::new()</c> e
    /// <c>New-Object</c>; un altro script lanciato per percorso è un <c>calls</c> verso il suo modulo. Il codice fuori
    /// dalle funzioni è del modulo, e il <c>param()</c> dello script dà i parametri del modulo.
    /// </para>
    /// Un file con errori di sintassi si emette lo stesso con l'albero parziale del parser, e lo si dice nei warning.
    /// </summary>
    public sealed class PowerShellExtractor : ICodeExtractor
    {
        public string LanguageId => CodeLanguages.PowerShell;
        public string ExtractorName => "powershell-parser";
        public string ExtractorVersion => "1";
        public string Availability => "ready";

        /// <summary>Come per Python: oltre questo numero di omonimi un nome non si attribuisce più a tutti i candidati.</summary>
        public const int MaxAmbiguousCandidates = 5;

        /// <summary>Tipi che non dicono niente del progetto: non diventano archi <c>references</c> (Roslyn salta i tipi speciali).</summary>
        private static readonly HashSet<string> TrivialTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "string", "char", "bool", "boolean", "switch", "int", "int16", "int32", "int64", "long", "uint16", "uint32", "uint64",
            "byte", "sbyte", "short", "double", "float", "single", "decimal", "object", "void", "array", "hashtable", "ordered",
            "psobject", "pscustomobject", "scriptblock", "datetime", "timespan", "guid", "regex", "xml", "ref", "type",
            "System.String", "System.Object", "System.Int32", "System.Boolean", "System.Collections.Hashtable",
        };

        private readonly ILogger<PowerShellExtractor> _logger;

        public PowerShellExtractor(ILogger<PowerShellExtractor> logger) { _logger = logger; }

        public CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var result = new CodeExtractionResult();
            if (request.FilesToEmit.Count == 0) return result;

            var parsed = new List<ParsedFile>();
            foreach (var f in request.AllFiles)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = File.ReadAllText(f.AbsolutePath, Encoding.UTF8); }
                catch (Exception ex) { result.Warnings.Add($"{f.ProjectRelativePath}: non leggibile ({ex.Message})"); continue; }
                var ast = Parser.ParseInput(text, f.AbsolutePath, out _, out var errors);
                parsed.Add(new ParsedFile(f, text, ast, errors));
            }
            var index = new PerimeterIndex();
            foreach (var pf in parsed) index.Collect(pf);

            var toEmit = new HashSet<string>(request.FilesToEmit.Select(f => f.ProjectRelativePath), StringComparer.Ordinal);
            foreach (var pf in parsed)
            {
                ct.ThrowIfCancellationRequested();
                if (!toEmit.Contains(pf.File.ProjectRelativePath)) continue;
                if (pf.Errors.Length > 0)
                {
                    var e = pf.Errors[0];
                    result.Warnings.Add($"{pf.File.ProjectRelativePath}: {pf.Errors.Length} errori di sintassi PowerShell (riga {e.Extent.StartLineNumber}: {e.Message}); emesso con l'albero parziale");
                }
                try { result.Scripts.Add((pf.File, new FileEmitter(this, pf, index).Emit())); }
                catch (Exception ex)
                {
                    result.Warnings.Add($"{pf.File.ProjectRelativePath}: estrazione fallita ({ex.GetType().Name}: {ex.Message})");
                    _logger?.LogWarning(ex, "[CodeGraph/powershell] estrazione fallita per {File}", pf.File.ProjectRelativePath);
                }
            }
            return result;
        }

        // ------------------------------------------------------------------ naming

        /// <summary><c>tools/build.ps1</c> → <c>tools/build</c>; <c>Mod/Mod.psm1</c> → <c>Mod/Mod</c>.</summary>
        public static string ModuleName(string projectRelativePath)
        {
            var ext = Path.GetExtension(projectRelativePath);
            return string.IsNullOrEmpty(ext) ? projectRelativePath : projectRelativePath.Substring(0, projectRelativePath.Length - ext.Length);
        }

        public static string StableId(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId(CodeLanguages.PowerShell, kind, qualifiedName);

        private static string Sha256(string content)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        }

        private sealed record ParsedFile(CodeSourceFile File, string Text, ScriptBlockAst Ast, ParseError[] Errors)
        {
            public string Module => ModuleName(File.ProjectRelativePath);
        }

        // ------------------------------------------------------------------ shared syntax helpers

        /// <summary>
        /// Le funzioni dichiarate dall'utente. Il parser avvolge ogni metodo di classe in una FunctionDefinitionAst interna
        /// (figlia della FunctionMemberAst): quella non è una funzione del modulo.
        /// </summary>
        private static IEnumerable<FunctionDefinitionAst> Functions(Ast root)
            => root.FindAll(a => a is FunctionDefinitionAst && a.Parent is not FunctionMemberAst, true).Cast<FunctionDefinitionAst>();

        /// <summary>Il nome qualificato dentro il modulo: le funzioni annidate prendono quello delle funzioni che le contengono.</summary>
        private static string LocalName(FunctionDefinitionAst fn)
        {
            var parts = new List<string> { fn.Name };
            for (var a = fn.Parent; a != null; a = a.Parent)
                if (a is FunctionDefinitionAst outer && outer.Parent is not FunctionMemberAst) parts.Insert(0, outer.Name);
            return string.Join(".", parts);
        }

        private static IReadOnlyList<ParameterAst> ParametersOf(FunctionDefinitionAst fn)
            => (IReadOnlyList<ParameterAst>)fn.Parameters ?? fn.Body?.ParamBlock?.Parameters ?? (IReadOnlyList<ParameterAst>)Array.Empty<ParameterAst>();

        private static int Line(Ast a) => a.Extent.StartLineNumber;
        private static int EndLine(Ast a) => a.Extent.EndLineNumber;

        /// <summary>Il tipo dichiarato di un parametro: l'ultimo <c>[tipo]</c> fra gli attributi.</summary>
        private static ITypeName ParameterType(ParameterAst p) => p.Attributes.OfType<TypeConstraintAst>().LastOrDefault()?.TypeName;

        /// <summary>I nomi di tipo dentro un tipo: <c>List[Foo]</c> → List, Foo; <c>Foo[]</c> → Foo.</summary>
        private static IEnumerable<string> TypeNames(ITypeName type)
        {
            switch (type)
            {
                case null: yield break;
                case ArrayTypeName arr:
                    foreach (var n in TypeNames(arr.ElementType)) yield return n;
                    break;
                case GenericTypeName gen:
                    foreach (var n in TypeNames(gen.TypeName)) yield return n;
                    foreach (var arg in gen.GenericArguments) foreach (var n in TypeNames(arg)) yield return n;
                    break;
                default:
                    if (!TrivialTypes.Contains(type.Name) && !TrivialTypes.Contains(type.FullName)) yield return type.Name;
                    break;
            }
        }

        private static string ParameterText(ParameterAst p)
        {
            var type = ParameterType(p);
            var text = (type == null ? string.Empty : "[" + type.FullName + "]") + "$" + p.Name.VariablePath.UserPath;
            return p.DefaultValue == null ? text : text + " = " + p.DefaultValue.Extent.Text;
        }

        /// <summary>I tipi di <c>[OutputType(...)]</c> nel <c>param()</c>: il «ritorno» dichiarato di una funzione.</summary>
        private static string OutputType(FunctionDefinitionAst fn)
        {
            var attrs = fn.Body?.ParamBlock?.Attributes;
            if (attrs == null) return null;
            var types = attrs.Where(a => string.Equals(a.TypeName.Name, "OutputType", StringComparison.OrdinalIgnoreCase))
                .SelectMany(a => a.PositionalArguments)
                .Select(arg => arg switch
                {
                    TypeExpressionAst t => t.TypeName.FullName,
                    StringConstantExpressionAst s => s.Value,
                    _ => arg.Extent.Text,
                })
                .ToList();
            return types.Count == 0 ? null : string.Join(" | ", types);
        }

        // ------------------------------------------------------------------ perimeter index

        private sealed class PerimeterIndex
        {
            public readonly Dictionary<string, string> Modules = new(StringComparer.OrdinalIgnoreCase);
            /// <summary>Il nome del file senza estensione: è così che <c>Import-Module Nome</c> trova un modulo.</summary>
            public readonly Dictionary<string, List<string>> ModulesByStem = new(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, List<string>> FunctionsByName = new(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, List<string>> ClassesByName = new(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, List<string>> MethodsByName = new(StringComparer.OrdinalIgnoreCase);
            /// <summary>Per classe (id): nome del metodo → id; serve a <c>$this.X()</c> e <c>[Classe]::X()</c>.</summary>
            public readonly Dictionary<string, Dictionary<string, string>> MethodsOfClass = new(StringComparer.Ordinal);

            public void Collect(ParsedFile pf)
            {
                var moduleId = StableId("module", pf.Module);
                Modules[pf.Module] = moduleId;
                Add(ModulesByStem, Path.GetFileName(pf.Module), moduleId);
                foreach (var fn in Functions(pf.Ast))
                    Add(FunctionsByName, fn.Name, StableId("function", pf.Module + ":" + LocalName(fn)));
                foreach (var type in pf.Ast.FindAll(a => a is TypeDefinitionAst, true).Cast<TypeDefinitionAst>())
                {
                    var qn = pf.Module + ":" + type.Name;
                    var classId = StableId(type.IsEnum ? "enum" : "class", qn);
                    Add(ClassesByName, type.Name, classId);
                    var own = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var m in type.Members.OfType<FunctionMemberAst>().Where(m => !m.IsConstructor))
                    {
                        var id = StableId("method", qn + "." + m.Name);
                        own[m.Name] = id;
                        Add(MethodsByName, m.Name, id);
                    }
                    MethodsOfClass[classId] = own;
                }
            }

            public static void Add(Dictionary<string, List<string>> map, string key, string value)
            {
                if (!map.TryGetValue(key, out var list)) map[key] = list = new List<string>();
                if (!list.Contains(value)) list.Add(value);
            }
        }

        // ------------------------------------------------------------------ emitter

        private sealed class FileEmitter
        {
            private readonly PowerShellExtractor _owner;
            private readonly ParsedFile _pf;
            private readonly PerimeterIndex _index;
            private readonly CodeScript _script = new();
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly Dictionary<string, ScriptEdge> _edges = new(StringComparer.Ordinal);
            private readonly HashSet<string> _declared = new(StringComparer.Ordinal);
            /// <summary>Il nodo della dichiarazione → id: un comando appartiene alla dichiarazione più vicina che lo contiene.</summary>
            private readonly Dictionary<Ast, string> _owners = new(ReferenceEqualityComparer.Instance);
            /// <summary>Il nodo di una classe → id, per risolvere <c>$this</c>.</summary>
            private readonly Dictionary<Ast, string> _classes = new(ReferenceEqualityComparer.Instance);
            private readonly string _moduleId;
            private readonly string _moduleDir;

            public FileEmitter(PowerShellExtractor owner, ParsedFile pf, PerimeterIndex index)
            {
                _owner = owner; _pf = pf; _index = index;
                _moduleId = StableId("module", pf.Module);
                _moduleDir = Path.GetDirectoryName(pf.File.ProjectRelativePath)?.Replace('\\', '/') ?? string.Empty;
            }

            public CodeScript Emit()
            {
                _script.Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = _pf.File.ProjectRelativePath,
                    Language = _owner.LanguageId, SourceHash = Sha256(_pf.Text), Extractor = _owner.ExtractorName,
                    ExtractorVersion = _owner.ExtractorVersion, GeneratedAt = DateTime.UtcNow.ToString("o"),
                };
                var root = _pf.Ast;
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = _moduleId, Kind = "module", Name = Path.GetFileName(_pf.File.ProjectRelativePath), QualifiedName = _pf.Module,
                    LineStart = 1, LineEnd = EndLine(root), Signature = _pf.File.ProjectRelativePath, Visibility = "public",
                });
                _declared.Add(_moduleId);
                // Il param() dello script: sono i parametri con cui lo si lancia, li porta il modulo.
                if (root.ParamBlock != null) EmitParameters(root.ParamBlock.Parameters, _moduleId);

                EmitRequirements(root);
                foreach (var u in root.UsingStatements) EmitUsing(u);
                foreach (var type in root.FindAll(a => a is TypeDefinitionAst, true).Cast<TypeDefinitionAst>()) EmitType(type);
                var exported = ExportedFunctions(root);
                foreach (var fn in Functions(root)) EmitFunction(fn, exported);
                EmitModuleVariables(root);
                WalkBodies(root);
                return _script;
            }

            // ---- imports

            private void EmitRequirements(ScriptBlockAst root)
            {
                var required = root.ScriptRequirements?.RequiredModules;
                if (required == null) return;
                foreach (var m in required) Edge(_moduleId, ResolveModuleByName(m.Name), "imports", "extracted", 1);
            }

            private void EmitUsing(UsingStatementAst u)
            {
                if (u.UsingStatementKind != UsingStatementKind.Module) return;
                var name = u.Name?.Value ?? u.ModuleSpecification?.Extent.Text;
                if (name == null) return;
                Edge(_moduleId, LooksLikePath(name) ? ResolveModulePath(name) : ResolveModuleByName(name), "imports", "extracted", Line(u));
            }

            private static bool LooksLikePath(string s)
                => s.Contains('/') || s.Contains('\\') || s.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)
                   || s.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase);

            /// <summary><c>Import-Module Nome</c>: il modulo del perimetro col file che si chiama così, se è uno solo.</summary>
            private string ResolveModuleByName(string name)
            {
                if (_index.ModulesByStem.TryGetValue(name, out var list) && list.Count == 1) return list[0];
                return External("module", name, name);
            }

            /// <summary>
            /// Un percorso di script o modulo (<c>$PSScriptRoot\lib\util.ps1</c>, <c>.\util.ps1</c>, <c>../Mod/Mod.psd1</c>)
            /// relativo alla cartella del file; un <c>.psd1</c> (manifesto) vale il <c>.psm1</c> accanto con lo stesso nome.
            /// </summary>
            private string ResolveModulePath(string raw)
            {
                var path = raw.Trim().Trim('"', '\'').Replace('\\', '/');
                foreach (var root in new[] { "$PSScriptRoot", "${PSScriptRoot}" })
                    if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) path = "." + path.Substring(root.Length);
                var parts = Path.IsPathRooted(path) || path.Contains('$') ? null : new List<string>(_moduleDir.Length == 0 ? Array.Empty<string>() : _moduleDir.Split('/'));
                if (parts != null)
                {
                    foreach (var seg in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (seg == ".") continue;
                        if (seg == "..") { if (parts.Count > 0) parts.RemoveAt(parts.Count - 1); continue; }
                        parts.Add(seg);
                    }
                    var candidate = ModuleName(string.Join("/", parts));
                    if (_index.Modules.TryGetValue(candidate, out var id)) return id;
                }
                var stem = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(stem) && _index.ModulesByStem.TryGetValue(stem, out var byStem) && byStem.Count == 1) return byStem[0];
                return External("module", raw, string.IsNullOrEmpty(stem) ? raw : stem);
            }

            /// <summary>
            /// Il testo di un percorso dato come argomento: costante, stringa espandibile, o <c>(Join-Path $PSScriptRoot 'x')</c>.
            /// Null quando non è ricostruibile dalla sola sintassi.
            /// </summary>
            private static string PathText(CommandElementAst element)
            {
                switch (element)
                {
                    case StringConstantExpressionAst s: return s.Value;
                    case ExpandableStringExpressionAst e: return e.Value;
                    case ParenExpressionAst paren when paren.Pipeline is PipelineAst { PipelineElements.Count: 1 } pl
                                                      && pl.PipelineElements[0] is CommandAst join
                                                      && string.Equals(join.GetCommandName(), "Join-Path", StringComparison.OrdinalIgnoreCase):
                        {
                            var pieces = join.CommandElements.Skip(1).Where(c => c is not CommandParameterAst)
                                .Select(c => c is VariableExpressionAst v ? "$" + v.VariablePath.UserPath : PathText(c)).ToList();
                            return pieces.Any(p => p == null) ? null : string.Join("/", pieces);
                        }
                    default: return null;
                }
            }

            /// <summary>Il valore di <c>-Nome</c>, o il primo argomento posizionale se il parametro non c'è.</summary>
            private static CommandElementAst Argument(CommandAst cmd, string parameter)
            {
                var elements = cmd.CommandElements;
                for (var i = 1; i < elements.Count; i++)
                {
                    if (elements[i] is CommandParameterAst p && string.Equals(p.ParameterName, parameter, StringComparison.OrdinalIgnoreCase))
                        return p.Argument ?? (i + 1 < elements.Count ? elements[i + 1] : null);
                }
                for (var i = 1; i < elements.Count; i++)
                {
                    if (elements[i] is CommandParameterAst p) { if (p.Argument == null) i++; continue; }  // salta -Altro valore
                    return elements[i];
                }
                return null;
            }

            /// <summary>
            /// Le funzioni esportate da un <c>.psm1</c> con <c>Export-ModuleMember -Function</c> a nomi letterali; null =
            /// tutte pubbliche (nessun export, un nome con <c>*</c>, o un valore non letterale).
            /// </summary>
            private HashSet<string> ExportedFunctions(ScriptBlockAst root)
            {
                if (!_pf.File.ProjectRelativePath.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase)) return null;
                var exports = root.FindAll(a => a is CommandAst c && string.Equals(c.GetCommandName(), "Export-ModuleMember", StringComparison.OrdinalIgnoreCase), true)
                    .Cast<CommandAst>().ToList();
                if (exports.Count == 0) return null;
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var cmd in exports)
                {
                    var arg = Argument(cmd, "Function");
                    var values = arg switch
                    {
                        StringConstantExpressionAst s => new[] { s.Value },
                        ArrayLiteralAst arr when arr.Elements.All(e => e is StringConstantExpressionAst) => arr.Elements.Cast<StringConstantExpressionAst>().Select(e => e.Value).ToArray(),
                        null => Array.Empty<string>(),
                        _ => null,
                    };
                    if (values == null || values.Any(v => v.Contains('*'))) return null;
                    names.UnionWith(values);
                }
                return names;
            }

            // ---- definitions

            private void EmitType(TypeDefinitionAst type)
            {
                var qn = _pf.Module + ":" + type.Name;
                var kind = type.IsEnum ? "enum" : "class";
                var bases = type.BaseTypes.Select(b => b.TypeName.FullName).ToList();
                var signature = (type.IsEnum ? "enum " : "class ") + type.Name + (bases.Count > 0 ? " : " + string.Join(", ", bases) : string.Empty);
                var classId = Declare(type, kind, type.Name, qn, signature, null, "public", _moduleId);
                _classes[type] = classId;
                Edge(_moduleId, classId, "contains", "extracted", Line(type));

                // La sintassi non distingue classe base e interfacce: per convenzione la prima è la base.
                for (var i = 0; i < type.BaseTypes.Count; i++)
                {
                    var b = type.BaseTypes[i];
                    foreach (var n in TypeNames(b.TypeName)) Edge(classId, ResolveType(n), i == 0 && !type.IsEnum ? "inherits" : "implements", ConfidenceOfType(n), Line(b));
                }

                foreach (var member in type.Members)
                {
                    switch (member)
                    {
                        case FunctionMemberAst m:
                            {
                                var mkind = m.IsConstructor ? "constructor" : "method";
                                var returnType = m.IsConstructor ? null : m.ReturnType?.TypeName.FullName ?? "void";
                                var prefix = (m.IsStatic ? "static " : string.Empty) + (m.IsHidden ? "hidden " : string.Empty);
                                var sig = prefix + (m.IsConstructor ? string.Empty : "[" + returnType + "] ") + m.Name + "(" + string.Join(", ", m.Parameters.Select(ParameterText)) + ")";
                                var id = Declare(m, mkind, m.Name, qn + "." + m.Name, sig, returnType, m.IsHidden ? "private" : "public", classId, isStatic: m.IsStatic);
                                _owners[m] = id;
                                Edge(classId, id, "contains", "extracted", Line(m));
                                if (m.ReturnType != null) foreach (var n in TypeNames(m.ReturnType.TypeName)) Edge(id, ResolveType(n), "references", ConfidenceOfType(n), Line(m));
                                EmitParameters(m.Parameters, id);
                                break;
                            }
                        case PropertyMemberAst p:
                            {
                                var pkind = type.IsEnum ? "field" : "property";
                                var ptype = p.PropertyType?.TypeName.FullName;
                                var prefix = (p.IsStatic ? "static " : string.Empty) + (p.IsHidden ? "hidden " : string.Empty);
                                var sig = type.IsEnum ? p.Extent.Text : prefix + (ptype == null ? string.Empty : "[" + ptype + "] ") + "$" + p.Name;
                                var id = Declare(p, pkind, p.Name, qn + "." + p.Name, sig, type.IsEnum ? type.Name : ptype, p.IsHidden ? "private" : "public", classId, isStatic: p.IsStatic);
                                _owners[p] = id;
                                Edge(classId, id, "contains", "extracted", Line(p));
                                if (p.PropertyType != null) foreach (var n in TypeNames(p.PropertyType.TypeName)) Edge(id, ResolveType(n), "references", ConfidenceOfType(n), Line(p));
                                break;
                            }
                    }
                }
            }

            private void EmitFunction(FunctionDefinitionAst fn, HashSet<string> exported)
            {
                var local = LocalName(fn);
                var parentId = _moduleId;
                for (var a = fn.Parent; a != null; a = a.Parent)
                    if (a is FunctionDefinitionAst outer && _owners.TryGetValue(outer, out var outerId)) { parentId = outerId; break; }

                var parameters = ParametersOf(fn);
                var returnType = OutputType(fn);
                var sig = (fn.IsFilter ? "filter " : "function ") + fn.Name + "(" + string.Join(", ", parameters.Select(ParameterText)) + ")";
                var visibility = parentId == _moduleId && exported != null && !exported.Contains(fn.Name) ? "private" : "public";
                var id = Declare(fn, "function", fn.Name, _pf.Module + ":" + local, sig, returnType, visibility, parentId);
                _owners[fn] = id;
                Edge(parentId, id, "contains", "extracted", Line(fn));
                var outputAttr = fn.Body?.ParamBlock?.Attributes.Where(a => string.Equals(a.TypeName.Name, "OutputType", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(a => a.PositionalArguments).OfType<TypeExpressionAst>() ?? Enumerable.Empty<TypeExpressionAst>();
                foreach (var t in outputAttr) foreach (var n in TypeNames(t.TypeName)) Edge(id, ResolveType(n), "references", ConfidenceOfType(n), Line(fn));
                EmitParameters(parameters, id);
            }

            /// <summary>Le variabili assegnate al livello dello script (<c>$Config = …</c>, <c>$script:X = …</c>): campi del modulo.</summary>
            private void EmitModuleVariables(ScriptBlockAst root)
            {
                if (root.EndBlock == null) return;
                foreach (var st in root.EndBlock.Statements.OfType<AssignmentStatementAst>())
                {
                    var left = st.Left is ConvertExpressionAst conv ? conv.Child : st.Left;
                    if (left is not VariableExpressionAst v) continue;
                    var name = v.VariablePath.UserPath;
                    var colon = name.IndexOf(':');
                    if (colon >= 0)
                    {
                        var scope = name.Substring(0, colon);
                        if (!scope.Equals("script", StringComparison.OrdinalIgnoreCase) && !scope.Equals("global", StringComparison.OrdinalIgnoreCase)) continue; // $env:X non è del modulo
                        name = name.Substring(colon + 1);
                    }
                    var fqn = _pf.Module + ":" + name;
                    var fid = StableId("field", fqn);
                    if (_declared.Contains(fid)) { _owners[st] = fid; continue; }
                    var type = (st.Left as ConvertExpressionAst)?.Type.TypeName.FullName;
                    var firstLine = st.Extent.Text.Split('\n')[0].Trim();
                    Declare(st, "field", name, fqn, firstLine, type, "public", _moduleId);
                    _owners[st] = fid;
                    Edge(_moduleId, fid, "contains", "extracted", Line(st));
                }
            }

            private void EmitParameters(IEnumerable<ParameterAst> parameters, string ownerId)
            {
                var pos = 0;
                foreach (var p in parameters)
                {
                    var type = ParameterType(p);
                    var paramAttrs = p.Attributes.OfType<AttributeAst>().ToList();
                    var remaining = paramAttrs.Where(a => string.Equals(a.TypeName.Name, "Parameter", StringComparison.OrdinalIgnoreCase))
                        .SelectMany(a => a.NamedArguments)
                        .Any(n => string.Equals(n.ArgumentName, "ValueFromRemainingArguments", StringComparison.OrdinalIgnoreCase)
                                  && (n.ExpressionOmitted || !string.Equals(n.Argument.Extent.Text, "$false", StringComparison.OrdinalIgnoreCase)));
                    var direction = remaining ? "params" : type != null && string.Equals(type.Name, "ref", StringComparison.OrdinalIgnoreCase) ? "ref" : "in";
                    var def = p.DefaultValue?.Extent.Text;
                    _script.Parameters.Add(new ScriptParameter
                    {
                        SymbolId = ownerId, Position = pos++, Name = p.Name.VariablePath.UserPath, ParameterType = type?.FullName,
                        Direction = direction, DefaultValue = def,
                        IsNullable = string.Equals(def, "$null", StringComparison.OrdinalIgnoreCase)
                                     || paramAttrs.Any(a => a.TypeName.Name is "AllowNull" or "AllowNullAttribute"),
                    });
                    foreach (var n in TypeNames(type)) Edge(ownerId, ResolveType(n), "references", ConfidenceOfType(n), Line(p));
                }
            }

            // ---- bodies: commands, member calls, instances

            private string OwnerOf(Ast node)
            {
                for (var a = node.Parent; a != null; a = a.Parent)
                    if (_owners.TryGetValue(a, out var id)) return id;
                return _moduleId;
            }

            private string ClassOf(Ast node)
            {
                for (var a = node.Parent; a != null; a = a.Parent)
                    if (_classes.TryGetValue(a, out var id)) return id;
                return null;
            }

            private void WalkBodies(ScriptBlockAst root)
            {
                foreach (var node in root.FindAll(a => a is CommandAst || a is InvokeMemberExpressionAst, true))
                {
                    var owner = OwnerOf(node);
                    var line = Line(node);
                    if (node is CommandAst cmd) EmitCommand(cmd, owner, line);
                    else EmitMemberCall((InvokeMemberExpressionAst)node, owner, line);
                }
            }

            private void EmitCommand(CommandAst cmd, string owner, int line)
            {
                // Dot-sourcing: «. percorso» porta le funzioni di un altro script dentro questo.
                if (cmd.InvocationOperator == TokenKind.Dot)
                {
                    var path = PathText(cmd.CommandElements[0]);
                    Edge(_moduleId, path == null ? External("module", cmd.CommandElements[0].Extent.Text, cmd.CommandElements[0].Extent.Text) : ResolveModulePath(path), "imports", path == null ? "ambiguous" : "extracted", line);
                    return;
                }
                // Un altro script lanciato per percorso (.\altro.ps1, & "$PSScriptRoot\altro.ps1"): chiama quel modulo.
                var scriptPath = PathText(cmd.CommandElements[0]);
                if (scriptPath != null && scriptPath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    Edge(owner, ResolveModulePath(scriptPath), "calls", "extracted", line);
                    return;
                }
                var name = cmd.GetCommandName();
                if (string.IsNullOrEmpty(name)) return;                  // & $scriptblock: la sola sintassi non sa chi
                if (string.Equals(name, "Import-Module", StringComparison.OrdinalIgnoreCase))
                {
                    var arg = Argument(cmd, "Name");
                    var text = arg == null ? null : PathText(arg);
                    if (text != null) Edge(_moduleId, LooksLikePath(text) ? ResolveModulePath(text) : ResolveModuleByName(text), "imports", "extracted", line);
                    return;
                }
                if (string.Equals(name, "New-Object", StringComparison.OrdinalIgnoreCase))
                {
                    if (Argument(cmd, "TypeName") is StringConstantExpressionAst t) Edge(owner, ResolveType(t.Value), "instantiates", ConfidenceOfType(t.Value), line);
                    return;
                }
                if (_index.FunctionsByName.TryGetValue(name, out var fns) && fns.Count <= MaxAmbiguousCandidates)
                {
                    var conf = fns.Count == 1 ? "inferred" : "ambiguous";
                    foreach (var id in fns) Edge(owner, id, "calls", conf, line);
                }
                else Edge(owner, External("command", fns != null ? "?." + name : name, name), "calls", fns != null ? "ambiguous" : "extracted", line);
            }

            private void EmitMemberCall(InvokeMemberExpressionAst call, string owner, int line)
            {
                if (call.Member is not StringConstantExpressionAst member) return;   // $obj.$nome(): dinamico
                var name = member.Value;

                if (call.Static && call.Expression is TypeExpressionAst typeExpr)
                {
                    var typeName = typeExpr.TypeName.Name;
                    if (string.Equals(name, "new", StringComparison.OrdinalIgnoreCase))
                    {
                        Edge(owner, ResolveType(typeName), "instantiates", ConfidenceOfType(typeName), line);
                        return;
                    }
                    if (_index.ClassesByName.TryGetValue(typeName, out var classes))
                    {
                        var targets = classes.Select(c => _index.MethodsOfClass.TryGetValue(c, out var ms) && ms.TryGetValue(name, out var mid) ? mid : null).Where(t => t != null).ToList();
                        foreach (var t in targets) Edge(owner, t, "calls", targets.Count == 1 ? "inferred" : "ambiguous", line);
                        if (targets.Count > 0) return;
                    }
                    Edge(owner, External("method", typeExpr.TypeName.FullName + "::" + name, name), "calls", "extracted", line);
                    return;
                }

                if (call.Expression is VariableExpressionAst v && string.Equals(v.VariablePath.UserPath, "this", StringComparison.OrdinalIgnoreCase))
                {
                    var cls = ClassOf(call);
                    if (cls != null && _index.MethodsOfClass.TryGetValue(cls, out var own) && own.TryGetValue(name, out var ownId))
                    {
                        Edge(owner, ownId, "calls", "extracted", line);
                        return;
                    }
                }

                if (_index.MethodsByName.TryGetValue(name, out var ms2) && ms2.Count <= MaxAmbiguousCandidates)
                {
                    var conf = ms2.Count == 1 ? "inferred" : "ambiguous";
                    foreach (var id in ms2) Edge(owner, id, "calls", conf, line);
                }
                else Edge(owner, External("method", ms2 != null ? "?." + name : call.Expression.Extent.Text + "." + name, name), "calls", "ambiguous", line);
            }

            // ---- resolution

            private string ResolveType(string name)
            {
                if (_index.ClassesByName.TryGetValue(name, out var list) && list.Count >= 1) return list[0];
                return External("type", name, name);
            }

            private string ConfidenceOfType(string name)
                => _index.ClassesByName.TryGetValue(name, out var list) ? (list.Count == 1 ? "inferred" : "ambiguous") : "extracted";

            private string External(string what, string qualifiedName, string name)
            {
                var id = StableId("external", what + ":" + qualifiedName);
                if (_externals.Add(id)) _script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = name, QualifiedName = qualifiedName });
                return id;
            }

            // ---- helpers

            private string Declare(Ast node, string kind, string name, string qualifiedName, string signature, string returnType, string visibility, string parentId, bool isStatic = false)
            {
                var id = StableId(kind, qualifiedName);
                if (!_declared.Add(id)) return id;
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
                // Come per Python: un arco per (sorgente, destinazione, relazione); vince la confidenza migliore.
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
