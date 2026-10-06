using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Il compendio delle dinamiche tra persone e agenti (<c>*.workflow.json</c>, standard v1): chi incarica chi, chi avvia,
    /// chi aspetta chi, i cicli. È la regola che MDE applica; il documento <c>mde_type: workflow</c> lo mostra come diagramma.
    /// La scheda di un agente dice come fa il suo lavoro, il workflow come il lavoro passa di mano.
    /// </summary>
    public class WorkflowDescriptor
    {
        public int Version { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<WorkflowStep> Steps { get; } = new();
        public List<WorkflowLoop> Loops { get; } = new();

        public WorkflowStep Step(string id) => Steps.FirstOrDefault(s => s.Id == id);
    }

    /// <summary>Un turno di lavoro di un agente: cosa lo fa partire, chi lo avvia, cosa produce.</summary>
    public class WorkflowStep
    {
        public string Id { get; set; }
        public string Agent { get; set; }
        /// <summary>L'etichetta nel diagramma; senza, l'id.</summary>
        public string Title { get; set; }
        public WorkflowTrigger Trigger { get; set; }
        public WorkflowStart Start { get; set; }
        /// <summary>I percorsi degli artefatti, dalla radice del progetto. «*» nel nome del file è ammesso.</summary>
        public List<string> Produces { get; } = new();

        public string Label => string.IsNullOrWhiteSpace(Title) ? Id : Title;
    }

    public enum WorkflowStart
    {
        /// <summary>La persona lancia l'agente a mano.</summary>
        Manual,
        /// <summary>Arriva al responsabile dell'agente, che lo avvia da una schermata di lancio.</summary>
        AskOwner,
        /// <summary>Parte da solo.</summary>
        Auto,
    }

    public enum WorkflowTriggerKind
    {
        /// <summary>La persona lancia l'agente.</summary>
        Launch,
        /// <summary>La persona preme un pulsante sotto il messaggio di un passo.</summary>
        Reply,
        /// <summary>Un [INCARICO] mandato dall'agente di un passo.</summary>
        Assignment,
        /// <summary>L'[APPROVATO] degli artefatti di uno o più passi.</summary>
        Approval,
    }

    public enum WorkflowWait
    {
        All,
        Any,
    }

    public class WorkflowTrigger
    {
        public WorkflowTriggerKind Kind { get; set; }
        /// <summary><see cref="WorkflowTriggerKind.Reply"/>: l'id della risposta dichiarata nella scheda.</summary>
        public string ReplyId { get; set; }
        /// <summary>
        /// Il passo da cui arriva: il messaggio a cui si risponde (<c>reply</c>) o chi manda l'incarico
        /// (<c>assignment</c>). Null per <c>launch</c> e <c>approval</c>.
        /// </summary>
        public string FromStep { get; set; }
        /// <summary><see cref="WorkflowTriggerKind.Approval"/>: i passi i cui artefatti si aspettano.</summary>
        public List<string> ApprovalOf { get; } = new();
        public WorkflowWait Wait { get; set; } = WorkflowWait.All;

        /// <summary>I passi da cui questo passo dipende: gli archi entranti del grafo.</summary>
        public IEnumerable<string> Sources =>
            Kind == WorkflowTriggerKind.Approval ? ApprovalOf
            : FromStep != null ? new[] { FromStep }
            : Enumerable.Empty<string>();
    }

    /// <summary>Un ciclo dichiarato. Nella v1 l'unico è il rifacimento dopo un rifiuto, riavviato dalla persona.</summary>
    public class WorkflowLoop
    {
        public string Id { get; set; }
        public List<string> Steps { get; } = new();
        /// <summary>v1: sempre «rejected».</summary>
        public string On { get; set; }
        /// <summary>v1: sempre «manual» (la persona preme «Fai ripartire»).</summary>
        public string Restart { get; set; }
        public int Max { get; set; }
        /// <summary>v1: sempre «stop».</summary>
        public string Then { get; set; }
    }

    public enum WorkflowSeverity
    {
        Error,
        Warning,
    }

    /// <summary>Un problema del workflow: dove (percorso JSON), cosa, e come si corregge quando lo si sa.</summary>
    public class WorkflowIssue
    {
        public WorkflowSeverity Severity { get; set; }
        /// <summary>Il percorso nel JSON, per esempio <c>steps[2].trigger</c>; vuoto = il file intero.</summary>
        public string Path { get; set; }
        public string Message { get; set; }
        public string Fix { get; set; }

        public override string ToString() =>
            $"{(Severity == WorkflowSeverity.Error ? "errore" : "avviso")} {(string.IsNullOrEmpty(Path) ? "(file)" : Path)}: {Message}" +
            (string.IsNullOrEmpty(Fix) ? "" : $" — {Fix}");
    }

    public class WorkflowCheckResult
    {
        /// <summary>Ciò che si è potuto leggere. Null se il file non è nemmeno un oggetto JSON.</summary>
        public WorkflowDescriptor Descriptor { get; set; }
        public List<WorkflowIssue> Issues { get; } = new();
        public bool IsValid => Issues.All(i => i.Severity != WorkflowSeverity.Error);
    }
}
