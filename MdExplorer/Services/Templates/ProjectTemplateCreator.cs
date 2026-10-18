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
    }

    public sealed class TemplateCreateOutcome
    {
        public string Path { get; init; }
        public string Id { get; init; }
        public string Title { get; init; }
        public string Version { get; init; }
        public string Origin { get; init; }
    }

    public interface IProjectTemplateCreator
    {
        /// <summary>
        /// Crea il progetto dal template (P1 del piano dei template). Qualunque cosa vada storta, la cartella e l'origin
        /// locale creati qui vengono tolti e l'eccezione dice perché, per la persona.
        /// </summary>
        Task<TemplateCreateOutcome> CreateAsync(TemplateCreateRequest request, CancellationToken ct = default);
    }

    public sealed class ProjectTemplateCreator : IProjectTemplateCreator
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

                // Il nome vero del remoto si sa dal manifesto: si scarica con un nome provvisorio e poi si rinomina.
                const string probe = RemotePrefix + "new";
                await Run(path, ct, "remote", "add", probe, request.TemplateUrl.Trim());
                var fetched = await _transport.FetchAsync(path, probe, ct);
                if (!fetched.Ok)
                    throw Problem($"Il template non si scarica da '{request.TemplateUrl}': {fetched.Error}", $"The template cannot be downloaded from '{request.TemplateUrl}': {fetched.Error}");

                var (id, found) = await FindManifestAsync(path, probe, ct);
                var branch = found.BranchFor(request.Language, request.Example);
                var remote = RemotePrefix + id;
                await Run(path, ct, "remote", "rename", probe, remote);

                var tip = $"{remote}/{branch}";
                if (!(await _git.RunAsync(path, new[] { "rev-parse", "--verify", "--quiet", tip + "^{commit}" }, ct)).Ok)
                    throw Problem($"Il manifesto del template indica il ramo '{branch}', che nel repository non c'è.",
                                  $"The template manifest names the branch '{branch}', which is not in the repository.");

                // Fa fede il manifesto del ramo che si prende.
                var manifest = TemplateManifestReader.ReadOrThrow(await ShowAsync(path, tip, TemplateManifest.PathOf(id), ct), id, $"{TemplateManifest.PathOf(id)} ({branch})");
                await CheckAsync(path, tip, manifest, request.MdeVersion, ct);

                await Run(path, ct, "checkout", "-q", "--no-track", "-b", ProjectBranch, tip);

                RecordTemplate(path, new ProjectTemplateConfig
                {
                    Id = manifest.Id,
                    Url = request.TemplateUrl.Trim(),
                    Kind = manifest.IsMethod ? null : TemplateManifest.Project,
                    Language = request.Language,
                    Example = request.Example,
                    Version = manifest.Version,
                });
                await Run(path, ct, "add", DevelopmentConfigCleanup.FileName);
                await Run(path, ct, "commit", "-q", "-m",
                    UiText.Say($"Progetto creato dal template «{manifest.Title}» {manifest.Version}", $"Project created from the template «{manifest.Title}» {manifest.Version}"));

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

                _logger.LogInformation("[Template] '{Path}' creato da '{Id}' {Version} ({Branch}); origin '{Origin}'.", path, manifest.Id, manifest.Version, branch, origin);
                return new TemplateCreateOutcome { Path = path, Id = manifest.Id, Title = manifest.Title, Version = manifest.Version, Origin = origin };
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

        private async Task CheckAsync(string path, string tip, TemplateManifest manifest, string mdeVersion, CancellationToken ct)
        {
            if (manifest.Requires?.Mde != null && TemplateManifestReader.CompareAppVersions(manifest.Requires.Mde, mdeVersion) > 0)
                throw Problem($"Il template «{manifest.Title}» {manifest.Version} chiede MdExplorer {manifest.Requires.Mde} o più nuovo; questo è {mdeVersion}. Aggiorna MdExplorer.",
                              $"The template «{manifest.Title}» {manifest.Version} needs MdExplorer {manifest.Requires.Mde} or newer; this is {mdeVersion}. Update MdExplorer.");
            if (manifest.Requires?.Templates?.Count > 0)
                throw Problem($"Il template «{manifest.Title}» ha bisogno anche di {string.Join(", ", manifest.Requires.Templates)}: la creazione da più template insieme non c'è ancora.",
                              $"The template «{manifest.Title}» also needs {string.Join(", ", manifest.Requires.Templates)}: creating from several templates together is not there yet.");

            var files = (await Run(path, ct, "ls-tree", "-r", "--name-only", tip))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToHashSet(StringComparer.Ordinal);
            if (manifest.IsMethod)
            {
                var outside = TemplateManifestReader.OutsideZone(manifest.Id, files);
                if (outside.Count > 0)
                    throw Problem($"Il metodo «{manifest.Title}» ha file fuori dalla sua cartella ({string.Join(", ", outside.Take(5))}): non lo unisco.",
                                  $"The method «{manifest.Title}» has files outside its folder ({string.Join(", ", outside.Take(5))}): I will not merge it.");
            }
            var missing = (manifest.Workflows ?? new List<string>()).Append(manifest.Instructions).Where(f => f != null && !files.Contains(f)).ToList();
            if (missing.Count > 0)
                throw Problem($"Il manifesto del template nomina file che nel ramo non ci sono: {string.Join(", ", missing)}.",
                              $"The template manifest names files that are not in the branch: {string.Join(", ", missing)}.");
        }

        /// <summary>Aggiunge la riga del template in <c>.development.yml</c>, letto con tolleranza zero com'è arrivato dal template.</summary>
        private static void RecordTemplate(string path, ProjectTemplateConfig row)
        {
            var file = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            var config = File.Exists(file) ? DevelopmentConfigCleanup.ReadStrict(file) : new DevelopmentConfig();
            config.Templates ??= new List<ProjectTemplateConfig>();
            if (config.Templates.Any(t => t.Id == row.Id))
                throw Problem($"Il template porta già una riga '{row.Id}' in {DevelopmentConfigCleanup.FileName}: un template non scrive le sue righe, le scrive MDE.",
                              $"The template already carries a '{row.Id}' row in {DevelopmentConfigCleanup.FileName}: a template does not write its rows, MDE does.");
            config.Templates.Add(row);
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
            File.WriteAllText(file, serializer.Serialize(config));
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
