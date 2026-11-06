using System;
using MdExplorer.Features.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// L'ultima parte della verifica: il workflow, già unito alle schede (<see cref="WorkflowBinder"/>), confrontato con il
    /// progetto. Le cartelle delle uscite esistono, i documenti letti esistono con il capitolo che si nomina, i ruoli delle
    /// persone e di chi approva sono righe di <c>responsabilita.md</c>.
    /// </summary>
    public static class WorkflowProjectCheck
    {
        private static readonly Regex Heading = new(@"^\s{0,3}#{1,6}\s+(.+?)\s*#*\s*$", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <param name="folderExists">Se una cartella esiste, dato il percorso dalla radice del progetto con '/'.</param>
        /// <param name="readFile">Il testo di un file dalla radice del progetto con '/'; null se non c'è.</param>
        /// <param name="roles">I ruoli di <c>responsabilita.md</c>; null se il progetto non ha il documento.</param>
        public static IList<WorkflowIssue> Check(
            WorkflowDescriptor wf,
            Func<string, bool> folderExists,
            Func<string, string> readFile,
            IReadOnlyCollection<string> roles)
        {
            var issues = new List<WorkflowIssue>();
            if (wf == null) return issues;

            for (var i = 0; i < wf.Steps.Count; i++)
            {
                var step = wf.Steps[i];
                var path = $"steps[{i}]";

                // Le uscite di un agente vengono dalla scheda: il percorso del problema è la scheda, non il workflow.
                for (var j = 0; j < step.Outputs.Count; j++)
                {
                    var file = step.Outputs[j].Path;
                    var slash = file?.LastIndexOf('/') ?? -1;
                    var folder = slash < 0 ? "" : file.Substring(0, slash);
                    if (folder.Length > 0 && !folderExists(folder))
                        issues.Add(Error(step.IsPerson ? $"{path}.produces[{j}]" : $"{path}.skill", UiText.Say($"la cartella '{folder}' dell'uscita '{step.Outputs[j].Id}' non esiste.", $"the folder '{folder}' of output '{step.Outputs[j].Id}' does not exist."),
                            UiText.Say("creala nel progetto, con un README.md: non tutti i motori creano le cartelle da soli",
                                "create it in the project, with a README.md: not every engine creates folders by itself")));
                }

                foreach (var read in step.Reads)
                {
                    var text = readFile(read.Path);
                    if (text == null)
                    {
                        issues.Add(Error($"{path}.skill", UiText.Say($"il lavoro '{step.Skill}' di '{step.Agent}' legge '{read.Path}', che non esiste.", $"job '{step.Skill}' of '{step.Agent}' reads '{read.Path}', which does not exist."),
                            UiText.Say("correggi il percorso in reads della scheda, dalla radice del progetto", "fix the path in the card's reads, from the project root")));
                        continue;
                    }
                    if (read.Section == null) continue;
                    var titles = Heading.Matches(text).Select(m => m.Groups[1].Value.Trim()).ToList();
                    if (!titles.Any(t => string.Equals(t, read.Section.Trim(), StringComparison.OrdinalIgnoreCase)))
                        issues.Add(Error($"{path}.skill", UiText.Say($"'{read.Path}' non ha il capitolo '{read.Section}' che il lavoro '{step.Skill}' di '{step.Agent}' legge.", $"'{read.Path}' has no chapter '{read.Section}' that job '{step.Skill}' of '{step.Agent}' reads."),
                            titles.Count == 0
                                ? UiText.Say("il documento non ha titoli", "the document has no headings")
                                : UiText.Say("capitoli del documento: ", "chapters of the document: ") + string.Join(", ", titles)));
                }

                if (step.Person != null) CheckRole(step.Person, $"{path}.person", roles, issues);
                if (step.ApprovalBy != null) CheckRole(step.ApprovalBy, $"{path}.approval.by", roles, issues);
            }
            return issues;
        }

        private static void CheckRole(string role, string path, IReadOnlyCollection<string> roles, List<WorkflowIssue> issues)
        {
            if (roles == null)
                issues.Add(Error(path, UiText.Say($"il ruolo '{role}' dovrebbe stare in responsabilita.md, che il progetto non ha.", $"the role '{role}' should be in responsabilita.md, which the project does not have."),
                    UiText.Say("crea il documento delle responsabilità (mde_type: ownership) con la colonna Ruolo", "create the responsibilities document (mde_type: ownership) with the Ruolo column")));
            else if (!roles.Contains(role.Trim(), StringComparer.OrdinalIgnoreCase))
                issues.Add(Error(path, UiText.Say($"'{role}' non è un ruolo di responsabilita.md.", $"'{role}' is not a role in responsabilita.md."),
                    roles.Count == 0
                        ? UiText.Say("responsabilita.md non ha ruoli", "responsabilita.md has no roles")
                        : UiText.Say("ruoli: ", "roles: ") + string.Join(", ", roles.Distinct(StringComparer.OrdinalIgnoreCase))));
        }

        private static WorkflowIssue Error(string path, string message, string fix = null) =>
            new() { Severity = WorkflowSeverity.Error, Path = path, Message = message, Fix = fix };
    }
}
