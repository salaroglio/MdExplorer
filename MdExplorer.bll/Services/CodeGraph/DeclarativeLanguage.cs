using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Un linguaggio definito nel progetto di documentazione: un file
    /// <c>.mde-languages/&lt;id&gt;.lang.yml</c> di regole dichiarative — blocchi aperti e chiusi da parole chiave, etichette,
    /// parametri, riferimenti — letto da <see cref="DeclarativeLanguageExtractor"/>. Niente codice eseguibile dal repository:
    /// solo espressioni regolari, con un tempo massimo. Lettura <b>rigida</b>: una chiave che il formato non conosce, una
    /// regex che non compila, un gruppo obbligatorio che manca o un riferimento a una regola inesistente sono errori con il
    /// file e il campo, mai righe ignorate.
    /// </summary>
    public sealed class DeclarativeLanguage
    {
        public const string FolderName = ".mde-languages";
        public const string FileSuffix = ".lang.yml";
        public const int CurrentSchema = 1;
        /// <summary>Il nome che le regole dei riferimenti usano per il file stesso (il modulo).</summary>
        public const string ModuleRule = "module";
        /// <summary>L'id di regola dei frammenti (<see cref="FragmentRule"/>): riservato, come <see cref="ModuleRule"/>.</summary>
        public const string FragmentsRule = "fragment";
        /// <summary>Il kind dei frammenti e la relazione dei salti fra frammenti: solo la sezione <c>fragments</c> li produce.</summary>
        public const string FragmentKind = "fragment";
        public const string JumpsRelation = "jumps";
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

        public string Id { get; private init; }
        public string DisplayName { get; private init; }
        public int Version { get; private init; }
        public IReadOnlyList<string> Extensions { get; private init; }
        public bool IgnoreCase { get; private init; }
        public string DefinitionPath { get; private init; }
        /// <summary>Le prime 8 cifre dello SHA-256 del file: cambiare una regola vuol dire riparsare tutto.</summary>
        public string DefinitionHash { get; private init; }
        public StringComparer NameComparer => IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        public string LineComment { get; private init; }
        public char? StringDelimiter { get; private init; }
        public Regex Continuation { get; private init; }
        public Regex ModuleName { get; private init; }
        public IReadOnlyList<BlockRule> Blocks { get; private init; }
        public IReadOnlyList<LabelRule> Labels { get; private init; }
        public IReadOnlyList<DeclarationRule> Declarations { get; private init; }
        public ParameterRule Parameters { get; private init; }
        public IReadOnlyDictionary<string, string> Types { get; private init; }
        public IReadOnlyList<ReferenceRule> References { get; private init; }
        public FragmentRule Fragments { get; private init; }

        public sealed class BlockRule
        {
            public string Id { get; init; }
            public string Kind { get; init; }
            public Regex Begin { get; init; }
            public Regex End { get; init; }
            public IReadOnlyList<string> In { get; init; }
            public string NameTemplate { get; init; }
            public string SignatureTemplate { get; init; }
        }

        public sealed class LabelRule
        {
            public string Id { get; init; }
            public string Kind { get; init; }
            public Regex At { get; init; }
            public IReadOnlyList<string> In { get; init; }
            public string NameTemplate { get; init; }
            public bool OnlyIfReferenced { get; init; }
        }

        /// <summary>
        /// Un simbolo su una riga sola (una macro <c>#define F(x)</c>, una variabile). Con <c>each</c> la riga ne
        /// dichiara più d'uno (<c>VAR A, B, C</c>). Con <c>emit: false</c> serve solo da indice: dice che un nome è una
        /// variabile, così <c>X(3)</c> non diventa una chiamata, senza riempire il grafo di variabili.
        /// </summary>
        public sealed class DeclarationRule
        {
            public string Id { get; init; }
            public string Kind { get; init; }
            public Regex At { get; init; }
            public Regex Each { get; init; }
            public IReadOnlyList<string> In { get; init; }
            public string NameTemplate { get; init; }
            public bool Emit { get; init; }
        }

        public sealed class ParameterRule
        {
            public Regex Item { get; init; }
            public IReadOnlyDictionary<string, string> Direction { get; init; }
            public string DefaultDirection { get; init; }
            /// <summary>La parola di direzione vale anche per i parametri che seguono, finché un'altra la cambia (<c>OUT A, B IN C</c>: A e B in uscita).</summary>
            public bool Sticky { get; init; }
        }

        /// <summary>
        /// I blocchi troppo lunghi perché l'LLM li legga in una volta (utente 23/10/2026: «per avere una buona descrizione
        /// si deve per forza dare il codice, tutto ed in maniera fruibile»): oltre il limite il blocco si spezza a ogni riga
        /// che <see cref="At"/> riconosce, e i <c>GOTO</c> (<see cref="Jump"/>) diventano archi <c>jumps</c> fra i pezzi.
        /// <see cref="Stop"/> = l'ultima istruzione chiude il flusso: niente arco verso il pezzo che segue. Un pezzo ancora
        /// troppo lungo (codice strutturato, senza etichette) si spezza ai rami della sua selezione più esterna
        /// (<see cref="Branches"/>), e così via finché ci sta.
        /// </summary>
        public sealed class FragmentRule
        {
            public IReadOnlyList<string> In { get; init; }
            public Regex At { get; init; }
            public Regex Jump { get; init; }
            public Regex Stop { get; init; }
            public BranchRule Branches { get; init; }
        }

        /// <summary>Una selezione a rami: <c>SELECT</c> … <c>CASE</c>/<c>DEFAULT</c> … <c>ENDSEL</c>.</summary>
        public sealed class BranchRule
        {
            public Regex Open { get; init; }
            public Regex Case { get; init; }
            public Regex Close { get; init; }
        }

        public enum TextKind { Code, Raw }
        /// <summary><c>file</c> = per nome del file; <c>module</c> = per nome del programma (<c>module.name</c>); <c>symbol</c> = un simbolo di una regola.</summary>
        public enum ResolveKind { File, Module, Symbol }
        public enum ScopeKind { Block, Module, Perimeter }

        public sealed class ReferenceRule
        {
            public string Relation { get; init; }
            public Regex Pattern { get; init; }
            public TextKind Text { get; init; }
            public Regex After { get; init; }
            public Regex NotAfter { get; init; }
            public Regex Strip { get; init; }
            public ResolveKind Resolve { get; init; }
            /// <summary>Le regole dei simboli che il bersaglio può essere (<c>rule</c> o <c>rules</c> nel file).</summary>
            public IReadOnlyList<string> Rules { get; init; }
            /// <summary>Parole che non sono riferimenti (parole chiave, funzioni del linguaggio): niente arco, niente nodo esterno.</summary>
            public ISet<string> Ignore { get; init; }
            /// <summary>Regole di dichiarazione che escludono il bersaglio non risolto: <c>X(3)</c> con <c>X</c> variabile non è una chiamata.</summary>
            public IReadOnlyList<string> Unless { get; init; }
            public ScopeKind Scope { get; init; }
            /// <summary>La confidenza massima che la regola può dare (null = quella che dice la risoluzione).</summary>
            public string Confidence { get; init; }
            public bool ExternalWhenUnresolved { get; init; }
        }

        // ------------------------------------------------------------------ catalogo del progetto

        public static string FolderOf(string projectPath) => Path.Combine(projectPath, FolderName);

        /// <summary>I file di definizione del progetto, in ordine; l'id è il nome del file (<c>toy.lang.yml</c> → <c>toy</c>).</summary>
        public static IReadOnlyList<string> FilesOf(string projectPath)
        {
            var folder = FolderOf(projectPath);
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(folder)) return Array.Empty<string>();
            return Directory.EnumerateFiles(folder, "*" + FileSuffix).OrderBy(f => f, StringComparer.Ordinal).ToList();
        }

        public static string IdFromPath(string path)
        {
            var name = Path.GetFileName(path);
            return name.EndsWith(FileSuffix, StringComparison.OrdinalIgnoreCase) ? name[..^FileSuffix.Length] : name;
        }

        // ------------------------------------------------------------------ lettura

        public static DeclarativeLanguage Load(string path) => Parse(File.ReadAllText(path, Encoding.UTF8), path);

        public static DeclarativeLanguage Parse(string yaml, string path)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            Yaml.Definition d;
            try { d = deserializer.Deserialize<Yaml.Definition>(yaml); }
            catch (YamlException ex)
            {
                var why = ex.InnerException?.Message ?? ex.Message;
                throw new LanguageDefinitionException(path, UiText.Say($"riga {ex.Start.Line}: {why}", $"line {ex.Start.Line}: {why}"), ex);
            }
            if (d == null) throw new LanguageDefinitionException(path, UiText.Say("il file è vuoto", "the file is empty"));
            return new Builder(path, d).Build(Hash(yaml));
        }

        private static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)), 0, 4).ToLowerInvariant();
        }

        /// <summary>Le regole del file prese una per una: ogni errore dice dove, così chi scrive il file lo trova subito.</summary>
        private sealed class Builder
        {
            private readonly string _path;
            private readonly Yaml.Definition _d;
            private RegexOptions _options;

            public Builder(string path, Yaml.Definition d) { _path = path; _d = d; }

            private LanguageDefinitionException Fail(string italian, string english) => new(_path, UiText.Say(italian, english));

            public DeclarativeLanguage Build(string hash)
            {
                if (_d.Schema != CurrentSchema)
                    throw Fail($"schema: {_d.Schema} non è uno schema che questo MdExplorer sa leggere (atteso {CurrentSchema})",
                                $"schema: {_d.Schema} is not a schema this MdExplorer can read (expected {CurrentSchema})");
                var fileId = IdFromPath(_path);
                if (string.IsNullOrWhiteSpace(_d.Id)) throw Fail("id: obbligatorio", "id: required");
                if (!Regex.IsMatch(_d.Id, "^[a-z][a-z0-9-]*$"))
                    throw Fail($"id: '{_d.Id}' — solo minuscole, cifre e trattini, e comincia con una lettera", $"id: '{_d.Id}' — lowercase letters, digits and dashes only, starting with a letter");
                if (!string.Equals(_d.Id, fileId, StringComparison.Ordinal))
                    throw Fail($"id: '{_d.Id}' deve essere uguale al nome del file ('{fileId}{FileSuffix}')", $"id: '{_d.Id}' must match the file name ('{fileId}{FileSuffix}')");
                if (_d.Version < 1) throw Fail("version: obbligatoria, intero da 1 in su", "version: required, an integer from 1 up");
                if (_d.Extensions == null || _d.Extensions.Count == 0) throw Fail("extensions: almeno una (es. .toy)", "extensions: at least one (e.g. .toy)");
                foreach (var e in _d.Extensions)
                    if (string.IsNullOrWhiteSpace(e) || !e.StartsWith('.') || e.Length < 2)
                        throw Fail($"extensions: '{e}' — un'estensione comincia col punto", $"extensions: '{e}' — an extension starts with a dot");

                _options = RegexOptions.CultureInvariant | (_d.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                var lexical = _d.Lexical ?? new Yaml.Lexical();
                if (lexical.String != null && lexical.String.Length != 1)
                    throw Fail($"lexical.string: '{lexical.String}' — un solo carattere", $"lexical.string: '{lexical.String}' — a single character");

                var blocks = (_d.Blocks ?? new List<Yaml.Block>()).Select((b, i) => BuildBlock(b, i)).ToList();
                var labels = (_d.Labels ?? new List<Yaml.Label>()).Select((l, i) => BuildLabel(l, i)).ToList();
                var declarations = (_d.Declarations ?? new List<Yaml.Declaration>()).Select((x, i) => BuildDeclaration(x, i)).ToList();
                var ruleIds = new HashSet<string>(StringComparer.Ordinal) { ModuleRule, FragmentsRule };
                foreach (var id in blocks.Select(b => b.Id).Concat(labels.Select(l => l.Id)).Concat(declarations.Select(x => x.Id)))
                    if (!ruleIds.Add(id)) throw Fail($"id di regola ripetuto o riservato: '{id}'", $"rule id repeated or reserved: '{id}'");
                var containers = new HashSet<string>(blocks.Select(b => b.Id).Append(ModuleRule), StringComparer.Ordinal);
                foreach (var b in blocks) CheckIn($"blocks[{b.Id}].in", b.In, containers);
                foreach (var l in labels) CheckIn($"labels[{l.Id}].in", l.In, containers);
                foreach (var x in declarations) CheckIn($"declarations[{x.Id}].in", x.In, containers);
                var fragments = BuildFragments(blocks);

                return new DeclarativeLanguage
                {
                    Id = _d.Id, DisplayName = string.IsNullOrWhiteSpace(_d.Name) ? _d.Id : _d.Name, Version = _d.Version,
                    Extensions = _d.Extensions.Select(e => e.ToLowerInvariant()).Distinct().ToList(),
                    IgnoreCase = _d.IgnoreCase, DefinitionPath = _path, DefinitionHash = hash,
                    LineComment = string.IsNullOrEmpty(lexical.LineComment) ? null : lexical.LineComment,
                    StringDelimiter = lexical.String?[0],
                    Continuation = lexical.Continuation == null ? null : Rx("lexical.continuation", lexical.Continuation),
                    ModuleName = _d.Module?.Name == null ? null : Rx("module.name", _d.Module.Name, "name"),
                    Blocks = blocks, Labels = labels, Declarations = declarations,
                    Parameters = BuildParameters(),
                    Types = new Dictionary<string, string>(_d.Types ?? new Dictionary<string, string>(), StringComparer.Ordinal),
                    References = (_d.References ?? new List<Yaml.Reference>()).Select((r, i) => BuildReference(r, i, ruleIds, declarations)).ToList(),
                    Fragments = fragments,
                };
            }

            /// <summary>Solo un blocco (o il modulo) contiene qualcosa: etichette e dichiarazioni no.</summary>
            private void CheckIn(string where, IReadOnlyList<string> parents, HashSet<string> containers)
            {
                foreach (var p in parents)
                    if (!containers.Contains(p)) throw Fail($"{where}: '{p}' non è una regola di blocco né 'module'", $"{where}: '{p}' is neither a block rule nor 'module'");
            }

            private string Kind(string where, string kind)
            {
                static bool Reserved(string k) => k == "external" || k == "module" || k == FragmentKind;
                if (string.IsNullOrWhiteSpace(kind) || Reserved(kind) || !CodeScriptParser.SymbolKinds.Contains(kind))
                    throw Fail($"{where}.kind: '{kind}' — ammessi: {string.Join(", ", CodeScriptParser.SymbolKinds.Where(k => !Reserved(k)))}",
                                $"{where}.kind: '{kind}' — allowed: {string.Join(", ", CodeScriptParser.SymbolKinds.Where(k => !Reserved(k)))}");
                return kind;
            }

            private BlockRule BuildBlock(Yaml.Block b, int i)
            {
                var where = $"blocks[{i}]";
                if (string.IsNullOrWhiteSpace(b.Id)) throw Fail($"{where}.id: obbligatorio", $"{where}.id: required");
                where = $"blocks[{b.Id}]";
                if (b.Begin == null || b.End == null) throw Fail($"{where}: servono begin e end", $"{where}: begin and end are required");
                return new BlockRule
                {
                    Id = b.Id, Kind = Kind(where, b.Kind), Begin = Rx(where + ".begin", b.Begin, "name"), End = Rx(where + ".end", b.End),
                    In = b.In is { Count: > 0 } ? b.In : new List<string> { ModuleRule },
                    NameTemplate = b.Name ?? "{name}", SignatureTemplate = b.Signature,
                };
            }

            private LabelRule BuildLabel(Yaml.Label l, int i)
            {
                var where = $"labels[{i}]";
                if (string.IsNullOrWhiteSpace(l.Id)) throw Fail($"{where}.id: obbligatorio", $"{where}.id: required");
                where = $"labels[{l.Id}]";
                if (l.At == null) throw Fail($"{where}.at: obbligatorio", $"{where}.at: required");
                return new LabelRule
                {
                    Id = l.Id, Kind = Kind(where, l.Kind), At = Rx(where + ".at", l.At, "name"),
                    In = l.In is { Count: > 0 } ? l.In : new List<string> { ModuleRule },
                    NameTemplate = l.Name ?? "{name}", OnlyIfReferenced = l.OnlyIfReferenced,
                };
            }

            private DeclarationRule BuildDeclaration(Yaml.Declaration x, int i)
            {
                var where = $"declarations[{i}]";
                if (string.IsNullOrWhiteSpace(x.Id)) throw Fail($"{where}.id: obbligatorio", $"{where}.id: required");
                where = $"declarations[{x.Id}]";
                if (x.At == null) throw Fail($"{where}.at: obbligatorio", $"{where}.at: required");
                var each = x.Each == null ? null : Rx(where + ".each", x.Each, "name");
                var at = each == null ? Rx(where + ".at", x.At, "name") : Rx(where + ".at", x.At, "list");
                var emit = x.Emit ?? true;
                return new DeclarationRule
                {
                    Id = x.Id, Kind = emit ? Kind(where, x.Kind) : x.Kind ?? "field", At = at, Each = each,
                    In = x.In is { Count: > 0 } ? x.In : new List<string> { ModuleRule },
                    NameTemplate = x.Name ?? "{name}", Emit = emit,
                };
            }

            private ParameterRule BuildParameters()
            {
                var p = _d.Parameters;
                if (p == null) return null;
                if (p.Item == null) throw Fail("parameters.item: obbligatorio", "parameters.item: required");
                var directions = new Dictionary<string, string>(p.Direction ?? new Dictionary<string, string>(), _d.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
                foreach (var (word, dir) in directions)
                    if (!CodeScriptParser.Directions.Contains(dir)) throw Fail($"parameters.direction.{word}: '{dir}' — ammesse: {string.Join(", ", CodeScriptParser.Directions)}", $"parameters.direction.{word}: '{dir}' — allowed: {string.Join(", ", CodeScriptParser.Directions)}");
                var def = p.Default ?? "in";
                if (!CodeScriptParser.Directions.Contains(def)) throw Fail($"parameters.default: '{def}' — ammesse: {string.Join(", ", CodeScriptParser.Directions)}", $"parameters.default: '{def}' — allowed: {string.Join(", ", CodeScriptParser.Directions)}");
                return new ParameterRule { Item = Rx("parameters.item", p.Item, "name"), Direction = directions, DefaultDirection = def, Sticky = p.Sticky };
            }

            private FragmentRule BuildFragments(List<BlockRule> blocks)
            {
                var f = _d.Fragments;
                if (f == null) return null;
                if (f.In == null || f.In.Count == 0) throw Fail("fragments.in: almeno una regola di blocco", "fragments.in: at least one block rule");
                foreach (var b in f.In)
                    if (!blocks.Any(x => x.Id == b)) throw Fail($"fragments.in: '{b}' non è una regola di blocco", $"fragments.in: '{b}' is not a block rule");
                if (f.At == null || f.Jump == null) throw Fail("fragments: servono at e jump", "fragments: at and jump are required");
                return new FragmentRule
                {
                    In = f.In, At = Rx("fragments.at", f.At, "name"), Jump = Rx("fragments.jump", f.Jump, "target"),
                    Stop = f.Stop == null ? null : Rx("fragments.stop", f.Stop),
                    Branches = f.Branches == null ? null
                        : f.Branches.Open == null || f.Branches.Case == null || f.Branches.Close == null
                            ? throw Fail("fragments.branches: servono open, case e close", "fragments.branches: open, case and close are required")
                            : new BranchRule { Open = Rx("fragments.branches.open", f.Branches.Open), Case = Rx("fragments.branches.case", f.Branches.Case), Close = Rx("fragments.branches.close", f.Branches.Close) },
                };
            }

            private ReferenceRule BuildReference(Yaml.Reference r, int i, HashSet<string> ruleIds, List<DeclarationRule> declarations)
            {
                var where = $"references[{i}]";
                // contains lo fanno i blocchi, jumps la sezione fragments
                if (!CodeScriptParser.Relations.Contains(r.Relation ?? string.Empty) || r.Relation == "contains" || r.Relation == JumpsRelation)
                    throw Fail($"{where}.relation: '{r.Relation}' — ammesse: {string.Join(", ", CodeScriptParser.Relations.Where(x => x != "contains" && x != JumpsRelation))}", $"{where}.relation: '{r.Relation}' — allowed: {string.Join(", ", CodeScriptParser.Relations.Where(x => x != "contains" && x != JumpsRelation))}");
                if (r.Pattern == null) throw Fail($"{where}.pattern: obbligatorio", $"{where}.pattern: required");
                var resolve = Enum<ResolveKind>(where + ".resolve", r.Resolve, null);
                var scope = Enum(where + ".scope", r.Scope, ScopeKind.Perimeter);
                if (r.Rule != null && r.Rules != null) throw Fail($"{where}: rule oppure rules, non tutti e due", $"{where}: rule or rules, not both");
                var rules = r.Rules ?? (r.Rule == null ? new List<string>() : new List<string> { r.Rule });
                if (resolve == ResolveKind.Symbol)
                {
                    if (rules.Count == 0) throw Fail($"{where}.rule: con resolve: symbol serve l'id di un blocco, di un'etichetta o di una dichiarazione", $"{where}.rule: resolve: symbol needs the id of a block, label or declaration rule");
                    foreach (var rule in rules)
                        if (!ruleIds.Contains(rule) || rule == ModuleRule)
                            throw Fail($"{where}.rule: '{rule}' — con resolve: symbol serve l'id di un blocco, di un'etichetta o di una dichiarazione", $"{where}.rule: '{rule}' — resolve: symbol needs the id of a block, label or declaration rule");
                }
                else if (rules.Count > 0 || r.Scope != null) throw Fail($"{where}: rule e scope valgono solo con resolve: symbol", $"{where}: rule and scope only apply to resolve: symbol");
                foreach (var u in r.Unless ?? new List<string>())
                    if (!declarations.Any(x => x.Id == u)) throw Fail($"{where}.unless: '{u}' non è una regola di declarations", $"{where}.unless: '{u}' is not a declarations rule");
                if (r.Confidence != null && !CodeScriptParser.Confidences.Contains(r.Confidence))
                    throw Fail($"{where}.confidence: '{r.Confidence}' — ammesse: {string.Join(", ", CodeScriptParser.Confidences)}", $"{where}.confidence: '{r.Confidence}' — allowed: {string.Join(", ", CodeScriptParser.Confidences)}");
                var unresolved = r.Unresolved ?? "ignore";
                if (unresolved != "ignore" && unresolved != "external") throw Fail($"{where}.unresolved: '{unresolved}' — ammessi: ignore, external", $"{where}.unresolved: '{unresolved}' — allowed: ignore, external");
                return new ReferenceRule
                {
                    Relation = r.Relation, Pattern = Rx(where + ".pattern", r.Pattern, "target"),
                    Text = Enum(where + ".text", r.Text, TextKind.Code),
                    After = r.After == null ? null : Rx(where + ".after", r.After), NotAfter = r.NotAfter == null ? null : Rx(where + ".notAfter", r.NotAfter),
                    Strip = r.Strip == null ? null : Rx(where + ".strip", r.Strip),
                    Resolve = resolve, Rules = rules, Scope = scope, Confidence = r.Confidence, ExternalWhenUnresolved = unresolved == "external",
                    Ignore = new HashSet<string>(r.Ignore ?? new List<string>(), _d.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal),
                    Unless = r.Unless ?? new List<string>(),
                };
            }

            private T Enum<T>(string where, string value, T? fallback) where T : struct, System.Enum
            {
                if (value == null)
                {
                    if (fallback.HasValue) return fallback.Value;
                    throw Fail($"{where}: obbligatorio ({Names<T>()})", $"{where}: required ({Names<T>()})");
                }
                if (System.Enum.TryParse<T>(value, true, out var parsed) && System.Enum.IsDefined(parsed) && !int.TryParse(value, out _)) return parsed;
                throw Fail($"{where}: '{value}' — ammessi: {Names<T>()}", $"{where}: '{value}' — allowed: {Names<T>()}");
            }

            private T Enum<T>(string where, string value, T fallback) where T : struct, System.Enum => Enum<T>(where, value, (T?)fallback);

            private static string Names<T>() where T : struct, System.Enum => string.Join(", ", System.Enum.GetNames<T>().Select(n => char.ToLowerInvariant(n[0]) + n[1..]));

            private Regex Rx(string where, string pattern, params string[] requiredGroups)
            {
                Regex rx;
                try { rx = new Regex(pattern, _options, RegexTimeout); }
                catch (ArgumentException ex) { throw Fail($"{where}: l'espressione regolare non compila ({ex.Message})", $"{where}: the regular expression does not compile ({ex.Message})"); }
                foreach (var g in requiredGroups)
                    if (!rx.GetGroupNames().Contains(g)) throw Fail($"{where}: manca il gruppo (?<{g}>…)", $"{where}: the group (?<{g}>…) is missing");
                return rx;
            }
        }

        /// <summary>La forma del file YAML: solo DTO, i controlli stanno nel <see cref="Builder"/>.</summary>
        private static class Yaml
        {
            public sealed class Definition
            {
                public int Schema { get; set; }
                public string Id { get; set; }
                public string Name { get; set; }
                public int Version { get; set; }
                public List<string> Extensions { get; set; }
                public bool IgnoreCase { get; set; }
                public Lexical Lexical { get; set; }
                public Module Module { get; set; }
                public List<Block> Blocks { get; set; }
                public List<Label> Labels { get; set; }
                public List<Declaration> Declarations { get; set; }
                public Parameters Parameters { get; set; }
                public Dictionary<string, string> Types { get; set; }
                public List<Reference> References { get; set; }
                public Fragments Fragments { get; set; }
            }
            public sealed class Fragments
            {
                public List<string> In { get; set; }
                public string At { get; set; }
                public string Jump { get; set; }
                public string Stop { get; set; }
                public Branches Branches { get; set; }
            }
            public sealed class Branches
            {
                public string Open { get; set; }
                public string Case { get; set; }
                public string Close { get; set; }
            }
            public sealed class Lexical
            {
                public string LineComment { get; set; }
                public string String { get; set; }
                public string Continuation { get; set; }
            }
            public sealed class Module { public string Name { get; set; } }
            public sealed class Block
            {
                public string Id { get; set; }
                public string Kind { get; set; }
                public string Begin { get; set; }
                public string End { get; set; }
                public List<string> In { get; set; }
                public string Name { get; set; }
                public string Signature { get; set; }
            }
            public sealed class Label
            {
                public string Id { get; set; }
                public string Kind { get; set; }
                public string At { get; set; }
                public List<string> In { get; set; }
                public string Name { get; set; }
                public bool OnlyIfReferenced { get; set; }
            }
            public sealed class Declaration
            {
                public string Id { get; set; }
                public string Kind { get; set; }
                public string At { get; set; }
                public string Each { get; set; }
                public List<string> In { get; set; }
                public string Name { get; set; }
                public bool? Emit { get; set; }
            }
            public sealed class Parameters
            {
                public bool Sticky { get; set; }
                public string Item { get; set; }
                public Dictionary<string, string> Direction { get; set; }
                public string Default { get; set; }
            }
            public sealed class Reference
            {
                public string Relation { get; set; }
                public string Pattern { get; set; }
                public string Text { get; set; }
                public string After { get; set; }
                public string NotAfter { get; set; }
                public string Strip { get; set; }
                public string Resolve { get; set; }
                public string Rule { get; set; }
                public List<string> Rules { get; set; }
                public List<string> Ignore { get; set; }
                public List<string> Unless { get; set; }
                public string Scope { get; set; }
                public string Confidence { get; set; }
                public string Unresolved { get; set; }
            }
        }
    }

    /// <summary>Una definizione di linguaggio che non si può usare: il messaggio dice il file e il campo.</summary>
    public sealed class LanguageDefinitionException : Exception
    {
        public string DefinitionPath { get; }
        /// <summary>Il motivo senza il percorso davanti: per chi il percorso lo mostra già (la finestra «Ambiente di sviluppo»).</summary>
        public string Reason { get; }

        public LanguageDefinitionException(string path, string message, Exception inner = null)
            : base($"{path}: {message}", inner)
        {
            DefinitionPath = path;
            Reason = message;
        }
    }
}
