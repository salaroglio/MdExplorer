import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MarkAssistantService } from './mark-assistant.service';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';

/**
 * Context of one diagram box, collected in the markdown iframe by
 * `wwwroot/javascripts/jqueryForFirstPage/interactive-svg/mark-diagram-context.js`
 * straight from the PlantUML SVG. Mirrors MarkDiagramContextDto on the backend.
 */
export interface MarkDiagramRelation {
  direction: 'outgoing' | 'incoming';
  other: string;
  /** UML type, or null on legacy diagrams that do not declare one. Never guessed. */
  type: string | null;
  label: string | null;
  sourceLine: string | null;
}

export interface MarkDiagramContext {
  documentPath: string;
  projectPath: string;
  diagramTitle: string | null;
  diagramType: string | null;
  svgFormat: 'plantuml-2026' | 'legacy';
  box: { name: string; kind: string; sourceLine: string | null };
  relations: MarkDiagramRelation[];
  plantumlSource: string | null;
}

/**
 * A point of a slide ("Chiedi a MarkAgent" in a deck), collected by
 * `wwwroot/javascripts/slides/slide-edit.js`: explained from the project's documents.
 * Mirrors MarkDiagramContextDto.Point on the backend.
 * Sprint: docs-internal/Sprints/2026-09-25-Slide-Chiedi-A-MarkAgent.md
 */
export interface MarkPointContext {
  documentPath: string;
  projectPath: string;
  point: { label: string; text: string; kind: 'text'; slideTitle: string | null; slideText: string | null };
}

/**
 * "Ask to MarkAgent" — bridge between the markdown iframe and Mark's dialog.
 *
 * Listens for the `mde-mark.askAboutBox` postMessage the iframe fires when the
 * user picks the context-menu entry on a diagram box, then asks the backend to
 * explain it. The answer arrives over SignalR (`markDiagramExplain`) and is
 * rendered by MarkAssistantService.
 *
 * Instantiated in AppComponent — the listener must exist before the user opens
 * any document, the same way ExecutionService works for ▶ Run blocks.
 */
@Injectable({ providedIn: 'root' })
export class MarkDiagramService {
  private readonly baseUrl = '../api/markdiagram';

  constructor(
    private http: HttpClient,
    private mark: MarkAssistantService,
    private serverMessages: MdServerMessagesService,
  ) {
    this.setupIframeListener();
    // Si registra come inoltratore delle domande di seguito: è questo servizio a
    // conoscere MarkAssistantService, non il contrario — vedi il commento su
    // registerDiagramFollowUpSender.
    this.mark.registerDiagramFollowUpSender(q => this.askFollowUp(q));
    this.mark.registerDiagramEditActions(
      () => this.applyEdit(),
      () => this.discardEdit(),
    );
  }

  private setupIframeListener(): void {
    window.addEventListener('message', (event: MessageEvent) => {
      const data = event.data;
      if (!data) return;
      if (data.type === 'mde-mark.askAboutPoint' && data.context?.point?.label) {
        this.askAboutPoint(data.context as MarkPointContext);
        return;
      }
      if (data.type !== 'mde-mark.askAboutBox') return;
      if (!data.context?.box?.name) return;
      this.ask(data.context as MarkDiagramContext);
    });
  }

  /**
   * Asks MarkAgent about one box. The reply is not awaited here: it streams
   * back over SignalR into Mark's dialog.
   */
  ask(context: MarkDiagramContext): void {
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId) {
      // Without SignalR there is no channel for the answer. Say so in the
      // dialog rather than firing a request whose reply can never arrive.
      this.mark.showDiagramError(
        context.box.name,
        'Non sono connesso al servizio: riapri il documento e riprova.',
      );
      return;
    }

    this.mark.beginDiagramExplanation(context);

    this.explainBox(context, connectionId).subscribe({
      error: (err) => {
        console.warn('[MarkDiagram] explain-box request failed', err);
        this.mark.showDiagramError(
          context.box.name,
          err?.error || 'Non sono riuscito ad avviare la spiegazione.',
        );
      },
    });
  }

  /**
   * Asks MarkAgent about a point of a slide. Same dialog, same stream as a box: the point's
   * label plays the box name's part (the backend sends it back on every event).
   */
  askAboutPoint(context: MarkPointContext): void {
    const label = context.point.label;
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId) {
      this.mark.showDiagramError(label, 'Non sono connesso al servizio: riapri la presentazione e riprova.');
      return;
    }

    this.mark.beginDiagramExplanation({ documentPath: context.documentPath, box: { name: label } }, 'point');

    this.http.post<{ started: boolean }>(`${this.baseUrl}/explain-point`, { connectionId, context }).subscribe({
      error: (err) => {
        console.warn('[MarkDiagram] explain-point request failed', err);
        this.mark.showDiagramError(label, err?.error || 'Non sono riuscito ad avviare la spiegazione.');
      },
    });
  }

  /**
   * Domanda di seguito sullo stesso box. La risposta arriva sullo stesso canale
   * SignalR della spiegazione, quindi qui non si aspetta nulla.
   */
  askFollowUp(question: string): void {
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId) {
      this.mark.showDiagramError('', 'Non sono connesso al servizio: riapri il documento e riprova.');
      return;
    }

    this.mark.beginDiagramFollowUp();

    this.http.post(`${this.baseUrl}/follow-up`, { connectionId, question }).subscribe({
      error: (err) => {
        // 409 = nessuna conversazione aperta. Dirlo è più utile che tacere:
        // significa che l'utente ha scritto senza aver prima chiesto di un box.
        const message = err?.status === 409
          ? 'Non stiamo parlando di nessun box né di nessun punto di una slide: fai prima tasto destro su un elemento.'
          : (err?.error?.message || 'Non sono riuscito a inoltrare la domanda.');
        this.mark.showDiagramError('', message);
      },
    });
  }

  /** Conferma la modifica proposta: il contenuto è già sul server. */
  applyEdit(): void {
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId) return;
    this.http.post(`${this.baseUrl}/apply-edit`, { connectionId }).subscribe({
      error: (err) => this.mark.showDiagramError('',
        err?.error?.message || 'Non sono riuscito ad applicare la modifica.'),
    });
  }

  /** Butta via la modifica proposta. */
  discardEdit(): void {
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId) return;
    this.http.post(`${this.baseUrl}/discard-edit`, { connectionId }).subscribe({
      error: () => { /* niente da applicare: nulla di grave */ },
    });
  }

  private explainBox(context: MarkDiagramContext, connectionId: string): Observable<{ started: boolean }> {
    return this.http.post<{ started: boolean }>(`${this.baseUrl}/explain-box`, {
      connectionId,
      context,
    });
  }
}
