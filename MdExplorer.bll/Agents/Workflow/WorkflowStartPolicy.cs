using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            IEnumerable<WorkflowStep> candidates;
            if (isApproval)
                candidates = wf.Steps.Where(s => Same(s.Agent, toAgent) && s.Trigger?.Kind == WorkflowTriggerKind.Approval);
            else if (!string.IsNullOrWhiteSpace(fromAgent) && !Same(fromAgent, ConversationHopGuard.UserRecipient))
                candidates = wf.Steps.Where(s => Same(s.Agent, toAgent)
                                                 && s.Trigger?.Kind == WorkflowTriggerKind.Assignment
                                                 && Same(wf.Step(s.Trigger.FromStep)?.Agent, fromAgent));
            else
                return null;

            var list = candidates.ToList();
            return list.FirstOrDefault(s => s.Start == WorkflowStart.AskOwner) ?? list.FirstOrDefault();
        }

        private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
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
        public static WorkflowDescriptor LoadActive(string projectRoot, string documentPath, out string problem)
        {
            problem = null;
            if (string.IsNullOrWhiteSpace(documentPath)) return null;
            try
            {
                var json = JsonPathOf(projectRoot, documentPath);
                var full = Path.Combine(projectRoot, json.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(full))
                {
                    problem = $"il workflow '{json}' nominato da '{documentPath}' non esiste.";
                    return null;
                }
                var parsed = WorkflowParser.Parse(File.ReadAllText(full));
                if (!parsed.IsValid)
                {
                    var errors = parsed.Issues.Where(i => i.Severity == WorkflowSeverity.Error).ToList();
                    problem = $"il workflow '{json}' ha {errors.Count} {(errors.Count == 1 ? "errore" : "errori")}, il primo: {errors[0]}.";
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

        /// <summary>Il JSON del workflow, dalla radice del progetto con '/'. Solleva con il motivo se qualcosa non torna.</summary>
        public static string JsonPathOf(string projectRoot, string documentPath)
        {
            if (string.IsNullOrWhiteSpace(documentPath))
                throw new InvalidOperationException("Nessun documento del workflow configurato (agentCity.workflowDoc).");
            var relative = documentPath.Trim().Replace('\\', '/').TrimStart('/');
            var full = Path.Combine(projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full))
                throw new InvalidOperationException($"Il documento del workflow '{relative}' non esiste (agentCity.workflowDoc).");

            var m = FrontMatter.Match(File.ReadAllText(full));
            if (!m.Success)
                throw new InvalidOperationException($"'{relative}' non ha il front matter: serve «mde_type: workflow» e «workflow: <file>.workflow.json».");
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
                throw new InvalidOperationException($"'{relative}' non è un documento del workflow: nel front matter serve «mde_type: workflow».");
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException($"'{relative}' non dice quale file è il workflow: nel front matter serve «workflow: <file>.workflow.json».");

            var folder = relative.Contains('/') ? relative.Substring(0, relative.LastIndexOf('/') + 1) : "";
            var combined = (folder + json.Replace('\\', '/')).Split('/');
            var parts = new List<string>();
            foreach (var p in combined)
            {
                if (p == "" || p == ".") continue;
                if (p == "..")
                {
                    if (parts.Count == 0) throw new InvalidOperationException($"'{json}' esce dal progetto.");
                    parts.RemoveAt(parts.Count - 1);
                }
                else parts.Add(p);
            }
            return string.Join("/", parts);
        }
    }
}
