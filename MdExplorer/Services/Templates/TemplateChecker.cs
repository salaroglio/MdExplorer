using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Services;
using MdExplorer.Features.Templates;
using MdExplorer.Features.Yaml;
using MdExplorer.Service.Models;
using MdExplorer.Services.Git;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Templates
{
    /// <summary>Un problema di un template: dove (ramo e file) e che cosa, con il rimedio quando si sa.</summary>
    public sealed class TemplateCheckIssue
    {
        public const string Error = "error";
        public const string Warning = "warning";

        /// <summary><see cref="Error"/>: MDE non lo unisce, o lo unisce guasto. <see cref="Warning"/>: si unisce, ma va detto.</summary>
        public string Severity { get; init; }
        /// <summary>Il ramo controllato; null per ciò che riguarda il repository intero (i tag).</summary>
        public string Branch { get; init; }
        /// <summary>Il file, dalla radice del repository; null se il problema non è di un file.</summary>
        public string File { get; init; }
        public string Message { get; init; }
        public string Fix { get; init; }
    }

    /// <summary>Un ramo controllato: dai file della cartella (quello in cui si lavora) o dal suo ultimo commit.</summary>
    public sealed class TemplateCheckedBranch
    {
        public string Branch { get; init; }
        /// <summary><c>working-tree</c> o <c>commit</c>.</summary>
        public string From { get; init; }
        public int Files { get; init; }
    }

    public sealed class TemplateCheckReport
    {
        public bool Ok => Issues.All(i => i.Severity != TemplateCheckIssue.Error);
        public string Id { get; init; }
        public string Kind { get; init; }
        public string Version { get; init; }
        public List<TemplateCheckedBranch> Branches { get; } = new();
        public List<TemplateCheckIssue> Issues { get; } = new();
        public int Errors => Issues.Count(i => i.Severity == TemplateCheckIssue.Error);
        public int Warnings => Issues.Count(i => i.Severity == TemplateCheckIssue.Warning);
    }

    public interface ITemplateChecker
    {
        /// <summary>
        /// La verifica di un template (P0, «La verifica di un template»), sul suo repository: il ramo in cui si lavora dai
        /// file della cartella, gli altri rami dichiarati nel manifesto dal loro ultimo commit. Tutti i problemi in un giro.
        /// Solleva solo se la cartella non è un repository git.
        /// </summary>
        Task<TemplateCheckReport> CheckAsync(string folder, CancellationToken ct = default);
    }

    /// <summary>
    /// Controlla un template come lo vedrebbe MDE unendolo: manifesto, regola della zona, CHANGELOG, schede e skill con il
    /// prefisso dell'id, file nominati dal manifesto, e i workflow letti in un progetto fatto solo del template (i semi al
    /// loro posto, i ruoli del manifesto nel documento delle responsabilità). Non tocca il repository: ogni ramo si guarda
    /// in una cartella temporanea.
    /// </summary>
    public sealed class TemplateChecker : ITemplateChecker
    {
        private const string WorkingTree = "working-tree";
        private const string Commit = "commit";
        private static readonly Regex ManifestPath = new(@"^\.mde-template/([^/]+)/mde-template\.yml$", RegexOptions.Compiled);
        private static readonly Regex SkillFile = new(@"^\.claude/skills/([^/]+)/", RegexOptions.Compiled);
        private static readonly Regex SkillName = new(@"\A﻿?---\r?\n(?:.*\r?\n)*?name:\s*[""']?([^""'\r\n]+?)[""']?\s*\r?\n", RegexOptions.Compiled);
        private const int MaxListed = 20;

        private readonly INativeGitRunner _git;
        private readonly ILogger<TemplateChecker> _logger;

        public TemplateChecker(INativeGitRunner git, ILogger<TemplateChecker> logger)
        {
            _git = git;
            _logger = logger;
        }

        public async Task<TemplateCheckReport> CheckAsync(string folder, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                throw Problem($"La cartella del template '{folder}' non esiste.", $"The template folder '{folder}' does not exist.");
            folder = Path.GetFullPath(folder);
            var top = await _git.RunAsync(folder, new[] { "rev-parse", "--show-toplevel" }, ct);
            if (!top.Ok)
                throw Problem($"'{folder}' non è un repository git: un template è un repository, con un ramo per lingua.",
                              $"'{folder}' is not a git repository: a template is a repository, with a branch per language.");
            folder = Path.GetFullPath(top.Stdout.Trim());

            var current = (await Run(folder, ct, "rev-parse", "--abbrev-ref", "HEAD")).Trim();
            var issues = new List<TemplateCheckIssue>();
            var branches = new List<TemplateCheckedBranch>();

            // Il ramo in cui si lavora: i file come sono ora, anche quelli non ancora committati (non gli ignorati).
            var listed = (await Run(folder, ct, "ls-files", "--cached", "--others", "--exclude-standard", "-z"))
                .Split('\0', StringSplitOptions.RemoveEmptyEntries)
                .Where(f => File.Exists(Path.Combine(folder, f.Replace('/', Path.DirectorySeparatorChar)))).Distinct().ToList();
            var manifest = CheckTree(folder, listed, current, issues);
            branches.Add(new TemplateCheckedBranch { Branch = current, From = WorkingTree, Files = listed.Count });
            if ((await Run(folder, ct, "status", "--porcelain")).Trim().Length > 0)
                issues.Add(Warn(current, null,
                    UiText.Say($"Nel ramo '{current}' ci sono modifiche non committate: le ho controllate così come sono, ma chi usa il template vede solo i commit.",
                               $"The branch '{current}' has uncommitted changes: I checked them as they are, but whoever uses the template sees the commits only."),
                    UiText.Say("Committale quando sono a posto.", "Commit them once they are right.")));

            if (manifest != null)
            {
                var versions = await CheckBranchesAsync(folder, current, manifest, branches, issues, ct);
                foreach (var version in versions.Distinct(StringComparer.Ordinal)) await CheckTagAsync(folder, version, issues, ct);
            }

            var report = new TemplateCheckReport { Id = manifest?.Id, Kind = manifest?.Kind, Version = manifest?.Version };
            report.Branches.AddRange(branches);
            report.Issues.AddRange(issues);
            return report;
        }

        /// <summary>
        /// I rami dichiarati in <c>languages</c>: ci sono (locali, o di <c>origin</c> in un clone), portano lo stesso template
        /// e, controllati dal loro commit, sono in regola. L'esempio parte dalla struttura della sua lingua e ha la sua
        /// versione. Due lingue possono avere versioni diverse (la demo: la 1.2.0 è solo inglese): un progetto segue il ramo
        /// della sua lingua. Torna le versioni dei rami letti, per i tag.
        /// </summary>
        private async Task<List<string>> CheckBranchesAsync(string folder, string current, TemplateManifest manifest, List<TemplateCheckedBranch> branches,
            List<TemplateCheckIssue> issues, CancellationToken ct)
        {
            var declared = (manifest.Languages ?? new Dictionary<string, TemplateLanguage>())
                .SelectMany(l => new[] { (Language: l.Key, Branch: l.Value?.Structure, Example: false), (Language: l.Key, Branch: l.Value?.Example, Example: true) })
                .Where(b => !string.IsNullOrWhiteSpace(b.Branch)).ToList();
            var tips = new Dictionary<string, string>(StringComparer.Ordinal);
            var versions = new Dictionary<string, string>(StringComparer.Ordinal) { [current] = manifest.Version };
            foreach (var branch in declared.Select(d => d.Branch).Distinct(StringComparer.Ordinal))
            {
                var tip = await TipAsync(folder, branch, ct);
                if (tip == null)
                {
                    issues.Add(Err(null, TemplateManifest.PathOf(manifest.Id),
                        UiText.Say($"Il manifesto dichiara il ramo '{branch}', che nel repository non c'è.", $"The manifest declares the branch '{branch}', which is not in the repository."),
                        UiText.Say($"Crea il ramo '{branch}' o togli la riga da languages.", $"Create the branch '{branch}' or remove the line from languages.")));
                    continue;
                }
                tips[branch] = tip;
                if (branch == current) continue;

                var temp = TempFolder();
                try
                {
                    var clone = await _git.RunAsync(Path.GetTempPath(), new[] { "clone", "-q", "--shared", "--no-checkout", folder, temp }, ct);
                    if (!clone.Ok) throw Problem($"Non riesco a leggere il ramo '{branch}': {clone.Describe()}", $"I cannot read the branch '{branch}': {clone.Describe()}");
                    await Run(temp, ct, "checkout", "-q", "--detach", tip);
                    var files = (await Run(temp, ct, "ls-files", "-z")).Split('\0', StringSplitOptions.RemoveEmptyEntries).ToList();
                    var other = CheckTree(temp, files, branch, issues);
                    branches.Add(new TemplateCheckedBranch { Branch = branch, From = Commit, Files = files.Count });
                    if (other == null) continue;
                    versions[branch] = other.Version;
                    if (other.Id != manifest.Id)
                        issues.Add(Err(branch, TemplateManifest.PathOf(other.Id),
                            UiText.Say($"Il ramo '{branch}' porta il template '{other.Id}', il ramo '{current}' '{manifest.Id}': un repository porta un template solo.",
                                       $"The branch '{branch}' carries the template '{other.Id}', the branch '{current}' '{manifest.Id}': a repository carries one template only."), null));
                }
                finally { DeleteTree(temp); }
            }

            foreach (var lang in declared.Where(d => d.Example).Select(d => d.Language))
            {
                var language = manifest.Languages[lang];
                if (!tips.TryGetValue(language.Structure ?? "", out var structure) || !tips.TryGetValue(language.Example, out var example)) continue;
                if (versions.TryGetValue(language.Structure, out var vs) && versions.TryGetValue(language.Example, out var ve) && vs != ve)
                    issues.Add(Warn(language.Example, TemplateManifest.PathOf(manifest.Id),
                        UiText.Say($"La struttura '{language.Structure}' è alla {vs}, l'esempio '{language.Example}' alla {ve}: chi sceglie l'esempio riceve gli aggiornamenti di un'altra versione.",
                                   $"The structure '{language.Structure}' is at {vs}, the example '{language.Example}' at {ve}: whoever picks the example gets the updates of another version."),
                        UiText.Say($"Unisci '{language.Structure}' in '{language.Example}' e porta le due alla stessa versione.", $"Merge '{language.Structure}' into '{language.Example}' and bring both to the same version.")));
                var ancestor = await _git.RunAsync(folder, new[] { "merge-base", "--is-ancestor", structure, example }, ct);
                if (ancestor.ExitCode > 1) throw Problem($"git merge-base: {ancestor.Describe()}", $"git merge-base: {ancestor.Describe()}");
                if (ancestor.ExitCode == 1)
                    issues.Add(Warn(language.Example, null,
                        UiText.Say($"L'esempio '{language.Example}' non contiene l'ultimo commit della struttura '{language.Structure}': chi sceglie l'esempio non ha le ultime modifiche.",
                                   $"The example '{language.Example}' does not contain the last commit of the structure '{language.Structure}': whoever picks the example misses the latest changes."),
                        UiText.Say($"Unisci '{language.Structure}' in '{language.Example}'.", $"Merge '{language.Structure}' into '{language.Example}'.")));
            }
            return versions.Values.ToList();
        }

        /// <summary>
        /// Il tag <c>v&lt;versione&gt;</c> (P0): il segno della versione pubblicata. MDE confronta la versione del manifesto, non
        /// il tag, per questo manca solo un avviso.
        /// </summary>
        private async Task CheckTagAsync(string folder, string version, List<TemplateCheckIssue> issues, CancellationToken ct)
        {
            var tag = "v" + version;
            var found = await _git.RunAsync(folder, new[] { "rev-parse", "-q", "--verify", $"refs/tags/{tag}" }, ct);
            if (found.ExitCode > 1) throw Problem($"git rev-parse: {found.Describe()}", $"git rev-parse: {found.Describe()}");
            if (!found.Ok)
                issues.Add(Warn(null, null,
                    UiText.Say($"Manca il tag {tag} della versione {version}.", $"The tag {tag} of the version {version} is missing."),
                    UiText.Say($"Quando la versione è pronta: git tag {tag}, e pubblicalo insieme ai rami.", $"When the version is ready: git tag {tag}, and push it with the branches.")));
        }

        private async Task<string> TipAsync(string folder, string branch, CancellationToken ct)
        {
            foreach (var reference in new[] { $"refs/heads/{branch}", $"refs/remotes/origin/{branch}" })
            {
                var r = await _git.RunAsync(folder, new[] { "rev-parse", "-q", "--verify", reference + "^{commit}" }, ct);
                if (r.Ok) return r.Stdout.Trim();
                if (r.ExitCode > 1) throw Problem($"git rev-parse: {r.Describe()}", $"git rev-parse: {r.Describe()}");
            }
            return null;
        }

        /// <summary>Un ramo, già su disco in <paramref name="root"/> con i suoi <paramref name="files"/>. Il manifesto, se si legge.</summary>
        private TemplateManifest CheckTree(string root, IReadOnlyList<string> files, string branch, List<TemplateCheckIssue> issues)
        {
            var ids = files.Select(f => ManifestPath.Match(f)).Where(m => m.Success).Select(m => m.Groups[1].Value).Distinct().ToList();
            if (ids.Count == 0)
            {
                issues.Add(Err(branch, null,
                    UiText.Say($"Nel ramo non c'è il manifesto {TemplateManifest.Folder}/<id>/{TemplateManifest.FileName}: non è un template.",
                               $"The branch has no manifest {TemplateManifest.Folder}/<id>/{TemplateManifest.FileName}: it is not a template."),
                    UiText.Say("Vedi la skill mde-template.", "See the mde-template skill.")));
                return null;
            }
            if (ids.Count > 1)
            {
                issues.Add(Err(branch, null,
                    UiText.Say($"Nel ramo ci sono più manifesti ({string.Join(", ", ids)}): un repository porta un template solo.",
                               $"The branch has several manifests ({string.Join(", ", ids)}): a repository carries one template only."), null));
                return null;
            }

            var manifestFile = TemplateManifest.PathOf(ids[0]);
            var manifest = TemplateManifestReader.Read(File.ReadAllText(Full(root, manifestFile)), ids[0], out var problems);
            foreach (var p in problems) issues.Add(Err(branch, manifestFile, p, null));
            if (problems.Count > 0 || manifest == null) return null;

            var zone = TemplateManifest.ZoneOf(manifest.Id);
            if (manifest.IsMethod)
            {
                var outside = TemplateManifestReader.OutsideZone(manifest.Id, files);
                foreach (var f in outside.Take(MaxListed))
                    issues.Add(Err(branch, f,
                        UiText.Say($"Il file è fuori dalla zona del metodo ({zone} e .claude/skills/{manifest.Id}-*/): MDE non unisce il metodo.",
                                   $"The file is outside the method's zone ({zone} and .claude/skills/{manifest.Id}-*/): MDE will not merge the method."),
                        UiText.Say($"Spostalo in {zone}; se deve finire nel progetto, dichiaralo in seeds.", $"Move it into {zone}; if it must end up in the project, declare it in seeds.")));
                if (outside.Count > MaxListed)
                    issues.Add(Err(branch, null, UiText.Say($"E altri {outside.Count - MaxListed} file fuori dalla zona.", $"And {outside.Count - MaxListed} more files outside the zone."), null));
            }

            CheckChangelog(root, files, branch, manifest, issues);
            if (!files.Contains(zone + "README.md"))
                issues.Add(Warn(branch, zone + "README.md",
                    UiText.Say("Manca il README del template: è ciò che legge chi lo sceglie.", "The template's README is missing: it is what whoever picks it reads."), null));

            var named = (manifest.Workflows ?? new List<string>()).Append(manifest.Instructions)
                .Concat((manifest.Seeds ?? new List<TemplateSeed>()).Select(s => s.From)).Where(f => f != null);
            var missing = named.Where(f => !files.Contains(f)).Distinct().ToList();
            foreach (var f in missing)
                issues.Add(Err(branch, f, UiText.Say($"Il manifesto nomina questo file, che nel ramo non c'è.", $"The manifest names this file, which is not in the branch."), null));

            CheckCards(root, files, branch, manifest, issues);
            CheckSkills(root, files, branch, issues);
            if (missing.Count == 0) CheckAsProject(root, files, branch, manifest, issues);
            return manifest;
        }

        private static void CheckChangelog(string root, IReadOnlyList<string> files, string branch, TemplateManifest manifest, List<TemplateCheckIssue> issues)
        {
            var file = TemplateManifest.ZoneOf(manifest.Id) + TemplateChangelog.FileName;
            if (!files.Contains(file))
            {
                issues.Add(Err(branch, file,
                    UiText.Say("Manca il CHANGELOG: senza, chi aggiorna il progetto non sa che cosa cambia.", "The CHANGELOG is missing: without it, whoever updates the project does not know what changes."),
                    UiText.Say($"Crealo con la voce «## {manifest.Version} — aaaa-mm-gg».", $"Create it with the entry «## {manifest.Version} — yyyy-mm-dd».")));
                return;
            }
            TemplateChangelog.Read(File.ReadAllText(Full(root, file)), manifest.Version, out var problems);
            foreach (var p in problems) issues.Add(Err(branch, file, p, null));
        }

        /// <summary>
        /// Le schede del template: si leggono, il nome è quello del file e, in un metodo, comincia con l'id (due metodi non
        /// devono poter portare lo stesso agente: un nome doppio esclude tutti e due). Gli agenti dei ruoli hanno la scheda.
        /// </summary>
        private static void CheckCards(string root, IReadOnlyList<string> files, string branch, TemplateManifest manifest, List<TemplateCheckIssue> issues)
        {
            var parser = new YamlAgentCardParser();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var card in files.Where(f => f.EndsWith(".agent.md", StringComparison.OrdinalIgnoreCase)))
            {
                var name = Path.GetFileName(card)[..^".agent.md".Length];
                var parsed = parser.GetDescriptor(File.ReadAllText(Full(root, card)));
                if (!parsed.IsValid || parsed.Card == null)
                {
                    issues.Add(Err(branch, card, UiText.Say($"La scheda non si legge: {parsed.RegistrationError ?? "manca il blocco a2a:"}",
                                                            $"The card cannot be read: {parsed.RegistrationError ?? "the a2a: block is missing"}"), null));
                    continue;
                }
                if (!string.Equals(parsed.Card.Name, name, StringComparison.OrdinalIgnoreCase))
                    issues.Add(Err(branch, card, UiText.Say($"La scheda si chiama '{parsed.Card.Name}', il file '{name}': MDE la cerca per nome del file.",
                                                            $"The card is named '{parsed.Card.Name}', the file '{name}': MDE looks it up by file name."), null));
                if (manifest.IsMethod && !name.StartsWith(manifest.Id + "-", StringComparison.Ordinal))
                    issues.Add(Err(branch, card,
                        UiText.Say($"L'agente '{name}' non comincia con l'id del metodo: un altro metodo potrebbe portarne uno con lo stesso nome.",
                                   $"The agent '{name}' does not start with the method's id: another method could carry one with the same name."),
                        UiText.Say($"Chiamalo '{manifest.Id}-…', nel file e in a2a.name.", $"Name it '{manifest.Id}-…', in the file and in a2a.name.")));
                if (!names.Add(name))
                    issues.Add(Err(branch, card, UiText.Say($"Due schede '{name}': MDE non sa quale usare.", $"Two cards '{name}': MDE does not know which to use."), null));
            }
            foreach (var role in manifest.Roles ?? new List<TemplateRole>())
                foreach (var agent in (role.Agents ?? new List<string>()).Where(a => !names.Contains(a)))
                    issues.Add(Err(branch, TemplateManifest.PathOf(manifest.Id),
                        UiText.Say($"Il ruolo «{role.Role}» nomina l'agente '{agent}', che nel ramo non ha la scheda.", $"The role «{role.Role}» names the agent '{agent}', which has no card in the branch."), null));
        }

        /// <summary>Ogni skill ha il suo SKILL.md con il nome della cartella: è così che i motori la trovano.</summary>
        private static void CheckSkills(string root, IReadOnlyList<string> files, string branch, List<TemplateCheckIssue> issues)
        {
            foreach (var skill in files.Select(f => SkillFile.Match(f)).Where(m => m.Success).Select(m => m.Groups[1].Value).Distinct())
            {
                var file = $".claude/skills/{skill}/SKILL.md";
                if (!files.Contains(file))
                {
                    issues.Add(Err(branch, file, UiText.Say("La skill non ha il suo SKILL.md: nessun motore la vede.", "The skill has no SKILL.md: no engine sees it."), null));
                    continue;
                }
                var m = SkillName.Match(File.ReadAllText(Full(root, file)));
                if (!m.Success || m.Groups[1].Value.Trim() != skill)
                    issues.Add(Err(branch, file,
                        UiText.Say($"Il front matter della skill deve avere «name: {skill}», il nome della sua cartella.", $"The skill's front matter must have «name: {skill}», its folder's name."), null));
            }
        }

        /// <summary>
        /// Il template come progetto: i suoi file in una cartella nuova, i semi al loro posto, le cartelle di codice che ci
        /// sono, i ruoli del manifesto come ruoli aperti; poi ogni workflow letto e verificato come lo legge MDE, e tutti
        /// insieme (un agente apre i giri di un workflow solo).
        /// </summary>
        private void CheckAsProject(string root, IReadOnlyList<string> files, string branch, TemplateManifest manifest, List<TemplateCheckIssue> issues)
        {
            var project = TempFolder();
            try
            {
                foreach (var f in files)
                {
                    var target = Full(project, f);
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    File.Copy(Full(root, f), target);
                }
                foreach (var seed in manifest.Seeds ?? new List<TemplateSeed>())
                {
                    var target = Full(project, seed.To);
                    if (File.Exists(target) || Directory.Exists(target)) continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    File.Copy(Full(project, seed.From), target);
                }
                foreach (var program in manifest.Programs ?? new List<TemplateProgram>())
                    if (!Directory.Exists(Full(project, program.Path)))
                        issues.Add(Err(branch, TemplateManifest.PathOf(manifest.Id),
                            UiText.Say($"programs: la cartella '{program.Path}' non c'è, nemmeno dopo i semi: MDE non crea il progetto.",
                                       $"programs: the folder '{program.Path}' is not there, not even after the seeds: MDE will not create the project."),
                            UiText.Say("Portala con un seme, o toglila da programs.", "Bring it with a seed, or remove it from programs.")));

                var ownership = OwnershipDocOf(project, branch, manifest, issues);
                var roles = (manifest.Roles ?? new List<TemplateRole>()).Select(r => (r.Role, r.Description, (IReadOnlyList<string>)(r.Agents ?? new List<string>()))).ToList();
                if (roles.Count > 0)
                {
                    var full = Full(project, ownership);
                    var before = File.Exists(full) ? File.ReadAllText(full) : null;
                    Directory.CreateDirectory(Path.GetDirectoryName(full));
                    File.WriteAllText(full, OwnershipDocWriter.AddOpenRoles(before, roles, out _));
                }
                var ownershipDoc = File.Exists(Full(project, ownership)) ? ownership : null;

                var docs = manifest.Workflows ?? new List<string>();
                var clean = true;
                foreach (var doc in docs)
                {
                    string json;
                    try { json = WorkflowDocument.JsonPathOf(project, doc); }
                    catch (InvalidOperationException ex) { issues.Add(Err(branch, doc, ex.Message, null)); clean = false; continue; }
                    if (!File.Exists(Full(project, json)))
                    {
                        issues.Add(Err(branch, doc, UiText.Say($"Il documento nomina il workflow '{json}', che non c'è.", $"The document names the workflow '{json}', which is not there."), null));
                        clean = false;
                        continue;
                    }
                    var result = WorkflowFile.Check(project, json, ownershipDoc);
                    foreach (var i in result.Issues)
                    {
                        var error = i.Severity == WorkflowSeverity.Error;
                        clean &= !error;
                        var message = string.IsNullOrEmpty(i.Path) ? i.Message : $"{i.Path}: {i.Message}";
                        issues.Add(error ? Err(branch, json, message, i.Fix) : Warn(branch, json, message, i.Fix));
                    }
                }
                if (clean && docs.Count > 1)
                {
                    WorkflowDocument.LoadAll(project, docs, ownershipDoc, out var problem);
                    if (problem != null) issues.Add(Err(branch, TemplateManifest.PathOf(manifest.Id), problem, null));
                }
            }
            finally { DeleteTree(project); }
        }

        /// <summary>Il documento delle responsabilità del progetto nato dal template: quello della base, o quello che crea MDE.</summary>
        private static string OwnershipDocOf(string project, string branch, TemplateManifest manifest, List<TemplateCheckIssue> issues)
        {
            var file = Path.Combine(project, DevelopmentConfigCleanup.FileName);
            if (manifest.IsMethod || !File.Exists(file)) return ProjectTemplateCreator.OwnershipFile;
            try
            {
                var configured = DevelopmentConfigCleanup.ReadStrict(file).AgentCity?.OwnershipDoc;
                return string.IsNullOrWhiteSpace(configured) ? ProjectTemplateCreator.OwnershipFile : configured.Trim().Replace('\\', '/').TrimStart('/');
            }
            catch (InvalidOperationException ex)
            {
                issues.Add(Err(branch, DevelopmentConfigCleanup.FileName, ex.Message, null));
                return ProjectTemplateCreator.OwnershipFile;
            }
        }

        private static string Full(string root, string relative) => Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));

        private static string TempFolder()
        {
            var path = Path.Combine(Path.GetTempPath(), "mde-template-check-" + Guid.NewGuid().ToString("N")[..12]);
            Directory.CreateDirectory(path);
            return path;
        }

        private void DeleteTree(string target)
        {
            try
            {
                if (!Directory.Exists(target)) return;
                foreach (var f in Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
                Directory.Delete(target, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Template] non riesco a togliere la cartella di prova '{Target}': va tolta a mano.", target);
            }
        }

        private async Task<string> Run(string dir, CancellationToken ct, params string[] args)
        {
            var r = await _git.RunAsync(dir, args, ct);
            if (!r.Ok) throw Problem($"git {string.Join(' ', args)}: {r.Describe()}", $"git {string.Join(' ', args)}: {r.Describe()}");
            return r.Stdout ?? string.Empty;
        }

        private static TemplateCheckIssue Err(string branch, string file, string message, string fix)
            => new() { Severity = TemplateCheckIssue.Error, Branch = branch, File = file, Message = message, Fix = fix };

        private static TemplateCheckIssue Warn(string branch, string file, string message, string fix)
            => new() { Severity = TemplateCheckIssue.Warning, Branch = branch, File = file, Message = message, Fix = fix };

        private static InvalidOperationException Problem(string it, string en) => new(UiText.Say(it, en));
    }
}
