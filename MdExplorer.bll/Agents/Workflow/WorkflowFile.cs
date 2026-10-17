using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Un <c>*.workflow.json</c> del progetto, letto e verificato per intero: struttura, unione con le schede degli agenti,
    /// progetto. È il punto da cui passano tutti (l'endpoint di verifica, il diagramma, lo schedulatore, i giri), così il
    /// file si legge in un modo solo.
    /// </summary>
    public static class WorkflowFile
    {
        public const string Extension = ".workflow.json";

        /// <param name="projectRoot">La cartella del progetto.</param>
        /// <param name="path">Il file: dalla radice del progetto, o assoluto ma dentro il progetto.</param>
        /// <param name="ownershipDoc">Il documento delle responsabilità (<c>agentCity.ownershipDoc</c>), dalla radice; null se non c'è.</param>
        public static WorkflowCheckResult Check(string projectRoot, string path, string ownershipDoc) =>
            Read(projectRoot, File.ReadAllText(Resolve(projectRoot, path)), ownershipDoc);

        /// <summary>Il testo di un workflow, letto, unito alle schede del progetto e verificato contro il progetto.</summary>
        public static WorkflowCheckResult Read(string projectRoot, string json, string ownershipDoc)
        {
            var result = ReadBound(projectRoot, json);
            if (result.Descriptor?.Cards == null) return result;

            var wf = result.Descriptor;
            var root = Path.GetFullPath(projectRoot);
            var roles = Roles(root, ownershipDoc, out var problem);
            if (problem != null)
                result.Issues.Add(new WorkflowIssue { Severity = WorkflowSeverity.Error, Path = "", Message = problem });
            result.Issues.AddRange(WorkflowProjectCheck.Check(wf,
                folder => Directory.Exists(Path.Combine(root, folder.Replace('/', Path.DirectorySeparatorChar))),
                file =>
                {
                    var full = Path.Combine(root, file.Replace('/', Path.DirectorySeparatorChar));
                    return File.Exists(full) ? File.ReadAllText(full) : null;
                },
                problem != null ? Array.Empty<string>() : roles));
            return result;
        }

        /// <summary>
        /// Il testo di un workflow, letto e unito alle schede del progetto, senza confrontarlo con il resto del progetto: ciò
        /// che serve per disegnarlo. Le schede non si cercano se la struttura ha errori.
        /// </summary>
        public static WorkflowCheckResult ReadBound(string projectRoot, string json)
        {
            var result = WorkflowParser.Parse(json);
            // Con errori di struttura l'unione segnalerebbe conseguenze, non cause: prima si corregge il file.
            if (!result.IsValid) return result;
            var wf = result.Descriptor;
            var cards = WorkflowCardSet.Load(projectRoot, wf.Steps.Where(s => !s.IsPerson).Select(s => s.Agent));
            result.Issues.AddRange(WorkflowBinder.Bind(wf, cards));
            return result;
        }

        /// <summary>
        /// Il workflow di un giro, con le schede salvate quando è nato: si unisce a quelle, non alle schede di adesso. Il
        /// progetto non si riverifica: il giro va avanti con ciò con cui è partito.
        /// </summary>
        public static WorkflowCheckResult ReadSnapshot(string json, string cardsJson)
        {
            var result = WorkflowParser.Parse(json);
            if (!result.IsValid) return result;
            if (cardsJson == null)
            {
                result.Issues.Add(new WorkflowIssue
                {
                    Severity = WorkflowSeverity.Error, Path = "",
                    Message = UiText.Say("il giro non ha salvato le schede con cui è nato.", "the round did not save the cards it was born with."),
                    Fix = UiText.Say("chiudi il giro e aprine uno nuovo", "close the round and open a new one"),
                });
                return result;
            }
            var cards = WorkflowCardSet.FromJson(cardsJson);
            result.Issues.AddRange(WorkflowBinder.Bind(result.Descriptor, cards));
            return result;
        }

        /// <summary>
        /// I ruoli del documento delle responsabilità; null senza documento. Un documento configurato che non si legge è un
        /// problema: senza, non si sa chi fa i passi delle persone.
        /// </summary>
        public static IReadOnlyCollection<string> Roles(string projectRoot, string ownershipDoc, out string problem)
        {
            problem = null;
            if (string.IsNullOrWhiteSpace(ownershipDoc)) return null;
            var relative = ownershipDoc.Trim().Replace('\\', '/').TrimStart('/');
            var full = Path.Combine(projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full))
            {
                problem = UiText.Say($"il documento delle responsabilità '{relative}' non esiste (agentCity.ownershipDoc).", $"the responsibilities document '{relative}' does not exist (agentCity.ownershipDoc).");
                return null;
            }
            var parsed = OwnershipDocParser.Parse(File.ReadAllText(full));
            if (!parsed.IsOwnershipDoc)
            {
                problem = UiText.Say($"'{relative}' non ha «mde_type: ownership» nel front matter.", $"'{relative}' has no «mde_type: ownership» in the front matter.");
                return null;
            }
            if (parsed.HasErrors)
            {
                problem = UiText.Say($"'{relative}' ha errori: ", $"'{relative}' has errors: ") + string.Join(" | ", parsed.Errors);
                return null;
            }
            return parsed.Entries.Select(e => e.Role).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>Il percorso completo del file, solo se è un workflow dentro il progetto e c'è: altrimenti un errore che dice perché.</summary>
        public static string Resolve(string projectRoot, string path)
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                throw new ArgumentException(UiText.Say($"La cartella del progetto '{projectRoot}' non esiste.", $"The project folder '{projectRoot}' does not exist."));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(UiText.Say("Manca il percorso del workflow.", "The workflow path is missing."));

            var root = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var trimmed = path.Trim().Replace('\\', '/').TrimStart('/');
            var full = Path.GetFullPath(Path.IsPathRooted(path.Trim()) ? path.Trim() : Path.Combine(root, trimmed.Replace('/', Path.DirectorySeparatorChar)));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!full.StartsWith(root, comparison))
                throw new ArgumentException(UiText.Say($"'{path}' è fuori dal progetto.", $"'{path}' is outside the project."));
            if (!full.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(UiText.Say($"'{path}' non è un workflow: il nome finisce con «{Extension}».", $"'{path}' is not a workflow: the name ends with «{Extension}»."));
            if (!File.Exists(full))
                throw new FileNotFoundException(UiText.Say($"Il workflow '{path}' non esiste nel progetto.", $"The workflow '{path}' does not exist in the project."), full);
            return full;
        }
    }
}
