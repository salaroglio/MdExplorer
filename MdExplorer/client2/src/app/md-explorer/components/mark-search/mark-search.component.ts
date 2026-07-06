import { AfterViewChecked, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { marked } from 'marked';
import { AiChatService } from '../../../services/ai-chat.service';
import { SearchService } from '../../../services/search.service';
import { SearchResult } from '../../../models/search.models';
import { MarkSearchService, MarkSearchAnswerDocument } from '../../services/mark-search.service';
import { MdFileService } from '../../services/md-file.service';
import { ProjectsService } from '../../services/projects.service';

interface MarkResultBox {
  path: string;
  title: string;
  reason?: string;
  searchHint?: string;
}

interface MarkTurn {
  role: 'user' | 'assistant';
  text: string;
  keywords?: string[];
  results?: MarkResultBox[];
  document?: MarkSearchAnswerDocument;
  contextFiles?: string[];
  error?: string;
  raw?: string;
}

const RESULTS_MARKER = '===MDE-RESULTS===';
const DOCUMENT_MARKER = '===MDE-DOCUMENT===';
const END_MARKER = '===MDE-END===';

/**
 * "Mark Search" sidenav tab: a prompt-driven search/answer assistant.
 * Frontend-orchestrated protocol on a private AiChatHub channel:
 *   round 1 — the AI answers with a JSON keywords block;
 *   the component runs the instant search (/api/search/quick) for each keyword;
 *   round 2 — the AI gets the compacted results and answers with
 *   commentary + selected results (+ optional generated markdown document,
 *   persisted via MarkSearchService and opened in the viewer).
 * Format violations are surfaced as errors, never silently recovered.
 */
@Component({
  selector: 'app-mark-search',
  templateUrl: './mark-search.component.html',
  styleUrls: ['./mark-search.component.scss']
})
export class MarkSearchComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('transcript') transcriptEl: ElementRef<HTMLDivElement>;
  @ViewChild('promptInput') promptInput: ElementRef<HTMLTextAreaElement>;

  turns: MarkTurn[] = [];
  prompt = '';
  isBusy = false;
  phase: 'idle' | 'keywords' | 'searching' | 'answering' = 'idle';
  sendError: string | null = null;

  // Result files ticked by the user, to be injected as context with the NEXT prompt.
  // Once injected they live in the channel history for the whole conversation, so the
  // checkbox freezes (no double injection); "new search" resets everything.
  contextChecked = new Set<string>();
  contextInjected = new Set<string>();

  private round: 1 | 2 = 1;
  private streamBuffer = '';
  private currentTurn: MarkTurn | null = null;
  private firstMessageSent = false;
  private readonly channelId = 'mark-search';
  private shouldScroll = false;
  private destroy$ = new Subject<void>();

  constructor(
    private aiChatService: AiChatService,
    private searchService: SearchService,
    private markSearchService: MarkSearchService,
    private mdFileService: MdFileService,
    private projectsService: ProjectsService,
    private router: Router,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.aiChatService.getChannelStream$(this.channelId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        switch (event.type) {
          case 'chunk':
            this.streamBuffer += event.data;
            if (this.round === 2 && this.currentTurn) {
              // Live commentary: everything before the first protocol marker
              this.currentTurn.text = this.visibleCommentary(this.streamBuffer);
              this.shouldScroll = true;
            }
            break;
          case 'complete':
            if (!this.isBusy) {
              return;
            }
            if (this.round === 1) {
              this.handleKeywordsComplete();
            } else {
              this.handleAnswerComplete();
            }
            break;
          case 'error':
            this.fail(String(event.data));
            break;
        }
      });
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll && this.transcriptEl) {
      this.transcriptEl.nativeElement.scrollTop = this.transcriptEl.nativeElement.scrollHeight;
      this.shouldScroll = false;
    }
  }

  ngOnDestroy(): void {
    if (this.isBusy) {
      this.aiChatService.cancelPrompt();
    }
    if (this.firstMessageSent) {
      this.aiChatService.clearChannelHistory(this.channelId);
    }
    this.destroy$.next();
    this.destroy$.complete();
  }

  get statusLabel(): string {
    switch (this.phase) {
      case 'keywords': return 'MARK_SEARCH.PHASE_KEYWORDS';
      case 'searching': return 'MARK_SEARCH.PHASE_SEARCHING';
      case 'answering': return 'MARK_SEARCH.PHASE_ANSWERING';
      default: return '';
    }
  }

  toggleContext(result: MarkResultBox, event: Event): void {
    event.stopPropagation();
    const path = result.path;
    if (this.contextInjected.has(path)) {
      return;
    }
    if (this.contextChecked.has(path)) {
      this.contextChecked.delete(path);
    } else {
      this.contextChecked.add(path);
    }
  }

  isContextChecked(path: string): boolean {
    return this.contextChecked.has(path) || this.contextInjected.has(path);
  }

  isContextInjected(path: string): boolean {
    return this.contextInjected.has(path);
  }

  send(): void {
    const request = this.prompt.trim();
    if (!request || this.isBusy) {
      return;
    }
    this.sendError = null;

    const pendingPaths = Array.from(this.contextChecked);
    // The checked files are injected as context by the SERVER (it reads them fresh from
    // disk): the client ships only the paths, never the content — no oversized SignalR
    // payload. Empty list → plain send.
    this.dispatchPrompt(request, pendingPaths);
  }

  private dispatchPrompt(request: string, injectedNow: string[]): void {
    this.turns.push({ role: 'user', text: request, contextFiles: injectedNow.length ? injectedNow : undefined });
    this.currentTurn = { role: 'assistant', text: '' };
    this.turns.push(this.currentTurn);

    this.isBusy = true;
    this.phase = 'keywords';
    this.round = 1;
    this.streamBuffer = '';
    this.shouldScroll = true;

    const hasContext = injectedNow.length > 0;
    const message = this.firstMessageSent
      ? this.buildFollowUpKeywordsPrompt(request, hasContext)
      : this.buildFirstPrompt(request, hasContext);
    this.firstMessageSent = true;
    injectedNow.forEach(p => {
      this.contextChecked.delete(p);
      this.contextInjected.add(p);
    });
    this.aiChatService.sendMessageWithContextToChannel(message, this.channelId, injectedNow);
    this.prompt = '';
  }

  stop(): void {
    if (!this.isBusy) {
      return;
    }
    this.aiChatService.cancelPrompt();
    if (this.currentTurn && !this.currentTurn.text) {
      this.currentTurn.error = 'MARK_SEARCH.INTERRUPTED';
    }
    this.finish();
  }

  newSearch(): void {
    if (this.isBusy) {
      this.aiChatService.cancelPrompt();
    }
    if (this.firstMessageSent) {
      this.aiChatService.clearChannelHistory(this.channelId);
    }
    this.turns = [];
    this.currentTurn = null;
    this.firstMessageSent = false;
    this.contextChecked.clear();
    this.contextInjected.clear();
    this.sendError = null;
    this.finish();
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  openResult(result: MarkResultBox): void {
    const project = this.projectsService.currentProjects$.value;
    if (!project?.path) {
      return;
    }
    if (result.searchHint) {
      this.mdFileService.setPendingDocumentSearch(result.searchHint);
    }
    this.openInViewer(project.path, result.path, result.title);
  }

  private openInViewer(projectPath: string, rootRelativePath: string, name: string): void {
    const rel = '/' + this.normalizeRootRelative(rootRelativePath);
    const full = projectPath.replace(/\\/g, '/').replace(/\/+$/, '') + rel;
    const mdFile = {
      fullPath: full,
      relativePath: rel,
      path: rel,
      name: name || rel.substring(rel.lastIndexOf('/') + 1),
      type: 'mdFile'
    };
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(mdFile as any);
  }

  openDocument(doc: MarkSearchAnswerDocument): void {
    const mdFile = {
      fullPath: doc.fullPath,
      relativePath: doc.relativePath,
      path: doc.relativePath,
      name: doc.fileName,
      type: 'mdFile'
    };
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(mdFile as any);
  }

  renderMarkdown(content: string): SafeHtml {
    if (!content) {
      return '';
    }
    const html = marked.parse(content, { breaks: true }) as string;
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  // ---------------------------------------------------------------- protocol

  private handleKeywordsComplete(): void {
    const payload = this.extractJsonPayload(this.streamBuffer);
    if (payload === null) {
      this.fail('MARK_SEARCH.ERROR_KEYWORDS_FORMAT', this.streamBuffer);
      return;
    }
    let keywords: string[];
    try {
      const parsed = JSON.parse(payload);
      keywords = parsed?.keywords;
    } catch {
      this.fail('MARK_SEARCH.ERROR_KEYWORDS_FORMAT', this.streamBuffer);
      return;
    }
    if (!Array.isArray(keywords) || keywords.some(k => typeof k !== 'string')) {
      this.fail('MARK_SEARCH.ERROR_KEYWORDS_FORMAT', this.streamBuffer);
      return;
    }
    keywords = keywords.map(k => k.trim()).filter(k => k.length > 0).slice(0, 5);
    if (this.currentTurn) {
      this.currentTurn.keywords = keywords;
    }
    this.streamBuffer = '';
    this.shouldScroll = true;

    if (keywords.length === 0) {
      this.sendAnswerRound('Nessuna ricerca eseguita: la richiesta non la richiede. Usa il contesto della conversazione.');
      return;
    }

    this.phase = 'searching';
    forkJoin(keywords.map(k => this.searchService.quickSearch(k, 24)))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: results => {
          const compact = this.compactResults(keywords, results);
          this.sendAnswerRound(
            'Risultati della ricerca istantanea (path relativi alla radice del progetto):\n' +
            JSON.stringify(compact));
        },
        error: err => {
          this.fail(`Ricerca fallita: ${err?.error?.error || err?.message || err}`);
        }
      });
  }

  private sendAnswerRound(resultsText: string): void {
    this.phase = 'answering';
    this.round = 2;
    this.streamBuffer = '';
    this.aiChatService.sendMessageToChannel(this.buildAnswerPrompt(resultsText), this.channelId);
  }

  private handleAnswerComplete(): void {
    const raw = this.streamBuffer;
    const iResults = raw.indexOf(RESULTS_MARKER);
    const iEnd = raw.lastIndexOf(END_MARKER);
    if (iResults < 0 || iEnd < 0 || iEnd < iResults) {
      this.fail('MARK_SEARCH.ERROR_ANSWER_FORMAT', raw);
      return;
    }
    const commentary = raw.slice(0, iResults).trim();
    const iDoc = raw.indexOf(DOCUMENT_MARKER, iResults);
    const hasDoc = iDoc >= 0 && iDoc < iEnd;
    const resultsRaw = raw.slice(iResults + RESULTS_MARKER.length, hasDoc ? iDoc : iEnd);
    const documentContent = hasDoc ? raw.slice(iDoc + DOCUMENT_MARKER.length, iEnd).trim() : null;

    const payload = this.extractJsonPayload(resultsRaw);
    let results: MarkResultBox[];
    try {
      const parsed = JSON.parse(payload ?? '');
      if (!Array.isArray(parsed)) {
        throw new Error('not an array');
      }
      results = parsed
        .filter(r => r && typeof r.path === 'string' && r.path.trim().length > 0)
        .map(r => ({
          path: this.normalizeRootRelative(r.path),
          title: typeof r.title === 'string' && r.title.trim() ? r.title.trim() : r.path,
          reason: typeof r.reason === 'string' ? r.reason : undefined,
          searchHint: typeof r.searchHint === 'string' && r.searchHint.trim() ? r.searchHint.trim() : undefined
        }));
    } catch {
      this.fail('MARK_SEARCH.ERROR_ANSWER_FORMAT', raw);
      return;
    }

    if (this.currentTurn) {
      this.currentTurn.text = commentary;
      this.currentTurn.results = results;
    }
    this.shouldScroll = true;

    if (documentContent) {
      const turn = this.currentTurn;
      this.markSearchService.saveAnswer(documentContent)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: doc => {
            if (turn) {
              turn.document = doc;
            }
            this.finish();
            this.openDocument(doc);
          },
          error: err => {
            if (turn) {
              turn.error = `Salvataggio del documento fallito: ${err?.error?.error || err?.message || err}`;
            }
            this.finish();
          }
        });
    } else {
      this.finish();
    }
  }

  private fail(error: string, raw?: string): void {
    if (this.currentTurn) {
      this.currentTurn.error = error;
      this.currentTurn.raw = raw;
    }
    this.finish();
  }

  private finish(): void {
    this.isBusy = false;
    this.phase = 'idle';
    this.round = 1;
    this.streamBuffer = '';
    this.shouldScroll = true;
    setTimeout(() => this.promptInput?.nativeElement?.focus(), 100);
  }

  // ------------------------------------------------------------------ helpers

  private visibleCommentary(buffer: string): string {
    const i = buffer.indexOf('===MDE-');
    return i < 0 ? buffer : buffer.slice(0, i);
  }

  /**
   * The contract accepts a ```json fenced block or bare JSON: both are declared
   * valid in the instructions, so this is grammar tolerance, not a fallback.
   */
  private extractJsonPayload(text: string): string | null {
    const fence = /```(?:json)?\s*([\s\S]*?)```/i.exec(text);
    if (fence) {
      return fence[1].trim();
    }
    const trimmed = text.trim();
    if (trimmed.startsWith('{') || trimmed.startsWith('[')) {
      return trimmed;
    }
    return null;
  }

  private normalizeRootRelative(path: string): string {
    return path.replace(/\\/g, '/').replace(/^\/+/, '');
  }

  private toRootRelative(fullPath: string): string {
    const project = this.projectsService.currentProjects$.value;
    let normalized = (fullPath || '').replace(/\\/g, '/');
    if (project?.path) {
      const projectPath = project.path.replace(/\\/g, '/').replace(/\/+$/, '');
      if (normalized.toLowerCase().startsWith(projectPath.toLowerCase())) {
        normalized = normalized.substring(projectPath.length);
      }
    }
    return normalized.replace(/^\/+/, '');
  }

  private stripHtml(html: string): string {
    return (html || '').replace(/<[^>]+>/g, '');
  }

  private compactResults(keywords: string[], results: SearchResult[]): any {
    return {
      searches: keywords.map((keyword, i) => {
        const r = results[i];
        return {
          keyword,
          files: (r?.files || []).slice(0, 8).map(f => ({
            path: this.toRootRelative(f.path),
            name: f.fileName
          })),
          links: (r?.links || []).slice(0, 8).map(l => ({
            path: this.toRootRelative(l.fullPath),
            title: l.mdTitle || l.htmlTitle || '',
            context: this.stripHtml(l.mdContext || '').slice(0, 120)
          })),
          contents: (r?.contents || []).slice(0, 8).map(c => ({
            path: this.toRootRelative(c.path),
            name: c.fileName,
            snippet: this.stripHtml(c.snippet).slice(0, 200)
          }))
        };
      })
    };
  }

  // ------------------------------------------------------------------ prompts

  private buildFirstPrompt(request: string, hasContext: boolean): string {
    return [
      'Sei "Mark Search", l\'assistente di ricerca del progetto di documenti markdown aperto in MdExplorer.',
      'Lavori in due fasi. In questa FASE 1 devi SOLO scegliere le parole chiave per la ricerca istantanea',
      'del progetto (cerca per nome file, titoli dei link e contenuto full-text).',
      '',
      'Rispondi ESCLUSIVAMENTE con un blocco ```json contenente:',
      '{"keywords": ["parola1", "parola2"]}',
      '',
      'Regole:',
      '- da 1 a 5 keyword, brevi (1-2 parole), termini che plausibilmente compaiono nei documenti',
      '- niente operatori o virgolette: solo parole semplici',
      '- array vuoto [] SOLO se la richiesta non necessita di alcuna ricerca nei documenti',
      ...(hasContext ? ['  (ad esempio perché i file forniti sopra come contesto bastano a rispondere)'] : []),
      '- nessun testo fuori dal blocco JSON',
      '- non usare i tuoi tool: la ricerca la eseguo io per te',
      '',
      `Richiesta dell'utente: «${request}»`
    ].join('\n');
  }

  private buildFollowUpKeywordsPrompt(request: string, hasContext: boolean): string {
    return [
      `Nuova richiesta dell'utente: «${request}»`,
      '',
      'FASE 1 come in precedenza: rispondi ESCLUSIVAMENTE con il blocco ```json {"keywords": [...]};',
      ...(hasContext
        ? ['array vuoto [] se questa richiesta non necessita di una nuova ricerca (ad esempio perché', 'i file forniti sopra come contesto bastano a rispondere).']
        : ['array vuoto [] se questa richiesta non necessita di una nuova ricerca.'])
    ].join('\n');
  }

  private buildAnswerPrompt(resultsText: string): string {
    return [
      resultsText,
      '',
      'FASE 2 — Componi ORA la risposta finale in questo formato ESATTO:',
      '',
      '1. Un breve commento in markdown per l\'utente: cosa hai cercato, cosa hai trovato, con quale criterio hai selezionato.',
      `2. Una riga contenente esattamente: ${RESULTS_MARKER}`,
      '3. Un array JSON (anche vuoto []) dei soli risultati UTILI alla richiesta, ciascuno:',
      '   {"path": "path/relativo/file.md", "title": "Titolo leggibile", "reason": "perché è utile", "searchHint": "termine da evidenziare aprendo il documento (facoltativo)"}',
      `4. SOLO se la richiesta comporta un elaborato (riassunto, grafico, confronto, tabella): una riga esattamente ${DOCUMENT_MARKER}`,
      '   seguita dal documento markdown completo (inizia con un titolo #). Per i grafici usa blocchi ```plantuml.',
      '   I link ai file del progetto vanno scritti con path relativi alla radice del progetto, es. [Titolo](cartella/file.md).',
      `5. Una riga finale contenente esattamente: ${END_MARKER}`,
      '',
      'Regole: usa SOLO path presenti nei risultati o nei file di contesto forniti in questa conversazione',
      `(mai inventare file); se nulla è pertinente, spiegalo nel commento e restituisci []. Non aggiungere testo dopo ${END_MARKER}.`
    ].join('\n');
  }
}
