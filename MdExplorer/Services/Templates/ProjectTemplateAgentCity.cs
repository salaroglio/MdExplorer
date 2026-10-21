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

namespace MdExplorer.Services.Templates
{
    /// <summary>
    /// La città degli agenti scelta alla creazione (sprint 2026-10-20-Citta-Degli-Agenti-Come-Funzione.md, C1): MDE porta
    /// nel progetto la sua parte — la città accesa, il documento delle responsabilità con il ruolo dell'esempio, il
    /// workflow «hello world» con il suo agente — prendendola da sé (CF6), nella lingua del progetto.
    /// </summary>
    public interface IProjectAgentCity
    {
        /// <summary>«Accendi la città» (C6): vedi <see cref="ProjectTemplateCreator.EnableAgentCityAsync"/>.</summary>
        Task<IReadOnlyList<string>> EnableAgentCityAsync(string projectPath, string language, bool withGuide, CancellationToken ct = default);
    }

    public sealed partial class ProjectTemplateCreator : IProjectAgentCity
    {
        /// <summary>Che cosa porta la città in una lingua: la cartella, il workflow d'esempio, il suo ruolo.</summary>
        public sealed record AgentCityStarter(string Language, string Folder, string WorkflowDoc, string GuideFolder,
            string Role, string RoleDescription, string Agent);

        private const string AgentCityResources = "MdExplorer.Service.agent-city/";

        public static AgentCityStarter AgentCityStarterFor(string language) => language == "it"
            ? new AgentCityStarter("it", "citta/", "citta/ciao.md", "citta/guida/",
                "Responsabile della città", "prova la città con il giro «Ciao mondo» e ne risponde", "citta-ciao")
            : new AgentCityStarter("en", "agent-city/", "agent-city/hello.md", "agent-city/guide/",
                "City owner", "tries the city with the «Hello world» round and answers for it", "city-hello");

        /// <summary>
        /// I file che la città porta in una lingua, come percorsi del progetto e risorse incorporate. Sul Windows di chi
        /// compila <c>%(RecursiveDir)</c> scrive «\»: il nome della risorsa si legge con «/» in tutti e due i casi.
        /// </summary>
        public static IReadOnlyList<(string Path, string Resource)> AgentCityFiles(AgentCityStarter starter, bool withGuide)
        {
            var prefix = AgentCityResources + starter.Language + "/";
            var files = typeof(ProjectTemplateCreator).Assembly.GetManifestResourceNames()
                .Select(r => (Resource: r, Name: r.Replace('\\', '/')))
                .Where(r => r.Name.StartsWith(prefix, StringComparison.Ordinal))
                .Select(r => (Path: r.Name[prefix.Length..], r.Resource))
                .Where(f => withGuide || !f.Path.StartsWith(starter.GuideFolder, StringComparison.Ordinal))
                .OrderBy(f => f.Path, StringComparer.Ordinal)
                .ToList();
            if (!files.Any(f => f.Path == starter.WorkflowDoc))
                throw Problem($"MdExplorer non contiene i file della città degli agenti in '{starter.Language}': la build è incompleta.",
                              $"MdExplorer does not contain the agent city's files in '{starter.Language}': the build is incomplete.");
            return files;
        }

        /// <summary>
        /// Porta la città nel progetto, senza committare: i file che mancano (uno che c'è già resta com'è), la città accesa,
        /// il documento delle responsabilità con il ruolo dell'esempio, il workflow d'esempio nell'elenco del progetto. I
        /// workflow del progetto devono poi leggersi tutti insieme. Mette nell'indice ciò che ha scritto; le note lo dicono.
        /// </summary>
        private async Task<IReadOnlyList<string>> ApplyAgentCityAsync(string path, string language, bool withGuide, CancellationToken ct)
        {
            var starter = AgentCityStarterFor(language);
            var notes = new List<string>();
            var touched = new List<string> { DevelopmentConfigCleanup.FileName };
            var kept = new List<string>();
            foreach (var (file, resource) in AgentCityFiles(starter, withGuide))
            {
                var full = System.IO.Path.Combine(path, file.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (File.Exists(full)) { kept.Add(file); continue; }
                using var stream = typeof(ProjectTemplateCreator).Assembly.GetManifestResourceStream(resource);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full));
                using (var target = File.Create(full)) await stream.CopyToAsync(target, ct);
                touched.Add(file);
            }

            var configFile = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            var config = File.Exists(configFile) ? DevelopmentConfigCleanup.ReadStrict(configFile) : new DevelopmentConfig();
            config.AgentCity ??= new AgentCityConfig();
            config.AgentCity.Enabled = true;
            var ownership = string.IsNullOrWhiteSpace(config.AgentCity.OwnershipDoc)
                ? OwnershipFileFor(starter.Language)
                : config.AgentCity.OwnershipDoc.Trim().Replace('\\', '/').TrimStart('/');
            config.AgentCity.OwnershipDoc = ownership;
            config.AgentCity.WorkflowDocs ??= new List<string>();
            if (!config.AgentCity.WorkflowDocs.Contains(starter.WorkflowDoc, StringComparer.OrdinalIgnoreCase))
                config.AgentCity.WorkflowDocs.Add(starter.WorkflowDoc);
            Write(configFile, config);

            var ownershipFull = System.IO.Path.Combine(path, ownership.Replace('/', System.IO.Path.DirectorySeparatorChar));
            var before = File.Exists(ownershipFull) ? await File.ReadAllTextAsync(ownershipFull, ct) : null;
            var text = MdExplorer.Features.Agents.OwnershipDocWriter.AddOpenRoles(before,
                new[] { (starter.Role, starter.RoleDescription, (IReadOnlyList<string>)new[] { starter.Agent }) }, out _, starter.Language);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ownershipFull));
            await File.WriteAllTextAsync(ownershipFull, text, ct);
            touched.Add(ownership);

            MdExplorer.Features.Agents.Workflow.WorkflowDocument.LoadAll(path, config.AgentCity.WorkflowDocs, ownership, out var problem);
            if (problem != null)
                throw Problem($"Con la città degli agenti i workflow del progetto non si leggono: {problem}",
                              $"With the agent city the project's workflows cannot be read: {problem}");
            await _git.RunAsync(path, new[] { "add", "--" }.Concat(touched.Distinct()).ToArray(), ct);

            // Le skill di MDE per gli agenti si installano solo con la città (C4): ora che è accesa, arrivano. Senza un
            // motore MDE non installa niente, come sempre.
            var harness = HarnessSettings.Read(path);
            if (harness != null && harness != HarnessTarget.None)
                MdeSkillUpdater.EnsureCatalogsInstalled(path, HarnessLayout.For(harness.Value), agentCityEnabled: true);

            notes.Add(UiText.Say(
                $"Città degli agenti accesa, con il workflow d'esempio {starter.WorkflowDoc}: prendi il ruolo «{starter.Role}» in {ownership}, dichiara tuo l'agente {starter.Agent} («È mio») e lancialo.",
                $"Agent city turned on, with the example workflow {starter.WorkflowDoc}: take the role «{starter.Role}» in {ownership}, declare the agent {starter.Agent} yours («It is mine») and launch it."));
            if (withGuide)
                notes.Add(UiText.Say($"La documentazione della città in {starter.GuideFolder}: come funziona, ruoli e responsabilità, il primo giro, come si scrive un workflow, la ISO 9001.",
                                     $"The agent city's documentation in {starter.GuideFolder}: how it works, roles and responsibilities, the first round, how to write a workflow, ISO 9001."));
            if (kept.Count > 0)
                notes.Add(UiText.Say($"Questi file della città c'erano già e restano come sono: {string.Join(", ", kept)}.",
                                     $"These files of the city were already there and stay as they are: {string.Join(", ", kept)}."));
            return notes;
        }

        /// <summary>
        /// «Accendi la città» dalle impostazioni del progetto (C6): ciò che avrebbe fatto la creazione. La parte
        /// <c>agent_city</c> di ogni template del progetto (formato 2, non staccato), poi i file della città — con la sua
        /// documentazione, se chiesta — e le skill degli agenti, in un commit. Con la città già accesa porta solo la
        /// documentazione, se manca. Il progetto deve essere un repository git senza modifiche non committate; in errore
        /// torna com'era. Non pubblica.
        /// </summary>
        public async Task<IReadOnlyList<string>> EnableAgentCityAsync(string projectPath, string language, bool withGuide, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                throw Problem($"Il progetto '{projectPath}' non esiste.", $"The project '{projectPath}' does not exist.");
            var path = Full(projectPath);
            await EnsureCommitReadyAsync(path, UiText.Say("accendere la città degli agenti", "turning the agent city on"), ct);

            var rows = PresentTemplates(path).Where(r => string.IsNullOrEmpty(r.Detached)).ToList();
            // La lingua è quella del progetto, cioè dei suoi template; un progetto senza template usa quella chiesta.
            var lang = rows.FirstOrDefault()?.Language ?? (string.IsNullOrWhiteSpace(language) ? "it" : language.Trim());
            var starter = AgentCityStarterFor(lang);
            var configFile = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            var enabled = File.Exists(configFile) && DevelopmentConfigCleanup.ReadStrict(configFile).AgentCity?.Enabled == true;
            if (enabled && (!withGuide || Directory.Exists(System.IO.Path.Combine(path, starter.GuideFolder.TrimEnd('/').Replace('/', System.IO.Path.DirectorySeparatorChar)))))
                throw Problem(withGuide ? "La città è già accesa, e la sua documentazione c'è già." : "La città degli agenti è già accesa.",
                              withGuide ? "The agent city is already on, and its documentation is already there." : "The agent city is already on.");

            var head = (await Run(path, ct, "rev-parse", "HEAD")).Trim();
            try
            {
                var notes = new List<string>();
                if (enabled)
                {
                    notes.Add(await ApplyAgentCityGuideAsync(path, starter, ct));
                    await Run(path, ct, "commit", "-q", "-m", UiText.Say("La documentazione della città degli agenti", "The agent city's documentation"));
                    return notes;
                }
                foreach (var row in rows)
                {
                    var local = System.IO.Path.Combine(path, TemplateManifest.PathOf(row.Id).Replace('/', System.IO.Path.DirectorySeparatorChar));
                    if (!File.Exists(local)) continue;
                    var manifest = TemplateManifestReader.ReadOrThrow(await File.ReadAllTextAsync(local, ct), row.Id, TemplateManifest.PathOf(row.Id));
                    if (!manifest.IsFormat2 || !manifest.HasAgentCityPart) continue;
                    notes.AddRange(await ApplyTemplateAgentCityPartAsync(path, manifest, row, ct));
                }
                notes.InsertRange(0, await ApplyAgentCityAsync(path, starter.Language, withGuide, ct));
                await Run(path, ct, "commit", "-q", "-m", UiText.Say("Città degli agenti accesa", "Agent city turned on"));
                _logger.LogInformation("[Template] '{Path}': città degli agenti accesa.", path);
                return notes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Template] '{Path}': la città non si accende; il progetto torna com'era.", path);
                await UndoAsync(path, head, ct);
                if (ex is InvalidOperationException) throw;
                throw Problem($"La città non si è accesa: {ex.Message}", $"The city was not turned on: {ex.Message}");
            }
        }

        /// <summary>La parte <c>agent_city</c> di un template già nel progetto: ruoli, workflow, skill degli agenti, nell'indice.</summary>
        private async Task<IReadOnlyList<string>> ApplyTemplateAgentCityPartAsync(string path, TemplateManifest manifest, ProjectTemplateConfig row, CancellationToken ct)
        {
            var notes = new List<string>();
            var touched = new List<string> { DevelopmentConfigCleanup.FileName };
            var configFile = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
            var config = DevelopmentConfigCleanup.ReadStrict(configFile);
            config.AgentCity ??= new AgentCityConfig();

            var roles = manifest.RolesFor(cityOn: true).Select(r => (r.Role, r.Description, (IReadOnlyList<string>)(r.Agents ?? new List<string>()))).ToList();
            if (roles.Count > 0)
            {
                var ownership = string.IsNullOrWhiteSpace(config.AgentCity.OwnershipDoc)
                    ? OwnershipFileFor(row.Language)
                    : config.AgentCity.OwnershipDoc.Trim().Replace('\\', '/').TrimStart('/');
                config.AgentCity.OwnershipDoc = ownership;
                var full = System.IO.Path.Combine(path, ownership.Replace('/', System.IO.Path.DirectorySeparatorChar));
                var before = File.Exists(full) ? await File.ReadAllTextAsync(full, ct) : null;
                var text = MdExplorer.Features.Agents.OwnershipDocWriter.AddOpenRoles(before, roles, out var added, row.Language);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full));
                await File.WriteAllTextAsync(full, text, ct);
                touched.Add(ownership);
                if (added.Count > 0)
                    notes.Add(UiText.Say($"Ruoli aperti di «{manifest.Title}» in {ownership}: {string.Join(", ", added)}.",
                                         $"Open roles of «{manifest.Title}» in {ownership}: {string.Join(", ", added)}."));
            }
            config.AgentCity.WorkflowDocs ??= new List<string>();
            var newDocs = manifest.WorkflowsFor(cityOn: true).Where(d => !config.AgentCity.WorkflowDocs.Contains(d, StringComparer.OrdinalIgnoreCase)).ToList();
            config.AgentCity.WorkflowDocs.AddRange(newDocs);
            if (newDocs.Count > 0)
                notes.Add(UiText.Say($"Workflow di «{manifest.Title}»: {string.Join(", ", newDocs)}.", $"Workflows of «{manifest.Title}»: {string.Join(", ", newDocs)}."));
            Write(configFile, config);
            ApplySkills(path, manifest, cityOn: true, notes, touched);
            await _git.RunAsync(path, new[] { "add", "--" }.Concat(touched.Distinct()).ToArray(), ct);
            return notes;
        }

        /// <summary>Solo la documentazione della città, se manca: i suoi file, nell'indice.</summary>
        private async Task<string> ApplyAgentCityGuideAsync(string path, AgentCityStarter starter, CancellationToken ct)
        {
            var touched = new List<string>();
            foreach (var (file, resource) in AgentCityFiles(starter, withGuide: true).Where(f => f.Path.StartsWith(starter.GuideFolder, StringComparison.Ordinal)))
            {
                var full = System.IO.Path.Combine(path, file.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (File.Exists(full)) continue;
                using var stream = typeof(ProjectTemplateCreator).Assembly.GetManifestResourceStream(resource);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full));
                using (var target = File.Create(full)) await stream.CopyToAsync(target, ct);
                touched.Add(file);
            }
            if (touched.Count > 0) await _git.RunAsync(path, new[] { "add", "--" }.Concat(touched).ToArray(), ct);
            return UiText.Say($"La documentazione della città in {starter.GuideFolder}: {touched.Count} documenti.",
                              $"The agent city's documentation in {starter.GuideFolder}: {touched.Count} documents.");
        }
    }
}
