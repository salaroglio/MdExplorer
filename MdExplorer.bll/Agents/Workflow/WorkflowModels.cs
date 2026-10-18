using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Il compendio delle dinamiche tra persone e agenti (<c>*.workflow.json</c>, standard v3): il <b>vocabolario comune</b>
    /// con cui un LLM, tramite la skill <c>mde-workflow</c>, pianifica il lavoro, e che MDE esegue con regole fisse. Dice
    /// quali passi ci sono, che cosa fa partire ciascuno, chi lo avvia, che cosa riceve l'agente, i cicli. La scheda di un
    /// agente dice come fa il suo passo; il workflow come il lavoro passa di mano.
    /// </summary>
    public class WorkflowDescriptor
    {
        public int Version { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        /// <summary>Le variabili del giro (nome → che cos'è), riempite da un pulsante della persona e usate nei <c>brief</c>.</summary>
        public Dictionary<string, string> Variables { get; } = new();
        public List<WorkflowStep> Steps { get; } = new();
        public List<WorkflowLoop> Loops { get; } = new();
        /// <summary>
        /// Le schede unite ai passi (<see cref="WorkflowBinder"/>): ciò che un giro salva accanto al workflow. Null finché
        /// il workflow non è unito.
        /// </summary>
        public WorkflowCardSet Cards { get; set; }

        public WorkflowStep Step(string id) => Steps.FirstOrDefault(s => s.Id == id);

        /// <summary>
        /// Il ciclo di un tipo dichiarato per un passo, se c'è. Un passo sta al più in un ciclo per tipo: con tutti e due,
        /// il «fino a che» vale per ogni giro del «for» (tre giri voluti, ciascuno rifatto finché non è approvato).
        /// </summary>
        public WorkflowLoop LoopOf(string stepId, WorkflowLoopKind kind) =>
            Loops.FirstOrDefault(l => l.Kind == kind && l.Steps.Contains(stepId));
    }

    /// <summary>
    /// Un passo: il turno di lavoro di un agente (<see cref="Agent"/> + <see cref="Skill"/>) oppure il compito di una persona
    /// (<see cref="Person"/>, un ruolo di <c>responsabilita.md</c>). Che cosa lo fa partire, che cosa riceve, che cosa produce.
    /// </summary>
    public class WorkflowStep
    {
        public string Id { get; set; }
        /// <summary>Passo di un agente: il suo nome. Null per il passo di una persona.</summary>
        public string Agent { get; set; }
        /// <summary>Passo di un agente: il lavoro della scheda che fa (<c>a2a.skills[].id</c>).</summary>
        public string Skill { get; set; }
        /// <summary>Passo di una persona: il ruolo che lo fa. Null per il passo di un agente.</summary>
        public string Person { get; set; }
        /// <summary>L'etichetta nel diagramma; senza, l'id.</summary>
        public string Title { get; set; }
        public WorkflowTrigger Trigger { get; set; }
        /// <summary>Passo di un agente: chi lo avvia. Il passo di una persona aspetta lei, non si avvia.</summary>
        public WorkflowStart Start { get; set; }
        /// <summary>
        /// Passo di un agente, facoltativo: ciò che l'incarico aggiunge alla descrizione del lavoro e ai suoi ingressi, con
        /// le variabili del giro (<c>{codice}</c>).
        /// </summary>
        public string Brief { get; set; }
        /// <summary>Passo di una persona: che cosa le si chiede, con le variabili del giro.</summary>
        public string Task { get; set; }
        /// <summary>Per ogni ingresso: un testo con le variabili, o le uscite di passi da cui questo dipende.</summary>
        public Dictionary<string, WorkflowInput> Inputs { get; } = new(StringComparer.Ordinal);
        /// <summary>Passo di una persona: i pulsanti con cui lo chiude. Un passo dopo può partire da un esito.</summary>
        public List<WorkflowOutcome> Outcomes { get; } = new();
        /// <summary>Passo di un agente: il ruolo che approva al posto del responsabile dell'agente. Null = il responsabile.</summary>
        public string ApprovalBy { get; set; }
        /// <summary>
        /// Le uscite del passo. Di un agente: quelle della sua skill, unite dalla scheda. Di una persona: i documenti che
        /// dichiara di scrivere.
        /// </summary>
        public List<WorkflowOutput> Outputs { get; } = new();
        /// <summary>Passo di un agente: i documenti fissi che la sua skill legge, uniti dalla scheda.</summary>
        public List<WorkflowRead> Reads { get; } = new();
        /// <summary>I percorsi delle uscite, dalla radice del progetto. «*» nel nome del file è ammesso.</summary>
        public List<string> Produces { get; } = new();

        public bool IsPerson => Person != null;
        public string Label => string.IsNullOrWhiteSpace(Title) ? Id : Title;
    }

    /// <summary>Il valore di un ingresso: un testo (con le variabili del giro) oppure le uscite di altri passi.</summary>
    public class WorkflowInput
    {
        /// <summary>Un testo fisso, con eventuali <c>{variabili}</c>. Null se l'ingresso viene da uscite.</summary>
        public string Text { get; set; }
        /// <summary>Le uscite che l'ingresso riceve, come <c>passo.uscita</c>. Vuota se l'ingresso è un testo.</summary>
        public List<string> From { get; } = new();
    }

    /// <summary>Un esito con cui una persona chiude il suo passo: un pulsante.</summary>
    public class WorkflowOutcome
    {
        public string Id { get; set; }
        public string Label { get; set; }
        /// <summary>I valori che l'esito porta: diventano variabili del giro.</summary>
        public List<string> Values { get; } = new();
    }

    /// <summary>Un documento che un passo consegna.</summary>
    public class WorkflowOutput
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string Note { get; set; }
        /// <summary><c>writes</c>, <c>updates</c> o <c>downloads</c>.</summary>
        public string Action { get; set; } = "writes";
    }

    /// <summary>Un documento fisso che un passo legge; <see cref="Section"/> = il titolo del capitolo, se uno solo.</summary>
    public class WorkflowRead
    {
        public string Path { get; set; }
        public string Section { get; set; }
        public string Note { get; set; }
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
        /// <summary>Uno o più passi sono finiti (artefatto approvato, o concluso se non ne produce).</summary>
        After,
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
        /// <summary><see cref="WorkflowTriggerKind.Reply"/>: il passo sotto il cui messaggio sta il pulsante.</summary>
        public string FromStep { get; set; }
        /// <summary><see cref="WorkflowTriggerKind.After"/>: i passi che devono essere finiti.</summary>
        public List<string> After { get; } = new();
        public WorkflowWait Wait { get; set; } = WorkflowWait.All;

        /// <summary>I passi da cui questo passo dipende: gli archi entranti del grafo.</summary>
        public IEnumerable<string> Sources =>
            Kind == WorkflowTriggerKind.After ? After
            : FromStep != null ? new[] { FromStep }
            : Enumerable.Empty<string>();
    }

    public enum WorkflowLoopKind
    {
        /// <summary>«Fino a che»: dopo un rifiuto il passo si rifà, finché non è approvato (al massimo <c>max</c> volte, se c'è).</summary>
        UntilApproved,
        /// <summary>«For»: il passo si fa <c>times</c> giri, ciascuno approvato; i successori partono dopo l'ultimo.</summary>
        Times,
    }

    /// <summary>Un ciclo dichiarato dall'autore del workflow. Senza un ciclo, MDE non impone limiti.</summary>
    public class WorkflowLoop
    {
        public string Id { get; set; }
        public List<string> Steps { get; } = new();
        public WorkflowLoopKind Kind { get; set; }
        /// <summary><see cref="WorkflowLoopKind.UntilApproved"/>: quante volte al massimo si rifà. Null = nessun limite.</summary>
        public int? Max { get; set; }
        /// <summary><see cref="WorkflowLoopKind.UntilApproved"/>: dopo un rifiuto riparte da solo, o lo fa ripartire la persona.</summary>
        public WorkflowStart Restart { get; set; } = WorkflowStart.Manual;
        /// <summary><see cref="WorkflowLoopKind.Times"/>: quanti giri.</summary>
        public int Times { get; set; }
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
