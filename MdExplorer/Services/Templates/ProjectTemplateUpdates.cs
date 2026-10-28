using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Templates;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services.Templates
{
    /// <summary>A che punto è un template del progetto rispetto al suo repository (P3).</summary>
    public sealed class TemplateUpdateInfo
    {
        public string Id { get; init; }
        public string Title { get; init; }
        /// <summary><c>project</c> per la base, null per un metodo.</summary>
        public string Kind { get; init; }
        public string Url { get; init; }
        /// <summary>La versione unita nel progetto (<c>0.0.0</c> per una vecchia demo da migrare).</summary>
        public string Current { get; init; }
        /// <summary>La versione nuova del template; null = niente di nuovo.</summary>
        public string Available { get; init; }
        /// <summary>Le voci del CHANGELOG fra le due versioni, dalla più recente.</summary>
        public IReadOnlyList<TemplateChangelogEntry> Entries { get; init; } = Array.Empty<TemplateChangelogEntry>();
        /// <summary>Una delle voci rompe qualcosa: si conferma prima di unire.</summary>
        public bool Incompatible { get; init; }
        /// <summary>Una vecchia demo con il remoto <c>upstream</c>: l'aggiornamento la fa diventare <c>template-&lt;id&gt;</c>.</summary>
        public bool Migration { get; init; }
        /// <summary>Perché non si può aggiornare (o perché il template non risponde); null se va tutto bene.</summary>
        public string Problem { get; init; }
    }

    public sealed class TemplateUpdateRequest
    {
        public string ProjectPath { get; init; }
        public string Id { get; init; }
        /// <summary>La persona ha letto le righe «⚠️ incompatibile» e aggiorna lo stesso.</summary>
        public bool AcceptIncompatible { get; init; }
        public string MdeVersion { get; init; }
    }

    public sealed class TemplateUpdateOutcome
    {
        public string Id { get; init; }
        public string From { get; init; }
        public string To { get; init; }
        public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
    }

    public interface IProjectTemplateUpdates
    {
        /// <summary>Lo stato di ogni template non staccato (e di una vecchia demo da migrare). <paramref name="fetch"/> interroga i repository.</summary>
        Task<IReadOnlyList<TemplateUpdateInfo>> UpdatesAsync(string projectPath, bool fetch, string mdeVersion, CancellationToken ct = default);

        /// <summary>
        /// Porta un template all'ultima versione: unione, manifesto riapplicato, versione nella riga, un commit, <c>origin</c>
        /// allineato. Il progetto non deve avere modifiche senza commit; in errore torna com'era.
        /// </summary>
        Task<TemplateUpdateOutcome> UpdateAsync(TemplateUpdateRequest request, CancellationToken ct = default);

        /// <summary>
        /// Stacca il progetto da un template, per sempre: il remoto si toglie, la riga resta con la data. La struttura del template
        /// resta nel progetto, che da lì è del gruppo. Un commit di <c>.development.yml</c>.
        /// </summary>
        Task<string> DetachAsync(string projectPath, string id, CancellationToken ct = default);
    }

    public sealed partial class ProjectTemplateCreator : IProjectTemplateUpdates
    {
        /// <summary>Il remoto di una demo clonata prima dei template (P0: diventa <c>template-demo</c>).</summary>
        public const string LegacyRemote = "upstream";

        /// <summary>La versione di una vecchia demo, che non ha ancora una riga: ogni voce del CHANGELOG è nuova per lei.</summary>
        public const string NoVersion = "0.0.0";

        /// <summary>Un template del progetto e dove guardarne gli aggiornamenti.</summary>
        private sealed class Source
        {
            public ProjectTemplateConfig Row;
            public string Remote;
            public bool Migration;
        }

        public async Task<IReadOnlyList<TemplateUpdateInfo>> UpdatesAsync(string projectPath, bool fetch, string mdeVersion, CancellationToken ct = default)
        {
            var path = Full(projectPath);
            var result = new List<TemplateUpdateInfo>();
            List<Source> sources;
            try { sources = await SourcesAsync(path, fetch, ct); }
            catch (InvalidOperationException ex)
            {
                result.Add(new TemplateUpdateInfo { Id = "?", Problem = ex.Message });
                return result;
            }
            foreach (var s in sources)
            {
                try
                {
                    var (_, manifest, entries, _) = await NewVersionAsync(path, s, fetch, mdeVersion, ct);
                    var news = manifest == null ? null : manifest.Version;
                    result.Add(new TemplateUpdateInfo
                    {
                        Id = s.Row.Id, Title = manifest?.Title ?? s.Row.Id, Kind = s.Row.Kind, Url = s.Row.Url, Current = s.Row.Version,
                        Available = news, Entries = entries, Incompatible = entries.Any(e => e.Incompatible.Count > 0), Migration = s.Migration,
                    });
                }
                catch (InvalidOperationException ex)
                {
                    result.Add(new TemplateUpdateInfo { Id = s.Row.Id, Title = s.Row.Id, Kind = s.Row.Kind, Url = s.Row.Url, Current = s.Row.Version, Migration = s.Migration, Problem = ex.Message });
                }
            }
            return result;
        }

        public async Task<TemplateUpdateOutcome> UpdateAsync(TemplateUpdateRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request?.Id)) throw Problem("Manca il template da aggiornare.", "The template to update is missing.");
            if (!TemplateManifestReader.IsAppVersion(request.MdeVersion))
                throw Problem($"La versione di MdExplorer '{request.MdeVersion}' non si legge: serve per sapere se il template ci gira.",
                              $"The MdExplorer version '{request.MdeVersion}' cannot be read: it is needed to know whether the template runs here.");
            var path = Full(request.ProjectPath);
            await CleanProjectAsync(path, UiText.Say("aggiornare un template", "updating a template"), ct);

            var source = (await SourcesAsync(path, fetch: false, ct)).FirstOrDefault(s => s.Row.Id == request.Id)
                         ?? throw Problem($"Il progetto non ha un template '{request.Id}' da aggiornare (o se ne è staccato).",
                                          $"The project has no template '{request.Id}' to update (or it detached from it).");
            var (tip, manifest, entries, previous) = await NewVersionAsync(path, source, fetch: true, request.MdeVersion, ct);
            if (manifest == null)
                throw Problem($"Il template '{request.Id}' è già alla versione {source.Row.Version}: niente da aggiornare.",
                              $"The template '{request.Id}' is already at version {source.Row.Version}: nothing to update.");
            var incompatible = entries.SelectMany(e => e.Incompatible).ToList();
            if (incompatible.Count > 0 && !request.AcceptIncompatible)
                throw Problem($"La versione {manifest.Version} di «{manifest.Title}» rompe qualcosa: {string.Join(" ", incompatible)} Leggi e conferma prima di aggiornare.",
                              $"Version {manifest.Version} of «{manifest.Title}» breaks something: {string.Join(" ", incompatible)} Read and confirm before updating.");

            var head = (await Run(path, ct, "rev-parse", "HEAD")).Trim();
            var branch = (await Run(path, ct, "rev-parse", "--abbrev-ref", "HEAD")).Trim();
            var renamed = false;
            try
            {
                if (source.Migration)
                {
                    // La vecchia demo: il remoto «upstream» diventa quello del template, e la riga nasce con l'aggiornamento.
                    await Run(path, ct, "remote", "rename", LegacyRemote, source.Remote);
                    renamed = true;
                    tip = tip.Replace(LegacyRemote + "/", source.Remote + "/");
                }
                var merge = await _git.RunAsync(path, new[] { "merge", "--no-ff", "--no-commit", tip }, ct);
                if (!merge.Ok) await ResolveOrThrowAsync(path, tip, merge.Describe(), ct);

                var row = new ProjectTemplateConfig
                {
                    Id = source.Row.Id, Url = source.Row.Url, Kind = manifest.RowKind,
                    Language = source.Row.Language, Example = source.Row.Example, Theory = source.Row.Theory, Version = manifest.Version,
                };
                var notes = (await ApplyManifestAsync(path, manifest, row, ct, update: true, previous: previous)).ToList();
                await Run(path, ct, "commit", "-q", "-m",
                    UiText.Say($"Template «{manifest.Title}» aggiornato da {source.Row.Version} a {manifest.Version}",
                               $"Template «{manifest.Title}» updated from {source.Row.Version} to {manifest.Version}"));

                // origin allineato, come «Scarica gli aggiornamenti»: i posti di lavoro degli agenti partono da lì.
                if ((await _git.RunAsync(path, new[] { "remote", "get-url", "origin" }, ct)).Ok)
                {
                    var push = await _transport.PushAsync(path, "origin", branch, ct);
                    if (!push.Ok)
                        notes.Add(UiText.Say($"L'aggiornamento è nella tua cartella, ma non sono riuscito a portarlo su 'origin': {push.Error}. Pubblicalo dal pannello «in attesa di push».",
                                             $"The update is in your folder, but I could not push it to 'origin': {push.Error}. Push it from the «To push» panel."));
                }
                _logger.LogInformation("[Template] '{Path}': '{Id}' aggiornato da {From} a {To}.", path, row.Id, source.Row.Version, manifest.Version);
                return new TemplateUpdateOutcome { Id = row.Id, From = source.Row.Version, To = manifest.Version, Notes = notes };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Template] '{Path}': aggiornamento di '{Id}' non riuscito; il progetto torna com'era.", path, request.Id);
                await _git.RunAsync(path, new[] { "merge", "--abort" }, ct);
                await _git.RunAsync(path, new[] { "reset", "-q", "--hard", head }, ct);
                if (renamed) await _git.RunAsync(path, new[] { "remote", "rename", source.Remote, LegacyRemote }, ct);
                if (ex is InvalidOperationException) throw;
                throw Problem($"Il template non è stato aggiornato: {ex.Message}", $"The template was not updated: {ex.Message}");
            }
        }

        public async Task<string> DetachAsync(string projectPath, string id, CancellationToken ct = default)
        {
            var path = Full(projectPath);
            var file = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            if (!File.Exists(file)) throw Problem($"Il progetto non ha {DevelopmentConfigCleanup.FileName}: non viene da un template.", $"The project has no {DevelopmentConfigCleanup.FileName}: it does not come from a template.");
            var dirty = (await Run(path, ct, "status", "--porcelain", "--", DevelopmentConfigCleanup.FileName)).Trim();
            if (dirty.Length > 0)
                throw Problem($"{DevelopmentConfigCleanup.FileName} ha modifiche senza commit: fai il commit o annullale, poi staccati dal template.",
                              $"{DevelopmentConfigCleanup.FileName} has uncommitted changes: commit or discard them, then detach from the template.");

            var config = DevelopmentConfigCleanup.ReadStrict(file);
            var row = config.Templates?.FirstOrDefault(t => t.Id == id)
                      ?? throw Problem($"Il progetto non ha il template '{id}'.", $"The project does not have the template '{id}'.");
            if (!string.IsNullOrEmpty(row.Detached))
                throw Problem($"Il progetto si è già staccato da '{id}' il {row.Detached}.", $"The project already detached from '{id}' on {row.Detached}.");

            row.Detached = DateTime.Now.ToString("yyyy-MM-dd");
            Write(file, config);
            await Run(path, ct, "add", "--", DevelopmentConfigCleanup.FileName);
            await Run(path, ct, "commit", "-q", "-m", UiText.Say($"Staccato dal template «{id}»", $"Detached from the template «{id}»"), "--", DevelopmentConfigCleanup.FileName);
            await _git.RunAsync(path, new[] { "remote", "remove", RemotePrefix + id }, ct);
            _logger.LogInformation("[Template] '{Path}': staccato da '{Id}'.", path, id);
            return UiText.Say($"Staccato da «{id}»: la sua struttura resta nel progetto, che da qui è del gruppo. Il commit è da pubblicare con la barra di git.",
                              $"Detached from «{id}»: its structure stays in the project, which from now on belongs to the group. The commit is to publish with the git bar.");
        }

        // ---- di contorno ----

        /// <summary>
        /// I template da guardare: le righe non staccate (con il remoto ricreato dalla riga, se manca: i remoti non viaggiano con
        /// il clone) e, senza una base, una vecchia demo con il remoto <c>upstream</c> il cui ramo ora porta un manifesto di progetto.
        /// </summary>
        private async Task<List<Source>> SourcesAsync(string path, bool fetch, CancellationToken ct)
        {
            var rows = PresentTemplates(path);
            var sources = new List<Source>();
            foreach (var row in rows.Where(r => string.IsNullOrEmpty(r.Detached)))
            {
                var remote = RemotePrefix + row.Id;
                var current = await _git.RunAsync(path, new[] { "remote", "get-url", remote }, ct);
                if (!current.Ok)
                {
                    await Run(path, ct, "remote", "add", remote, row.Url);
                    _logger.LogInformation("[Template] '{Path}': ricreato il remoto '{Remote}' dalla riga di {File}.", path, remote, DevelopmentConfigCleanup.FileName);
                }
                else if (current.Stdout.Trim() != row.Url)
                    throw Problem($"Il remoto '{remote}' punta a '{current.Stdout.Trim()}', ma {DevelopmentConfigCleanup.FileName} dice '{row.Url}': correggi uno dei due.",
                                  $"The remote '{remote}' points to '{current.Stdout.Trim()}', but {DevelopmentConfigCleanup.FileName} says '{row.Url}': fix one of the two.");
                sources.Add(new Source { Row = row, Remote = remote });
            }
            if (rows.Any(r => r.Kind == TemplateManifest.Project)) return sources;

            // Una demo di prima dei template: «upstream» sul ramo del progetto. Lo diventa se quel ramo ha un manifesto di progetto.
            var legacy = await _git.RunAsync(path, new[] { "remote", "get-url", LegacyRemote }, ct);
            if (!legacy.Ok) return sources;
            var branch = (await Run(path, ct, "rev-parse", "--abbrev-ref", "HEAD")).Trim();
            if (fetch)
            {
                var fetched = await _transport.FetchAsync(path, LegacyRemote, ct);
                if (!fetched.Ok) return sources;   // la vecchia sorgente non risponde: resta il «Scarica gli aggiornamenti» di sempre
            }
            var tip = $"{LegacyRemote}/{branch}";
            if (!(await _git.RunAsync(path, new[] { "rev-parse", "--verify", "--quiet", tip + "^{commit}" }, ct)).Ok) return sources;
            var id = await ProjectManifestIdAsync(path, tip, ct);
            if (id == null) return sources;
            var manifest = TemplateManifestReader.ReadOrThrow(await ShowAsync(path, tip, TemplateManifest.PathOf(id), ct), id, $"{TemplateManifest.PathOf(id)} ({tip})");
            var language = manifest.Languages.FirstOrDefault(l => l.Value.Structure == branch || l.Value.Example == branch);
            if (language.Key == null)
                throw Problem($"La sorgente è ora il template «{manifest.Title}», ma nessuna sua lingua usa il ramo '{branch}' del progetto.",
                              $"The source is now the template «{manifest.Title}», but none of its languages uses the project's branch '{branch}'.");
            sources.Add(new Source
            {
                Migration = true,
                Remote = RemotePrefix + id,
                Row = new ProjectTemplateConfig
                {
                    Id = id, Url = legacy.Stdout.Trim(), Kind = TemplateManifest.Project, Language = language.Key,
                    Example = language.Value.Example == branch, Version = NoVersion,
                },
            });
            return sources;
        }

        /// <summary>L'id del manifesto di progetto su un ramo, o null se il ramo non ne ha uno.</summary>
        private async Task<string> ProjectManifestIdAsync(string path, string tip, CancellationToken ct)
        {
            var files = (await Run(path, ct, "ls-tree", "-r", "--name-only", tip, "--", TemplateManifest.Folder))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim())
                .Select(f => System.Text.RegularExpressions.Regex.Match(f, @"^\.mde-template/([^/]+)/mde-template\.yml$")).Where(m => m.Success)
                .Select(m => m.Groups[1].Value).Distinct().ToList();
            if (files.Count != 1) return null;
            var manifest = TemplateManifestReader.ReadOrThrow(await ShowAsync(path, tip, TemplateManifest.PathOf(files[0]), ct), files[0], $"{TemplateManifest.PathOf(files[0])} ({tip})");
            return manifest.Kind == TemplateManifest.Project ? files[0] : null;
        }

        /// <summary>
        /// La versione nuova di un template, se c'è: il ramo dalla lingua (dal manifesto nel progetto, o da quello del ramo per una
        /// vecchia demo), il manifesto del ramo con i suoi controlli, il CHANGELOG con le voci nuove. Manifest null = niente di nuovo.
        /// </summary>
        private async Task<(string Tip, TemplateManifest Manifest, IReadOnlyList<TemplateChangelogEntry> Entries, TemplateManifest Previous)> NewVersionAsync(
            string path, Source s, bool fetch, string mdeVersion, CancellationToken ct)
        {
            var fetchRemote = s.Migration ? LegacyRemote : s.Remote;
            if (fetch)
            {
                var fetched = await _transport.FetchAsync(path, fetchRemote, ct);
                if (!fetched.Ok) throw Problem($"Il template non risponde: {fetched.Error}", $"The template does not answer: {fetched.Error}");
            }

            TemplateManifest previous = null;
            var local = System.IO.Path.Combine(path, TemplateManifest.PathOf(s.Row.Id).Replace('/', System.IO.Path.DirectorySeparatorChar));
            string branch;
            if (File.Exists(local))
            {
                previous = TemplateManifestReader.ReadOrThrow(await File.ReadAllTextAsync(local, ct), s.Row.Id, TemplateManifest.PathOf(s.Row.Id));
                branch = previous.BranchFor(s.Row.Language, s.Row.Example == true);
            }
            else if (s.Migration)
                branch = (await Run(path, ct, "rev-parse", "--abbrev-ref", "HEAD")).Trim();
            else
                throw Problem($"Manca {TemplateManifest.PathOf(s.Row.Id)} nel progetto: non so da quale ramo arrivano gli aggiornamenti.",
                              $"{TemplateManifest.PathOf(s.Row.Id)} is missing from the project: I do not know which branch the updates come from.");

            var tip = $"{fetchRemote}/{branch}";
            if (!(await _git.RunAsync(path, new[] { "rev-parse", "--verify", "--quiet", tip + "^{commit}" }, ct)).Ok)
                throw Problem($"Il ramo '{branch}' del template non c'è (ancora): premi «Controlla» per interrogarlo.",
                              $"The template branch '{branch}' is not there (yet): press «Check» to ask it.");
            var manifest = TemplateManifestReader.ReadOrThrow(await ShowAsync(path, tip, TemplateManifest.PathOf(s.Row.Id), ct), s.Row.Id, $"{TemplateManifest.PathOf(s.Row.Id)} ({branch})");

            var behind = int.Parse((await Run(path, ct, "rev-list", "--count", $"HEAD..{tip}")).Trim());
            var compare = TemplateChangelog.Compare(manifest.Version, s.Row.Version);
            if (compare <= 0)
            {
                if (behind > 0 && compare == 0)
                    throw Problem($"Il template ha {behind} commit nuovi ma è ancora alla versione {manifest.Version}: arrivano quando l'autore pubblica una versione nuova.",
                                  $"The template has {behind} new commits but is still at version {manifest.Version}: they arrive when the author publishes a new version.");
                return (tip, null, Array.Empty<TemplateChangelogEntry>(), previous);
            }

            var present = PresentTemplates(path).Where(t => string.IsNullOrEmpty(t.Detached)).Select(t => t.Id).ToHashSet(StringComparer.Ordinal);
            await CheckAsync(path, tip, manifest, mdeVersion, present, ct);
            var changelogPath = $"{TemplateManifest.Folder}/{s.Row.Id}/{TemplateChangelog.FileName}";
            var changelog = await _git.RunAsync(path, new[] { "show", $"{tip}:{changelogPath}" }, ct);
            if (!changelog.Ok)
                throw Problem($"La versione {manifest.Version} del template non ha {changelogPath}: senza le note non si aggiorna.",
                              $"Version {manifest.Version} of the template has no {changelogPath}: without the notes it is not updated.");
            var entries = TemplateChangelog.Read(changelog.Stdout, manifest.Version, out var problems);
            // Le note fra le due versioni si sanno solo se il CHANGELOG ha ancora la voce della versione del progetto.
            if (s.Row.Version != NoVersion && !entries.Any(e => e.Version == s.Row.Version))
                problems.Add(UiText.Say($"Non ha la voce della {s.Row.Version}, la versione del progetto: le voci vecchie non si tolgono.",
                                        $"It has no entry for {s.Row.Version}, the project's version: old entries are not removed."));
            if (problems.Count > 0)
                throw Problem($"Il {changelogPath} della versione {manifest.Version} non è in regola: {string.Join(" ", problems)}",
                              $"The {changelogPath} of version {manifest.Version} is not right: {string.Join(" ", problems)}");
            return (tip, manifest, TemplateChangelog.Between(entries, s.Row.Version, manifest.Version), previous);
        }

        /// <summary>
        /// L'unione si è fermata: se l'unico file in conflitto è <c>.development.yml</c> lo si unisce voce per voce (lo scrivono sia
        /// il template sia il progetto); altrimenti si dice quali file sono in conflitto, e chi chiama rimette il progetto com'era.
        /// </summary>
        private async Task ResolveOrThrowAsync(string path, string tip, string why, CancellationToken ct)
        {
            var conflicts = (await Run(path, ct, "diff", "--name-only", "--diff-filter=U"))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();
            if (conflicts.Count == 1 && conflicts[0] == DevelopmentConfigCleanup.FileName)
            {
                var file = DevelopmentConfigCleanup.FileName;
                var baseText = await _git.RunAsync(path, new[] { "show", $":1:{file}" }, ct);
                var ours = await Run(path, ct, "show", $":2:{file}");
                var theirs = await Run(path, ct, "show", $":3:{file}");
                var merged = MdExplorer.Features.Git.DevelopmentYamlMerge.Merge(baseText.Ok ? baseText.Stdout : null, ours, theirs, out var conflict);
                if (merged == null)
                    throw Problem($"Il template e il progetto hanno cambiato in modo diverso la stessa voce di {file} ({conflict}): sistemala a mano e riprova.",
                                  $"The template and the project changed the same entry of {file} differently ({conflict}): fix it by hand and try again.");
                await File.WriteAllTextAsync(System.IO.Path.Combine(path, file), merged, ct);
                await Run(path, ct, "add", "--", file);
                return;
            }
            throw Problem(conflicts.Count > 0
                    ? $"L'aggiornamento tocca file che il progetto ha cambiato ({string.Join(", ", conflicts)}): non ho unito niente e il progetto è com'era."
                    : $"L'aggiornamento non si unisce: {why}",
                conflicts.Count > 0
                    ? $"The update touches files the project changed ({string.Join(", ", conflicts)}): I merged nothing and the project is as it was."
                    : $"The update does not merge: {why}");
        }

        /// <summary>Il progetto è la radice di un repository, senza unioni a metà né modifiche senza commit.</summary>
        private async Task CleanProjectAsync(string path, string doing, CancellationToken ct)
        {
            var top = await _git.RunAsync(path, new[] { "rev-parse", "--show-toplevel" }, ct);
            if (!top.Ok || !SamePath(top.Stdout.Trim(), path))
                throw Problem("Il progetto non è un repository git (o ne è una sottocartella).", "The project is not a git repository (or is a subfolder of one).");
            if ((await _git.RunAsync(path, new[] { "rev-parse", "-q", "--verify", "MERGE_HEAD" }, ct)).Ok)
                throw Problem($"Nel progetto c'è un'unione rimasta a metà: chiudila o annullala prima di {doing}.", $"The project has a merge left half done: finish or abort it before {doing}.");
            var dirty = (await Run(path, ct, "status", "--porcelain", "--untracked-files=no")).Trim();
            if (dirty.Length > 0)
                throw Problem($"Il progetto ha modifiche senza commit ({string.Join(", ", dirty.Split('\n').Take(5).Select(l => l.Trim()))}): fai il commit o annullale prima di {doing}.",
                              $"The project has uncommitted changes ({string.Join(", ", dirty.Split('\n').Take(5).Select(l => l.Trim()))}): commit or discard them before {doing}.");
        }

        private static string Full(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                throw Problem($"Il progetto '{projectPath}' non esiste.", $"The project '{projectPath}' does not exist.");
            return System.IO.Path.GetFullPath(projectPath).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        }

        private static void Write(string file, DevelopmentConfig config)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
            File.WriteAllText(file, serializer.Serialize(config));
        }
    }
}
