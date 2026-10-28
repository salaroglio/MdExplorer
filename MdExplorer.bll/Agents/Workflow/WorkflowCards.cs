using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MdExplorer.Features.Services;
using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Models;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Le interfacce delle schede che un workflow nomina (D8): per ogni agente il percorso della scheda, i suoi lavori con
    /// ingressi, documenti letti e uscite, e i pulsanti. È ciò che il workflow unisce ai suoi passi, ed è anche ciò che un
    /// giro salva accanto al workflow: una scheda cambiata a giro aperto non cambia il giro (domanda 7 dello sprint v3).
    /// </summary>
    public class WorkflowCardSet
    {
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        };

        /// <summary>Le schede lette, per nome dell'agente.</summary>
        public Dictionary<string, WorkflowCard> Agents { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gli agenti la cui scheda non si è potuta leggere, con il motivo.</summary>
        public Dictionary<string, string> Problems { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public string ToJson() => JsonSerializer.Serialize(this, Json);

        public static WorkflowCardSet FromJson(string json)
        {
            var set = JsonSerializer.Deserialize<WorkflowCardSet>(json, Json)
                      ?? throw new InvalidOperationException(UiText.Say("le schede salvate nel giro sono vuote.", "the cards saved in the round are empty."));
            // Il dizionario letto dal JSON confronta i nomi con le maiuscole: si rifà come quello di Load.
            return new WorkflowCardSet
            {
                Agents = new Dictionary<string, WorkflowCard>(set.Agents ?? new(), StringComparer.OrdinalIgnoreCase),
                Problems = new Dictionary<string, string>(set.Problems ?? new(), StringComparer.OrdinalIgnoreCase),
            };
        }

        /// <summary>Le schede degli agenti nominati, lette dal progetto. Una scheda che manca o non si legge va in <see cref="Problems"/>.</summary>
        public static WorkflowCardSet Load(string projectRoot, IEnumerable<string> agents)
        {
            var set = new WorkflowCardSet();
            var parser = new YamlAgentCardParser();
            foreach (var agent in agents.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var found = FindCard(projectRoot, agent, out var problem);
                if (found == null)
                {
                    set.Problems[agent] = problem;
                    continue;
                }
                var parsed = parser.GetDescriptor(File.ReadAllText(Path.Combine(projectRoot, found.Replace('/', Path.DirectorySeparatorChar))));
                if (!parsed.IsValid || parsed.Card == null)
                {
                    set.Problems[agent] = UiText.Say($"la scheda '{found}' non si legge: {parsed.RegistrationError ?? "manca il blocco a2a:"}",
                        $"the card '{found}' cannot be read: {parsed.RegistrationError ?? "the a2a: block is missing"}");
                    continue;
                }
                if (!string.Equals(parsed.Card.Name, agent, StringComparison.OrdinalIgnoreCase))
                {
                    set.Problems[agent] = UiText.Say($"la scheda '{found}' si chiama '{parsed.Card.Name}', non '{agent}'.", $"the card '{found}' is named '{parsed.Card.Name}', not '{agent}'.");
                    continue;
                }
                set.Agents[agent] = new WorkflowCard
                {
                    Path = found,
                    Skills = parsed.Card.Skills?.ToList() ?? new List<AgentCardSkill>(),
                    Replies = parsed.Card.Replies?.ToList() ?? new List<AgentCardReply>(),
                };
            }
            return set;
        }

        /// <summary>
        /// La scheda di un agente, dalla radice del progetto: <c>&lt;nome&gt;.agent.md</c>, prima nel posto solito
        /// (<c>.github/agents</c>) poi ovunque nel progetto, mai nelle scrivanie degli agenti. Null, con il motivo, se non
        /// c'è o se ce n'è più d'una: non si sceglie a caso.
        /// </summary>
        public static string FindCard(string projectRoot, string agent, out string problem)
        {
            problem = null;
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
            {
                problem = UiText.Say($"la cartella del progetto '{projectRoot}' non esiste.", $"the project folder '{projectRoot}' does not exist.");
                return null;
            }
            var fileName = agent + ".agent.md";
            if (File.Exists(Path.Combine(projectRoot, ".github", "agents", fileName))) return ".github/agents/" + fileName;

            var skip = new[] { ".git", ".worktrees", "node_modules", ".md" };
            var found = new List<string>();
            var pending = new Stack<string>();
            pending.Push(projectRoot);
            while (pending.Count > 0 && found.Count < 2)
            {
                var dir = pending.Pop();
                try
                {
                    found.AddRange(Directory.EnumerateFiles(dir, fileName));
                    foreach (var sub in Directory.EnumerateDirectories(dir))
                        if (!skip.Contains(Path.GetFileName(sub), StringComparer.OrdinalIgnoreCase)) pending.Push(sub);
                }
                catch (UnauthorizedAccessException) { /* una cartella che non si legge non ha schede da trovare */ }
            }
            if (found.Count == 1) return Path.GetRelativePath(projectRoot, found[0]).Replace('\\', '/');
            problem = found.Count == 0
                ? UiText.Say($"nel progetto non c'è la scheda '{fileName}'.", $"the project has no card '{fileName}'.")
                : UiText.Say($"nel progetto ci sono più schede '{fileName}': ne serve una sola.", $"the project has more than one card '{fileName}': only one is allowed.");
            return null;
        }
    }

    /// <summary>L'interfaccia di una scheda: dove sta, i suoi lavori, i suoi pulsanti.</summary>
    public class WorkflowCard
    {
        public string Path { get; set; }
        public List<AgentCardSkill> Skills { get; set; } = new();
        public List<AgentCardReply> Replies { get; set; } = new();

        public AgentCardSkill Skill(string id) => Skills.FirstOrDefault(s => s.Id == id);
        public AgentCardReply Reply(string id) => Replies.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// L'unione del workflow con le schede (D8): ogni passo di un agente prende dalla sua skill le uscite e i documenti
    /// letti, e si controlla che i puntini combacino — ogni ingresso della skill riceve un valore e uno solo, le uscite
    /// nominate esistono, i pulsanti sono proposti dal lavoro sotto cui stanno e i loro valori arrivano al giro.
    /// </summary>
    public static class WorkflowBinder
    {
        public static IList<WorkflowIssue> Bind(WorkflowDescriptor wf, WorkflowCardSet cards)
        {
            var issues = new List<WorkflowIssue>();
            if (wf == null) return issues;
            wf.Cards = cards;

            for (var i = 0; i < wf.Steps.Count; i++)
            {
                var step = wf.Steps[i];
                if (step.IsPerson || step.Agent == null) continue;
                var path = $"steps[{i}]";
                if (!cards.Agents.TryGetValue(step.Agent, out var card))
                {
                    issues.Add(Error($"{path}.agent", cards.Problems.TryGetValue(step.Agent, out var why)
                        ? why
                        : UiText.Say($"la scheda di '{step.Agent}' non è stata letta.", $"the card of '{step.Agent}' was not read.")));
                    continue;
                }
                if (step.Skill == null) continue;
                var skill = card.Skill(step.Skill);
                if (skill == null)
                {
                    issues.Add(Error($"{path}.skill", UiText.Say($"la scheda di '{step.Agent}' ({card.Path}) non ha il lavoro '{step.Skill}'.", $"the card of '{step.Agent}' ({card.Path}) has no job '{step.Skill}'."),
                        card.Skills.Count == 0
                            ? UiText.Say("dichiaralo in a2a.skills della scheda", "declare it in a2a.skills in the card")
                            : UiText.Say("lavori della scheda: ", "jobs in the card: ") + string.Join(", ", card.Skills.Select(s => s.Id))));
                    continue;
                }

                var declared = skill.Inputs ?? new List<AgentCardInput>();
                foreach (var input in declared.Where(d => !step.Inputs.ContainsKey(d.Id)))
                    issues.Add(Error($"{path}.inputs", UiText.Say($"l'ingresso '{input.Id}' del lavoro '{skill.Id}' non riceve niente ({input.Description}).", $"the input '{input.Id}' of job '{skill.Id}' receives nothing ({input.Description})."),
                        UiText.Say($"aggiungi \"{input.Id}\": un testo con le {{variabili}} o {{ \"from\": \"passo.uscita\" }}", $"add \"{input.Id}\": a text with {{variables}} or {{ \"from\": \"step.output\" }}")));
                foreach (var name in step.Inputs.Keys.Where(n => declared.All(d => d.Id != n)))
                    issues.Add(Error($"{path}.inputs.{name}", UiText.Say($"il lavoro '{skill.Id}' di '{step.Agent}' non ha l'ingresso '{name}'.", $"job '{skill.Id}' of '{step.Agent}' has no input '{name}'."),
                        declared.Count == 0
                            ? UiText.Say("il lavoro non dichiara ingressi: ciò che vuoi aggiungere va nel \"brief\"", "the job declares no inputs: what you want to add goes in the \"brief\"")
                            : UiText.Say("ingressi del lavoro: ", "inputs of the job: ") + string.Join(", ", declared.Select(d => d.Id))));

                step.Outputs.Clear();
                step.Produces.Clear();
                foreach (var o in skill.Outputs ?? new List<AgentCardOutput>())
                {
                    step.Outputs.Add(new WorkflowOutput { Id = o.Id, Path = o.Path, Note = o.Note, Action = o.Action ?? "writes" });
                    step.Produces.Add(o.Path);
                }
                step.Reads.Clear();
                foreach (var rd in skill.Reads ?? new List<AgentCardRead>())
                    step.Reads.Add(new WorkflowRead { Path = rd.Path, Section = rd.Section, Note = rd.Note });

                if (step.ApprovalBy != null && step.Outputs.Count == 0)
                    issues.Add(Error($"{path}.approval", UiText.Say($"il lavoro '{skill.Id}' non consegna documenti: non c'è niente da approvare.", $"job '{skill.Id}' delivers no documents: there is nothing to approve."),
                        UiText.Say("togli \"approval\", o dichiara le uscite del lavoro nella scheda", "remove \"approval\", or declare the job's outputs in the card")));
            }

            for (var i = 0; i < wf.Steps.Count; i++)
            {
                var step = wf.Steps[i];
                CheckReply(wf, cards, step, $"steps[{i}]", issues);
                foreach (var (name, input) in step.Inputs)
                    for (var k = 0; k < input.From.Count; k++)
                    {
                        var dot = input.From[k].IndexOf('.');
                        var source = wf.Step(input.From[k].Substring(0, dot));
                        var output = input.From[k].Substring(dot + 1);
                        // Un passo sconosciuto lo dice già la lettura; uno di un agente senza scheda, l'errore sulla scheda.
                        if (source == null || (!source.IsPerson && (source.Agent == null || !cards.Agents.ContainsKey(source.Agent)))) continue;
                        if (source.Outputs.Any(o => o.Id == output)) continue;
                        var where = input.From.Count == 1 ? $"steps[{i}].inputs.{name}.from" : $"steps[{i}].inputs.{name}.from[{k}]";
                        issues.Add(Error(where, UiText.Say($"il passo '{source.Id}' non ha l'uscita '{output}'.", $"step '{source.Id}' has no output '{output}'."),
                            source.Outputs.Count == 0
                                ? UiText.Say($"'{source.Id}' non consegna documenti", $"'{source.Id}' delivers no documents")
                                : UiText.Say("uscite del passo: ", "outputs of the step: ") + string.Join(", ", source.Outputs.Select(o => o.Id))));
                    }
            }

            // Un ciclo ripete un lavoro finché non è approvato, o lo fa un numero di volte approvando ogni giro: senza uscite
            // non c'è niente da approvare, e il ciclo non finirebbe mai.
            for (var l = 0; l < wf.Loops.Count; l++)
                for (var j = 0; j < wf.Loops[l].Steps.Count; j++)
                {
                    var step = wf.Step(wf.Loops[l].Steps[j]);
                    if (step == null || step.IsPerson || step.Agent == null || !cards.Agents.ContainsKey(step.Agent) || step.Outputs.Count > 0) continue;
                    if (cards.Agents[step.Agent].Skill(step.Skill) == null) continue;
                    issues.Add(Error($"loops[{l}].steps[{j}]", UiText.Say($"il passo '{step.Id}' non consegna documenti: un ciclo aspetta un'approvazione che non arriverebbe.", $"step '{step.Id}' delivers no documents: a loop waits for an approval that would never come."),
                        UiText.Say("togli il passo dal ciclo, o dichiara le uscite del lavoro nella scheda", "remove the step from the loop, or declare the job's outputs in the card")));
                }
            return issues;
        }

        /// <summary>Un pulsante sotto il messaggio di un agente: lo dichiara la scheda, lo propone il lavoro, i suoi valori arrivano al giro.</summary>
        private static void CheckReply(WorkflowDescriptor wf, WorkflowCardSet cards, WorkflowStep step, string path, List<WorkflowIssue> issues)
        {
            var t = step.Trigger;
            if (t?.Kind != WorkflowTriggerKind.Reply || t.ReplyId == null || t.FromStep == null) return;
            var source = wf.Step(t.FromStep);
            if (source == null || source.IsPerson || source.Agent == null || !cards.Agents.TryGetValue(source.Agent, out var card)) return;
            var skill = card.Skill(source.Skill);
            if (skill == null) return;

            var reply = card.Reply(t.ReplyId);
            if (reply == null)
            {
                issues.Add(Error($"{path}.trigger.reply", UiText.Say($"la scheda di '{source.Agent}' non dichiara la risposta '{t.ReplyId}'.", $"the card of '{source.Agent}' does not declare the reply '{t.ReplyId}'."),
                    card.Replies.Count == 0
                        ? UiText.Say("aggiungila in a2a.replies della scheda", "add it to a2a.replies in the card")
                        : UiText.Say("risposte dichiarate: ", "declared replies: ") + string.Join(", ", card.Replies.Select(d => d.Id))));
                return;
            }
            if (skill.Replies == null || !skill.Replies.Contains(reply.Id, StringComparer.OrdinalIgnoreCase))
                issues.Add(Error($"{path}.trigger.reply", UiText.Say($"il lavoro '{skill.Id}' di '{source.Agent}' non propone la risposta '{t.ReplyId}': il pulsante non comparirebbe.", $"job '{skill.Id}' of '{source.Agent}' does not propose the reply '{t.ReplyId}': the button would never appear."),
                    UiText.Say($"aggiungi '{reply.Id}' a replies del lavoro '{skill.Id}' nella scheda", $"add '{reply.Id}' to the replies of job '{skill.Id}' in the card")));

            // I valori del pulsante diventano variabili del giro: ognuno deve essere dichiarato, o si perderebbe.
            var carried = (reply.Values ?? new List<string>())
                .Concat(new[] { reply.Label, reply.Description, reply.Message }
                    .Where(x => !string.IsNullOrEmpty(x))
                    .SelectMany(x => WorkflowParser.Placeholder.Matches(x).Select(m => m.Groups[1].Value)))
                .Distinct(StringComparer.Ordinal);
            foreach (var name in carried.Where(n => !wf.Variables.ContainsKey(n)))
                issues.Add(Error($"{path}.trigger.reply",
                    UiText.Say($"il pulsante '{t.ReplyId}' porta il valore '{{{name}}}', che non è una variabile del workflow: ai passi successivi non arriverebbe.",
                        $"the button '{t.ReplyId}' carries the value '{{{name}}}', which is not a workflow variable: it would not reach the next steps."),
                    UiText.Say($"dichiaralo in \"variables\": {{ \"{name}\": \"che cos'è\" }}",
                        $"declare it in \"variables\": {{ \"{name}\": \"what it is\" }}")));
        }

        private static WorkflowIssue Error(string path, string message, string fix = null) =>
            new() { Severity = WorkflowSeverity.Error, Path = path, Message = message, Fix = fix };
    }
}
