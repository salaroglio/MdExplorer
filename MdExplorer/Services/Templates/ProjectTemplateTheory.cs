using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Templates;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Templates
{
    /// <summary>La teoria di un template che il progetto ha già, chiesta dopo la creazione (MC5).</summary>
    public sealed class TemplateTheoryRequest
    {
        public string ProjectPath { get; init; }
        /// <summary>L'id del template, come nella sua riga di <c>.development.yml</c>.</summary>
        public string Id { get; init; }
    }

    public interface IProjectTemplateTheory
    {
        /// <summary>
        /// «Scarica la teoria» (sprint 2026-10-19-Metodi-Componibili.md, M3): copia la teoria del template dalla sua zona nel
        /// progetto — cioè dalla versione che il progetto usa, senza rete — e lo ricorda nella riga, con un commit. Il progetto
        /// deve essere un repository git senza modifiche non committate; in errore torna com'era. Non pubblica.
        /// </summary>
        Task<TemplateAddOutcome> AddTheoryAsync(TemplateTheoryRequest request, CancellationToken ct = default);
    }

    public sealed partial class ProjectTemplateCreator : IProjectTemplateTheory
    {
        public async Task<TemplateAddOutcome> AddTheoryAsync(TemplateTheoryRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request?.ProjectPath) || !Directory.Exists(request.ProjectPath))
                throw Problem($"Il progetto '{request?.ProjectPath}' non esiste.", $"The project '{request?.ProjectPath}' does not exist.");
            if (string.IsNullOrWhiteSpace(request.Id)) throw Problem("Manca il template di cui scaricare la teoria.", "The template whose theory to download is missing.");
            var path = Full(request.ProjectPath);
            await EnsureCommitReadyAsync(path, UiText.Say("scaricare la teoria", "downloading the theory"), ct);

            var rows = PresentTemplates(path);
            var row = rows.FirstOrDefault(r => r.Id == request.Id.Trim())
                ?? throw Problem($"Il progetto non ha il template '{request.Id}': ha {(rows.Count == 0 ? "nessun template" : string.Join(", ", rows.Select(r => r.Id)))}.",
                                 $"The project has no template '{request.Id}': it has {(rows.Count == 0 ? "no template" : string.Join(", ", rows.Select(r => r.Id)))}.");
            var local = System.IO.Path.Combine(path, TemplateManifest.PathOf(row.Id).Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (!File.Exists(local))
                throw Problem($"Manca {TemplateManifest.PathOf(row.Id)} nel progetto: senza, non so dove sta la teoria.",
                              $"{TemplateManifest.PathOf(row.Id)} is missing from the project: without it, I do not know where the theory is.");
            var manifest = TemplateManifestReader.ReadOrThrow(await File.ReadAllTextAsync(local, ct), row.Id, TemplateManifest.PathOf(row.Id));
            if (row.Theory == true)
                throw Problem($"La teoria di «{manifest.Title}» è già nel progetto, in {manifest.TheoryFor(row.Language).To}/.",
                              $"The theory of «{manifest.Title}» is already in the project, in {manifest.TheoryFor(row.Language).To}/.");
            manifest.TheoryFor(row.Language);

            var head = (await Run(path, ct, "rev-parse", "HEAD")).Trim();
            var notes = new List<string>();
            var touched = new List<string> { DevelopmentConfigCleanup.FileName };
            try
            {
                row.Theory = true;
                var file = System.IO.Path.Combine(path, DevelopmentConfigCleanup.FileName);
                var config = DevelopmentConfigCleanup.ReadStrict(file);
                config.Templates[config.Templates.FindIndex(t => t.Id == row.Id)] = row;
                Write(file, config);
                try { await ApplyTheoryAsync(path, manifest, row, notes, touched, update: false, previous: null, ct); }
                finally
                {
                    var existing = touched.Distinct().Where(t => File.Exists(System.IO.Path.Combine(path, t.Replace('/', System.IO.Path.DirectorySeparatorChar)))).ToArray();
                    await _git.RunAsync(path, new[] { "add", "--" }.Concat(existing).ToArray(), ct);
                }
                await Run(path, ct, "commit", "-q", "-m",
                    UiText.Say($"Teoria di «{manifest.Title}» {manifest.Version} aggiunta dal template", $"Theory of «{manifest.Title}» {manifest.Version} added from the template"));
                _logger.LogInformation("[Template] '{Path}': aggiunta la teoria di '{Id}' {Version}.", path, row.Id, manifest.Version);
                return new TemplateAddOutcome { Id = row.Id, Title = manifest.Title, Version = manifest.Version, Notes = notes };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Template] '{Path}': la teoria di '{Id}' non si aggiunge; il progetto torna com'era.", path, row.Id);
                await UndoAsync(path, head, ct);
                if (ex is InvalidOperationException) throw;
                throw Problem($"La teoria non è stata aggiunta: {ex.Message}", $"The theory was not added: {ex.Message}");
            }
        }

        /// <summary>
        /// Il progetto può ricevere un commit di MDE: è la radice di un repository git, senza unioni a metà né modifiche non
        /// committate (il commit di MDE non deve portarsi dietro il lavoro della persona). <paramref name="doing"/> è che cosa
        /// si sta per fare, per i messaggi.
        /// </summary>
        private async Task EnsureCommitReadyAsync(string path, string doing, CancellationToken ct)
        {
            var top = await _git.RunAsync(path, new[] { "rev-parse", "--show-toplevel" }, ct);
            if (!top.Ok || !SamePath(top.Stdout.Trim(), path))
                throw Problem($"Il progetto non è un repository git (o ne è una sottocartella): serve per {doing}.",
                              $"The project is not a git repository (or is a subfolder of one): it is needed for {doing}.");
            if (File.Exists(System.IO.Path.Combine(path, ".git", "MERGE_HEAD")) || (await _git.RunAsync(path, new[] { "rev-parse", "-q", "--verify", "MERGE_HEAD" }, ct)).Ok)
                throw Problem($"Nel progetto c'è un'unione rimasta a metà: chiudila o annullala prima di {doing}.",
                              $"The project has a merge left half done: finish or abort it before {doing}.");
            var dirty = (await Run(path, ct, "status", "--porcelain", "--untracked-files=no")).Trim();
            if (dirty.Length > 0)
                throw Problem($"Il progetto ha modifiche non committate ({string.Join(", ", dirty.Split('\n').Take(5).Select(l => l.Trim()))}): committale o annullale, poi riprova. MDE fa un commit suo, che non deve portarsi dietro il tuo lavoro.",
                              $"The project has uncommitted changes ({string.Join(", ", dirty.Split('\n').Take(5).Select(l => l.Trim()))}): commit or discard them, then try again. MDE makes its own commit, which must not carry your work.");
        }
    }
}
