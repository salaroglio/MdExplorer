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
    /// Un linguaggio definito nel progetto di documentazione: il file <c>language.yml</c> di un pacchetto
    /// (<see cref="LanguagePackage"/>, <c>.mde-languages/&lt;id&gt;/</c>), di regole dichiarative — blocchi aperti e chiusi da parole chiave, etichette,
    /// parametri, riferimenti — letto da <see cref="DeclarativeLanguageExtractor"/>. Niente codice eseguibile dal repository:
    /// solo espressioni regolari, con un tempo massimo. Lettura <b>rigida</b>: una chiave che il formato non conosce, una
    /// regex che non compila, un gruppo obbligatorio che manca o un riferimento a una regola inesistente sono errori con il
    /// file e il campo, mai righe ignorate.
    /// </summary>
    public sealed class DeclarativeLanguage
    {
        public const string FolderName = ".mde-languages";
        /// <summary>Il formato di prima, un file piatto <c>.mde-languages/&lt;id&gt;.lang.yml</c>: si sposta nel pacchetto (<see cref="LanguagePackage.MigrateFlat"/>).</summary>
        public const string LegacyFileSuffix = ".lang.yml";
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
        /// <summary>Le pagine a direttive (D42 passo 2): con questa sezione il linguaggio è un dialetto di pagine, letto da <see cref="TemplatePageExtractor"/>; null per i linguaggi a righe.</summary>
        public TemplateRule Template { get; private init; }

        /// <summary>
        /// Un dialetto di pagine HTML che il server riempie (D42 passo 2). L'HTML (controlli, script) lo legge MdExplorer,
        /// uguale per tutti; il dialetto dice come riconoscere il resto, in due forme che si possono anche unire:
        /// <list type="bullet">
        /// <item>la forma <b>a direttive</b> (<c>$include[x]$</c>, <c>$if[A==S]$…$endif$</c>): una sola espressione
        /// <see cref="Directive"/> (gruppi <c>word</c> e, facoltativo, <c>arg</c>) e il significato delle parole;</item>
        /// <item>la forma <b>a riconoscitori</b> (<see cref="Recognizers"/>), per le tecnologie dove il server mescola codice e
        /// HTML (Razor, JSP, Thymeleaf…): ogni espressione ha un ruolo (include, condizione, ramo, chiusura, chiamata), e un
        /// blocco può chiudersi alla graffa corrispondente (24/10/2026, utente: «il lettore lo genera l'LLM del cliente»).</item>
        /// </list>
        /// </summary>
        public sealed class TemplateRule
        {
            /// <summary>La forma a direttive; null se il dialetto usa solo i riconoscitori.</summary>
            public Regex Directive { get; init; }
            /// <summary>Le parole il cui argomento è un'altra pagina: arco <c>includes</c>.</summary>
            public ISet<string> Include { get; init; }
            public IReadOnlyList<TemplateBlock> Blocks { get; init; }
            public IReadOnlyList<TemplateRecognizer> Recognizers { get; init; }
            /// <summary>I nomi degli attributi evento di un elemento (default <c>on…</c>; Angular <c>(click)</c>, Vue <c>@click</c>).</summary>
            public IReadOnlyList<Regex> Events { get; init; }
            /// <summary>
            /// Che cosa dentro una stringa la rende scritta dal server (non letterale): <c>'@Model.Id'</c>, <c>'$codice$'</c>.
            /// Default: la <see cref="Directive"/>; null = nessuna.
            /// </summary>
            public Regex Dynamic { get; init; }
        }

        public enum TemplateRole { Include, Open, Else, Case, Close, Call }

        /// <summary>
        /// Un riconoscitore: un'espressione con un ruolo. <c>include</c> (gruppo <c>arg</c> = la pagina), <c>open</c> (apre un
        /// blocco; con <see cref="Guard"/> il gruppo <c>arg</c> è la condizione; si chiude alla graffa corrispondente con
        /// <c>close: brace</c>, o con il riconoscitore <c>close</c> che ha l'id scritto in <see cref="Close"/>), <c>else</c>
        /// (nega la condizione: dentro il blocco aperto, o con <c>close: brace</c> un blocco nuovo dopo quello appena chiuso),
        /// <c>case</c> (<c>condizione=caso</c>), <c>close</c>, <c>call</c> (una chiamata con stringhe: il nome da
        /// <see cref="Callee"/> o dal gruppo <c>callee</c>, gli argomenti dai gruppi <c>a0</c>…<c>a9</c>; dentro un tag fa di
        /// quell'elemento un controllo).
        /// </summary>
        public sealed class TemplateRecognizer
        {
            public string Id { get; init; }
            public TemplateRole Role { get; init; }
            public Regex Match { get; init; }
            /// <summary><c>brace</c>, l'id di un riconoscitore <c>close</c>, o null.</summary>
            public string Close { get; init; }
            public bool Brace => Close == BraceClose;
            public bool Guard { get; init; }
            public string Callee { get; init; }
            /// <summary>Gli indici dei gruppi <c>a0</c>…<c>a9</c> presenti nell'espressione.</summary>
            public IReadOnlyList<int> ArgGroups { get; init; }
        }

        public const string BraceClose = "brace";

        /// <summary>
        /// Una parte della pagina aperta e chiusa da due parole (<c>if</c>/<c>endif</c>, <c>loop</c>/<c>endloop</c>). Con
        /// <see cref="Guard"/> l'argomento dell'apertura è la condizione di quello che contiene; <see cref="Else"/> la nega,
        /// <see cref="Cases"/> la sostituisce con <c>argomento=caso</c> (un <c>switch</c>).
        /// </summary>
        public sealed class TemplateBlock
        {
            public string Open { get; init; }
            public string Close { get; init; }
            public IReadOnlyList<string> Else { get; init; }
            public IReadOnlyList<string> Cases { get; init; }
            public bool Guard { get; init; }
        }

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
                // l'id è il nome della cartella del pacchetto (.mde-languages/toy/language.yml → toy)
                var folderId = Path.GetFileName(Path.GetDirectoryName(_path) ?? string.Empty);
                if (string.IsNullOrWhiteSpace(_d.Id)) throw Fail("id: obbligatorio", "id: required");
                if (!Regex.IsMatch(_d.Id, "^[a-z][a-z0-9-]*$"))
                    throw Fail($"id: '{_d.Id}' — solo minuscole, cifre e trattini, e comincia con una lettera", $"id: '{_d.Id}' — lowercase letters, digits and dashes only, starting with a letter");
                if (!string.Equals(_d.Id, folderId, StringComparison.Ordinal))
                    throw Fail($"id: '{_d.Id}' deve essere uguale al nome della cartella del pacchetto ('{folderId}')", $"id: '{_d.Id}' must match the package folder name ('{folderId}')");
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
                var template = BuildTemplate();

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
                    Fragments = fragments, Template = template,
                };
            }

            /// <summary>
            /// La sezione <c>template</c>: esclude le regole dei linguaggi a righe (una pagina non ha blocchi né etichette), e
            /// ogni parola ha un solo significato.
            /// </summary>
            private TemplateRule BuildTemplate()
            {
                var t = _d.Template;
                if (t == null) return null;
                var lineRules = new (string Name, bool Present)[]
                {
                    ("lexical", _d.Lexical != null), ("module", _d.Module != null), ("blocks", _d.Blocks != null), ("labels", _d.Labels != null),
                    ("declarations", _d.Declarations != null), ("parameters", _d.Parameters != null), ("types", _d.Types != null),
                    ("references", _d.References != null), ("fragments", _d.Fragments != null),
                };
                foreach (var (name, present) in lineRules)
                    if (present) throw Fail($"{name}: con template non vale (è dei linguaggi a righe); togli una delle due sezioni", $"{name}: does not apply with template (it is for line-based languages); remove one of the two sections");
                var comparer = _d.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

                // la forma a direttive
                Regex directive = null;
                var blocks = new List<TemplateBlock>();
                var meaning = new Dictionary<string, string>(comparer);
                if (string.IsNullOrWhiteSpace(t.Directive))
                {
                    if (t.Include != null || t.Blocks != null) throw Fail("template.include e template.blocks sono della forma a direttive: serve anche template.directive", "template.include and template.blocks belong to the directive form: template.directive is needed too");
                }
                else
                {
                    directive = Rx("template.directive", t.Directive, "word");
                    void Word(string where, string word)
                    {
                        if (string.IsNullOrWhiteSpace(word)) throw Fail($"{where}: parola vuota", $"{where}: empty word");
                        if (!meaning.TryAdd(word, where)) throw Fail($"{where}: '{word}' è già in {meaning[word]}: una parola ha un solo significato", $"{where}: '{word}' is already in {meaning[word]}: a word has one meaning only");
                    }
                    foreach (var w in t.Include ?? new List<string>()) Word("template.include", w);
                    foreach (var (b, i) in (t.Blocks ?? new List<Yaml.TemplateBlock>()).Select((b, i) => (b, i)))
                    {
                        var where = $"template.blocks[{i}]";
                        if (string.IsNullOrWhiteSpace(b.Open) || string.IsNullOrWhiteSpace(b.Close)) throw Fail($"{where}: servono open e close", $"{where}: open and close are required");
                        if ((b.Else != null || b.Cases != null) && !b.Guard) throw Fail($"{where}: else e cases valgono solo con guard: true", $"{where}: else and cases only apply with guard: true");
                        Word(where + ".open", b.Open);
                        Word(where + ".close", b.Close);
                        foreach (var w in b.Else ?? new List<string>()) Word(where + ".else", w);
                        foreach (var w in b.Cases ?? new List<string>()) Word(where + ".cases", w);
                        blocks.Add(new TemplateBlock
                        {
                            Open = b.Open, Close = b.Close, Else = b.Else ?? new List<string>(), Cases = b.Cases ?? new List<string>(), Guard = b.Guard,
                        });
                    }
                    if (meaning.Count == 0) throw Fail("template: con directive serve almeno una parola in include o in blocks", "template: with directive at least one word in include or blocks is needed");
                }

                // la forma a riconoscitori
                var recognizers = new List<TemplateRecognizer>();
                var ids = new HashSet<string>(StringComparer.Ordinal);
                var raw = t.Recognizers ?? new List<Yaml.TemplateRecognizer>();
                foreach (var (r, i) in raw.Select((r, i) => (r, i)))
                    if (r.Id != null && !ids.Add(r.Id)) throw Fail($"template.recognizers[{i}].id: '{r.Id}' ripetuto", $"template.recognizers[{i}].id: '{r.Id}' repeated");
                var closers = raw.Where(r => string.Equals(r.Role, "close", StringComparison.OrdinalIgnoreCase)).Select(r => r.Id).ToHashSet(StringComparer.Ordinal);
                foreach (var (r, i) in raw.Select((r, i) => (r, i)))
                {
                    var where = r.Id == null ? $"template.recognizers[{i}]" : $"template.recognizers[{r.Id}]";
                    var role = Enum<TemplateRole>(where + ".role", r.Role, null);
                    if (string.IsNullOrWhiteSpace(r.Match)) throw Fail($"{where}.match: obbligatorio", $"{where}.match: required");
                    var match = role == TemplateRole.Include ? Rx(where + ".match", r.Match, "arg") : Rx(where + ".match", r.Match);
                    if (role == TemplateRole.Close && r.Id == null) throw Fail($"{where}.id: un riconoscitore close ha un id, quello che gli open scrivono in close", $"{where}.id: a close recognizer has an id, the one open recognizers write in close");
                    if (role == TemplateRole.Open && r.Close == null) throw Fail($"{where}.close: obbligatorio per open: brace, o l'id di un riconoscitore close", $"{where}.close: required for open: brace, or the id of a close recognizer");
                    if (r.Close != null)
                    {
                        if (role is not (TemplateRole.Open or TemplateRole.Else or TemplateRole.Case)) throw Fail($"{where}.close: vale solo per open, else e case", $"{where}.close: only applies to open, else and case");
                        if (r.Close != BraceClose && (role != TemplateRole.Open || !closers.Contains(r.Close)))
                            throw Fail($"{where}.close: '{r.Close}' — brace{(role == TemplateRole.Open ? ", o l'id di un riconoscitore close" : "")}", $"{where}.close: '{r.Close}' — brace{(role == TemplateRole.Open ? ", or the id of a close recognizer" : "")}");
                    }
                    if (r.Guard && role != TemplateRole.Open) throw Fail($"{where}.guard: vale solo per open", $"{where}.guard: only applies to open");
                    var argGroups = Enumerable.Range(0, 10).Where(k => match.GetGroupNames().Contains("a" + k)).ToList();
                    if (role == TemplateRole.Call)
                    {
                        if (string.IsNullOrWhiteSpace(r.Callee) && !match.GetGroupNames().Contains("callee"))
                            throw Fail($"{where}: una chiamata ha un nome, in callee o nel gruppo (?<callee>…)", $"{where}: a call has a name, in callee or in the group (?<callee>…)");
                    }
                    else if (r.Callee != null || argGroups.Count > 0) throw Fail($"{where}: callee e i gruppi a0…a9 valgono solo per call", $"{where}: callee and the groups a0…a9 only apply to call");
                    recognizers.Add(new TemplateRecognizer
                    {
                        Id = r.Id ?? $"{role.ToString().ToLowerInvariant()}#{i}", Role = role, Match = match, Close = r.Close, Guard = r.Guard,
                        Callee = string.IsNullOrWhiteSpace(r.Callee) ? null : r.Callee.Trim(), ArgGroups = argGroups,
                    });
                }
                if (directive == null && recognizers.Count == 0) throw Fail("template: serve directive (forma a direttive) o recognizers (forma a riconoscitori)", "template: directive (directive form) or recognizers (recognizer form) is needed");

                var events = (t.Events ?? new List<string> { "^on.+$" }).Select((e, i) => RxIgnoreCase($"template.events[{i}]", e)).ToList();
                var dynamic = t.Dynamic != null ? Rx("template.dynamic", t.Dynamic) : directive;
                return new TemplateRule
                {
                    Directive = directive, Include = new HashSet<string>(t.Include ?? new List<string>(), comparer), Blocks = blocks,
                    Recognizers = recognizers, Events = events, Dynamic = dynamic,
                };
            }

            /// <summary>I nomi degli attributi HTML non badano alle maiuscole, qualunque sia ignoreCase.</summary>
            private Regex RxIgnoreCase(string where, string pattern)
            {
                try { return new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, RegexTimeout); }
                catch (ArgumentException ex) { throw Fail($"{where}: l'espressione regolare non compila ({ex.Message})", $"{where}: the regular expression does not compile ({ex.Message})"); }
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
                public Template Template { get; set; }
            }
            public sealed class Template
            {
                public string Directive { get; set; }
                public List<string> Include { get; set; }
                public List<TemplateBlock> Blocks { get; set; }
                public List<TemplateRecognizer> Recognizers { get; set; }
                public List<string> Events { get; set; }
                public string Dynamic { get; set; }
            }
            public sealed class TemplateRecognizer
            {
                public string Id { get; set; }
                public string Role { get; set; }
                public string Match { get; set; }
                public string Close { get; set; }
                public bool Guard { get; set; }
                public string Callee { get; set; }
            }
            public sealed class TemplateBlock
            {
                public string Open { get; set; }
                public string Close { get; set; }
                public List<string> Else { get; set; }
                public List<string> Cases { get; set; }
                public bool Guard { get; set; }
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
