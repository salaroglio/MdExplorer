using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>
    /// I linguaggi che MarkAgent scrive per il progetto (D42 passo 2, 24/10/2026): di solito il dialetto delle pagine a
    /// direttive. La prova a secco (<see cref="TryAsync"/>) legge i file del progetto con il linguaggio proposto senza
    /// scrivere niente; il «Sì» del wizard (<see cref="ApplyAsync"/>) scrive il pacchetto <c>.mde-languages/&lt;id&gt;/</c>,
    /// aggiunge il linguaggio alle cartelle <c>program</c> che hanno i suoi file e le rianalizza in sfondo;
    /// <see cref="UndoAsync"/> torna a com'era, ma solo per un linguaggio scritto da qui in questa sessione del servizio:
    /// un pacchetto importato o scritto a mano non si cancella da un pulsante.
    /// </summary>
    public sealed class CodeLanguageService
    {
        private readonly ICodeAnalysisService _analysis;
        private readonly ILogger<CodeLanguageService> _logger;
        /// <summary>
        /// Che cosa ha fatto ogni «Sì», per poterlo annullare: (progetto, id) → testo di prima (null = pacchetto nuovo), le
        /// cartelle a cui il linguaggio è stato aggiunto ADESSO (quelle che lo avevano già lo tengono) e tutte quelle toccate.
        /// </summary>
        private readonly ConcurrentDictionary<string, Applied> _applied = new(StringComparer.OrdinalIgnoreCase);

        private sealed record Applied(string PreviousDefinition, bool CreatedFolder, List<string> AddedTo, List<string> Folders);

        public CodeLanguageService(ICodeAnalysisService analysis, ILogger<CodeLanguageService> logger)
        {
            _analysis = analysis;
            _logger = logger;
        }

        private static string Key(string projectPath, string id) => Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "|" + id;

        public async Task<CodeLanguagePreview> TryAsync(string projectPath, string definition, CancellationToken ct = default)
        {
            var package = CodeLanguageTrial.PackageOf(projectPath, definition);
            var catalog = CodeLanguageCatalog.ForProject(projectPath).With(package);
            var existing = Path.Combine(package.FolderPath, LanguagePackage.DefinitionFile);
            int? replaces = null;
            if (File.Exists(existing))
            {
                try { replaces = LanguagePackage.Load(package.FolderPath).Language.Version; }
                catch (LanguageDefinitionException) { replaces = 0; }   // quello di oggi non si legge: si sostituisce comunque
            }
            var files = await _analysis.FilesOfLanguageAsync(projectPath, catalog, package.Id, ct);
            return CodeLanguageTrial.Evaluate(projectPath, package, files, replaces, ct);
        }

        /// <summary>
        /// Il «Sì»: il pacchetto nel progetto (il <c>language.yml</c>, e un README con il problema se non c'è), il linguaggio
        /// nelle cartelle program con i suoi file, poi la loro analisi in sfondo (il pannello dei lavori la mostra).
        /// </summary>
        public async Task<CodeLanguagePreview> ApplyAsync(CodeLanguageProposal proposal, CancellationToken ct = default)
        {
            var projectPath = proposal.ProjectPath;
            var preview = await TryAsync(projectPath, proposal.Definition, ct);
            if (!preview.Accepted) throw new CodeRuleException(UiText.Say("Il linguaggio non è più accettato: ", "The language is no longer accepted: ") + string.Join(" ", preview.RejectReasons));

            var folder = Path.Combine(DeclarativeLanguage.FolderOf(projectPath), preview.LanguageId);
            var file = Path.Combine(folder, LanguagePackage.DefinitionFile);
            var created = !Directory.Exists(folder);
            var previous = File.Exists(file) ? File.ReadAllText(file, Encoding.UTF8) : null;
            Directory.CreateDirectory(folder);
            File.WriteAllText(file, proposal.Definition, new UTF8Encoding(false));
            var readme = Path.Combine(folder, LanguagePackage.ReadmeFile);
            if (!File.Exists(readme))
                File.WriteAllText(readme, $"# {preview.DisplayName}\n\n{proposal.Problem}\n\n" +
                    UiText.Say($"Scritto da {proposal.ProposedBy}, approvato dall'utente il {DateTime.Now:yyyy-MM-dd}.\n", $"Written by {proposal.ProposedBy}, approved by the user on {DateTime.Now:yyyy-MM-dd}.\n"), new UTF8Encoding(false));

            var programs = await _analysis.ListProgramsAsync(projectPath, ct);
            var folders = preview.Folders.Select(f => f.Folder).ToList();
            var added = new List<string>();
            foreach (var f in folders)
            {
                var languages = programs.FirstOrDefault(p => string.Equals(p.Folder, f, StringComparison.OrdinalIgnoreCase))?.Languages ?? new List<string>();
                if (languages.Contains(preview.LanguageId, StringComparer.OrdinalIgnoreCase)) continue;
                _analysis.SaveLanguages(projectPath, f, languages.Append(preview.LanguageId).ToList());
                added.Add(f);
            }
            _applied[Key(projectPath, preview.LanguageId)] = new Applied(previous, created, added, folders);
            _logger.LogInformation("[CodeGraph] linguaggio '{Id}' v{Version} scritto da {By} (approvato dall'utente) per {Folders}", preview.LanguageId, preview.Version, proposal.ProposedBy, string.Join(", ", folders));
            Reanalyze(projectPath, folders);
            return preview;
        }

        /// <summary>
        /// «Annulla»: il linguaggio esce dalle cartelle a cui il «Sì» l'aveva aggiunto, il pacchetto torna com'era (cancellato se
        /// era nuovo), le cartelle si rianalizzano (gli script vanno via, o si riscrivono con il dialetto di prima).
        /// </summary>
        public async Task<bool> UndoAsync(string projectPath, string languageId, CancellationToken ct = default)
        {
            if (!_applied.TryRemove(Key(projectPath, languageId), out var applied))
                throw new CodeRuleException(UiText.Say($"Il linguaggio '{languageId}' non l'ha scritto il wizard in questa sessione: per toglierlo usa «Ambiente di sviluppo» e la cartella {DeclarativeLanguage.FolderName}/{languageId}/.",
                    $"The language '{languageId}' was not written by the wizard in this session: to remove it use «Development environment» and the folder {DeclarativeLanguage.FolderName}/{languageId}/."));
            foreach (var p in (await _analysis.ListProgramsAsync(projectPath, ct))
                         .Where(p => applied.AddedTo.Contains(p.Folder, StringComparer.OrdinalIgnoreCase) && p.Languages.Contains(languageId, StringComparer.OrdinalIgnoreCase)))
                _analysis.SaveLanguages(projectPath, p.Folder, p.Languages.Where(l => !string.Equals(l, languageId, StringComparison.OrdinalIgnoreCase)).ToList());
            var folder = Path.Combine(DeclarativeLanguage.FolderOf(projectPath), languageId);
            if (applied.PreviousDefinition != null) File.WriteAllText(Path.Combine(folder, LanguagePackage.DefinitionFile), applied.PreviousDefinition, new UTF8Encoding(false));
            else if (applied.CreatedFolder && Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
            _logger.LogInformation("[CodeGraph] linguaggio '{Id}' annullato dall'utente", languageId);
            Reanalyze(projectPath, applied.Folders);
            return true;
        }

        /// <summary>In sfondo e una cartella alla volta: con includeExtractorChanges anche i file che non hanno ancora uno script.</summary>
        private void Reanalyze(string projectPath, List<string> folders)
            => _ = Task.Run(async () =>
            {
                foreach (var f in folders)
                {
                    try
                    {
                        var outcome = await _analysis.AnalyzeFolderAsync(projectPath, f, includeExtractorChanges: true);
                        if (outcome.Error != null) _logger.LogWarning("[CodeGraph] '{Folder}' dopo il linguaggio: {Error}", f, outcome.Error);
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "[CodeGraph] '{Folder}': analisi dopo il linguaggio non riuscita", f); }
                }
            });
    }
}
