import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { MarkAssistantService } from './mark-assistant.service';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { AiChatService } from '../services/ai-chat.service';

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
  /** True when the diagram is on a slide: the box is then explained from the project's documents. */
  slideDeck?: boolean;
}

/**
 * A point of a slide ("Chiedi a MarkAgent" in a deck), collected by
 * `wwwroot/javascripts/slides/slide-edit.js` — or a box of a diagram on a slide, then with the
 * diagram's fields too. Explained from the project's documents. Mirrors MarkDiagramContextDto.
 * Sprint: docs-internal/Sprints/2026-09-25-Slide-Chiedi-A-MarkAgent.md
 */
export interface MarkPointContext extends Partial<Omit<MarkDiagramContext, 'documentPath' | 'projectPath'>> {
  documentPath: string;
  projectPath: string;
  point: { label: string; text: string; kind: 'text' | 'box'; slideTitle: string | null; slideText: string | null };
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
    private aiChat: AiChatService,
  ) {
    this.setupIframeListener();
    // Si registra come inoltratore delle domande di seguito: è questo servizio a
    // conoscere MarkAssistantService, non il contrario — vedi il commento su
    // registerDiagramFollowUpSender.
    this.mark.registerDiagramFollowUpSender(q =>
      this.pointContext ? this.runPoint(this.pointContext, q) : this.askFollowUp(q));
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
      if (data.context.slideDeck) {
        // A box on a slide is a point of the slide: explained from the project's documents,
        // with the diagram's context too (sprint D3).
        const box = data.context as MarkDiagramContext;
        this.askAboutPoint({
          ...box,
          point: { label: box.box.name, text: box.box.name, kind: 'box', slideTitle: null, slideText: null },
        });
        return;
      }
      this.ask(data.context as MarkDiagramContext);
    });
  }

  /**
   * Asks MarkAgent about one box. The reply is not awaited here: it streams
   * back over SignalR into Mark's dialog.
   */
  ask(context: MarkDiagramContext): void {
    this.pointContext = null;
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

  // ──────────────────────────────────────────────────────────────────────
  //  "Chiedi a MarkAgent" on a point of a slide
  // ──────────────────────────────────────────────────────────────────────

  /** The point being talked about: follow-up questions go to it. Null while talking about a box of a document. */
  private pointContext: MarkPointContext | null = null;
  /** Each run has its own channel: the events of a superseded run cannot land on the next one. */
  private pointRun = 0;

  /**
   * Asks MarkAgent about a point of a slide, as Mark Search asks: two turns on a channel of the
   * AI chat — the same CLI session as the MarkAgent tab, so each knows what happened in the other
   * (user's decision, 25/09/2026). The server builds the prompts and runs the search in between.
   */
  askAboutPoint(context: MarkPointContext): void {
    this.pointContext = context;
    const cached = this.mark.beginDiagramExplanation(
      { documentPath: context.documentPath, box: { name: context.point.label } }, 'point');
    if (cached) return;
    this.runPoint(context, null);
  }

  private async runPoint(context: MarkPointContext, question: string | null): Promise<void> {
    const run = ++this.pointRun;
    const channelId = `mark-point-${run}`;
    const box = context.point.label;
    const emit = (evt: any) => { if (run === this.pointRun) this.mark.emitDiagramEvent({ box, ...evt }); };

    if (question !== null) this.mark.beginDiagramFollowUp(false);
    emit({ phase: 'start' });
    try {
      emit({ phase: 'status', message: 'Chiedo a MarkAgent cosa cercare nel progetto...' });
      const first = await firstValueFrom(this.http.post<{ prompt: string }>(
        `${this.baseUrl}/point/keywords-prompt`, { context, question }));
      const keywordsAnswer = await this.converse(channelId, first.prompt, null);
      if (run !== this.pointRun) return;

      const second = await firstValueFrom(this.http.post<{ prompt: string; keywords: string[]; sources: string[] }>(
        `${this.baseUrl}/point/answer-prompt`, { context, question, keywordsAnswer }));
      emit({
        phase: 'status',
        message: second.keywords.length
          ? `Ho cercato ${second.keywords.map(k => `«${k}»`).join(', ')}: ${second.sources.length} documenti. Chiedo a MarkAgent di spiegare...`
          : 'MarkAgent non ha indicato parole da cercare: spiega con la sola presentazione...',
      });

      const answer = await this.converse(channelId, second.prompt, chunk => emit({ phase: 'chunk', text: chunk }));
      emit({ phase: 'done', text: answer, keywords: second.keywords, sources: second.sources, followUp: question !== null });
    } catch (err: any) {
      console.warn('[MarkDiagram] point explanation failed', err);
      emit({ phase: 'error', message: err?.error?.message || err?.error || err?.message || 'Non sono riuscito a spiegare questo punto.' });
    } finally {
      this.aiChat.clearChannelHistory(channelId);
    }
  }

  /** One turn on the channel: resolves with the whole answer, or rejects with the chat's error. */
  private converse(channelId: string, prompt: string, onChunk: ((chunk: string) => void) | null): Promise<string> {
    return new Promise<string>((resolve, reject) => {
      let text = '';
      const sub = this.aiChat.getChannelStream$(channelId).subscribe(evt => {
        switch (evt.type) {
          case 'chunk':
            text += evt.data ?? '';
            onChunk?.(evt.data ?? '');
            break;
          case 'complete':
            sub.unsubscribe();
            resolve(text);
            break;
          case 'error':
            sub.unsubscribe();
            reject(new Error(String(evt.data)));
            break;
        }
      });
      this.aiChat.sendMessageToChannel(prompt, channelId);
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
