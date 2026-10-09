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

        /// <summary>
        /// The point of a slide asked about (a title, an item, a paragraph, a cell, a box of a
        /// diagram): it is explained from the project's documents, not only from the deck.
        /// Null for a box of a diagram in a document.
        /// </summary>
        public MarkPoint? Point { get; set; }
    }

    /// <summary>A point of a slide, as the slide page reads it.</summary>
    public class MarkPoint
    {
        /// <summary>
        /// What Mark's dialog calls the point, chosen by the page; the events of the answer carry it
        /// back unchanged, so the dialog matches them to its question. Null: the first words of <see cref="Text"/>.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>The text of the point as the slide shows it (for a box, its name).</summary>
        public string? Text { get; set; }

        /// <summary>"text" or "box".</summary>
        public string? Kind { get; set; }

        /// <summary>The title of the slide holding the point, when it has one.</summary>
        public string? SlideTitle { get; set; }

        /// <summary>All the text of that slide, so the point is read in its place.</summary>
        public string? SlideText { get; set; }

        /// <summary>
        /// The project documents the point links to, project-relative, as the page reads its links. They are
        /// read whole: a point that is a link to another deck is explained through that deck.
        /// </summary>
        public List<string>? Links { get; set; }

        public bool IsBox => string.Equals(Kind, "box", System.StringComparison.OrdinalIgnoreCase);
    }

    public class MarkDiagramExplainRequest
    {
        public string? ConnectionId { get; set; }
        public MarkDiagramContextDto? Context { get; set; }
    }

    /// <summary>
    /// «Chiedi a MarkAgent» on a point of a slide: the prompts of the two phases. For the second,
    /// <see cref="KeywordsAnswer"/> is MarkAgent's answer to the first.
    /// </summary>
    public class MarkPointPromptRequest
    {
        public MarkDiagramContextDto? Context { get; set; }
        public string? Question { get; set; }
        public string? KeywordsAnswer { get; set; }
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
