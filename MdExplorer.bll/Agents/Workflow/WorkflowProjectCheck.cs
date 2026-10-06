using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// La seconda metà della verifica: il workflow confrontato con il progetto. Ciò che il workflow nomina deve esistere
    /// (agenti, risposte dichiarate nelle schede, cartelle degli artefatti), e i valori che un pulsante porta devono essere
    /// variabili del giro, altrimenti non arriverebbero ai passi successivi. Le schede non instradano più (W14): chi incarica
    /// chi lo dice il workflow, quindi non si confrontano i loro campi di instradamento.
    /// </summary>
    public static class WorkflowProjectCheck
    {
        private static readonly Regex Placeholder = new(@"\{([A-Za-z0-9_\-]+)\}", RegexOptions.Compiled);

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
                if (step.Agent != null && !byName.ContainsKey(step.Agent))
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
                if (t?.Kind != WorkflowTriggerKind.Reply) continue;
                var source = t.FromStep == null ? null : wf.Step(t.FromStep);
                if (source?.Agent == null) continue;

                // Il pulsante sta sotto il messaggio dell'agente di quel passo: è la sua scheda a dichiararlo. Il passo che
                // fa partire può essere di un altro agente (lo schedulatore lo avvia, W14).
                if (!byName.TryGetValue(source.Agent, out var writer) || t.ReplyId == null) continue;

                var declared = writer.Replies ?? new List<AgentRegistryReply>();
                var reply = declared.FirstOrDefault(rp => string.Equals(rp.Id, t.ReplyId, StringComparison.OrdinalIgnoreCase));
                if (reply == null)
                {
                    issues.Add(Error($"{path}.trigger.reply", $"la scheda di '{source.Agent}' non dichiara la risposta '{t.ReplyId}'.",
                        declared.Count == 0 ? "aggiungila in a2a.replies della scheda" : "risposte dichiarate: " + string.Join(", ", declared.Select(d => d.Id))));
                    continue;
                }
                // I valori del pulsante diventano variabili del giro: ognuno deve essere dichiarato, o si perderebbe.
                var carried = new[] { reply.Label, reply.Description, reply.Message }
                    .Where(x => !string.IsNullOrEmpty(x))
                    .SelectMany(x => Placeholder.Matches(x).Cast<Match>().Select(m => m.Groups[1].Value))
                    .Distinct(StringComparer.Ordinal);
                foreach (var name in carried.Where(n => !wf.Variables.ContainsKey(n)))
                    issues.Add(Error($"{path}.trigger.reply", $"il pulsante '{t.ReplyId}' porta il valore '{{{name}}}', che non è una variabile del workflow: ai passi successivi non arriverebbe.",
                        $"dichiaralo in \"variables\": {{ \"{name}\": \"che cos'è\" }}"));
            }
            return issues;
        }

        private static WorkflowIssue Error(string path, string message, string fix = null) =>
            new() { Severity = WorkflowSeverity.Error, Path = path, Message = message, Fix = fix };
    }
}
