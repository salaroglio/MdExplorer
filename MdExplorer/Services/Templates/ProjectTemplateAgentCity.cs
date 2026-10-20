using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;

namespace MdExplorer.Services.Templates
{
    /// <summary>
    /// La città degli agenti scelta alla creazione (sprint 2026-10-20-Citta-Degli-Agenti-Come-Funzione.md, C1): MDE porta
    /// nel progetto la sua parte — la città accesa, il documento delle responsabilità con il ruolo dell'esempio, il
    /// workflow «hello world» con il suo agente — prendendola da sé (CF6), nella lingua del progetto.
    /// </summary>
    public sealed partial class ProjectTemplateCreator
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
    }
}
