using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Quale passo del workflow sta facendo partire un messaggio, e quindi chi lo avvia. È la regola che MdExplorer
    /// applica al risveglio: un incarico che il workflow dice <c>ask-owner</c> non sveglia l'agente, aspetta il suo
    /// responsabile.
    /// </summary>
    public static class WorkflowStartPolicy
    {
        /// <param name="fromAgent">Il mittente: un agente, o «user».</param>
        /// <param name="toAgent">L'agente che il messaggio sveglierebbe.</param>
        /// <param name="isApproval">Il messaggio è l'avviso di un'approvazione (<c>[APPROVATO]</c>).</param>
        /// <returns>
        /// Il passo, o null se il workflow non ne descrive uno per questo messaggio. Una persona che lancia o risponde ha
        /// già deciso: per lei non c'è passo da cercare. Con più passi possibili vince il più prudente: se uno chiede il
        /// responsabile, si chiede.
        /// </returns>
        public static WorkflowStep StepFor(WorkflowDescriptor wf, string fromAgent, string toAgent, bool isApproval)
        {
            if (wf == null || string.IsNullOrWhiteSpace(toAgent)) return null;
            var fromAnAgent = !string.IsNullOrWhiteSpace(fromAgent) && !Same(fromAgent, ConversationHopGuard.UserRecipient);
            if (!isApproval && !fromAnAgent) return null;

            // Un passo parte dopo altri passi (after): dopo un passo di chi scrive, o (approvazione) dopo un passo che
            // produce un artefatto, perché solo quello si approva.
            var list = wf.Steps.Where(s => Same(s.Agent, toAgent)
                                           && s.Trigger?.Kind == WorkflowTriggerKind.After
                                           && (isApproval
                                               ? s.Trigger.After.Any(a => (wf.Step(a)?.Produces.Count ?? 0) > 0)
                                               : s.Trigger.After.Any(a => Same(wf.Step(a)?.Agent, fromAgent))))
                               .ToList();
            return list.FirstOrDefault(s => s.Start == WorkflowStart.AskOwner) ?? list.FirstOrDefault();
        }

        /// <summary>
        /// Come sopra, su tutti i workflow del progetto (PW): il passo e il workflow in cui sta. Con passi possibili in più
        /// workflow vince ancora il più prudente.
        /// </summary>
        public static (ProjectWorkflow Workflow, WorkflowStep Step) StepFor(IReadOnlyList<ProjectWorkflow> workflows, string fromAgent, string toAgent, bool isApproval)
        {
            var found = (workflows ?? Array.Empty<ProjectWorkflow>())
                .Select(w => (Workflow: w, Step: StepFor(w.Descriptor, fromAgent, toAgent, isApproval)))
                .Where(x => x.Step != null)
                .ToList();
            return found.FirstOrDefault(x => x.Step.Start == WorkflowStart.AskOwner) is { Step: not null } prudent ? prudent : found.FirstOrDefault();
        }

        /// <summary>I titoli dei workflow, per dirli in un messaggio: «Gara», «DDD».</summary>
        public static string Titles(IReadOnlyList<ProjectWorkflow> workflows)
            => string.Join(", ", (workflows ?? Array.Empty<ProjectWorkflow>()).Select(w => $"«{w.Descriptor.Title}»"));

        /// <summary>Il motivo per cui un messaggio tra agenti non è previsto da nessuno dei workflow del progetto.</summary>
        public static string NotForeseen(IReadOnlyList<ProjectWorkflow> workflows, string fromAgent, string toAgent)
        {
            if (workflows != null && workflows.Count == 1) return NotForeseen(workflows[0].Descriptor, fromAgent, toAgent);
            var allowed = (workflows ?? Array.Empty<ProjectWorkflow>()).SelectMany(w => AssigneesOf(w.Descriptor, fromAgent))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return UiText.Say(
                $"Nessuno dei workflow del progetto ({Titles(workflows)}) prevede che '{fromAgent}' scriva a '{toAgent}'. " +
                (allowed.Count == 0
                    ? $"Dopo '{fromAgent}' non viene nessun altro agente: scrive solo alla persona."
                    : $"Dopo '{fromAgent}' vengono: {string.Join(", ", allowed)}.") +
                " Se il passaggio serve, va aggiunto a un workflow.",
                $"None of the project's workflows ({Titles(workflows)}) provides for '{fromAgent}' writing to '{toAgent}'. " +
                (allowed.Count == 0
                    ? $"No other agent comes after '{fromAgent}': it writes only to the person."
                    : $"After '{fromAgent}' come: {string.Join(", ", allowed)}.") +
                " If this handoff is needed, it must be added to a workflow.");
        }

        /// <summary>
        /// Il passo che parte quando la persona approva il lavoro di <paramref name="producer"/> e avvisa
        /// <paramref name="recipient"/>: un passo di <paramref name="recipient"/> che parte dopo un passo di
        /// <paramref name="producer"/>. Null = il workflow non prevede questo avviso.
        /// </summary>
        public static WorkflowStep ApprovalStepFor(WorkflowDescriptor wf, string producer, string recipient)
            => wf?.Steps.FirstOrDefault(s => Same(s.Agent, recipient)
                                             && s.Trigger?.Kind == WorkflowTriggerKind.After
                                             && s.Trigger.After.Any(of => Same(wf.Step(of)?.Agent, producer)
                                                                         && (wf.Step(of)?.Produces.Count ?? 0) > 0));

        /// <summary>A chi passa il lavoro un agente secondo il workflow: per dirlo all'agente quando sbaglia.</summary>
        public static IReadOnlyList<string> AssigneesOf(WorkflowDescriptor wf, string fromAgent)
            => wf == null ? Array.Empty<string>()
               : wf.Steps.Where(s => !s.IsPerson && s.Trigger?.Kind == WorkflowTriggerKind.After
                                     && s.Trigger.After.Any(a => Same(wf.Step(a)?.Agent, fromAgent))
                                     && !Same(s.Agent, fromAgent))
                         .Select(s => s.Agent).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        /// <summary>Il motivo per cui un messaggio tra agenti non è previsto dal workflow, da dare a chi lo manda.</summary>
        public static string NotForeseen(WorkflowDescriptor wf, string fromAgent, string toAgent)
        {
            var allowed = AssigneesOf(wf, fromAgent);
            return UiText.Say(
                $"Il workflow «{wf?.Title}» non prevede che '{fromAgent}' scriva a '{toAgent}'. " +
                (allowed.Count == 0
                    ? $"Dopo '{fromAgent}' non viene nessun altro agente: scrive solo alla persona."
                    : $"Dopo '{fromAgent}' vengono: {string.Join(", ", allowed)}.") +
                " Se il passaggio serve, va aggiunto al workflow.",
                $"The workflow «{wf?.Title}» does not provide for '{fromAgent}' writing to '{toAgent}'. " +
                (allowed.Count == 0
                    ? $"No other agent comes after '{fromAgent}': it writes only to the person."
                    : $"After '{fromAgent}' come: {string.Join(", ", allowed)}.") +
                " If this handoff is needed, it must be added to the workflow.");
        }

        /// <summary>
        /// Quante volte si può ancora far ripartire il lavoro di un passo dopo un rifiuto. Il limite lo decide solo chi scrive
        /// il workflow (W9): null = nessun limite (nessun ciclo, o un ciclo «fino a che» senza <c>max</c>). <paramref name="rejections"/>
        /// conta i rifiuti già avuti, compreso l'ultimo.
        /// </summary>
        public static int? ReworksLeft(WorkflowDescriptor wf, WorkflowStep step, int rejections, out string why)
        {
            why = null;
            var loop = step == null ? null : wf?.LoopOf(step.Id, WorkflowLoopKind.UntilApproved);
            if (loop?.Max == null) return null;
            var left = Math.Max(0, loop.Max.Value - (rejections - 1));
            if (left == 0)
                why = UiText.Say(
                    $"«{step.Label}» è già stato rifatto {loop.Max} {(loop.Max == 1 ? "volta" : "volte")}: è il massimo che il workflow dà al ciclo «{loop.Id}». Il lavoro resta fermo.",
                    $"«{step.Label}» has already been redone {loop.Max} {(loop.Max == 1 ? "time" : "times")}: that is the most the workflow allows for the loop «{loop.Id}». The work stays stopped.");
            return left;
        }

        private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Un workflow del progetto: il documento nell'elenco, il JSON che nomina, la regola letta.</summary>
    public sealed class ProjectWorkflow
    {
        public string Document { get; init; }
        public string JsonPath { get; init; }
        public WorkflowDescriptor Descriptor { get; init; }
    }

    /// <summary>
    /// Il documento del workflow (<c>mde_type: workflow</c>) e il JSON che nomina nel front matter (<c>workflow:</c>, relativo
    /// al documento). La configurazione punta al documento perché è ciò che le persone leggono; la regola sta nel JSON.
    /// </summary>
    public static class WorkflowDocument
    {
        private static readonly Regex FrontMatter = new(@"\A﻿?---\r?\n(.*?)\r?\n---", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Il workflow da applicare. Null senza problema = il progetto non ne ha uno (gli incarichi partono come sempre).
        /// Null con problema = ce n'è uno configurato che non si legge o ha errori: senza la regola non si sa chi avvia,
        /// e chi chiama non deve fare come se non ci fosse.
        /// </summary>
        public static WorkflowDescriptor LoadActive(string projectRoot, string documentPath, string ownershipDoc, out string problem)
        {
            problem = null;
            if (string.IsNullOrWhiteSpace(documentPath)) return null;
            try
            {
                var json = JsonPathOf(projectRoot, documentPath);
                var full = Path.Combine(projectRoot, json.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(full))
                {
                    problem = UiText.Say($"il workflow '{json}' nominato da '{documentPath}' non esiste.", $"the workflow '{json}' named by '{documentPath}' does not exist.");
                    return null;
                }
                var parsed = WorkflowFile.Read(projectRoot, File.ReadAllText(full), ownershipDoc);
                if (!parsed.IsValid)
                {
                    var errors = parsed.Issues.Where(i => i.Severity == WorkflowSeverity.Error).ToList();
                    problem = UiText.Say(
                        $"il workflow '{json}' ha {errors.Count} {(errors.Count == 1 ? "errore" : "errori")}, il primo: {errors[0]}.",
                        $"the workflow '{json}' has {errors.Count} {(errors.Count == 1 ? "error" : "errors")}, the first: {errors[0]}.");
                    return null;
                }
                return parsed.Descriptor;
            }
            catch (InvalidOperationException ex)
            {
                problem = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// I workflow del progetto (PW, 18/10/2026: <c>agentCity.workflowDocs</c> è un elenco). Vuoto senza problema = il
        /// progetto non ne ha. Null con problema = uno non si legge, o due non stanno insieme: lo stesso documento due volte,
        /// due documenti con lo stesso JSON, o lo stesso agente che apre un giro in due workflow (al lancio non si saprebbe
        /// quale giro aprire). Senza tutte le regole non si sa chi avvia: chi chiama si ferma, come con un workflow solo.
        /// </summary>
        public static IReadOnlyList<ProjectWorkflow> LoadAll(string projectRoot, IReadOnlyList<string> documents, string ownershipDoc, out string problem)
        {
            problem = null;
            var result = new List<ProjectWorkflow>();
            if (documents == null || documents.Count == 0) return result;
            foreach (var document in documents)
            {
                var relative = (document ?? "").Trim().Replace('\\', '/').TrimStart('/');
                var twin = result.FirstOrDefault(w => string.Equals(w.Document, relative, StringComparison.OrdinalIgnoreCase));
                if (twin != null)
                {
                    problem = UiText.Say($"il documento del workflow '{relative}' è nell'elenco due volte (agentCity.workflowDocs).", $"the workflow document '{relative}' is listed twice (agentCity.workflowDocs).");
                    return null;
                }
                var wf = LoadActive(projectRoot, relative, ownershipDoc, out problem);
                if (problem != null) return null;
                if (wf == null)
                {
                    problem = UiText.Say("una riga vuota nell'elenco dei workflow (agentCity.workflowDocs).", "an empty row in the list of workflows (agentCity.workflowDocs).");
                    return null;
                }
                var json = JsonPathOf(projectRoot, relative);
                var same = result.FirstOrDefault(w => string.Equals(w.JsonPath, json, StringComparison.OrdinalIgnoreCase));
                if (same != null)
                {
                    problem = UiText.Say($"'{same.Document}' e '{relative}' nominano lo stesso workflow '{json}': ne basta uno.", $"'{same.Document}' and '{relative}' name the same workflow '{json}': one is enough.");
                    return null;
                }
                foreach (var launch in wf.Steps.Where(s => s.Trigger?.Kind == WorkflowTriggerKind.Launch))
                {
                    var other = result.FirstOrDefault(w => w.Descriptor.Steps.Any(s => s.Trigger?.Kind == WorkflowTriggerKind.Launch && Same(s.Agent, launch.Agent)));
                    if (other != null)
                    {
                        problem = UiText.Say(
                            $"'{launch.Agent}' apre un giro sia in «{other.Descriptor.Title}» sia in «{wf.Title}»: lanciandolo non si saprebbe quale. Un agente apre i giri di un workflow solo.",
                            $"'{launch.Agent}' opens a round both in «{other.Descriptor.Title}» and in «{wf.Title}»: launching it, nobody would know which. An agent opens the rounds of one workflow only.");
                        return null;
                    }
                }
                result.Add(new ProjectWorkflow { Document = relative, JsonPath = json, Descriptor = wf });
            }
            return result;
        }

        private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        /// <summary>Il JSON del workflow, dalla radice del progetto con '/'. Solleva con il motivo se qualcosa non torna.</summary>
        public static string JsonPathOf(string projectRoot, string documentPath)
        {
            if (string.IsNullOrWhiteSpace(documentPath))
                throw new InvalidOperationException(UiText.Say("Nessun documento del workflow configurato (agentCity.workflowDocs).", "No workflow document configured (agentCity.workflowDocs)."));
            var relative = documentPath.Trim().Replace('\\', '/').TrimStart('/');
            var full = Path.Combine(projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full))
                throw new InvalidOperationException(UiText.Say($"Il documento del workflow '{relative}' non esiste (agentCity.workflowDocs).", $"The workflow document '{relative}' does not exist (agentCity.workflowDocs)."));

            var m = FrontMatter.Match(File.ReadAllText(full));
            if (!m.Success)
                throw new InvalidOperationException(UiText.Say($"'{relative}' non ha il front matter: serve «mde_type: workflow» e «workflow: <file>.workflow.json».", $"'{relative}' has no front matter: it needs «mde_type: workflow» and «workflow: <file>.workflow.json»."));
            string type = null, json = null;
            foreach (var line in m.Groups[1].Value.Split('\n'))
            {
                var kv = line.Split(new[] { ':' }, 2);
                if (kv.Length != 2) continue;
                var key = kv[0].Trim();
                var value = kv[1].Trim().Trim('"', '\'');
                if (key == "mde_type") type = value;
                else if (key == "workflow") json = value;
            }
            if (type != "workflow")
                throw new InvalidOperationException(UiText.Say($"'{relative}' non è un documento del workflow: nel front matter serve «mde_type: workflow».", $"'{relative}' is not a workflow document: the front matter needs «mde_type: workflow»."));
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException(UiText.Say($"'{relative}' non dice quale file è il workflow: nel front matter serve «workflow: <file>.workflow.json».", $"'{relative}' does not say which file is the workflow: the front matter needs «workflow: <file>.workflow.json»."));

            var folder = relative.Contains('/') ? relative.Substring(0, relative.LastIndexOf('/') + 1) : "";
            var combined = (folder + json.Replace('\\', '/')).Split('/');
            var parts = new List<string>();
            foreach (var p in combined)
            {
                if (p == "" || p == ".") continue;
                if (p == "..")
                {
                    if (parts.Count == 0) throw new InvalidOperationException(UiText.Say($"'{json}' esce dal progetto.", $"'{json}' goes outside the project."));
                    parts.RemoveAt(parts.Count - 1);
                }
                else parts.Add(p);
            }
            return string.Join("/", parts);
        }
    }
}
