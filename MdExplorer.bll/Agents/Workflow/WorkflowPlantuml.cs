using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Il diagramma di un workflow, generato e mai scritto a mano: stesso JSON, stesso PlantUML. Si legge dall'alto in basso
    /// nell'ordine in cui le cose succedono. I riquadri per agente non funzionano: un agente che lavora all'inizio e alla
    /// fine (l'account manager) tira la fine in alto e le frecce risalgono. L'agente sta dentro il passo, il colore dice chi
    /// lo avvia, i file sono gli artefatti; i link portano alla scheda dell'agente e all'artefatto.
    /// </summary>
    public static class WorkflowPlantuml
    {
        /// <summary>Prefisso dei link interni: il visualizzatore li apre in MdExplorer (percorso dalla radice del progetto).</summary>
        public const string LinkScheme = "mde:";

        // La palette della skill mde-plantuml: la tinta sopravvive al tema scuro, il significato sta nel colore.
        private const string Neutral = "#F1F3F4", NeutralLine = "#5F6368";
        private const string Attention = "#FEF7E0", AttentionLine = "#F29900";
        private const string Focus = "#E8F0FE", FocusLine = "#1A73E8";
        private const string ErrorLine = "#D93025";

        /// <param name="cardPath">Il percorso della scheda di un agente dalla radice del progetto, o null se non si trova.</param>
        /// <param name="artifactExists">Se un artefatto c'è già (percorso dalla radice). Null = non si sa: tutti come scritti.</param>
        public static string Render(WorkflowDescriptor wf, Func<string, string> cardPath, Func<string, bool> artifactExists = null)
        {
            if (wf == null) throw new ArgumentNullException(nameof(wf));
            cardPath ??= _ => null;
            artifactExists ??= _ => true;
            var anyMissing = false;
            var sb = new StringBuilder();
            sb.AppendLine("@startuml");
            sb.AppendLine("!theme plain");
            sb.AppendLine($"skinparam ArrowColor {NeutralLine}");
            sb.AppendLine($"skinparam ArrowFontColor {NeutralLine}");
            sb.AppendLine("skinparam rectangle {");
            sb.AppendLine($"  BackgroundColor {Neutral}");
            sb.AppendLine($"  BorderColor {NeutralLine}");
            sb.AppendLine("  RoundCorner 10");
            sb.AppendLine($"  BackgroundColor<<askowner>> {Attention}");
            sb.AppendLine($"  BorderColor<<askowner>> {AttentionLine}");
            sb.AppendLine($"  BackgroundColor<<manual>> {Focus}");
            sb.AppendLine($"  BorderColor<<manual>> {FocusLine}");
            sb.AppendLine("}");
            sb.AppendLine("skinparam file {");
            sb.AppendLine("  BackgroundColor #FFFFFF");
            sb.AppendLine("  BorderColor #9AA0A6");
            sb.AppendLine("  FontSize 11");
            sb.AppendLine("}");
            sb.AppendLine("skinparam hexagon {");
            sb.AppendLine($"  BackgroundColor {Neutral}");
            sb.AppendLine($"  BorderColor {NeutralLine}");
            sb.AppendLine("}");
            sb.AppendLine("hide stereotype");
            // Ogni passo ha un link: sottolineati tutti, i testi si leggono peggio e non dicono niente di più.
            sb.AppendLine("skinparam HyperlinkUnderline false");
            sb.AppendLine($"skinparam HyperlinkColor {NeutralLine}");
            sb.AppendLine();

            var steps = wf.Steps.Where(s => s.Id != null).ToList();
            if (steps.Any(s => s.Trigger?.Kind == WorkflowTriggerKind.Launch || s.Trigger?.Kind == WorkflowTriggerKind.Reply))
                sb.AppendLine("actor \"Persona\" as person");

            foreach (var s in steps)
            {
                var stereotype = s.Start switch { WorkflowStart.AskOwner => " <<askowner>>", WorkflowStart.Manual => " <<manual>>", _ => "" };
                var link = Link(cardPath(s.Agent));
                sb.AppendLine($"rectangle \"**{Text(s.Label)}**\\n<size:11>{Text(s.Agent)}</size>\\n<size:10><i>{StartText(s.Start)}</i></size>\"{stereotype} as {Node(s.Id)}{link}");
            }

            // L'attesa di più approvazioni è un punto del flusso, e si vede: un esagono prima del passo.
            foreach (var s in steps.Where(IsJoin))
                sb.AppendLine($"hexagon \"{(s.Trigger.Wait == WorkflowWait.All ? "attende tutti" : "basta il primo")}\" as {Join(s.Id)}");

            foreach (var s in steps)
                for (var i = 0; i < s.Produces.Count; i++)
                {
                    var produced = s.Produces[i];
                    var name = produced.Substring(produced.LastIndexOf('/') + 1);
                    // Un nome con «*» è una famiglia di file: niente link, non c'è un file solo da aprire. Un artefatto non
                    // ancora scritto ha il bordo tratteggiato e nessun link: aprirlo porterebbe a un documento che non c'è.
                    var family = produced.Contains('*');
                    var exists = family || artifactExists(produced);
                    anyMissing |= !exists;
                    var look = exists ? "" : " #FFFFFF;line.dashed;line:9AA0A6";
                    var link = family || !exists ? "" : Link(produced);
                    sb.AppendLine($"file \"{Text(name)}\" as {File(s.Id, i)}{look}{link}");
                }
            sb.AppendLine();

            foreach (var s in steps.Where(s => s.Trigger != null))
            {
                var t = s.Trigger;
                switch (t.Kind)
                {
                    case WorkflowTriggerKind.Launch:
                        sb.AppendLine($"person --> {Node(s.Id)} : lancia");
                        break;
                    case WorkflowTriggerKind.Reply:
                        sb.AppendLine($"{Node(t.FromStep)} --> {Node(s.Id)} : pulsante\\n«{Text(t.ReplyId)}»");
                        break;
                    // Un passo che produce un artefatto è finito quando è approvato; uno che non ne produce, quando ha finito.
                    case WorkflowTriggerKind.After when IsJoin(s):
                        foreach (var of in t.After)
                            sb.AppendLine($"{Node(of)} --> {Join(s.Id)} : {Done(wf, of)}");
                        sb.AppendLine($"{Join(s.Id)} --> {Node(s.Id)}");
                        break;
                    case WorkflowTriggerKind.After:
                        sb.AppendLine($"{Node(t.After.Single())} --> {Node(s.Id)} : {Done(wf, t.After.Single())}");
                        break;
                }
            }
            // I cicli si disegnano a destra del passo (PlantUML li mette sempre lì): il file di un passo con un ciclo va a
            // sinistra, altrimenti frecce ed etichette si accavallano. Ma non se il passo ha fratelli affiancati (che partono
            // dagli stessi passi): lì il file a sinistra finisce lontano e le frecce attraversano il diagramma.
            foreach (var s in steps)
            {
                var sources = s.Trigger?.Sources.OrderBy(x => x).ToList() ?? new List<string>();
                var hasSiblings = steps.Any(o => o != s && o.Trigger != null && o.Trigger.Sources.OrderBy(x => x).SequenceEqual(sources));
                var side = wf.Loops.Any(l => l.Steps.Contains(s.Id)) && !hasSiblings ? "left" : "right";
                for (var i = 0; i < s.Produces.Count; i++)
                    sb.AppendLine($"{Node(s.Id)} .{side}.> {File(s.Id, i)}");
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
            var starts = steps.Select(s => s.Start).Distinct().ToList();
            if (starts.Contains(WorkflowStart.Manual)) sb.AppendLine($"  <back:{Focus}>   </back> la persona lo lancia a mano");
            if (starts.Contains(WorkflowStart.AskOwner)) sb.AppendLine($"  <back:{Attention}>   </back> lo avvia il responsabile dell'agente");
            if (starts.Contains(WorkflowStart.Auto)) sb.AppendLine($"  <back:{Neutral}>   </back> parte da solo");
            if (anyMissing) sb.AppendLine("  <color:#9AA0A6>- - -</color> artefatto non ancora scritto");
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

        private static bool IsJoin(WorkflowStep s) => s.Trigger?.Kind == WorkflowTriggerKind.After && s.Trigger.After.Count > 1;

        /// <summary>Quando è finito un passo: approvato se produce un artefatto, finito se no.</summary>
        private static string Done(WorkflowDescriptor wf, string stepId) => (wf.Step(stepId)?.Produces.Count ?? 0) > 0 ? "approvato" : "finito";

        private static string StartText(WorkflowStart start) => start switch
        {
            WorkflowStart.Manual => "a mano",
            WorkflowStart.AskOwner => "lo avvia il responsabile",
            _ => "da solo",
        };

        private static string Node(string id) => "s_" + id.Replace('-', '_');
        private static string Join(string id) => "j_" + id.Replace('-', '_');
        private static string File(string id, int i) => $"f_{id.Replace('-', '_')}_{i}";
        private static string Link(string path) => string.IsNullOrWhiteSpace(path) ? "" : $" [[{LinkScheme}{path.Replace('\\', '/').TrimStart('/')}]]";

        /// <summary>Un testo dentro le virgolette di PlantUML: le virgolette diventano apici, gli a capo spazi.</summary>
        private static string Text(string s) => (s ?? string.Empty).Replace('"', '\'').Replace("\r", " ").Replace("\n", " ");
    }
}
