import { AgentStartGuardService } from '../../services/agent-start-guard.service';
import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import {
  MatLegacyDialog as MatDialog,
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { EMPTY, forkJoin, of, Subscription } from 'rxjs';
import { catchError, finalize, switchMap } from 'rxjs/operators';

import { MailboxMessage, MailboxService, MailReply, AwaitedWork } from '../../services/mailbox.service';
import { AgentReviewService, MailArtifact, MergeRequest } from '../../services/agent-review.service';
import { FederationRequest, FederationService } from '../../services/federation.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { ThemeService } from '../../../services/theme.service';
import { MailboxDialogComponent } from '../mailbox-dialog/mailbox-dialog.component';

export interface AgentMailData {
  projectPath: string;
}

/** Una riga dell'elenco: un messaggio di un agente, un lavoro da approvare, o la richiesta di un collega. */
export interface MailItem {
  kind: 'message' | 'review' | 'federation' | 'awaited';
  id: string;
  when: string;
  from: string;
  preview: string;
  unread: boolean;
  message?: MailboxMessage;
  request?: MergeRequest;
  federation?: FederationRequest;
  /**
   * La voce sta sotto un'altra: è la richiesta di approvazione dell'artefatto, messa sotto il messaggio che
   * l'agente ha scritto nello stesso turno di lavoro. Il legame è l'identificativo del turno, non l'ora.
   */
  child?: boolean;
  /** Per un messaggio: ha sotto di sé qualcosa da decidere. */
  hasPending?: boolean;
  /** Una riga di stato: un lavoro chiesto a un altro agente, per chi lo aspetta. Non si seleziona. */
  awaited?: AwaitedWork;
  /** Lo stato è cambiato dall'ultima lettura: la riga si accende per qualche secondo. */
  changed?: boolean;
}

/** Il documento aperto nel riquadro di destra, al posto del dettaglio. */
interface OpenDocument {
  path: string;
  url: SafeResourceUrl;
  /** Dalla consegna dell'agente (non ancora nel progetto) oppure dal progetto. */
  pending: boolean;
}

/**
 * La posta degli agenti, a tutto schermo: a sinistra l'elenco (messaggi, lavori da approvare, richieste
 * dei colleghi), a destra il dettaglio con gli artefatti che il messaggio cita. Un artefatto si apre qui
 * dentro: dalla consegna dell'agente finché non è approvato, dal progetto dopo.
 */
@Component({
  selector: 'app-agent-mail',
  templateUrl: './agent-mail.component.html',
  styleUrls: ['./agent-mail.component.scss'],
})
export class AgentMailComponent implements OnInit, OnDestroy {
  items: MailItem[] = [];
  selected: MailItem | null = null;
  loading = false;
  error: string | null = null;
  /** Si guarda l'archivio invece della posta. */
  showArchive = false;
  unread = 0;
  /** Quanti messaggi ci sono nell'elenco: ciò che «Archivia tutti» toglierebbe. */
  messageCount = 0;

  /** Gli artefatti del messaggio selezionato: i percorsi che cita, e dove stanno. */
  artifacts: MailArtifact[] = [];
  document: OpenDocument | null = null;

  replyDraft = '';
  sending = false;
  deciding = false;

  private subs = new Subscription();

  constructor(
    private startGuard: AgentStartGuardService,
    public dialogRef: MatDialogRef<AgentMailComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentMailData,
    private mailbox: MailboxService,
    private review: AgentReviewService,
    private federation: FederationService,
    private serverMessages: MdServerMessagesService,
    private themeService: ThemeService,
    private sanitizer: DomSanitizer,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.reload();
    // Arriva posta, un agente consegna, un collega chiede: l'elenco si aggiorna da solo.
    this.subs.add(this.serverMessages.agentMessageReceived$.subscribe(() => this.reload()));
    this.subs.add(this.serverMessages.agentMergeRequested$.subscribe(() => this.reload()));
    this.subs.add(this.serverMessages.federationRequestReceived$.subscribe(() => this.reload()));
  }

  ngOnDestroy(): void {
    if (this.awaitedTimer) clearInterval(this.awaitedTimer);
    this.subs.unsubscribe();
  }

  get projectName(): string {
    return (this.data?.projectPath || '').replace(/[\\/]+$/, '').split(/[\\/]/).pop() || '';
  }

  reload(): void {
    const projectPath = this.data?.projectPath || '';
    this.loading = true;
    this.error = null;
    forkJoin({
      // In posta resta tutto ciò che non è archiviato, letto o no: «letto» toglie solo il pallino.
      inbox: this.mailbox.inbox(projectPath, true, this.showArchive),
      // Le altre due fonti non devono spegnere la posta se non rispondono: lo si dice e si va avanti.
      reviews: this.review.pending(projectPath).pipe(catchError(() => of({ requests: [] as MergeRequest[] }))),
      federation: this.federation.requests(projectPath).pipe(catchError(() => of({ requests: [] as FederationRequest[] }))),
    }).subscribe({
      next: ({ inbox, reviews, federation }) => {
        this.unread = inbox.unread || 0;
        this.messageCount = (inbox.messages || []).length;
        // L'archivio contiene solo messaggi: ciò che è da decidere non si archivia.
        this.items = this.arrange(
          (inbox.messages || []).map(m => this.fromMessage(m)),
          this.showArchive ? [] : (reviews.requests || []).map(r => this.fromReview(r)),
          this.showArchive ? [] : (federation.requests || []).filter(f => f.status === 'pending').map(f => this.fromFederation(f)));
        this.loading = false;

        // La selezione resta sulla stessa voce; se è sparita (letta, approvata), si passa alla prima.
        const kept = this.selected && this.items.find(i => i.kind === this.selected.kind && i.id === this.selected.id);
        if (kept) { this.selected = kept; this.loadArtifacts(); }
        else if (this.items.length) this.select(this.items[0]);
        else { this.selected = null; this.artifacts = []; this.document = null; }
      },
      error: (err) => {
        this.error = err?.error?.error || err?.message || this.translate.instant('AGENT_MAIL.LOAD_ERROR');
        this.loading = false;
      },
    });
  }

  /**
   * L'ordine dell'elenco. Un turno di lavoro lascia due cose — il messaggio dell'agente e la richiesta di
   * approvazione del suo artefatto — e sono una cosa sola per chi legge: la richiesta va SOTTO il messaggio
   * dello stesso turno (stesso `runId`). Una richiesta senza messaggio in elenco (archiviato, o un turno che
   * non ha scritto) resta in prima fila: quello che è da decidere non deve mai sparire.
   */
  private arrange(messages: MailItem[], reviews: MailItem[], federation: MailItem[]): MailItem[] {
    const newestFirst = (a: MailItem, b: MailItem) => (b.when || '').localeCompare(a.when || '');

    // Più messaggi nello stesso turno: la richiesta va sotto l'ultimo, che è quello che chiude il lavoro.
    const parentOf = new Map<string, MailItem>();
    for (const m of [...messages].sort(newestFirst)) {
      const run = m.message?.runId;
      if (run && !parentOf.has(run)) parentOf.set(run, m);
    }

    const under = new Map<MailItem, MailItem[]>();
    const alone: MailItem[] = [];
    for (const r of reviews) {
      const parent = r.request?.runId ? parentOf.get(r.request.runId) : undefined;
      if (!parent) { alone.push(r); continue; }
      r.child = true;
      parent.hasPending = true;
      under.set(parent, [...(under.get(parent) || []), r]);
    }

    const ordered: MailItem[] = [];
    const seen = new Map<string, string>();
    for (const top of [...messages, ...alone, ...federation].sort(newestFirst)) {
      ordered.push(top);
      ordered.push(...(under.get(top) || []).sort(newestFirst));
      // Ciò che questo turno ha chiesto ad altri: una riga per lavoro, che cambia stato da sola.
      for (const w of top.message?.awaited || []) {
        const before = this.awaitedStates.get(w.messageId);
        seen.set(w.messageId, w.state);
        ordered.push({
          kind: 'awaited', id: w.messageId, when: top.when, from: w.agent, preview: '', unread: false, child: true,
          awaited: w, changed: before !== undefined && before !== w.state,
        });
      }
    }
    this.awaitedStates = seen;
    this.watchAwaited(ordered);
    return ordered;
  }

  /** Lo stato visto all'ultima lettura, per lavoro atteso: serve a far notare un cambio. */
  private awaitedStates = new Map<string, string>();
  private awaitedTimer: any = null;
  /** Fino a quando ci si aspetta una risposta dell'agente a cui si è appena scritto. */
  private expectingUntil = 0;

  /** Finché c'è un lavoro atteso non ancora concluso, l'elenco si rilegge da solo: lo stato cambia senza che nessuno scriva. */
  private watchAwaited(items: MailItem[]): void {
    // Dopo una risposta l'agente si sveglia e scriverà: l'elenco si rilegge da solo anche in quell'attesa,
    // altrimenti chi ha premuto il pulsante non vede succedere niente finché non aggiorna a mano.
    const open = Date.now() < this.expectingUntil
      || items.some(i => i.awaited && ['working', 'approval', 'reworking', 'rejected'].includes(i.awaited.state));
    if (open && !this.awaitedTimer) this.awaitedTimer = setInterval(() => { if (!this.loading) this.reload(); }, 15000);
    if (!open && this.awaitedTimer) { clearInterval(this.awaitedTimer); this.awaitedTimer = null; }
  }

  awaitedIcon(state: string): string {
    return ({ working: 'hourglass_top', approval: 'rule', approved: 'check_circle', rejected: 'block', reworking: 'replay', done: 'check', failed: 'error' } as any)[state] || 'help';
  }

  private fromMessage(m: MailboxMessage): MailItem {
    return { kind: 'message', id: m.id, when: m.createdAt, from: m.fromAgent, preview: m.bodyPreview || m.body, unread: !m.read, message: m };
  }

  private fromReview(r: MergeRequest): MailItem {
    const count = (r.files || []).length;
    return {
      kind: 'review', id: r.id, when: r.createdAt, from: r.agentName, unread: true, request: r,
      preview: r.status === 'rejected'
        ? this.translate.instant('AGENT_MAIL.REVIEW_STOPPED')
        : this.translate.instant(count === 1 ? 'AGENT_MAIL.REVIEW_PREVIEW_ONE' : 'AGENT_MAIL.REVIEW_PREVIEW', { count }),
    };
  }

  private fromFederation(f: FederationRequest): MailItem {
    return {
      kind: 'federation', id: f.id, when: f.createdAt, from: f.fromOwner || '?', unread: true, federation: f,
      preview: this.translate.instant('AGENT_MAIL.FEDERATION_PREVIEW', { agent: f.targetAgent }),
    };
  }

  select(item: MailItem): void {
    this.selected = item;
    this.document = null;
    this.replyDraft = '';
    this.artifacts = [];
    this.loadArtifacts();

    // Aprire un messaggio è leggerlo.
    if (item.kind === 'message' && item.unread) {
      this.mailbox.markRead(item.id).subscribe({
        next: () => { item.unread = false; if (item.message) item.message.read = true; this.unread = Math.max(0, this.unread - 1); },
        error: () => { /* resta non letto: lo si vede dal pallino */ },
      });
    }
  }

  /** I percorsi di documenti che il messaggio cita, e dove stanno adesso. */
  private loadArtifacts(): void {
    const item = this.selected;
    if (!item || item.kind !== 'message') return;
    const paths = Array.from(new Set(((item.message.body || '').match(/[A-Za-z0-9_.\-\/]+\.md\b/g) || []).filter(p => p.includes('/'))));
    if (!paths.length) { this.artifacts = []; return; }
    this.review.artifacts(this.data.projectPath, item.from, paths).subscribe({
      next: res => { if (this.selected === item) this.artifacts = res.artifacts || []; },
      error: () => { if (this.selected === item) this.artifacts = []; },
    });
  }

  /** Apre un artefatto qui dentro: dalla consegna dell'agente finché è da approvare, dal progetto dopo. */
  openArtifact(a: MailArtifact): void {
    if (a.state === 'missing' || a.state === 'toPull') return;
    this.openDocument(a.path, a.state === 'pending' ? a.requestId : null);
  }

  /** Un file di un lavoro da approvare: sempre dalla consegna. */
  openDelivered(request: MergeRequest, path: string): void {
    this.openDocument(path, request.id);
  }

  private openDocument(path: string, requestId: string | null): void {
    const clean = path.split('/').map(encodeURIComponent).join('/');
    const query = `connectionId=${encodeURIComponent(this.serverMessages.connectionId || '')}&theme=${this.themeService.getResolvedTheme()}&time=${Date.now()}`;
    const url = requestId
      ? `/api/MdExplorerWorktree/render-request/${requestId}/${clean}?${query}`
      : `/api/mdexplorer/${clean}?${query}&source=detached`;
    this.document = { path, pending: !!requestId, url: this.sanitizer.bypassSecurityTrustResourceUrl(url) };
  }

  closeDocument(): void {
    this.document = null;
  }

  /** La richiesta da approvare che contiene un artefatto: per passare dal messaggio alla decisione. */
  goToReview(a: MailArtifact): void {
    const item = this.items.find(i => i.kind === 'review' && i.id === a.requestId);
    if (item) this.select(item);
  }

  canReply(): boolean {
    return this.selected?.kind === 'message' && this.replyDraft.trim().length > 0 && !this.sending && !this.replyLock();
  }

  /** Perché a questo messaggio non si può rispondere adesso (null = si può). L'ordine lo impone anche il servizio. */
  replyLock(): 'pending' | 'rejected' | null {
    const artifact = this.selected?.message?.artifact;
    return artifact === 'pending' || artifact === 'rejected' ? artifact : null;
  }

  /** Un pulsante di risposta: invia il messaggio che la scheda dell'agente dichiara per quella risposta. */
  replyWith(r: MailReply): void {
    if (this.sending || this.replyLock()) return;
    this.replyDraft = r.message;
    this.reply();
  }

  reply(): void {
    const message = this.selected?.message;
    const body = this.replyDraft.trim();
    if (!message || !body) return;
    this.sending = true;
    // Rispondere sveglia l'agente: prima si dice se nella cartella c'è lavoro che lui non vedrebbe.
    this.startGuard.beforeStart(this.data?.projectPath || '', message.fromAgent).pipe(
      switchMap(go => go ? this.mailbox.reply(message.conversationId, body) : EMPTY),
      finalize(() => { this.sending = false; }),
    ).subscribe({
      next: (res) => {
        this.sending = false;
        this.replyDraft = '';
        this.expectingUntil = Date.now() + 10 * 60 * 1000;
        this.watchAwaited(this.items);
        this.snackBar.open(this.translate.instant('MAILBOX.REPLY_SENT', { agent: res.toAgent }), 'OK', { duration: 4000 });
      },
      error: (err) => { this.sending = false; this.showError(err); },
    });
  }

  /** Archivia il messaggio aperto: esce dall'elenco, e la selezione passa al successivo. */
  archive(item: MailItem): void {
    if (item?.kind !== 'message') return;
    const index = this.items.indexOf(item);
    this.mailbox.archive(item.id).subscribe({
      next: () => {
        this.items = this.items.filter(i => i !== item);
        this.messageCount = Math.max(0, this.messageCount - 1);
        if (item.unread) this.unread = Math.max(0, this.unread - 1);
        const next = this.items[Math.min(index, this.items.length - 1)];
        if (next) this.select(next); else { this.selected = null; this.artifacts = []; this.document = null; }
      },
      error: (err) => this.showError(err),
    });
  }

  /** Riporta in posta un messaggio archiviato. */
  unarchive(item: MailItem): void {
    if (item?.kind !== 'message') return;
    this.mailbox.unarchive(item.id).subscribe({
      next: () => this.reload(),
      error: (err) => this.showError(err),
    });
  }

  archiveAll(): void {
    this.mailbox.archiveAll(this.data?.projectPath || '').subscribe({
      next: () => this.reload(),
      error: (err) => this.showError(err),
    });
  }

  toggleArchive(): void {
    this.showArchive = !this.showArchive;
    this.selected = null;
    this.reload();
  }

  approveFederation(f: FederationRequest): void {
    this.deciding = true;
    this.federation.approve(f.id).subscribe({
      next: (res) => {
        this.deciding = false;
        this.snackBar.open(this.translate.instant('FEDERATION.APPROVED', { agent: res.targetAgent }), 'OK', { duration: 5000 });
        this.reload();
      },
      error: (err) => { this.deciding = false; this.showError(err); },
    });
  }

  rejectFederation(f: FederationRequest): void {
    this.deciding = true;
    this.federation.reject(f.id).subscribe({
      next: () => { this.deciding = false; this.reload(); },
      error: (err) => { this.deciding = false; this.showError(err); },
    });
  }

  /** Le conversazioni (il governo dei thread) restano nella finestra di prima. */
  openConversations(): void {
    this.dialog.open(MailboxDialogComponent, {
      width: '640px',
      maxHeight: '82vh',
      data: { projectPath: this.data?.projectPath || '', initialTab: 1 },
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  trackItem = (_: number, i: MailItem) => i.kind + ':' + i.id;

  private showError(err: any): void {
    this.snackBar.open(err?.error?.error || err?.message || this.translate.instant('AGENT_MAIL.ACTION_ERROR'), 'OK', { duration: 8000 });
  }
}
