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

        public bool IsMethod => Kind == Method;

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
    }

    public sealed class TemplateLanguage
    {
        public string Structure { get; set; }
        public string Example { get; set; }
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
            if (m.Kind != TemplateManifest.Method && m.Kind != TemplateManifest.Project)
                problems.Add(UiText.Say($"kind '{m.Kind}': vale method o project", $"kind '{m.Kind}': it is method or project"));
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
                    if (m.Kind == TemplateManifest.Method && m.Id != null && !(a ?? string.Empty).StartsWith(m.Id + "-", StringComparison.Ordinal))
                        problems.Add(UiText.Say($"roles: l'agente '{a}' non comincia con '{m.Id}-': gli agenti di un metodo portano il suo id",
                                                $"roles: the agent '{a}' does not start with '{m.Id}-': a method's agents carry its id"));
            }

            foreach (var w in m.Workflows ?? new List<string>()) CheckPath(m, "workflows", w, problems);
            if (m.Instructions != null) CheckPath(m, "instructions", m.Instructions, problems);

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

        private static void CheckPath(TemplateManifest m, string key, string path, List<string> problems)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Contains('\\') || path.StartsWith("/") || path.Split('/').Contains(".."))
            {
                problems.Add(UiText.Say($"{key}: '{path}' deve essere un percorso dalla radice del progetto, con «/»",
                                        $"{key}: '{path}' must be a path from the project root, with «/»"));
                return;
            }
            if (m.Kind == TemplateManifest.Method && m.Id != null && !path.StartsWith(TemplateManifest.ZoneOf(m.Id), StringComparison.Ordinal))
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
