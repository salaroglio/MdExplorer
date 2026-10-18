using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Templates;
using MdExplorer.Service.Models;
using MdExplorer.Services.Git;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services.Templates
{
    /// <summary>Che cosa serve per creare un progetto da un template.</summary>
    public sealed class TemplateCreateRequest
    {
        /// <summary>La cartella del progetto: non esiste ancora, o è vuota.</summary>
        public string Path { get; init; }
        public string TemplateUrl { get; init; }
        public string Language { get; init; }
        /// <summary>L'esempio compilato invece della sola struttura.</summary>
        public bool Example { get; init; }
        /// <summary>
        /// Il repository vuoto del gruppo, dove il progetto si pubblica. Null = un <c>origin</c> locale accanto alla cartella
        /// (<c>&lt;cartella&gt;.origin.git</c>), per provare da soli: gli agenti consegnano su <c>origin</c>.
        /// </summary>
        public string OriginUrl { get; init; }
        /// <summary>La versione di questo MdExplorer, per <c>requires.mde</c> del manifesto.</summary>
        public string MdeVersion { get; init; }
        /// <summary>
        /// I metodi da unire dopo il primo template, nell'ordine (P2: un progetto nasce da più template). Stessa lingua e
        /// stessa scelta dell'esempio del primo.
        /// </summary>
        public IReadOnlyList<string> Methods { get; init; }
        /// <summary>
        /// Il motore del progetto (<c>copilot</c>, <c>opencode</c>, <c>claude</c>, <c>none</c>) quando la base non lo dichiara:
        /// un progetto che parte da un metodo senza base lo sceglie qui (P5). Una base che lo dichiara vince.
        /// </summary>
        public string Harness { get; init; }
    }

    public sealed class TemplateCreateOutcome
    {
        public string Path { get; init; }
        public string Id { get; init; }
        public string Title { get; init; }
        public string Version { get; init; }
        public string Origin { get; init; }
        /// <summary>I metodi uniti dopo il primo template, nell'ordine.</summary>
        public IReadOnlyList<TemplateAddOutcome> Methods { get; init; } = Array.Empty<TemplateAddOutcome>();
        /// <summary>Che cosa MDE ha scritto nei file comuni (ruoli, istruzioni, workflow), e ciò che resta da fare.</summary>
        public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
    }

    /// <summary>Che cosa serve per aggiungere un metodo a un progetto che c'è già.</summary>
    public sealed class TemplateAddRequest
    {
        public string ProjectPath { get; init; }
        public string TemplateUrl { get; init; }
        public string Language { get; init; }
        public bool Example { get; init; }
        public string MdeVersion { get; init; }
    }

    public sealed class TemplateAddOutcome
    {
        public string Id { get; init; }
        public string Title { get; init; }
        public string Version { get; init; }
        /// <summary>Che cosa MDE ha scritto nei file comuni, e ciò che resta da fare.</summary>
        public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
    }

    public interface IProjectTemplateCreator
    {
        /// <summary>
        /// Crea il progetto dal template (P1 del piano dei template). Qualunque cosa vada storta, la cartella e l'origin
        /// locale creati qui vengono tolti e l'eccezione dice perché, per la persona.
        /// </summary>
        Task<TemplateCreateOutcome> CreateAsync(TemplateCreateRequest request, CancellationToken ct = default);

        /// <summary>
        /// «Aggiungi un metodo» (P2): unisce un template <c>method</c> al progetto, applica il suo manifesto ai file comuni e
        /// fa un commit. Il progetto deve essere un repository git senza modifiche non committate. Qualunque cosa vada storta,
        /// il progetto torna com'era e l'eccezione dice perché. Non pubblica: il commit lo si manda con la barra di git.
        /// </summary>
        Task<TemplateAddOutcome> AddMethodAsync(TemplateAddRequest request, CancellationToken ct = default);
    }

    public sealed partial class ProjectTemplateCreator : IProjectTemplateCreator
    {
        /// <summary>Il ramo del progetto creato, qualunque sia il ramo del template da cui parte.</summary>
        public const string ProjectBranch = "main";
        public const string RemotePrefix = "template-";

        private static readonly Regex ManifestPath = new(@"^\.mde-template/([^/]+)/mde-template\.yml$", RegexOptions.Compiled);

        private readonly INativeGitRunner _git;
        private readonly INativeGitTransport _transport;
        private readonly ILogger<ProjectTemplateCreator> _logger;

        public ProjectTemplateCreator(INativeGitRunner git, INativeGitTransport transport, ILogger<ProjectTemplateCreator> logger)
        {
            _git = git;
            _transport = transport;
            _logger = logger;
        }

        public async Task<TemplateCreateOutcome> CreateAsync(TemplateCreateRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request?.Path)) throw Problem("Manca la cartella del progetto.", "The project folder is missing.");
            if (string.IsNullOrWhiteSpace(request.TemplateUrl)) throw Problem("Manca l'indirizzo del template.", "The template address is missing.");
            if (string.IsNullOrWhiteSpace(request.Language)) throw Problem("Manca la lingua del template.", "The template language is missing.");
            if (!TemplateManifestReader.IsAppVersion(request.MdeVersion))
                throw Problem($"La versione di MdExplorer '{request.MdeVersion}' non si legge: serve per sapere se il template ci gira.",
                              $"The MdExplorer version '{request.MdeVersion}' cannot be read: it is needed to know whether the template runs here.");

            HarnessTarget? requested = null;
            if (!string.IsNullOrWhiteSpace(request.Harness))
            {
                if (!HarnessLayout.TryParseId(request.Harness.Trim(), out var parsed))
                    throw Problem($"Il motore '{request.Harness}' non esiste: vale {HarnessLayout.AllowedIds}.",
                                  $"The engine '{request.Harness}' does not exist: it is one of {HarnessLayout.AllowedIds}.");
                requested = parsed;
            }

            var path = System.IO.Path.GetFullPath(request.Path).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            var parent = System.IO.Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent))
                throw Problem($"La cartella che deve contenere il progetto non esiste: '{parent}'.", $"The folder that must contain the project does not exist: '{parent}'.");
            if (File.Exists(path) || (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any()))
                throw Problem($"'{path}' esiste già e non è vuota: un progetto da template nasce in una cartella nuova.",
                              $"'{path}' already exists and is not empty: a project from a template starts in a new folder.");
            var localOrigin = string.IsNullOrWhiteSpace(request.OriginUrl) ? path + ".origin.git" : null;
            if (localOrigin != null && (Directory.Exists(localOrigin) || File.Exists(localOrigin)))
                throw Problem($"Esiste già '{localOrigin}': non lo sovrascrivo. Spostalo o scegli un'altra cartella.",
                              $"'{localOrigin}' already exists: I will not overwrite it. Move it or choose another folder.");

            var createdFolder = !Directory.Exists(path);
            var createdOrigin = false;
            try
            {
                Directory.CreateDirectory(path);
                await Run(path, ct, "init", "-q", "-b", ProjectBranch);

                var (tip, branch, manifest) = await PrepareAsync(path, request.TemplateUrl, request.Language, request.Example, request.MdeVersion,
                    new HashSet<string>(StringComparer.Ordinal), ct);
                await Run(path, ct, "checkout", "-q", "--no-track", "-b", ProjectBranch, tip);

                var (harness, chosen) = MdExplorer.Service.ProjectsManager.PrepareTemplateProject(path, requested);
                await Run(path, ct, "add", "--", ".gitignore", DevelopmentConfigCleanup.FileName);

                var notes = (await ApplyManifestAsync(path, manifest, Row(manifest, request.TemplateUrl, request.Language, request.Example), ct)).ToList();
                if (harness == HarnessTarget.None)
                    notes.Insert(0, UiText.Say("Il progetto non ha un motore: gli agenti e MarkAgent non partono finché non ne scegli uno nelle impostazioni.",
                                               "The project has no engine: the agents and MarkAgent do not start until you choose one in the settings."));
                else if (chosen)
                    notes.Insert(0, UiText.Say($"Motore del progetto: {HarnessSettings.IdOf(harness)}.", $"Project engine: {HarnessSettings.IdOf(harness)}."));
                await AddRoomSecretAsync(path, ct);
                await Run(path, ct, "commit", "-q", "-m",
                    UiText.Say($"Progetto creato dal template «{manifest.Title}» {manifest.Version}", $"Project created from the template «{manifest.Title}» {manifest.Version}"));

                // Gli altri metodi, nell'ordine: ognuno con la sua unione e il suo commit.
                var methods = new List<TemplateAddOutcome>();
                foreach (var url in request.Methods ?? Array.Empty<string>())
                    methods.Add(await MergeMethodAsync(path, url, request.Language, request.Example, request.MdeVersion, ct));

                string origin;
                if (localOrigin != null)
                {
                    await Run(parent, ct, "init", "-q", "--bare", "-b", ProjectBranch, localOrigin);
                    createdOrigin = true;
                    origin = localOrigin;
                }
                else origin = request.OriginUrl.Trim();
                await Run(path, ct, "remote", "add", "origin", origin);
                var pushed = await _transport.PushAsync(path, "origin", ProjectBranch, ct);
                if (!pushed.Ok)
                    throw Problem($"Il progetto non si pubblica su '{origin}': {pushed.Error} Serve un repository vuoto su cui puoi scrivere.",
                                  $"The project cannot be pushed to '{origin}': {pushed.Error} An empty repository you can write to is needed.");
                // I posti di lavoro degli agenti, la revisione e «Autorizza» partono da origin/HEAD.
                await Run(path, ct, "remote", "set-head", "origin", ProjectBranch);

                _logger.LogInformation("[Template] '{Path}' creato da '{Id}' {Version} ({Branch}){Methods}; origin '{Origin}'.", path, manifest.Id, manifest.Version, branch,
                    methods.Count == 0 ? "" : " + " + string.Join(", ", methods.Select(m => $"{m.Id} {m.Version}")), origin);
                return new TemplateCreateOutcome
                {
                    Path = path, Id = manifest.Id, Title = manifest.Title, Version = manifest.Version, Origin = origin,
                    Methods = methods, Notes = notes.Concat(methods.SelectMany(m => m.Notes)).ToList(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Template] creazione di '{Path}' da '{Url}' non riuscita: tolgo quello che ho creato.", path, request.TemplateUrl);
                if (createdFolder) DeleteTree(path);
                else foreach (var entry in Directory.EnumerateFileSystemEntries(path)) DeleteTree(entry);
                if (createdOrigin) DeleteTree(localOrigin);
                if (ex is InvalidOperationException) throw;
                throw Problem($"Il progetto non è stato creato: {ex.Message}", $"The project was not created: {ex.Message}");
            }
        }

        public async Task<TemplateAddOutcome> AddMethodAsync(TemplateAddRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request?.ProjectPath) || !Directory.Exists(request.ProjectPath))
                throw Problem($"Il progetto '{request?.ProjectPath}' non esiste.", $"The project '{request?.ProjectPath}' does not exist.");
            if (string.IsNullOrWhiteSpace(request.TemplateUrl)) throw Problem("Manca l'indirizzo del metodo.", "The method address is missing.");
            if (string.IsNullOrWhiteSpace(request.Language)) throw Problem("Manca la lingua del metodo.", "The method language is missing.");
            if (!TemplateManifestReader.IsAppVersion(request.MdeVersion))
                throw Problem($"La versione di MdExplorer '{request.MdeVersion}' non si legge: serve per sapere se il metodo ci gira.",
                              $"The MdExplorer version '{request.MdeVersion}' cannot be read: it is needed to know whether the method runs here.");
            var path = System.IO.Path.GetFullPath(request.ProjectPath).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            var top = await _git.RunAsync(path, new[] { "rev-parse", "--show-toplevel" }, ct);
            if (!top.Ok || !SamePath(top.Stdout.Trim(), path))
                throw Problem("Il progetto non è un repository git (o ne è una sottocartella): un metodo si unisce con git.",
                              "The project is not a git repository (or is a subfolder of one): a method is merged with git.");
            if (File.Exists(System.IO.Path.Combine(path, ".git", "MERGE_HEAD")) || (await _git.RunAsync(path, new[] { "rev-parse", "-q", "--verify", "MERGE_HEAD" }, ct)).Ok)
                throw Problem("Nel progetto c'è un'unione rimasta a metà: chiudila o annullala prima di aggiungere un metodo.",
                              "The project has a merge left half done: finish or abort it before adding a method.");
            var dirty = (await Run(path, ct, "status", "--porcelain", "--untracked-files=no")).Trim();
            if (dirty.Length > 0)
                throw Problem($"Il progetto ha modifiche non committate ({string.Join(", ", dirty.Split('\n').Take(5).Select(l => l.Trim()))}): committale o annullale, poi aggiungi il metodo. Il metodo arriva con un commit suo, che non deve portarsi dietro il tuo lavoro.",
                              $"The project has uncommitted changes ({string.Join(", ", dirty.Split('\n').Take(5).Select(l => l.Trim()))}): commit or discard them, then add the method. The method comes with its own commit, which must not carry your work.");

            var head = (await Run(path, ct, "rev-parse", "HEAD")).Trim();
            try
            {
                var outcome = await MergeMethodAsync(path, request.TemplateUrl, request.Language, request.Example, request.MdeVersion, ct);
                _logger.LogInformation("[Template] '{Path}': aggiunto il metodo '{Id}' {Version}.", path, outcome.Id, outcome.Version);
                return outcome;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Template] '{Path}': il metodo da '{Url}' non si aggiunge; il progetto torna com'era.", path, request.TemplateUrl);
                await UndoAsync(path, head, ct);
                if (ex is InvalidOperationException) throw;
                throw Problem($"Il metodo non è stato aggiunto: {ex.Message}", $"The method was not added: {ex.Message}");
            }
        }

        /// <summary>
        /// Unisce un metodo al progetto (che ha già un commit): scarica, controlla, unisce le storie indipendenti senza
        /// committare, applica il manifesto e fa un commit solo. Chi chiama rimette a posto se qualcosa va storto.
        /// </summary>
        private async Task<TemplateAddOutcome> MergeMethodAsync(string path, string url, string language, bool example, string mdeVersion, CancellationToken ct)
        {
            var present = PresentTemplates(path);
            var (tip, _, manifest) = await PrepareAsync(path, url, language, example, mdeVersion, present.Select(t => t.Id).ToHashSet(StringComparer.Ordinal), ct);
            if (!manifest.IsMethod)
                throw Problem($"«{manifest.Title}» è un template di progetto (la base di un progetto intero): si sceglie solo alla creazione, non si aggiunge.",
                              $"«{manifest.Title}» is a project template (the base of a whole project): it is chosen only at creation, it is not added.");

            // La zona del metodo deve essere libera: il metodo porta solo la sua cartella e le sue skill.
            var tracked = (await Run(path, ct, "ls-files"))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();
            var taken = tracked.Where(f => TemplateManifestReader.OutsideZone(manifest.Id, new[] { f }).Count == 0).ToList();
            if (taken.Count > 0)
                throw Problem($"Il progetto ha già file nella zona del metodo '{manifest.Id}' ({string.Join(", ", taken.Take(5))}): non li sovrascrivo.",
                              $"The project already has files in the zone of method '{manifest.Id}' ({string.Join(", ", taken.Take(5))}): I will not overwrite them.");

            var merge = await _git.RunAsync(path, new[] { "merge", "--no-ff", "--no-commit", "--allow-unrelated-histories", tip }, ct);
            if (!merge.Ok)
                throw Problem($"Il metodo non si unisce al progetto: {merge.Describe()}", $"The method does not merge into the project: {merge.Describe()}");

            var notes = await ApplyManifestAsync(path, manifest, Row(manifest, url, language, example), ct);
            await Run(path, ct, "commit", "-q", "-m",
                UiText.Say($"Metodo «{manifest.Title}» {manifest.Version} aggiunto dal template", $"Method «{manifest.Title}» {manifest.Version} added from the template"));
            return new TemplateAddOutcome { Id = manifest.Id, Title = manifest.Title, Version = manifest.Version, Notes = notes };
        }

        /// <summary>
        /// Scarica il template su un remoto provvisorio, ne trova il manifesto, rinomina il remoto <c>template-&lt;id&gt;</c> e
        /// controlla il ramo scelto. Il manifesto che fa fede è quello di quel ramo.
        /// </summary>
        private async Task<(string Tip, string Branch, TemplateManifest Manifest)> PrepareAsync(
            string path, string url, string language, bool example, string mdeVersion, ISet<string> presentIds, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(url)) throw Problem("Manca l'indirizzo di un template.", "A template address is missing.");
            // Il nome vero del remoto si sa dal manifesto: si scarica con un nome provvisorio e poi si rinomina.
            const string probe = RemotePrefix + "new";
            await Run(path, ct, "remote", "add", probe, url.Trim());
            var fetched = await _transport.FetchAsync(path, probe, ct);
            if (!fetched.Ok)
                throw Problem($"Il template non si scarica da '{url}': {fetched.Error}", $"The template cannot be downloaded from '{url}': {fetched.Error}");

            var (id, found) = await FindManifestAsync(path, probe, ct);
            if (presentIds.Contains(id))
                throw Problem($"Il progetto ha già il template '{id}': un template si aggiunge una volta sola.",
                              $"The project already has the template '{id}': a template is added only once.");
            var branch = found.BranchFor(language, example);
            var remote = RemotePrefix + id;
            if ((await _git.RunAsync(path, new[] { "remote", "get-url", remote }, ct)).Ok)
                throw Problem($"Il progetto ha già un remoto '{remote}' senza la sua riga in {DevelopmentConfigCleanup.FileName}: toglilo, o rimetti la riga, e riprova.",
                              $"The project already has a remote '{remote}' without its row in {DevelopmentConfigCleanup.FileName}: remove it, or put the row back, and try again.");
            await Run(path, ct, "remote", "rename", probe, remote);

            var tip = $"{remote}/{branch}";
            if (!(await _git.RunAsync(path, new[] { "rev-parse", "--verify", "--quiet", tip + "^{commit}" }, ct)).Ok)
                throw Problem($"Il manifesto del template indica il ramo '{branch}', che nel repository non c'è.",
                              $"The template manifest names the branch '{branch}', which is not in the repository.");

            var manifest = TemplateManifestReader.ReadOrThrow(await ShowAsync(path, tip, TemplateManifest.PathOf(id), ct), id, $"{TemplateManifest.PathOf(id)} ({branch})");
            await CheckAsync(path, tip, manifest, mdeVersion, presentIds, ct);
            return (tip, branch, manifest);
        }

        private static ProjectTemplateConfig Row(TemplateManifest manifest, string url, string language, bool example) => new()
        {
            Id = manifest.Id,
            Url = url.Trim(),
            Kind = manifest.IsMethod ? null : TemplateManifest.Project,
            Language = language,
            Example = example,
            Version = manifest.Version,
        };

        /// <summary>Le righe dei template del progetto, lette con tolleranza zero.</summary>
        private static List<ProjectTemplateConfig> PresentTemplates(string path)
        {
            var file = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            return (File.Exists(file) ? DevelopmentConfigCleanup.ReadStrict(file).Templates : null) ?? new List<ProjectTemplateConfig>();
        }

        /// <summary>
        /// Il manifesto applicato ai file del progetto (P0, «Il manifesto applicato ai file del progetto»): la riga del template
        /// e i suoi workflow in <c>.development.yml</c>, i suoi ruoli come ruoli aperti nel documento delle responsabilità, le
        /// sue istruzioni nel blocco del metodo dei file d'istruzioni. Mette i file nell'indice; poi i workflow del progetto
        /// devono leggersi tutti insieme, sennò non si va avanti. Le note dicono che cosa è stato scritto e cosa resta da fare.
        /// </summary>
        /// <param name="update">
        /// Aggiornamento (P3): la riga c'è già (o arriva dalla migrazione di una vecchia demo) e prende la versione nuova; i ruoli
        /// e i workflow nuovi si aggiungono, quelli che il metodo non ha più restano e lo si dice; il blocco delle istruzioni si
        /// sostituisce.
        /// </param>
        private async Task<IReadOnlyList<string>> ApplyManifestAsync(string path, TemplateManifest manifest, ProjectTemplateConfig row, CancellationToken ct,
            bool update = false, TemplateManifest previous = null)
        {
            var notes = new List<string>();
            var touched = new List<string> { DevelopmentConfigCleanup.FileName };
            try
            {
                await ApplyManifestCoreAsync(path, manifest, row, notes, touched, update, previous, ct);
            }
            finally
            {
                // Nell'indice anche se un passo non va: chi annulla torna al commit di prima e così toglie anche i file nuovi.
                var existing = touched.Distinct().Where(t => File.Exists(System.IO.Path.Combine(path, t.Replace('/', System.IO.Path.DirectorySeparatorChar)))).ToArray();
                if (existing.Length > 0) await _git.RunAsync(path, new[] { "add", "--" }.Concat(existing).ToArray(), ct);
            }
            return notes;
        }

        private async Task ApplyManifestCoreAsync(string path, TemplateManifest manifest, ProjectTemplateConfig row, List<string> notes, List<string> touched,
            bool update, TemplateManifest previous, CancellationToken ct)
        {
            var file = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            var config = File.Exists(file) ? DevelopmentConfigCleanup.ReadStrict(file) : new DevelopmentConfig();

            config.Templates ??= new List<ProjectTemplateConfig>();
            var existing = config.Templates.FindIndex(t => t.Id == row.Id);
            if (existing >= 0 && !update)
                throw Problem($"Il template porta già una riga '{row.Id}' in {DevelopmentConfigCleanup.FileName}: un template non scrive le sue righe, le scrive MDE.",
                              $"The template already carries a '{row.Id}' row in {DevelopmentConfigCleanup.FileName}: a template does not write its rows, MDE does.");
            if (existing >= 0) config.Templates[existing] = row;
            else config.Templates.Add(row);

            // Ciò che la versione nuova non porta più resta nel progetto (è del gruppo, ora): lo si dice.
            if (previous != null)
            {
                var goneRoles = (previous.Roles ?? new List<TemplateRole>()).Select(r => r.Role)
                    .Where(r => !(manifest.Roles ?? new List<TemplateRole>()).Any(n => string.Equals(n.Role, r, StringComparison.OrdinalIgnoreCase))).ToList();
                if (goneRoles.Count > 0)
                    notes.Add(UiText.Say($"La versione nuova non ha più i ruoli {string.Join(", ", goneRoles)}: restano nel documento delle responsabilità, toglili tu se non servono.",
                                         $"The new version no longer has the roles {string.Join(", ", goneRoles)}: they stay in the responsibilities document, remove them yourself if not needed."));
                var goneDocs = (previous.Workflows ?? new List<string>())
                    .Where(d => !(manifest.Workflows ?? new List<string>()).Contains(d, StringComparer.OrdinalIgnoreCase)).ToList();
                if (goneDocs.Count > 0)
                    notes.Add(UiText.Say($"La versione nuova non ha più i workflow {string.Join(", ", goneDocs)}: restano nell'elenco del progetto, toglili dalle impostazioni se non servono.",
                                         $"The new version no longer has the workflows {string.Join(", ", goneDocs)}: they stay in the project's list, remove them from the settings if not needed."));
            }

            // I ruoli: righe aperte nel documento delle responsabilità; se il progetto non ne ha uno, nasce alla radice.
            var roles = (manifest.Roles ?? new List<TemplateRole>()).Select(r => (r.Role, r.Description, (IReadOnlyList<string>)(r.Agents ?? new List<string>()))).ToList();
            if (roles.Count > 0)
            {
                config.AgentCity ??= new AgentCityConfig();
                var ownership = string.IsNullOrWhiteSpace(config.AgentCity.OwnershipDoc) ? OwnershipFile : config.AgentCity.OwnershipDoc.Trim().Replace('\\', '/').TrimStart('/');
                var ownershipFull = System.IO.Path.Combine(path, ownership.Replace('/', System.IO.Path.DirectorySeparatorChar));
                var before = File.Exists(ownershipFull) ? await File.ReadAllTextAsync(ownershipFull, ct) : null;
                var text = MdExplorer.Features.Agents.OwnershipDocWriter.AddOpenRoles(before, roles, out var added);
                if (before == null || added.Count > 0)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ownershipFull));
                    await File.WriteAllTextAsync(ownershipFull, text, ct);
                    touched.Add(ownership);
                }
                if (string.IsNullOrWhiteSpace(config.AgentCity.OwnershipDoc))
                {
                    config.AgentCity.OwnershipDoc = ownership;
                    notes.Add(UiText.Say($"Creato {ownership}, il documento delle responsabilità del progetto.", $"Created {ownership}, the project's responsibilities document."));
                }
                if (added.Count > 0)
                    notes.Add(UiText.Say($"Ruoli aperti aggiunti in {ownership}: {string.Join(", ", added)}. Chi ne risponde scrive lì il suo nome e la sua email.",
                                         $"Open roles added in {ownership}: {string.Join(", ", added)}. Whoever answers for them writes their name and email there."));
            }

            // I workflow del metodo nell'elenco del progetto (PW).
            var docs = manifest.Workflows ?? new List<string>();
            var docsBefore = (config.AgentCity?.WorkflowDocs ?? new List<string>()).ToList();
            if (docs.Count > 0)
            {
                config.AgentCity ??= new AgentCityConfig();
                config.AgentCity.WorkflowDocs ??= new List<string>();
                var newDocs = docs.Where(d => !config.AgentCity.WorkflowDocs.Contains(d, StringComparer.OrdinalIgnoreCase)).ToList();
                config.AgentCity.WorkflowDocs.AddRange(newDocs);
                if (newDocs.Count > 0)
                    notes.Add(UiText.Say($"Workflow del progetto: {string.Join(", ", newDocs)}.", $"Project workflows: {string.Join(", ", newDocs)}."));
                // I workflow girano nella città degli agenti: un metodo che li porta la accende (P5), e lo dice.
                if (!config.AgentCity.Enabled)
                {
                    config.AgentCity.Enabled = true;
                    notes.Add(UiText.Say($"Città degli agenti accesa: i workflow di «{manifest.Title}» girano lì.",
                                         $"Agent city turned on: the workflows of «{manifest.Title}» run there."));
                }
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
            await File.WriteAllTextAsync(file, serializer.Serialize(config), ct);

            // Le istruzioni: un blocco del metodo in ogni file d'istruzioni che il progetto ha, e in quello del suo motore.
            if (!string.IsNullOrWhiteSpace(manifest.Instructions))
            {
                var instructions = await File.ReadAllTextAsync(System.IO.Path.Combine(path, manifest.Instructions.Replace('/', System.IO.Path.DirectorySeparatorChar)), ct);
                var targets = HarnessLayout.All.Select(l => l.InstructionsFile)
                    .Where(f => File.Exists(System.IO.Path.Combine(path, f.Replace('/', System.IO.Path.DirectorySeparatorChar)))).ToList();
                var harness = config.Harness?.Target;
                var own = HarnessLayout.All.FirstOrDefault(l => string.Equals(l.Id, harness, StringComparison.OrdinalIgnoreCase))?.InstructionsFile;
                if (own != null && !targets.Contains(own)) targets.Add(own);
                if (targets.Count == 0)
                    notes.Add(UiText.Say($"Le istruzioni del metodo ({manifest.Instructions}) non sono in nessun file d'istruzioni: il progetto non ne ha e non ha un motore. Scegline uno nelle impostazioni e aggiungi di nuovo il blocco.",
                                         $"The method's instructions ({manifest.Instructions}) are in no instructions file: the project has none and no engine. Choose one in the settings and add the block again."));
                foreach (var target in targets)
                {
                    var full = System.IO.Path.Combine(path, target.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    var before = File.Exists(full) ? await File.ReadAllTextAsync(full, ct) : null;
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full));
                    await File.WriteAllTextAsync(full, TemplateInstructions.Apply(before, manifest.Id, instructions), ct);
                    touched.Add(target);
                }
                if (targets.Count > 0)
                    notes.Add(UiText.Say($"Istruzioni del metodo in {string.Join(", ", targets)}.", $"Method instructions in {string.Join(", ", targets)}."));
            }

            await ApplySeedsAsync(path, manifest, notes, touched, update, previous, ct);

            // I workflow del progetto devono stare insieme (PW). Un metodo che li rompe non entra. Se erano già guasti prima di
            // lui, si controllano i suoi da soli e lo si dice. Per la base del progetto (la demo) un workflow che non si legge
            // non ferma la creazione: il progetto nasce, e una nota dice che i workflow sono fermi finché non si correggono.
            if (config.AgentCity?.WorkflowDocs?.Count > 0)
            {
                var ownershipDoc = config.AgentCity.OwnershipDoc;
                MdExplorer.Features.Agents.Workflow.WorkflowDocument.LoadAll(path, config.AgentCity.WorkflowDocs, ownershipDoc, out var problem);
                if (problem != null && !manifest.IsMethod)
                    notes.Add(UiText.Say($"I workflow del progetto non si leggono ({problem}): finché non si correggono, gli incarichi del workflow restano fermi.",
                                         $"The project's workflows cannot be read ({problem}): until they are fixed, the workflow's assignments stay stopped."));
                else if (problem != null)
                {
                    string before = null;
                    if (docsBefore.Count > 0) MdExplorer.Features.Agents.Workflow.WorkflowDocument.LoadAll(path, docsBefore, ownershipDoc, out before);
                    if (before == null)
                        throw Problem($"Con «{manifest.Title}» i workflow del progetto non si leggono: {problem}",
                                      $"With «{manifest.Title}» the project's workflows cannot be read: {problem}");
                    var own = (manifest.Workflows ?? new List<string>()).ToList();
                    if (own.Count > 0)
                    {
                        MdExplorer.Features.Agents.Workflow.WorkflowDocument.LoadAll(path, own, ownershipDoc, out var mine);
                        if (mine != null)
                            throw Problem($"I workflow di «{manifest.Title}» non si leggono: {mine}", $"The workflows of «{manifest.Title}» cannot be read: {mine}");
                    }
                    notes.Add(UiText.Say($"I workflow del progetto non si leggevano già prima di «{manifest.Title}» ({before}): finché non si correggono, restano fermi tutti.",
                                         $"The project's workflows could not be read already before «{manifest.Title}» ({before}): until they are fixed, they all stay stopped."));
                }
            }
            if (manifest.Roles?.Any(r => r.Agents?.Count > 0) == true)
                notes.Add(UiText.Say("Gli agenti del metodo partono quando qualcuno li dichiara suoi («È mio») e li autorizza.",
                                     "The method's agents start once someone declares them theirs («It's mine») and trusts them."));

        }

        /// <summary>
        /// I file iniziali (P5, <c>seeds</c>): ognuno si copia dalla cartella del template al suo posto nel progetto se lì non
        /// c'è niente; da quel momento è del gruppo. Un file che c'è già non si tocca mai. Aggiornando, un seme che la versione
        /// nuova ha cambiato è solo una nota (il confronto è con il commit di prima dell'unione, che è ancora HEAD), e un seme
        /// che c'era già nella versione di prima non rinasce: se manca, il gruppo l'ha tolto.
        /// </summary>
        private async Task ApplySeedsAsync(string path, TemplateManifest manifest, List<string> notes, List<string> touched, bool update,
            TemplateManifest previous, CancellationToken ct)
        {
            var created = new List<string>();
            var kept = new List<string>();
            var changed = new List<string>();
            var before = (previous?.Seeds ?? new List<TemplateSeed>()).Select(s => s.To).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var seed in manifest.Seeds ?? new List<TemplateSeed>())
            {
                var source = System.IO.Path.Combine(path, seed.From.Replace('/', System.IO.Path.DirectorySeparatorChar));
                var target = System.IO.Path.Combine(path, seed.To.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (!File.Exists(source))
                    throw Problem($"Il seme {seed.From} del template «{manifest.Title}» non c'è.", $"The seed {seed.From} of the template «{manifest.Title}» is missing.");
                if (File.Exists(target) || Directory.Exists(target))
                {
                    if (!update) { kept.Add(seed.To); continue; }
                    // L'unione è nell'indice: diff fra HEAD e indice sul modello. Uscita 1 = cambiato.
                    var diff = await _git.RunAsync(path, new[] { "diff", "--cached", "--quiet", "HEAD", "--", seed.From }, ct);
                    if (diff.ExitCode > 1)
                        throw Problem($"Non riesco a confrontare il seme {seed.From}: {diff.Describe()}", $"I cannot compare the seed {seed.From}: {diff.Describe()}");
                    if (diff.ExitCode == 1 && before.Contains(seed.To)) changed.Add(seed.To);
                    continue;
                }
                if (update && before.Contains(seed.To)) continue;
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target));
                File.Copy(source, target);
                touched.Add(seed.To);
                created.Add(seed.To);
            }
            if (created.Count > 0)
                notes.Add(UiText.Say($"File iniziali del metodo, ora del gruppo: {string.Join(", ", created)}.",
                                     $"The method's starting files, now the group's: {string.Join(", ", created)}."));
            if (kept.Count > 0)
                notes.Add(UiText.Say($"Questi file c'erano già e restano come sono: {string.Join(", ", kept)}.",
                                     $"These files were already there and stay as they are: {string.Join(", ", kept)}."));
            if (changed.Count > 0)
                notes.Add(UiText.Say($"La versione nuova ha cambiato i modelli di {string.Join(", ", changed)}: i tuoi non li tocco, confrontali con quelli in {TemplateManifest.ZoneOf(manifest.Id)}.",
                                     $"The new version changed the models of {string.Join(", ", changed)}: yours are left alone, compare them with those in {TemplateManifest.ZoneOf(manifest.Id)}."));
        }

        /// <summary>
        /// Il segreto della stanza del progetto, nel commit della creazione: altrimenti MDE lo scrive alla prima apertura della
        /// chat, e il progetto appena creato avrebbe subito una modifica da committare (che ferma aggiornamenti e metodi).
        /// Un template non lo porta: ogni progetto ha il suo.
        /// </summary>
        private async Task AddRoomSecretAsync(string path, CancellationToken ct)
        {
            var file = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            var config = DevelopmentConfigCleanup.ReadStrict(file);
            if (!string.IsNullOrWhiteSpace(config.AgentCity?.RoomSecret)) return;
            config.AgentCity ??= new AgentCityConfig();
            config.AgentCity.RoomSecret = MdExplorer.Services.ProjectMetadataService.GenerateRoomSecret();
            Write(file, config);
            await Run(path, ct, "add", "--", DevelopmentConfigCleanup.FileName);
        }

        /// <summary>Il documento delle responsabilità che nasce quando un metodo porta ruoli e il progetto non ne ha uno.</summary>
        public const string OwnershipFile = "responsabilita.md";

        /// <summary>Il progetto torna al commit di prima: l'unione annullata, i file nuovi tolti, il remoto del metodo tolto.</summary>
        private async Task UndoAsync(string path, string head, CancellationToken ct)
        {
            var steps = new List<string[]>
            {
                new[] { "merge", "--abort" },
                new[] { "reset", "-q", "--hard", head },
            };
            foreach (var step in steps) await _git.RunAsync(path, step, ct);
            var remotes = (await _git.RunAsync(path, new[] { "remote" }, ct)).Stdout ?? "";
            foreach (var r in remotes.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()))
                if (r.StartsWith(RemotePrefix, StringComparison.Ordinal) && !PresentTemplates(path).Any(t => RemotePrefix + t.Id == r))
                    await _git.RunAsync(path, new[] { "remote", "remove", r }, ct);
        }

        private static bool SamePath(string a, string b)
            => string.Equals(System.IO.Path.GetFullPath(a).TrimEnd('/', '\\'), System.IO.Path.GetFullPath(b).TrimEnd('/', '\\'),
                             OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        /// <summary>
        /// Il manifesto dei rami scaricati: ogni ramo che ne ha uno deve averne <b>uno</b> e con lo stesso id. Da lì si sa il
        /// nome del template e quale ramo prendere per la lingua.
        /// </summary>
        private async Task<(string Id, TemplateManifest Manifest)> FindManifestAsync(string path, string remote, CancellationToken ct)
        {
            var refs = await Run(path, ct, "for-each-ref", "--format=%(refname:short)", $"refs/remotes/{remote}/");
            var branches = refs.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).Where(r => r != $"{remote}/HEAD").ToList();
            if (branches.Count == 0) throw Problem("Il repository del template è vuoto.", "The template repository is empty.");

            string id = null, from = null;
            foreach (var b in branches)
            {
                var files = await Run(path, ct, "ls-tree", "-r", "--name-only", b, "--", TemplateManifest.Folder);
                var ids = files.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => ManifestPath.Match(f.Trim())).Where(m => m.Success).Select(m => m.Groups[1].Value).Distinct().ToList();
                if (ids.Count == 0) continue;
                if (ids.Count > 1)
                    throw Problem($"Nel ramo '{b}' ci sono più manifesti ({string.Join(", ", ids)}): un repository porta un template solo.",
                                  $"The branch '{b}' has several manifests ({string.Join(", ", ids)}): a repository carries one template only.");
                if (id != null && ids[0] != id)
                    throw Problem($"I rami del template non sono d'accordo sull'id: '{id}' in '{from}', '{ids[0]}' in '{b}'.",
                                  $"The template branches disagree on the id: '{id}' in '{from}', '{ids[0]}' in '{b}'.");
                id = ids[0];
                from ??= b;
            }
            if (id == null)
                throw Problem($"Questo repository non è un template di MdExplorer: nessun ramo ha {TemplateManifest.Folder}/<id>/{TemplateManifest.FileName}.",
                              $"This repository is not an MdExplorer template: no branch has {TemplateManifest.Folder}/<id>/{TemplateManifest.FileName}.");

            var manifest = TemplateManifestReader.ReadOrThrow(await ShowAsync(path, from, TemplateManifest.PathOf(id), ct), id, $"{TemplateManifest.PathOf(id)} ({from})");
            return (id, manifest);
        }

        private async Task CheckAsync(string path, string tip, TemplateManifest manifest, string mdeVersion, ISet<string> presentIds, CancellationToken ct)
        {
            if (manifest.Requires?.Mde != null && TemplateManifestReader.CompareAppVersions(manifest.Requires.Mde, mdeVersion) > 0)
                throw Problem($"Il template «{manifest.Title}» {manifest.Version} chiede MdExplorer {manifest.Requires.Mde} o più nuovo; questo è {mdeVersion}. Aggiorna MdExplorer.",
                              $"The template «{manifest.Title}» {manifest.Version} needs MdExplorer {manifest.Requires.Mde} or newer; this is {mdeVersion}. Update MdExplorer.");
            var missingTemplates = (manifest.Requires?.Templates ?? new List<string>()).Where(t => !presentIds.Contains(t)).ToList();
            if (missingTemplates.Count > 0)
                throw Problem($"Il template «{manifest.Title}» ha bisogno di {string.Join(", ", missingTemplates)}, che il progetto non ha: aggiungilo prima.",
                              $"The template «{manifest.Title}» needs {string.Join(", ", missingTemplates)}, which the project does not have: add it first.");

            var files = (await Run(path, ct, "ls-tree", "-r", "--name-only", tip))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToHashSet(StringComparer.Ordinal);
            if (manifest.IsMethod)
            {
                var outside = TemplateManifestReader.OutsideZone(manifest.Id, files);
                if (outside.Count > 0)
                    throw Problem($"Il metodo «{manifest.Title}» ha file fuori dalla sua cartella ({string.Join(", ", outside.Take(5))}): non lo unisco.",
                                  $"The method «{manifest.Title}» has files outside its folder ({string.Join(", ", outside.Take(5))}): I will not merge it.");
            }
            var missing = (manifest.Workflows ?? new List<string>()).Append(manifest.Instructions)
                .Concat((manifest.Seeds ?? new List<TemplateSeed>()).Select(s => s.From)).Where(f => f != null && !files.Contains(f)).ToList();
            if (missing.Count > 0)
                throw Problem($"Il manifesto del template nomina file che nel ramo non ci sono: {string.Join(", ", missing)}.",
                              $"The template manifest names files that are not in the branch: {string.Join(", ", missing)}.");
        }

        private async Task<string> ShowAsync(string path, string tip, string file, CancellationToken ct)
            => await Run(path, ct, "show", $"{tip}:{file}");

        private async Task<string> Run(string dir, CancellationToken ct, params string[] args)
        {
            var r = await _git.RunAsync(dir, args, ct);
            if (!r.Ok) throw Problem($"git {string.Join(' ', args)}: {r.Describe()}", $"git {string.Join(' ', args)}: {r.Describe()}");
            return r.Stdout ?? string.Empty;
        }

        private void DeleteTree(string target)
        {
            try
            {
                if (File.Exists(target)) { File.SetAttributes(target, FileAttributes.Normal); File.Delete(target); return; }
                if (!Directory.Exists(target)) return;
                // Su Windows gli oggetti di git sono di sola lettura: Directory.Delete li rifiuta.
                foreach (var f in Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
                Directory.Delete(target, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Template] non riesco a togliere '{Target}': va tolto a mano.", target);
            }
        }

        private static InvalidOperationException Problem(string it, string en) => new(UiText.Say(it, en));
    }
}
