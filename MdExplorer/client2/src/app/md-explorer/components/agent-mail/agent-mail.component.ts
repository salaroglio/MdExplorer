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

import { MailboxMessage, MailboxService, MailReply, AwaitedWork, ToStartAssignment, ReplyTarget, MailRound } from '../../services/mailbox.service';
import { AgentLaunchDialogComponent } from '../agent-launch-dialog/agent-launch-dialog.component';
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
  kind: 'message' | 'review' | 'federation' | 'awaited' | 'tostart' | 'section' | 'round';
  id: string;
  when: string;
  from: string;
  preview: string;
  unread: boolean;
  message?: MailboxMessage;
  request?: MergeRequest;
  federation?: FederationRequest;
  /** Un incarico che aspetta te, responsabile dell'agente, per partire. */
  assignment?: ToStartAssignment;
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
  /** Un giro del workflow: una voce, con i suoi passi; i suoi messaggi si aprono dal dettaglio. */
  round?: MailRound;
  /** I messaggi del giro (per il dettaglio del giro). */
  roundMessages?: MailItem[];
  /** Il giro a cui appartiene la voce (un messaggio del giro, una riga di un suo passo). */
  parentRound?: string;
  /** L'intestazione di una sezione dell'elenco (Da fare, Giri in corso, Messaggi), con quante voci ha. */
  section?: { key: 'todo' | 'rounds' | 'messages'; count: number };
}

/** Si seleziona tutto tranne le intestazioni e le righe di stato. */
function selectable(i: MailItem): boolean {
  return i.kind !== 'section' && i.kind !== 'awaited';
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
  /** Il modulo del rifiuto di un incarico da avviare è aperto; il motivo è obbligatorio. */
  declining = false;
  declineReason = '';

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
    this.subs.add(this.serverMessages.agentStartRequested$.subscribe(() => this.reload()));
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
      rounds: this.mailbox.rounds(projectPath).pipe(catchError(() => of({ rounds: [] as MailRound[] }))),
      federation: this.federation.requests(projectPath).pipe(catchError(() => of({ requests: [] as FederationRequest[] }))),
    }).subscribe({
      next: ({ inbox, reviews, federation, rounds }) => {
        // In posta i giri non archiviati; nell'archivio quelli archiviati (P3).
        this.rounds = (rounds.rounds || []).filter(r => !!r.archived === this.showArchive);
        this.unread = inbox.unread || 0;
        this.messageCount = (inbox.messages || []).length;
        // L'archivio contiene solo messaggi: ciò che è da decidere non si archivia.
        this.items = this.arrange(
          this.showArchive ? [] : (inbox.toStart || []).map(a => this.fromToStart(a)),
          (inbox.messages || []).map(m => this.fromMessage(m)),
          this.showArchive ? [] : (reviews.requests || []).map(r => this.fromReview(r)),
          this.showArchive ? [] : (federation.requests || []).filter(f => f.status === 'pending').map(f => this.fromFederation(f)));
        this.loading = false;

        // La selezione resta sulla stessa voce; se è sparita (letta, approvata), si passa alla prima.
        const kept = this.selected && (this.items.find(i => i.kind === this.selected.kind && i.id === this.selected.id)
          || ([] as MailItem[]).concat(...this.items.filter(i => i.roundMessages).map(i => i.roundMessages)).find(i => i.kind === this.selected.kind && i.id === this.selected.id));
        if (kept) { this.selected = kept; this.loadArtifacts(); }
        else if (this.items.some(selectable)) this.select(this.items.find(selectable));
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
  private arrange(toStart: MailItem[], messages: MailItem[], reviews: MailItem[], federation: MailItem[]): MailItem[] {
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

    // Ogni voce in alto con ciò che le sta sotto (la richiesta del suo artefatto, le righe del giro che ha aperto).
    const seen = new Map<string, string>();
    const group = (top: MailItem): MailItem[] => {
      const rows: MailItem[] = [top, ...(under.get(top) || []).sort(newestFirst)];
      for (const w of top.message?.awaited || []) {
        const before = this.awaitedStates.get(w.messageId);
        seen.set(w.messageId, w.state);
        rows.push({
          kind: 'awaited', id: w.messageId, when: top.when, from: w.agent, preview: '', unread: false, child: true,
          awaited: w, changed: before !== undefined && before !== w.state,
        });
      }
      return rows;
    };

    // Le tre sezioni (La posta in ordine, P1): ciò che aspetta una tua decisione, i giri, i messaggi da leggere.
    const closed = ['approved', 'done', 'declined', 'failed'];
    const needsYou = (top: MailItem): boolean => {
      if (this.showArchive) return false;                               // nell'archivio non c'è niente da decidere
      if (top.kind !== 'message') return true;                          // da avviare, da approvare da sola, richiesta di un collega
      const m = top.message;
      return !!top.hasPending                                           // il suo artefatto è da approvare
        || ((m?.replies?.length || 0) > 0 && !m?.answered);             // pulsanti a cui non hai ancora risposto
    };
    // Un messaggio è di un giro se l'ha scritto uno dei turni del giro (P3): sta dentro il giro, non sparso.
    const roundsOf = (m?: MailboxMessage): MailRound[] => {
      const run = (m?.runId || '').replace(/-/g, '').toLowerCase();
      return run ? this.rounds.filter(r => r.runs.some(x => x.toLowerCase() === run)) : [];
    };

    const sections: { [k: string]: MailItem[][] } = { todo: [], rounds: [], messages: [] };
    for (const top of [...toStart].sort(newestFirst)) sections.todo.push(group(top));
    const inRound = new Map<string, MailItem[]>();
    for (const top of [...messages, ...alone, ...federation].sort(newestFirst)) {
      if (needsYou(top)) { sections.todo.push(group(top)); continue; }
      const its = roundsOf(top.message);
      if (its.length) {
        for (const r of its) inRound.set(r.id, [...(inRound.get(r.id) || []), { ...top, child: true, parentRound: r.id }]);
        continue;
      }
      // Un messaggio con righe di attesa che non è di un giro (lavori chiesti ad altri agenti, senza workflow).
      const awaited = top.message?.awaited || [];
      sections[awaited.length && awaited.some(w => !closed.includes(w.state)) ? 'rounds' : 'messages'].push(group(top));
    }

    // I giri: quelli in corso prima, poi i conclusi; ognuno con le righe dei suoi passi finché è in corso.
    const roundGroups: MailItem[][] = [];
    for (const r of [...this.rounds].sort((x, y) => Number(x.finished) - Number(y.finished) || (y.lastActivityAt || '').localeCompare(x.lastActivityAt || ''))) {
      const msgs = inRound.get(r.id) || [];
      const item: MailItem = {
        kind: 'round', id: r.id, when: r.lastActivityAt, from: [r.title, ...r.values].join(' · '),
        preview: this.translate.instant(r.finished ? 'AGENT_MAIL.ROUND_FINISHED' : 'AGENT_MAIL.ROUND_PROGRESS', { done: r.stepsDone, total: r.stepsTotal }),
        unread: msgs.some(m => m.unread), round: r, roundMessages: msgs,
      };
      const rows: MailItem[] = [item];
      if (!r.finished)
        for (const w of r.steps) {
          const before = this.awaitedStates.get(w.messageId);
          seen.set(w.messageId, w.state);
          rows.push({ kind: 'awaited', id: w.messageId, when: item.when, from: w.agent, preview: '', unread: false, child: true,
                      awaited: w, parentRound: r.id, changed: before !== undefined && before !== w.state });
        }
      roundGroups.push(rows);
    }
    sections.rounds = [...roundGroups, ...sections.rounds];

    // «Archivia i letti» (P4): i messaggi letti della sezione Messaggi, quelli che non servono più.
    this.readToArchive = sections.messages.map(rows => rows[0]).filter(i => i.kind === 'message' && !i.unread).map(i => i.id);

    const ordered: MailItem[] = [];
    for (const key of ['todo', 'rounds', 'messages'] as const) {
      if (!sections[key].length) continue;
      ordered.push({ kind: 'section', id: 'section-' + key, when: '', from: '', preview: '', unread: false,
                     section: { key, count: sections[key].length } });
      for (const rows of sections[key]) ordered.push(...rows);
    }
    this.awaitedStates = seen;
    this.watchAwaited(ordered);
    return ordered;
  }

  /** I giri del workflow del progetto, dall'ultima lettura. */
  rounds: MailRound[] = [];

  /** Il giro di un messaggio aperto dal dettaglio del giro: per tornarci. */
  roundOf(item: MailItem): MailItem | undefined {
    return item?.parentRound ? this.items.find(i => i.kind === 'round' && i.id === item.parentRound) : undefined;
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
      || items.some(i => i.awaited && ['tostart', 'working', 'approval', 'reworking', 'rejected'].includes(i.awaited.state));
    if (open && !this.awaitedTimer) this.awaitedTimer = setInterval(() => { if (!this.loading) this.reload(); }, 15000);
    if (!open && this.awaitedTimer) { clearInterval(this.awaitedTimer); this.awaitedTimer = null; }
  }

  /** Chi chiede un incarico: un agente, o il giro del workflow (che scrive a nome di «user»). */
  requester(a: ToStartAssignment): string {
    return a.fromAgent === 'user' && a.step ? this.translate.instant('AGENT_MAIL.FROM_WORKFLOW') : a.fromAgent;
  }

  awaitedIcon(state: string): string {
    return ({ waiting: 'schedule', tostart: 'pending_actions', declined: 'do_not_disturb_on', working: 'hourglass_top', approval: 'rule', approved: 'check_circle', rejected: 'block', reworking: 'replay', done: 'check', failed: 'error' } as any)[state] || 'help';
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

  private fromToStart(a: ToStartAssignment): MailItem {
    return {
      kind: 'tostart', id: a.id, when: a.createdAt, from: a.toAgent, unread: true, assignment: a,
      preview: this.translate.instant('AGENT_MAIL.TOSTART_PREVIEW', { from: this.requester(a), step: a.step || a.toAgent }),
    };
  }

  /**
   * La schermata di lancio, con l'incarico dentro: chi ne risponde aggiunge le sue indicazioni, sceglie motore e
   * modello, e lo avvia. È la stessa che si usa per lanciare un agente a mano.
   */
  openStart(item: MailItem): void {
    const a = item?.assignment;
    if (!a) return;
    if (!a.agentFilePath) {
      this.snackBar.open(this.translate.instant('AGENT_MAIL.TOSTART_NO_CARD', { agent: a.toAgent }), 'OK', { duration: 6000 });
      return;
    }
    this.dialog.open(AgentLaunchDialogComponent, {
      width: '700px',
      data: {
        projectPath: this.data?.projectPath || '',
        agentFilePath: a.agentFilePath,
        agentName: a.toAgent,
        incoming: { messageId: a.id, fromAgent: this.requester(a), body: a.body, step: a.step },
      },
    }).afterClosed().subscribe(res => {
      if (res?.started) this.expectingUntil = Date.now() + 10 * 60 * 1000;
      this.reload();
    });
  }

  /** «Passa a un collega»: a chi, e una riga per lui. */
  passing = false;
  passTo = '';
  passNote = '';

  /** Il passo va a un collega che risponde dello stesso agente: esce dalla mia posta e va sul suo computer. */
  passStart(item: MailItem): void {
    if (!item?.assignment || !this.passTo || this.deciding) return;
    this.deciding = true;
    this.mailbox.passAssignment(item.assignment.id, this.passTo, this.passNote.trim() || undefined)
      .pipe(finalize(() => { this.deciding = false; })).subscribe({
        next: () => {
          const to = this.passTo;
          this.passing = false; this.passTo = ''; this.passNote = '';
          this.snackBar.open(this.translate.instant('AGENT_MAIL.TOSTART_PASSED', { step: item.assignment.step || item.assignment.toAgent, to }), 'OK', { duration: 5000 });
          this.reload();
        },
        error: (err) => this.showError(err),
      });
  }

  /** Non lo avvio: il motivo lo legge chi l'ha chiesto. */
  declineStart(item: MailItem): void {
    const reason = this.declineReason.trim();
    if (!item?.assignment || !reason || this.deciding) return;
    this.deciding = true;
    this.mailbox.declineAssignment(item.assignment.id, reason).pipe(finalize(() => { this.deciding = false; })).subscribe({
      next: () => {
        this.declining = false;
        this.declineReason = '';
        this.snackBar.open(this.translate.instant('AGENT_MAIL.TOSTART_DECLINED', { agent: item.assignment.toAgent }), 'OK', { duration: 4000 });
        this.reload();
      },
      error: (err) => this.showError(err),
    });
  }

  private fromFederation(f: FederationRequest): MailItem {
    return {
      kind: 'federation', id: f.id, when: f.createdAt, from: f.fromOwner || '?', unread: true, federation: f,
      preview: this.translate.instant('AGENT_MAIL.FEDERATION_PREVIEW', { agent: f.targetAgent }),
    };
  }

  select(item: MailItem): void {
    this.selected = item;
    this.declining = false;
    this.passing = false;
    this.choosing = null;
    this.declineReason = '';
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

  /** Il pulsante premuto che aspetta la scelta di chi fa ogni passo (agenti di un team). */
  choosing: { reply: MailReply; targets: ReplyTarget[]; picks: { [step: string]: string } } | null = null;

  /**
   * Un pulsante di risposta: invia il messaggio che la scheda dell'agente dichiara per quella risposta. Se fa partire un
   * passo di un agente con un team, prima si sceglie chi lo fa: da lì ne risponde quella persona.
   */
  replyWith(r: MailReply): void {
    const message = this.selected?.message;
    if (this.sending || this.replyLock() || !message) return;
    this.sending = true;
    this.mailbox.replyTargets(message.id, r.id).pipe(finalize(() => { this.sending = false; })).subscribe({
      next: (res) => {
        // Un pulsante del workflow non sveglia l'agente che l'ha proposto: fa partire i passi del giro, ciascuno avviato poi
        // da chi ne risponde. Il controllo «lavoro non salvato» lo fa chi li avvia.
        const workflow = (res.targets || []).length > 0;
        const toChoose = (res.targets || []).filter(t => t.needsChoice);
        if (!toChoose.length) { this.send(r.message, true, undefined, workflow); return; }
        this.choosing = { reply: r, targets: toChoose, picks: {} };
      },
      error: (err) => this.showError(err),
    });
  }

  canConfirmChoice(): boolean {
    return !!this.choosing && this.choosing.targets.every(t => !!this.choosing!.picks[t.step]) && !this.sending;
  }

  confirmChoice(): void {
    if (!this.canConfirmChoice()) return;
    const c = this.choosing!;
    this.send(c.reply.message, true, { ...c.picks }, true);
  }

  /** Il campo libero: l'agente lo riceve con il suo messaggio citato. */
  reply(): void {
    this.send(this.replyDraft.trim(), false);
  }

  private send(body: string, choice: boolean, assign?: { [step: string]: string }, workflow = false): void {
    const message = this.selected?.message;
    if (!message || !body) return;
    this.sending = true;
    // Rispondere sveglia l'agente: prima si dice se nella cartella c'è lavoro che lui non vedrebbe.
    (workflow ? of(true) : this.startGuard.beforeStart(this.data?.projectPath || '', message.fromAgent)).pipe(
      switchMap(go => go ? this.mailbox.reply(message.conversationId, body, choice, message.id, assign) : EMPTY),
      finalize(() => { this.sending = false; }),
    ).subscribe({
      next: (res) => {
        this.sending = false;
        this.replyDraft = '';
        this.choosing = null;
        this.expectingUntil = Date.now() + 10 * 60 * 1000;
        this.watchAwaited(this.items);
        // Un pulsante del workflow fa partire subito i passi: i loro «da avviare» sono già nella posta.
        if (res.workflow) this.reload();
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
        const next = this.items.slice(Math.min(index, this.items.length - 1)).find(selectable) || [...this.items].reverse().find(selectable);
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

  /** I messaggi letti della sezione Messaggi: quelli che «Archivia i letti» toglie dall'elenco. */
  readToArchive: string[] = [];

  /** «Archivia i letti» (P4): un messaggio non letto non si archivia senza averlo aperto. */
  archiveRead(): void {
    if (!this.readToArchive.length) return;
    this.mailbox.archiveMany(this.readToArchive).subscribe({
      next: () => this.reload(),
      error: (err) => this.showError(err),
    });
  }

  /** «Archivia il giro» (un giro concluso, con i suoi messaggi) o «Riporta in posta». */
  archiveRound(item: MailItem, archive: boolean): void {
    if (!item?.round) return;
    this.mailbox.archiveRound(this.data?.projectPath || '', item.round.id, archive).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant(archive ? 'AGENT_MAIL.ROUND_ARCHIVED' : 'AGENT_MAIL.ROUND_UNARCHIVED', { name: item.from }), 'OK', { duration: 4000 });
        this.selected = null;
        this.reload();
      },
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

  trackItem = (_: number, i: MailItem) => i.kind + ':' + (i.parentRound ? i.parentRound + '/' : '') + i.id;

  private showError(err: any): void {
    this.snackBar.open(err?.error?.error || err?.message || this.translate.instant('AGENT_MAIL.ACTION_ERROR'), 'OK', { duration: 8000 });
  }
}
