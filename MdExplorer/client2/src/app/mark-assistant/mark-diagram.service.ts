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
  }

  private setupIframeListener(): void {
    window.addEventListener('message', (event: MessageEvent) => {
      const data = event.data;
      if (!data || data.type !== 'mde-mark.askAboutBox') return;
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

  private explainBox(context: MarkDiagramContext, connectionId: string): Observable<{ started: boolean }> {
    return this.http.post<{ started: boolean }>(`${this.baseUrl}/explain-box`, {
      connectionId,
      context,
    });
  }
}
