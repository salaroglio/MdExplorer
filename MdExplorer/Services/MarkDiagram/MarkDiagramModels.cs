using System.Collections.Generic;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// One relation of the selected box, as the PlantUML SVG declares it.
    /// </summary>
    public class MarkDiagramRelation
    {
        /// <summary>"outgoing" (this box → other) or "incoming" (other → this box).</summary>
        public string? Direction { get; set; }

        /// <summary>Qualified name of the entity at the other end.</summary>
        public string? Other { get; set; }

        /// <summary>
        /// UML relation type as declared by PlantUML: extension, composition,
        /// aggregation, association, dependency. <c>null</c> on legacy diagrams,
        /// which carry no <c>data-link-type</c> — never guessed.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>Arrow label, when the diagram has one.</summary>
        public string? Label { get; set; }

        public string? SourceLine { get; set; }
    }

    /// <summary>The box the user right-clicked.</summary>
    public class MarkDiagramBox
    {
        public string? Name { get; set; }
        /// <summary>"entity" or "cluster".</summary>
        public string? Kind { get; set; }
        public string? SourceLine { get; set; }
    }

    /// <summary>
    /// Everything the page knows about the selected box, collected client-side by
    /// <c>mark-diagram-context.js</c> straight from the SVG.
    /// </summary>
    public class MarkDiagramContextDto
    {
        public string? DocumentPath { get; set; }
        public string? ProjectPath { get; set; }
        public string? DiagramTitle { get; set; }
        public string? DiagramType { get; set; }

        /// <summary>"plantuml-2026" (relation types available) or "legacy" (they are not).</summary>
        public string? SvgFormat { get; set; }

        public MarkDiagramBox? Box { get; set; }
        public List<MarkDiagramRelation>? Relations { get; set; }

        /// <summary>Full PlantUML source, decoded from the SVG. Null when absent.</summary>
        public string? PlantumlSource { get; set; }
    }

    public class MarkDiagramExplainRequest
    {
        public string? ConnectionId { get; set; }
        public MarkDiagramContextDto? Context { get; set; }
    }

    public class MarkDiagramFollowUpRequest
    {
        public string? ConnectionId { get; set; }
        public string? Question { get; set; }
    }

    /// <summary>
    /// Una sostituzione puntuale nel testo del documento.
    /// <see cref="Find"/> deve comparire <b>esattamente una volta</b>: se compare zero volte
    /// o più di una, la modifica non è applicabile senza ambiguità e l'intera proposta viene
    /// rifiutata. Meglio un rifiuto che una sostituzione nel punto sbagliato.
    /// </summary>
    public class MarkDiagramTextEdit
    {
        public string? Find { get; set; }
        public string? Replace { get; set; }
        /// <summary>Perché questa modifica consegue dal cambiamento del diagramma.</summary>
        public string? Why { get; set; }
    }

    /// <summary>
    /// Quello che MarkAgent propone di cambiare. Nulla di tutto ciò viene scritto finché
    /// l'utente non conferma.
    /// </summary>
    public class MarkDiagramEditProposal
    {
        /// <summary>Cosa cambia, nelle solite dieci frasi al massimo.</summary>
        public string? Summary { get; set; }

        /// <summary>Il sorgente PlantUML nuovo, integrale. Null = il diagramma non cambia.</summary>
        public string? NewPlantuml { get; set; }

        public List<MarkDiagramTextEdit>? TextEdits { get; set; }

        /// <summary>
        /// Altri documenti del progetto che nominano le entità toccate (F6). Trovati da noi
        /// con la ricerca trigram, non chiesti al modello: è un fatto, non un'opinione.
        /// <b>Non vengono modificati</b> — servono solo ad avvisare.
        /// </summary>
        public List<string>? OtherDocuments { get; set; }
    }

    public class MarkDiagramApplyRequest
    {
        public string? ConnectionId { get; set; }
    }
}
