using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Features.Templates
{
    /// <summary>
    /// Il manifesto di un template di progetto, <c>.mde-template/&lt;id&gt;/mde-template.yml</c>
    /// (docs-internal/Sprints/2026-10-17-Template-Di-Progetto.md, P0). Dice che cos'è il template, quale ramo prendere per
    /// ogni lingua e che cosa MDE deve scrivere nei file comuni del progetto.
    /// </summary>
    public sealed class TemplateManifest
    {
        public const string Folder = ".mde-template";
        public const string FileName = "mde-template.yml";
        public const string Method = "method";
        public const string Project = "project";
        /// <summary>
        /// Lo strato comune (sprint 2026-10-19-Metodi-Componibili.md, MC2): la scrittura sistematica dei documenti, che c'è in
        /// ogni progetto fatto di metodi. Ha le regole di un metodo (la zona, gli agenti con l'id), ma è sempre il primo.
        /// </summary>
        public const string Foundation = "foundation";

        public int MdeTemplate { get; set; }
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public TemplateRequires Requires { get; set; }
        public Dictionary<string, TemplateLanguage> Languages { get; set; }
        public List<TemplateRole> Roles { get; set; }
        public List<string> Workflows { get; set; }
        public string Instructions { get; set; }
        /// <summary>I file iniziali: copiati una volta nel progetto, se mancano; da lì sono del gruppo (P5).</summary>
        public List<TemplateSeed> Seeds { get; set; }
        /// <summary>
        /// Le cartelle di codice del template (P7, di solito quelle dell'esempio): MDE le segna come <c>program</c> con i loro
        /// linguaggi in <c>.development.yml</c>, come farebbe la finestra «Ambiente di sviluppo», se non ci sono già.
        /// </summary>
        public List<TemplateProgram> Programs { get; set; }
        /// <summary>
        /// La presentazione della vetrina di «Crea nuovo progetto» (sprint 2026-10-19-Vetrina-Dei-Template.md), una per lingua
        /// delle slide: lingua → file nel ramo. Facoltativa: senza, MDE mostra una slide di circostanza. Le slide inglesi sono
        /// quelle di riserva per chi sceglie una lingua che il template non parla (VD7).
        /// </summary>
        public Dictionary<string, string> Slides { get; set; }
        /// <summary>
        /// La teoria del metodo (MC1, MC5), una per lingua: la cartella nella zona e quella del progetto in cui si copia, se chi
        /// crea il progetto la chiede, o dopo. Facoltativa; un template di progetto non ne ha (la demo ha tutto incluso, MC6).
        /// </summary>
        public Dictionary<string, TemplateTheory> Theory { get; set; }

        /// <summary>La lingua delle slide di riserva (VD7).</summary>
        public const string SlidesFallbackLanguage = "en";

        /// <summary>Il file delle slide da mostrare in <paramref name="language"/>: le sue, o le inglesi; null = slide di circostanza.</summary>
        public (string Language, string File)? SlidesFor(string language)
        {
            if (Slides == null) return null;
            if (Slides.TryGetValue(language ?? string.Empty, out var own)) return (language, own);
            if (Slides.TryGetValue(SlidesFallbackLanguage, out var english)) return (SlidesFallbackLanguage, english);
            return null;
        }

        public bool IsMethod => Kind == Method;
        public bool IsFoundation => Kind == Foundation;
        /// <summary>Un metodo o lo strato comune: porta solo la sua zona e le sue skill, e gli agenti con il suo id.</summary>
        public bool IsZoned => Kind == Method || Kind == Foundation;

        /// <summary>Il <c>kind</c> della riga in <c>.development.yml</c>: assente per un metodo.</summary>
        public string RowKind => IsMethod ? null : Kind;

        /// <summary>La teoria in <paramref name="language"/>; un problema detto se il template non ne ha in quella lingua.</summary>
        public TemplateTheory TheoryFor(string language)
            => Theory != null && Theory.TryGetValue(language ?? string.Empty, out var theory)
                ? theory
                : throw new InvalidOperationException(UiText.Say(
                    $"Il template «{Title}» non ha la teoria in '{language}'" + (Theory?.Count > 0 ? $": c'è in {string.Join(", ", Theory.Keys)}." : "."),
                    $"The template «{Title}» has no theory in '{language}'" + (Theory?.Count > 0 ? $": there is in {string.Join(", ", Theory.Keys)}." : ".")));

        /// <summary>La cartella del template nel progetto: <c>.mde-template/&lt;id&gt;/</c>.</summary>
        public static string ZoneOf(string id) => $"{Folder}/{id}/";

        /// <summary>Il percorso del manifesto di <paramref name="id"/>, dalla radice del progetto.</summary>
        public static string PathOf(string id) => ZoneOf(id) + FileName;

        /// <summary>Il ramo da unire per una lingua: la struttura, o l'esempio compilato.</summary>
        public string BranchFor(string language, bool example)
        {
            if (Languages == null || !Languages.TryGetValue(language ?? string.Empty, out var branches))
                throw new InvalidOperationException(UiText.Say(
                    $"Il template «{Title}» non c'è nella lingua '{language}': ci sono {string.Join(", ", Languages?.Keys ?? Enumerable.Empty<string>())}.",
                    $"The template «{Title}» is not available in the language '{language}': there are {string.Join(", ", Languages?.Keys ?? Enumerable.Empty<string>())}."));
            if (!example) return branches.Structure;
            if (string.IsNullOrWhiteSpace(branches.Example))
                throw new InvalidOperationException(UiText.Say(
                    $"Il template «{Title}» non ha un esempio in '{language}': si può prendere solo la struttura.",
                    $"The template «{Title}» has no example in '{language}': only the structure can be taken."));
            return branches.Example;
        }
    }

    public sealed class TemplateRequires
    {
        /// <summary>La versione minima di MdExplorer, nel formato dell'app (<c>2026.10.17</c>).</summary>
        public string Mde { get; set; }
        /// <summary>Gli altri metodi senza cui questo non sta in piedi.</summary>
        public List<string> Templates { get; set; }
        /// <summary>
        /// Le funzioni di MDE da accendere per il progetto (P6), fra <see cref="KnownFeatures"/>: MDE non le accende da sé,
        /// dice con una nota se sono spente e dove si accendono.
        /// </summary>
        public List<string> Features { get; set; }

        public const string Fuseki = "fuseki";
        /// <summary>Il grafo del codice: almeno una cartella <c>program</c> con i suoi linguaggi (P7).</summary>
        public const string Code = "code";
        public static readonly IReadOnlyList<string> KnownFeatures = new[] { Fuseki, Code };
    }

    public sealed class TemplateLanguage
    {
        public string Structure { get; set; }
        public string Example { get; set; }
    }

    /// <summary>Una cartella di codice: <c>path</c> dalla radice del progetto, <c>languages</c> gli id del catalogo (<c>csharp</c>…).</summary>
    public sealed class TemplateProgram
    {
        public string Path { get; set; }
        public List<string> Languages { get; set; }
    }

    /// <summary>La teoria in una lingua: <c>from</c> è la cartella nella zona, <c>to</c> quella del progetto in cui si copia.</summary>
    public sealed class TemplateTheory
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    /// <summary>Un file iniziale: <c>from</c> sta nel template, <c>to</c> è dove nasce nel progetto.</summary>
    public sealed class TemplateSeed
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    public sealed class TemplateRole
    {
        public string Role { get; set; }
        public string Description { get; set; }
        public List<string> Agents { get; set; }
    }

    /// <summary>Legge il manifesto con tolleranza zero: una chiave sconosciuta o un valore sbagliato sono un problema detto.</summary>
    public static class TemplateManifestReader
    {
        public const int FormatVersion = 1;

        private static readonly Regex Kebab = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex SemVer = new(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$", RegexOptions.Compiled);
        private static readonly Regex AppVersion = new(@"^\d{4}\.\d{1,2}\.\d{1,2}(\.\d+)?$", RegexOptions.Compiled);
        private static readonly Regex LanguageCode = new(@"^[a-z]{2}$", RegexOptions.Compiled);

        /// <summary>Il manifesto, o lo stesso con l'elenco dei problemi pieno.</summary>
        /// <param name="folderId">Il nome della cartella sotto <c>.mde-template/</c> in cui sta il manifesto: deve essere l'id.</param>
        public static TemplateManifest Read(string yaml, string folderId, out List<string> problems)
        {
            problems = new List<string>();
            TemplateManifest m;
            try
            {
                m = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build()
                    .Deserialize<TemplateManifest>(yaml ?? string.Empty);
            }
            catch (YamlException ex)
            {
                problems.Add(UiText.Say($"riga {ex.Start.Line}: {ex.InnerException?.Message ?? ex.Message}",
                                        $"line {ex.Start.Line}: {ex.InnerException?.Message ?? ex.Message}"));
                return null;
            }
            if (m == null) { problems.Add(UiText.Say("il manifesto è vuoto", "the manifest is empty")); return null; }

            if (m.MdeTemplate != FormatVersion)
                problems.Add(UiText.Say($"mde_template: {m.MdeTemplate} non è un formato che questo MdExplorer conosce (conosce {FormatVersion}): aggiorna MdExplorer.",
                                        $"mde_template: {m.MdeTemplate} is not a format this MdExplorer knows (it knows {FormatVersion}): update MdExplorer."));
            if (string.IsNullOrWhiteSpace(m.Id) || !Kebab.IsMatch(m.Id))
                problems.Add(UiText.Say($"id '{m.Id}': serve minuscole, cifre e trattini (es. gara-iso)", $"id '{m.Id}': lowercase letters, digits and dashes are needed (e.g. gara-iso)"));
            else if (m.Id != folderId)
                problems.Add(UiText.Say($"id '{m.Id}' ma il manifesto sta in {TemplateManifest.Folder}/{folderId}/: devono essere uguali",
                                        $"id '{m.Id}' but the manifest is in {TemplateManifest.Folder}/{folderId}/: they must be the same"));
            if (m.Kind != TemplateManifest.Method && m.Kind != TemplateManifest.Project && m.Kind != TemplateManifest.Foundation)
                problems.Add(UiText.Say($"kind '{m.Kind}': vale method, foundation o project", $"kind '{m.Kind}': it is method, foundation or project"));
            if (string.IsNullOrWhiteSpace(m.Title))
                problems.Add(UiText.Say("manca title", "title is missing"));
            if (string.IsNullOrWhiteSpace(m.Version) || !SemVer.IsMatch(m.Version))
                problems.Add(UiText.Say($"version '{m.Version}': serve x.y.z (es. 1.2.0)", $"version '{m.Version}': x.y.z is needed (e.g. 1.2.0)"));

            if (m.Requires?.Mde != null && !AppVersion.IsMatch(m.Requires.Mde))
                problems.Add(UiText.Say($"requires.mde '{m.Requires.Mde}': serve la versione di MdExplorer (es. 2026.10.17)",
                                        $"requires.mde '{m.Requires.Mde}': the MdExplorer version is needed (e.g. 2026.10.17)"));
            foreach (var other in m.Requires?.Templates ?? new List<string>())
                if (string.IsNullOrWhiteSpace(other) || !Kebab.IsMatch(other) || other == m.Id)
                    problems.Add(UiText.Say($"requires.templates: '{other}' non è l'id di un altro template", $"requires.templates: '{other}' is not the id of another template"));
            var features = new HashSet<string>(StringComparer.Ordinal);
            foreach (var feature in m.Requires?.Features ?? new List<string>())
            {
                if (!TemplateRequires.KnownFeatures.Contains(feature))
                    problems.Add(UiText.Say($"requires.features: '{feature}' non è una funzione che questo MdExplorer conosce ({string.Join(", ", TemplateRequires.KnownFeatures)}): aggiorna MdExplorer o correggi il manifesto",
                                            $"requires.features: '{feature}' is not a feature this MdExplorer knows ({string.Join(", ", TemplateRequires.KnownFeatures)}): update MdExplorer or fix the manifest"));
                else if (!features.Add(feature))
                    problems.Add(UiText.Say($"requires.features: '{feature}' compare due volte", $"requires.features: '{feature}' appears twice"));
            }

            if (m.Languages == null || m.Languages.Count == 0)
                problems.Add(UiText.Say("languages: serve almeno una lingua con il suo ramo", "languages: at least one language with its branch is needed"));
            else
                foreach (var (code, branches) in m.Languages)
                {
                    if (!LanguageCode.IsMatch(code ?? string.Empty))
                        problems.Add(UiText.Say($"languages: '{code}' non è un codice di lingua (it, en…)", $"languages: '{code}' is not a language code (it, en…)"));
                    if (string.IsNullOrWhiteSpace(branches?.Structure))
                        problems.Add(UiText.Say($"languages.{code}: manca structure, il ramo della struttura", $"languages.{code}: structure, the branch of the structure, is missing"));
                }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in m.Roles ?? new List<TemplateRole>())
            {
                if (string.IsNullOrWhiteSpace(r?.Role)) { problems.Add(UiText.Say("roles: un ruolo senza role", "roles: a role without role")); continue; }
                if (!seen.Add(r.Role.Trim()))
                    problems.Add(UiText.Say($"roles: '{r.Role}' compare due volte", $"roles: '{r.Role}' appears twice"));
                foreach (var a in r.Agents ?? new List<string>())
                    if (m.IsZoned && m.Id != null && !(a ?? string.Empty).StartsWith(m.Id + "-", StringComparison.Ordinal))
                        problems.Add(UiText.Say($"roles: l'agente '{a}' non comincia con '{m.Id}-': gli agenti di un metodo portano il suo id",
                                                $"roles: the agent '{a}' does not start with '{m.Id}-': a method's agents carry its id"));
            }

            foreach (var w in m.Workflows ?? new List<string>()) CheckPath(m, "workflows", w, problems);
            foreach (var (code, file) in m.Slides ?? new Dictionary<string, string>())
            {
                if (!LanguageCode.IsMatch(code ?? string.Empty))
                    problems.Add(UiText.Say($"slides: '{code}' non è un codice di lingua (it, en…)", $"slides: '{code}' is not a language code (it, en…)"));
                CheckPath(m, $"slides.{code}", file, problems);
                if (file != null && !file.EndsWith(".md", StringComparison.Ordinal))
                    problems.Add(UiText.Say($"slides.{code}: '{file}' deve essere una presentazione markdown (.md)", $"slides.{code}: '{file}' must be a markdown presentation (.md)"));
            }
            if (m.Instructions != null) CheckPath(m, "instructions", m.Instructions, problems);

            if (m.Theory?.Count > 0 && m.Kind == TemplateManifest.Project)
                problems.Add(UiText.Say("theory: un template di progetto ha già tutto incluso; la teoria è di un metodo o dello strato comune",
                                        "theory: a project template already has everything included; theory belongs to a method or the common layer"));
            foreach (var (code, theory) in m.Theory ?? new Dictionary<string, TemplateTheory>())
            {
                if (!LanguageCode.IsMatch(code ?? string.Empty))
                    problems.Add(UiText.Say($"theory: '{code}' non è un codice di lingua (it, en…)", $"theory: '{code}' is not a language code (it, en…)"));
                else if (m.Languages?.ContainsKey(code) != true)
                    problems.Add(UiText.Say($"theory.{code}: il template non è in '{code}' (languages)", $"theory.{code}: the template is not in '{code}' (languages)"));
                if (theory == null) { problems.Add(UiText.Say($"theory.{code}: servono from e to", $"theory.{code}: from and to are needed")); continue; }
                CheckPath(m, $"theory.{code}.from", theory.From, problems);
                if (!IsProjectPath(theory.To) || !IsGroupPath(theory.To))
                    problems.Add(UiText.Say($"theory.{code}.to: '{theory.To}' deve essere una cartella del gruppo, dalla radice del progetto, con «/»",
                                            $"theory.{code}.to: '{theory.To}' must be a folder of the group, from the project root, with «/»"));
            }

            var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in m.Seeds ?? new List<TemplateSeed>())
            {
                if (s == null) { problems.Add(UiText.Say("seeds: un seme vuoto", "seeds: an empty seed")); continue; }
                CheckPath(m, "seeds.from", s.From, problems);
                if (!IsProjectPath(s.To))
                    problems.Add(UiText.Say($"seeds.to: '{s.To}' deve essere un percorso dalla radice del progetto, con «/»",
                                            $"seeds.to: '{s.To}' must be a path from the project root, with «/»"));
                else if (!IsGroupPath(s.To))
                    problems.Add(UiText.Say($"seeds.to: '{s.To}' non è un file del gruppo: un seme non nasce in {TemplateManifest.Folder}/, .claude/skills/, .git/, .md/ né in {DevelopmentFile}",
                                            $"seeds.to: '{s.To}' is not a file of the group: a seed is not born in {TemplateManifest.Folder}/, .claude/skills/, .git/, .md/ or {DevelopmentFile}"));
                else if (!targets.Add(s.To))
                    problems.Add(UiText.Say($"seeds.to: '{s.To}' compare due volte", $"seeds.to: '{s.To}' appears twice"));
            }

            var programs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in m.Programs ?? new List<TemplateProgram>())
            {
                if (p == null) { problems.Add(UiText.Say("programs: una cartella vuota", "programs: an empty folder")); continue; }
                if (!IsProjectPath(p.Path) || !IsGroupPath(p.Path))
                    problems.Add(UiText.Say($"programs.path: '{p.Path}' deve essere una cartella del gruppo, dalla radice del progetto, con «/»",
                                            $"programs.path: '{p.Path}' must be a folder of the group, from the project root, with «/»"));
                else if (!programs.Add(p.Path))
                    problems.Add(UiText.Say($"programs.path: '{p.Path}' compare due volte", $"programs.path: '{p.Path}' appears twice"));
                if (p.Languages == null || p.Languages.Count == 0)
                    problems.Add(UiText.Say($"programs: '{p.Path}' senza languages", $"programs: '{p.Path}' without languages"));
                foreach (var l in p.Languages ?? new List<string>())
                    if (MdExplorer.Features.Services.CodeGraph.CodeLanguages.ById(l) == null)
                        problems.Add(UiText.Say($"programs.languages: '{l}' non è un linguaggio che MdExplorer conosce ({string.Join(", ", MdExplorer.Features.Services.CodeGraph.CodeLanguages.All.Select(x => x.Id))})",
                                                $"programs.languages: '{l}' is not a language MdExplorer knows ({string.Join(", ", MdExplorer.Features.Services.CodeGraph.CodeLanguages.All.Select(x => x.Id))})"));
            }

            return m;
        }

        /// <summary>Come <see cref="Read"/>, ma un problema è un'eccezione che li dice tutti.</summary>
        public static TemplateManifest ReadOrThrow(string yaml, string folderId, string where)
        {
            var m = Read(yaml, folderId, out var problems);
            if (problems.Count > 0)
                throw new InvalidOperationException(UiText.Say($"Il manifesto {where} non va: {string.Join("; ", problems)}.",
                                                               $"The manifest {where} is wrong: {string.Join("; ", problems)}."));
            return m;
        }

        private const string DevelopmentFile = ".development.yml";

        private static bool IsProjectPath(string path)
            => !string.IsNullOrWhiteSpace(path) && !path.Contains('\\') && !path.StartsWith("/") && !path.EndsWith("/")
               && !path.Split('/').Any(p => p.Length == 0 || p == "." || p == "..");

        /// <summary>Un seme nasce fra i file del gruppo: mai nella cartella di un template, fra le skill, in git o nei file di MDE.</summary>
        private static bool IsGroupPath(string path)
            => !new[] { TemplateManifest.Folder + "/", ".claude/skills/", ".git/", ".md/" }.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
               && !string.Equals(path, DevelopmentFile, StringComparison.OrdinalIgnoreCase);

        private static void CheckPath(TemplateManifest m, string key, string path, List<string> problems)
        {
            if (!IsProjectPath(path))
            {
                problems.Add(UiText.Say($"{key}: '{path}' deve essere un percorso dalla radice del progetto, con «/»",
                                        $"{key}: '{path}' must be a path from the project root, with «/»"));
                return;
            }
            if (m.IsZoned && m.Id != null && !path.StartsWith(TemplateManifest.ZoneOf(m.Id), StringComparison.Ordinal))
                problems.Add(UiText.Say($"{key}: '{path}' è fuori dalla cartella del metodo {TemplateManifest.ZoneOf(m.Id)}",
                                        $"{key}: '{path}' is outside the method's folder {TemplateManifest.ZoneOf(m.Id)}"));
        }

        /// <summary>
        /// La regola della zona: i file di un metodo che stanno fuori da <c>.mde-template/&lt;id&gt;/</c> e da
        /// <c>.claude/skills/&lt;id&gt;-*/</c>. Vuoto = il metodo si può unire senza toccare niente di altri.
        /// </summary>
        public static List<string> OutsideZone(string id, IEnumerable<string> files)
            => files.Where(f => !f.StartsWith(TemplateManifest.ZoneOf(id), StringComparison.Ordinal)
                             && !f.StartsWith($".claude/skills/{id}-", StringComparison.Ordinal))
                    .ToList();

        /// <summary>Confronta due versioni di MdExplorer (<c>2026.10.17</c>, <c>2026.10.16.8</c>) parte per parte.</summary>
        public static int CompareAppVersions(string a, string b)
        {
            var x = a.Split('.').Select(int.Parse).ToArray();
            var y = b.Split('.').Select(int.Parse).ToArray();
            for (var i = 0; i < Math.Max(x.Length, y.Length); i++)
            {
                var c = (i < x.Length ? x[i] : 0).CompareTo(i < y.Length ? y[i] : 0);
                if (c != 0) return c;
            }
            return 0;
        }

        public static bool IsAppVersion(string v) => v != null && AppVersion.IsMatch(v);
    }
}
