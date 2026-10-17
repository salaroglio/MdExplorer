using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Il diagramma di un workflow, generato e mai scritto a mano: stesso workflow e stesse schede, stesso PlantUML. Si legge
    /// dall'alto in basso nell'ordine in cui le cose succedono. I riquadri per agente non funzionano: un agente che lavora
    /// all'inizio e alla fine (l'account manager) tira la fine in alto e le frecce risalgono. L'agente sta dentro il passo, il
    /// colore dice chi lo avvia; gli omini sono i ruoli di <c>responsabilita.md</c> (chi lancia, avvia, approva); in verde i
    /// documenti che un lavoro legge, in lilla quelli che consegna; i pulsanti e gli esiti sono esagoni. Un documento che
    /// un passo riceve con <c>from</c> porta la freccia dal file, non dal passo. I link portano alla scheda e al documento.
    /// </summary>
    public static class WorkflowPlantuml
    {
        /// <summary>Prefisso dei link interni: il visualizzatore li apre in MdExplorer (percorso dalla radice del progetto).</summary>
        public const string LinkScheme = "mde:";

        // La palette della skill mde-plantuml: la tinta sopravvive al tema scuro, il significato sta nel colore.
        private const string Neutral = "#F1F3F4", NeutralLine = "#5F6368";
        private const string Attention = "#FEF7E0", AttentionLine = "#F29900";
        private const string Focus = "#E8F0FE", FocusLine = "#1A73E8";
        private const string Read = "#E6F4EA", ReadLine = "#188038";
        private const string Written = "#F3E8FD", WrittenLine = "#9334E6";
        private const string ErrorLine = "#D93025";

        /// <param name="cardPath">Il percorso della scheda di un agente dalla radice del progetto, o null se non si trova.</param>
        /// <param name="artifactExists">Se un documento c'è già (percorso dalla radice). Null = non si sa: tutti come scritti.</param>
        /// <param name="roleOf">
        /// Il ruolo di chi risponde di un agente (la colonna Ruolo di <c>responsabilita.md</c>), o null se non si sa: allora
        /// l'omino dice «responsabile di» e il nome dell'agente.
        /// </param>
        /// <param name="folder">
        /// La cartella del workflow dalla radice del progetto: i nomi dei documenti dentro di lei si scrivono da lì, gli altri
        /// dalla radice. Null = solo il nome del file.
        /// </param>
        public static string Render(WorkflowDescriptor wf, Func<string, string> cardPath, Func<string, bool> artifactExists = null,
            Func<string, string> roleOf = null, string folder = null)
        {
            if (wf == null) throw new ArgumentNullException(nameof(wf));
            cardPath ??= _ => null;
            artifactExists ??= _ => true;
            roleOf ??= _ => null;
            var anyMissing = false;
            var sb = new StringBuilder();
            sb.AppendLine("@startuml");
            sb.AppendLine("!theme plain");
            sb.AppendLine($"skinparam ArrowColor {NeutralLine}");
            sb.AppendLine($"skinparam ArrowFontColor {NeutralLine}");
            sb.AppendLine("skinparam ArrowFontSize 11");
            sb.AppendLine("skinparam actorStyle awesome");
            sb.AppendLine("skinparam rectangle {");
            sb.AppendLine($"  BackgroundColor {Neutral}");
            sb.AppendLine($"  BorderColor {NeutralLine}");
            sb.AppendLine("  RoundCorner 10");
            sb.AppendLine($"  BackgroundColor<<askowner>> {Attention}");
            sb.AppendLine($"  BorderColor<<askowner>> {AttentionLine}");
            sb.AppendLine($"  BackgroundColor<<manual>> {Focus}");
            sb.AppendLine($"  BorderColor<<manual>> {FocusLine}");
            sb.AppendLine("  BackgroundColor<<person>> #FFFFFF");
            sb.AppendLine($"  BorderColor<<person>> {NeutralLine}");
            sb.AppendLine("  BorderThickness<<person>> 2");
            sb.AppendLine("}");
            sb.AppendLine("skinparam file {");
            sb.AppendLine($"  BackgroundColor {Written}");
            sb.AppendLine($"  BorderColor {WrittenLine}");
            sb.AppendLine("  FontSize 11");
            sb.AppendLine($"  BackgroundColor<<input>> {Read}");
            sb.AppendLine($"  BorderColor<<input>> {ReadLine}");
            sb.AppendLine("}");
            sb.AppendLine("skinparam hexagon {");
            sb.AppendLine($"  BackgroundColor {Neutral}");
            sb.AppendLine($"  BorderColor {NeutralLine}");
            sb.AppendLine("  FontSize 11");
            sb.AppendLine("}");
            sb.AppendLine("hide stereotype");
            // Ogni passo ha un link: sottolineati tutti, i testi si leggono peggio e non dicono niente di più.
            sb.AppendLine("skinparam HyperlinkUnderline false");
            sb.AppendLine($"skinparam HyperlinkColor {NeutralLine}");
            sb.AppendLine();

            var steps = wf.Steps.Where(s => s.Id != null).ToList();
            string Role(WorkflowStep s) => s.IsPerson ? s.Person : roleOf(s.Agent) ?? $"responsabile di {s.Agent}";

            // Per ogni passo, nell'ordine: chi lo lancia, avvia o approva, che cosa legge, il passo, che cosa consegna. Un
            // omino per passo, come in un disegno a mano: un ruolo che lavora all'inizio e alla fine compare due volte,
            // altrimenti le frecce risalirebbero.
            foreach (var s in steps)
            {
                var verbs = PersonVerbs(s);
                if (verbs != null) sb.AppendLine($"actor \"{Text(Role(s))}\" as {Actor(s.Id)}");
                if (s.ApprovalBy != null) sb.AppendLine($"actor \"{Text(s.ApprovalBy)}\" as {Approver(s.Id)}");
                for (var i = 0; i < s.Reads.Count; i++)
                {
                    var r = s.Reads[i];
                    var name = Shorten(r.Path, folder) + (string.IsNullOrWhiteSpace(r.Section) ? "" : " · " + r.Section);
                    sb.AppendLine($"file \"{Text(name)}{Note(r.Note)}\" <<input>> as {ReadNode(s.Id, i)}{Link(r.Path)}");
                }

                if (s.IsPerson)
                    sb.AppendLine($"rectangle \"**{Text(s.Label)}**\\n<size:11><&person> {Text(s.Person)}</size>\\n<size:10><i>tocca a una persona</i></size>\" <<person>> as {Node(s.Id)}");
                else
                {
                    var stereotype = s.Start switch { WorkflowStart.AskOwner => " <<askowner>>", WorkflowStart.Manual => " <<manual>>", _ => "" };
                    sb.AppendLine($"rectangle \"**{Text(s.Label)}**\\n<size:11>{Text(s.Agent)}</size>\\n<size:10><i>{StartText(s.Start)}</i></size>\"{stereotype} as {Node(s.Id)}{Link(cardPath(s.Agent))}");
                }

                for (var i = 0; i < s.Outputs.Count; i++)
                {
                    var o = s.Outputs[i];
                    // Un nome con «*» è una famiglia di file: niente link, non c'è un file solo da aprire. Un documento non
                    // ancora scritto ha il bordo tratteggiato e nessun link: aprirlo porterebbe a un documento che non c'è.
                    var family = o.Path.Contains('*');
                    var exists = family || artifactExists(o.Path);
                    anyMissing |= !exists;
                    var look = exists ? "" : " #FFFFFF;line.dashed;line:9AA0A6";
                    var link = family || !exists ? "" : Link(o.Path);
                    sb.AppendLine($"file \"{Text(Shorten(o.Path, folder))}{Note(o.Note)}\" as {OutputNode(s.Id, i)}{look}{link}");
                }

                // Gli esiti di una persona: un esagono ciascuno, anche quelli da cui non parte niente (chiudono il giro).
                foreach (var outcome in s.Outcomes)
                    sb.AppendLine($"hexagon \"esito «{Text(outcome.Label ?? outcome.Id)}»\" as {Outcome(s.Id, outcome.Id)}");
            }

            // I pulsanti sotto il messaggio di un agente: uno per pulsante, anche se fa partire più passi.
            var buttons = steps.Where(s => s.Trigger?.Kind == WorkflowTriggerKind.Reply && wf.Step(s.Trigger.FromStep) is { IsPerson: false })
                .Select(s => (From: s.Trigger.FromStep, Reply: s.Trigger.ReplyId)).Distinct().ToList();
            foreach (var (from, reply) in buttons)
            {
                var source = wf.Step(from);
                var label = wf.Cards != null && wf.Cards.Agents.TryGetValue(source.Agent, out var card) ? card.Reply(reply)?.Label : null;
                sb.AppendLine($"hexagon \"pulsante «{Text(label ?? reply)}»\\n<size:10>lo preme {Text(Role(source))}</size>\" as {Button(from, reply)}");
            }

            // L'attesa di più passi è un punto del flusso, e si vede: un esagono prima del passo.
            foreach (var s in steps.Where(IsJoin))
                sb.AppendLine($"hexagon \"{(s.Trigger.Wait == WorkflowWait.All ? "attende tutti" : "basta il primo")}\" as {Join(s.Id)}");
            sb.AppendLine();

            // Le persone e i documenti di ogni passo.
            foreach (var s in steps)
            {
                var verbs = PersonVerbs(s);
                if (verbs != null) sb.AppendLine($"{Actor(s.Id)} --> {Node(s.Id)} : {verbs}");
                if (s.ApprovalBy != null) sb.AppendLine($"{Approver(s.Id)} --> {Node(s.Id)} : approva");
                for (var i = 0; i < s.Reads.Count; i++)
                    sb.AppendLine($"{ReadNode(s.Id, i)} ..> {Node(s.Id)} : legge");
                // Le uscite stanno sotto il passo, come in un disegno a mano: da lì vanno ai passi che le ricevono. A destra
                // ci sono i cicli (PlantUML li mette sempre lì), e un'uscita di lato tirerebbe le frecce attraverso il diagramma.
                for (var i = 0; i < s.Outputs.Count; i++)
                    sb.AppendLine($"{Node(s.Id)} ..> {OutputNode(s.Id, i)} : {ActionText(s.Outputs[i].Action)}");
                foreach (var outcome in s.Outcomes)
                    sb.AppendLine($"{Node(s.Id)} --> {Outcome(s.Id, outcome.Id)}");
            }
            foreach (var (from, reply) in buttons)
                sb.AppendLine($"{Node(from)} --> {Button(from, reply)}");

            // Che cosa fa partire ogni passo, e con che cosa.
            foreach (var s in steps.Where(s => s.Trigger != null))
            {
                var t = s.Trigger;
                var brief = Assignment(s);
                switch (t.Kind)
                {
                    case WorkflowTriggerKind.Launch:
                        break; // lo dice l'omino: «lancia»
                    case WorkflowTriggerKind.Reply when wf.Step(t.FromStep) is { IsPerson: true }:
                        sb.AppendLine($"{Outcome(t.FromStep, t.ReplyId)} --> {Node(s.Id)}{Label(brief)}");
                        break;
                    case WorkflowTriggerKind.Reply:
                        sb.AppendLine($"{Button(t.FromStep, t.ReplyId)} --> {Node(s.Id)}{Label(brief)}");
                        break;
                    case WorkflowTriggerKind.After when IsJoin(s):
                        foreach (var of in t.After) Finished(wf, s, of, Join(s.Id), sb, null);
                        sb.AppendLine($"{Join(s.Id)} --> {Node(s.Id)}{Label(brief)}");
                        break;
                    case WorkflowTriggerKind.After:
                        Finished(wf, s, t.After.Single(), Node(s.Id), sb, brief);
                        break;
                }

                // Un documento che arriva da un passo che non lo fa partire: la freccia dal file, tratteggiata.
                foreach (var (source, index) in Received(wf, s).Where(r => !t.Sources.Contains(r.Source.Id)))
                    sb.AppendLine($"{OutputNode(source.Id, index)} ..> {Node(s.Id)} : riceve");
            }

            // I cicli: una freccia che torna sul passo. Rossa tratteggiata = si rifà dopo un rifiuto («fino a che»);
            // blu = si fanno più giri («for»). La spiegazione, con i numeri, sta nella legenda.
            foreach (var loop in wf.Loops)
                foreach (var id in loop.Steps.Where(id => steps.Any(s => s.Id == id)))
                    sb.AppendLine(loop.Kind == WorkflowLoopKind.Times
                        ? $"{Node(id)} -[{FocusLine},bold]-> {Node(id)} : ×{loop.Times}"
                        : $"{Node(id)} -[{ErrorLine},dashed]-> {Node(id)}");

            sb.AppendLine();
            sb.AppendLine("legend right");
            var agents = steps.Where(s => !s.IsPerson).ToList();
            var starts = agents.Select(s => s.Start).Distinct().ToList();
            if (starts.Contains(WorkflowStart.Manual)) sb.AppendLine($"  <back:{Focus}>   </back> la persona lo lancia a mano");
            if (starts.Contains(WorkflowStart.AskOwner)) sb.AppendLine($"  <back:{Attention}>   </back> lo avvia il responsabile dell'agente");
            if (starts.Contains(WorkflowStart.Auto)) sb.AppendLine($"  <back:{Neutral}>   </back> parte da solo");
            if (steps.Any(s => s.IsPerson)) sb.AppendLine("  <&person> tocca a una persona: lo chiude con un esito");
            if (steps.Any(s => s.Reads.Count > 0)) sb.AppendLine($"  <back:{Read}>   </back> documento che il lavoro legge");
            if (steps.Any(s => s.Outputs.Count > 0)) sb.AppendLine($"  <back:{Written}>   </back> documento che il passo consegna (l'agente lo fa approvare)");
            if (anyMissing) sb.AppendLine("  <color:#9AA0A6>- - -</color> documento non ancora scritto");
            foreach (var loop in wf.Loops.Where(l => l.Id != null))
            {
                var which = Text(string.Join(", ", loop.Steps));
                if (loop.Kind == WorkflowLoopKind.Times)
                    sb.AppendLine($"  <color:{FocusLine}>⟳</color> {Text(loop.Id)} ({which}): {loop.Times} giri, ciascuno approvato; poi si va avanti");
                else
                    sb.AppendLine($"  <color:{ErrorLine}>- - ></color> {Text(loop.Id)} ({which}): dopo un rifiuto si rifà " +
                                  (loop.Restart == WorkflowStart.Auto ? "da solo" : "quando la persona preme «Fai ripartire»") +
                                  (loop.Max == null ? ", finché non è approvato" : $", al massimo {loop.Max} {(loop.Max == 1 ? "volta" : "volte")}"));
            }
            sb.AppendLine("endlegend");
            sb.AppendLine("@enduml");
            // AppendLine scrive l'a capo del sistema: su Windows «\r\n». Il diagramma ha sempre «\n», così è identico
            // ovunque (stessa cache) e chi lo modifica dopo (il corpo del blocco dopo @startuml) trova ciò che cerca.
            return sb.ToString().Replace("\r\n", "\n");
        }

        /// <summary>
        /// Che cosa fa la persona che risponde dell'agente sul passo: lo lancia o lo avvia, e ne approva le consegne se non le
        /// approva un altro ruolo (<c>approval.by</c>). Null = niente: l'omino non si disegna. Il passo di una persona è già
        /// la persona.
        /// </summary>
        private static string PersonVerbs(WorkflowStep s)
        {
            if (s.IsPerson) return null;
            var verbs = new List<string>();
            if (s.Start == WorkflowStart.Manual) verbs.Add("lancia");
            if (s.Start == WorkflowStart.AskOwner) verbs.Add("avvia");
            if (s.Outputs.Count > 0 && s.ApprovalBy == null) verbs.Add("approva");
            return verbs.Count == 0 ? null : string.Join("\\ne ", verbs);
        }

        /// <summary>
        /// Il passo <paramref name="of"/> è finito e <paramref name="s"/> parte. Se <paramref name="s"/> ne riceve dei
        /// documenti, la freccia parte da ciascun documento (approvato); altrimenti dal passo.
        /// </summary>
        private static void Finished(WorkflowDescriptor wf, WorkflowStep s, string of, string target, StringBuilder sb, string brief)
        {
            var files = Received(wf, s).Where(r => r.Source.Id == of).ToList();
            if (files.Count == 0)
            {
                sb.AppendLine($"{Node(of)} --> {target} : {Done(wf, of)}{(brief == null ? "" : "\\n" + brief)}");
                return;
            }
            foreach (var (source, index) in files)
                sb.AppendLine($"{OutputNode(source.Id, index)} --> {target} : approvato{(brief == null ? "" : "\\n" + brief)}");
        }

        /// <summary>Le uscite di altri passi che il passo riceve con <c>from</c>, senza ripetizioni.</summary>
        private static List<(WorkflowStep Source, int Index)> Received(WorkflowDescriptor wf, WorkflowStep s)
        {
            var found = new List<(WorkflowStep, int)>();
            foreach (var reference in s.Inputs.Values.SelectMany(i => i.From))
            {
                var dot = reference.IndexOf('.');
                if (dot < 0) continue;
                var source = wf.Step(reference.Substring(0, dot));
                var index = source?.Outputs.FindIndex(o => o.Id == reference.Substring(dot + 1)) ?? -1;
                if (index >= 0 && !found.Contains((source, index))) found.Add((source, index));
            }
            return found;
        }

        /// <summary>
        /// Gli ingressi di testo dell'incarico, sulla freccia che fa partire il passo: il nome di quelli che vengono da una
        /// variabile del giro, il valore di quelli fissi. I documenti si vedono già come frecce dai file.
        /// </summary>
        private static string Assignment(WorkflowStep s)
        {
            var texts = s.Inputs.Where(i => i.Value.Text != null).ToList();
            if (texts.Count == 0) return null;
            var parts = texts.Select(i => WorkflowParser.Placeholder.Match(i.Value.Text) is { Success: true } m && m.Value == i.Value.Text.Trim()
                ? (m.Groups[1].Value == i.Key ? i.Key : $"{i.Key} = {m.Groups[1].Value}")
                : $"{i.Key} «{Short(i.Value.Text)}»");
            return (s.IsPerson ? "riceve " : "incarico: ") + string.Join(", ", parts);
        }

        private static string Label(string text) => text == null ? "" : " : " + text;

        private static bool IsJoin(WorkflowStep s) => s.Trigger?.Kind == WorkflowTriggerKind.After && s.Trigger.After.Count > 1;

        /// <summary>Quando è finito un passo: approvato se consegna documenti, fatto se è di una persona, finito se no.</summary>
        private static string Done(WorkflowDescriptor wf, string stepId)
        {
            var step = wf.Step(stepId);
            if (step == null) return "finito";
            if (step.IsPerson) return "fatto";
            return step.Outputs.Count > 0 ? "approvato" : "finito";
        }

        private static string StartText(WorkflowStart start) => start switch
        {
            WorkflowStart.Manual => "a mano",
            WorkflowStart.AskOwner => "lo avvia il responsabile",
            _ => "da solo",
        };

        private static string ActionText(string action) => action switch
        {
            "updates" => "aggiorna",
            "downloads" => "scarica",
            _ => "scrive",
        };

        /// <summary>Il nome di un documento: dalla cartella del workflow se ci sta dentro, altrimenti dalla radice.</summary>
        private static string Shorten(string path, string folder)
        {
            var p = (path ?? string.Empty).Replace('\\', '/').TrimStart('/');
            if (folder == null) return p.Substring(p.LastIndexOf('/') + 1);
            var f = folder.Replace('\\', '/').Trim('/');
            return f.Length > 0 && p.StartsWith(f + "/", StringComparison.Ordinal) ? p.Substring(f.Length + 1) : p;
        }

        private static string Note(string note) => string.IsNullOrWhiteSpace(note) ? "" : $"\\n<size:10><i>{Text(note)}</i></size>";

        private static string Short(string s)
        {
            var t = Text(s).Trim();
            return t.Length <= 40 ? t : t.Substring(0, 39) + "…";
        }

        private static string Id(string id) => id.Replace('-', '_');
        private static string Node(string id) => "s_" + Id(id);
        private static string Join(string id) => "j_" + Id(id);
        private static string Actor(string id) => "p_" + Id(id);
        private static string Approver(string id) => "pa_" + Id(id);
        private static string ReadNode(string id, int i) => $"r_{Id(id)}_{i}";
        private static string OutputNode(string id, int i) => $"f_{Id(id)}_{i}";
        private static string Button(string from, string reply) => $"b_{Id(from)}_{Id(reply)}";
        private static string Outcome(string step, string outcome) => $"o_{Id(step)}_{Id(outcome)}";
        private static string Link(string path) => string.IsNullOrWhiteSpace(path) ? "" : $" [[{LinkScheme}{path.Replace('\\', '/').TrimStart('/')}]]";

        /// <summary>Un testo dentro le virgolette di PlantUML: le virgolette diventano apici, gli a capo spazi.</summary>
        private static string Text(string s) => (s ?? string.Empty).Replace('"', '\'').Replace("\r", " ").Replace("\n", " ");
    }
}
