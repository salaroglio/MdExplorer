using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// La seconda metà della verifica: il workflow confrontato con il progetto. Ciò che il workflow nomina deve esistere
    /// (agenti, risposte dichiarate, cartelle degli artefatti): altrimenti è un errore. Dove il workflow e le schede dicono
    /// cose diverse sugli stessi passaggi (<c>accepts_messages_from</c>, <c>on_approval_notify</c>) è un avviso: le schede
    /// restano la loro fonte, e chi legge decide quale correggere.
    /// </summary>
    public static class WorkflowProjectCheck
    {
        /// <param name="agents">Gli agenti del progetto, dal registro.</param>
        /// <param name="folderExists">Se una cartella esiste, dato il percorso dalla radice del progetto con '/'.</param>
        public static IList<WorkflowIssue> Check(
            WorkflowDescriptor wf,
            IEnumerable<AgentRegistryEntry> agents,
            Func<string, bool> folderExists)
        {
            var issues = new List<WorkflowIssue>();
            if (wf == null) return issues;
            var byName = (agents ?? Enumerable.Empty<AgentRegistryEntry>())
                .Where(a => !string.IsNullOrWhiteSpace(a?.Name))
                .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < wf.Steps.Count; i++)
            {
                var step = wf.Steps[i];
                var path = $"steps[{i}]";
                AgentRegistryEntry agent = null;
                if (step.Agent != null && !byName.TryGetValue(step.Agent, out agent))
                    issues.Add(Error($"{path}.agent", $"nel progetto non c'è un agente '{step.Agent}'.",
                        byName.Count == 0 ? "il progetto non ha agenti" : "agenti del progetto: " + string.Join(", ", byName.Keys.OrderBy(k => k))));

                for (var j = 0; j < step.Produces.Count; j++)
                {
                    var file = step.Produces[j];
                    var slash = file.LastIndexOf('/');
                    var folder = slash < 0 ? "" : file.Substring(0, slash);
                    if (folder.Length > 0 && !folderExists(folder))
                        issues.Add(Error($"{path}.produces[{j}]", $"la cartella '{folder}' non esiste.",
                            "creala nel progetto, con un README.md: non tutti i motori creano le cartelle da soli"));
                }

                var t = step.Trigger;
                if (t == null) continue;
                var source = t.FromStep == null ? null : wf.Step(t.FromStep);

                if (t.Kind == WorkflowTriggerKind.Reply && source?.Agent != null)
                {
                    // Il pulsante sta sotto il messaggio dell'agente di quel passo, e la risposta arriva a lui.
                    if (step.Agent != null && !string.Equals(step.Agent, source.Agent, StringComparison.OrdinalIgnoreCase))
                        issues.Add(Error($"{path}.agent", $"la risposta '{t.ReplyId}' arriva a '{source.Agent}', che ha scritto il messaggio di '{source.Id}': questo passo è suo, non di '{step.Agent}'."));
                    if (byName.TryGetValue(source.Agent, out var writer) && t.ReplyId != null
                        && !(writer.Replies ?? new List<AgentRegistryReply>()).Any(rp => string.Equals(rp.Id, t.ReplyId, StringComparison.OrdinalIgnoreCase)))
                    {
                        var declared = (writer.Replies ?? new List<AgentRegistryReply>()).Select(rp => rp.Id).ToList();
                        issues.Add(Error($"{path}.trigger.reply", $"la scheda di '{source.Agent}' non dichiara la risposta '{t.ReplyId}'.",
                            declared.Count == 0 ? "aggiungila in a2a.replies della scheda" : "risposte dichiarate: " + string.Join(", ", declared)));
                    }
                }

                if (t.Kind == WorkflowTriggerKind.Assignment && source?.Agent != null && agent != null
                    && !Accepts(agent, source.Agent))
                    issues.Add(Warning($"{path}.trigger.assignment",
                        $"la scheda di '{step.Agent}' non accetta messaggi da '{source.Agent}' (accepts_messages_from): l'incarico verrebbe rifiutato.",
                        $"aggiungi {source.Agent} ad accepts_messages_from nella scheda di {step.Agent}"));

                if (t.Kind == WorkflowTriggerKind.Approval && step.Agent != null)
                    foreach (var of in t.ApprovalOf.Select(wf.Step).Where(s => s?.Agent != null))
                        if (byName.TryGetValue(of.Agent, out var producer)
                            && !(producer.OnApprovalNotify ?? new List<string>()).Any(n => string.Equals(n, step.Agent, StringComparison.OrdinalIgnoreCase)))
                            issues.Add(Warning($"{path}.trigger.approval",
                                $"approvato il lavoro di '{of.Agent}' (passo '{of.Id}'), la sua scheda non avvisa '{step.Agent}' (on_approval_notify).",
                                $"aggiungi {step.Agent} a on_approval_notify nella scheda di {of.Agent}"));
            }
            return issues;
        }

        private static bool Accepts(AgentRegistryEntry receiver, string sender) =>
            (receiver.AcceptsMessagesFrom ?? new List<string>())
                .Any(a => a == "*" || string.Equals(a, sender, StringComparison.OrdinalIgnoreCase));

        private static WorkflowIssue Error(string path, string message, string fix = null) =>
            new() { Severity = WorkflowSeverity.Error, Path = path, Message = message, Fix = fix };

        private static WorkflowIssue Warning(string path, string message, string fix = null) =>
            new() { Severity = WorkflowSeverity.Warning, Path = path, Message = message, Fix = fix };
    }
}
