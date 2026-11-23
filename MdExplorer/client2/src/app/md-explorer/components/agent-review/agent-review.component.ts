import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output } from '@angular/core';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { AgentReviewService, ApproveChoice, ChangedFile, MergeRequest, NotifyCandidate } from '../../services/agent-review.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { ProjectsService } from '../../services/projects.service';
import { ReviewContextService } from '../../services/review-context.service';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { AgentWorkspaceService } from '../../services/agent-workspace.service';
import { MarkAssistantService } from '../../../mark-assistant/mark-assistant.service';

/**
 * La revisione del lavoro degli agenti, accanto a "Documenti progetto".
 *
 * Mostra cosa un agente ha prodotto — i file nuovi, modificati ed eliminati — e i tre gesti
 * possibili: autorizzo, rifiuto, ci metto mano. Il terzo è quello che rende il rifiuto
 * qualcosa di più di un "no": apre il worktree sul filesystem e mette l'agente in coda,
 * così il lavoro bocciato non resta in un limbo che nessuno riprende.
 */
@Component({
  selector: 'app-agent-review',
  templateUrl: './agent-review.component.html',
  styleUrls: ['./agent-review.component.scss'],
})
export class AgentReviewComponent implements OnInit, OnChanges, OnDestroy {
  /** Dentro la posta degli agenti: una sola richiesta, quella selezionata nell'elenco. Vuoto = tutte. */
  @Input() onlyRequestId: string | null = null;
  /** Dentro un'altra pagina: senza intestazione propria, e con «Apri» sui file. */
  @Input() embedded = false;
  /** Una richiesta è stata decisa o presa in mano: chi ospita il pannello rilegge il suo elenco. */
  @Output() changed = new EventEmitter<void>();
  /** «Apri» su un file della consegna. */
  @Output() openFile = new EventEmitter<{ request: MergeRequest; path: string }>();

  requests: MergeRequest[] = [];
  private allRequests: MergeRequest[] = [];
  loading = false;
  busyId: string | null = null;
  projectPath = '';

  /** Valore della scelta «nessun avviso»: non può coincidere con il nome di un agente (kebab-case). */
  readonly NOBODY = '__nobody__';

  /** La scelta della persona per ogni richiesta: nome del collega o NOBODY. Assente = non ha ancora scelto. */
  choice: { [requestId: string]: string } = {};

  private sub: Subscription;
  private projectSub: Subscription;

  constructor(
    private review: AgentReviewService,
    private serverMessages: MdServerMessagesService,
    private projects: ProjectsService,
    private context: ReviewContextService,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    private dialog: MatDialog,
    public workspace: AgentWorkspaceService,
    private mark: MarkAssistantService,
  ) {}

  ngOnInit(): void {
    this.projectPath = this.projects.currentProjects$.value?.path || '';
    this.refresh();

    // Il progetto puo' cambiare mentre il tab e' aperto: la revisione deve seguirlo,
    // altrimenti mostrerebbe le richieste di un progetto che non stai piu' guardando.
    this.projectSub = this.projects.currentProjects$.subscribe(p => {
      const path = p?.path || '';
      if (path === this.projectPath) return;
      this.projectPath = path;
      this.requests = [];
      this.refresh();
    });

    // Il tab "si accende" quando un agente consegna: il dispatcher lo annuncia.
    this.sub = this.serverMessages.agentMergeRequested$.subscribe(() => this.refresh());
  }

  ngOnChanges(): void {
    this.show();
  }

  /** L'elenco mostrato: tutto, oppure la sola richiesta chiesta da chi ospita il pannello. */
  private show(): void {
    this.requests = this.onlyRequestId ? this.allRequests.filter(r => r.id === this.onlyRequestId) : this.allRequests;
    this.preselect();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.projectSub?.unsubscribe();
  }

  refresh(): void {
    if (!this.projectPath) return;
    this.loading = true;
    this.review.pending(this.projectPath).subscribe({
      next: (res) => { this.allRequests = res?.requests || []; this.show(); this.loading = false; },
      error: () => { this.loading = false; },
    });
  }

  /**
   * Con un solo collega raggiungibile la scelta è già fatta (si può cambiare in «nessuno»); con più di uno
   * la persona deve scegliere: il pulsante resta spento finché non lo fa. Una scelta già espressa si tiene.
   */
  private preselect(): void {
    const next: { [id: string]: string } = {};
    for (const r of this.requests) {
      const c = r.notifyCandidates || [];
      if (c.length === 0) continue;
      const kept = this.choice[r.id];
      if (kept && (kept === this.NOBODY || c.some(x => x.name === kept && x.available))) { next[r.id] = kept; continue; }
      const open = c.filter(x => x.available);
      if (c.length === 1 && open.length === 1) next[r.id] = open[0].name;
    }
    this.choice = next;
  }

  hasRecipients(r: MergeRequest): boolean { return (r.notifyCandidates || []).length > 0; }

  /** «Autorizza» è disponibile solo se la persona ha scelto (o se non c'è nessuno da avvisare). */
  canApprove(r: MergeRequest): boolean { return !this.hasRecipients(r) || !!this.choice[r.id]; }

  approve(r: MergeRequest): void {
    this.busyId = r.id;
    const picked = this.choice[r.id];
    const choice: ApproveChoice = !picked ? {} : picked === this.NOBODY ? { nobody: true } : { notify: picked };
    choice.commit = r.headSha;
    this.review.approve(r.id, choice).subscribe({
      next: (res) => {
        this.busyId = null;
        const n = res?.notice;
        if (!n) this.toast('AGENT_REVIEW.MERGED');
        else if (n.notified)
          this.snackBar.open(this.translate.instant('AGENT_REVIEW.MERGED_AND_NOTIFIED', { name: n.recipient }), 'OK', { duration: 8000 });
        else
          // Fuso, ma l'avviso non è partito: la persona deve saperlo, il lavoro non è passato di mano.
          this.snackBar.open(this.translate.instant('AGENT_REVIEW.MERGED_NOT_NOTIFIED', { name: n.recipient, reason: n.error }), 'OK', { duration: 20000 });
        this.refresh(); this.changed.emit();
      },
      error: (err) => {
        this.busyId = null;
        // Nessuna scelta o scelta non valida: non si è fuso niente. Si aggiorna l'elenco e si dice il perché.
        if (err?.error?.code === 'choose-recipient' || err?.error?.code?.startsWith('recipient-')) {
          this.snackBar.open(err.error.error, 'OK', { duration: 12000 });
          this.refresh(); this.changed.emit();
          return;
        }
        // Autorizzata ma non fusa (tipicamente un conflitto): dirlo, non nasconderlo.
        const note = err?.error?.note || err?.error?.error || this.translate.instant('AGENT_REVIEW.MERGE_FAILED');
        this.snackBar.open(note, 'OK', { duration: 12000 });
        this.refresh(); this.changed.emit();
      },
    });
  }

  /** «Fai ripartire»: l'incarico torna in coda con il motivo del rifiuto. Un gesto, non un automatismo. */
  rework(r: MergeRequest): void {
    this.busyId = r.id;
    this.review.rework(r.id).subscribe({
      next: () => { this.busyId = null; this.toast('AGENT_REVIEW.REWORK_STARTED'); this.refresh(); this.changed.emit(); },
      error: (err) => {
        this.busyId = null;
        this.snackBar.open(err?.error?.error || this.translate.instant('AGENT_REVIEW.REWORK_FAILED'), 'OK', { duration: 7000 });
        this.refresh(); this.changed.emit();
      },
    });
  }

  /** La richiesta di cui si sta scrivendo il motivo del rifiuto (null = nessuna). */
  rejectingId: string | null = null;
  rejectNote = '';

  /** Rifiutare chiede il motivo: è ciò che l'agente legge se deve rifare il lavoro. */
  askReject(r: MergeRequest): void {
    this.rejectingId = r.id;
    this.rejectNote = '';
  }

  cancelReject(): void {
    this.rejectingId = null;
    this.rejectNote = '';
  }

  reject(r: MergeRequest): void {
    const note = this.rejectNote.trim();
    if (!note) return;
    this.busyId = r.id;
    this.review.reject(r.id, note).subscribe({
      next: () => {
        this.busyId = null; this.cancelReject();
        this.toast(r.someoneWaiting ? 'AGENT_REVIEW.REJECTED_STOPPED' : 'AGENT_REVIEW.REJECTED');
        this.refresh(); this.changed.emit();
      },
      error: () => { this.busyId = null; this.refresh(); this.changed.emit(); },
    });
  }

  /**
   * «Ci metto mano»: Mark spiega dove sta il lavoro dell'agente e come lo si corregge, e porta lui nella sua copia.
   * Prima apriva la cartella nel file manager, e lì ci si fermava senza sapere che cosa farne.
   */
  take(r: MergeRequest): void {
    this.mark.say(this.translate.instant('AGENT_REVIEW.TAKE_MARK', { agent: r.agentName }), [
      { labelKey: 'AGENT_REVIEW.TAKE_GO', icon: '➡️', handler: () => this.enterCopy(r) },
      { labelKey: 'AGENT_REVIEW.TAKE_CANCEL', icon: '✋', handler: () => this.mark.hide() },
    ]);
  }

  /** Rimette il lavoro su un posto (se serve), entra nella copia dell'agente e mostra il tab delle differenze. */
  enterCopy(r: MergeRequest): void {
    this.mark.hide();
    this.busyId = r.id;
    const failed = (err: any) => {
      this.busyId = null;
      this.snackBar.open(err?.error?.error || this.translate.instant('AGENT_REVIEW.TAKE_FAILED'), 'OK',
        { duration: 10000, panelClass: ['kg-stale-snack'] });
    };
    this.review.take(r.id).subscribe({
      next: () => this.workspace.enter(r.agentName).subscribe({
        next: entered => {
          this.busyId = null;
          if (!entered) return;   // un altro ingresso o un'uscita è già in corso
          // La revisione sta in una finestra sopra il progetto: da qui si lavora nel progetto, nella copia dell'agente.
          this.dialog.closeAll();
          this.context.showChanges();
          this.mark.say(this.translate.instant('AGENT_REVIEW.TAKE_INSIDE', { agent: r.agentName }), [
            { labelKey: 'AGENT_REVIEW.TAKE_UNDERSTOOD', icon: '👍', handler: () => this.mark.hide() },
          ]);
        },
        error: failed,
      }),
      error: failed,
    });
  }

  /** Chiude la sessione: `discard` rimette il lavoro in coda all'agente. */
  release(r: MergeRequest, discard: boolean): void {
    this.busyId = r.id;
    this.review.release(r.id, discard).subscribe({
      next: (res) => {
        this.busyId = null;
        // Sessione chiusa: il posto non e' piu' tuo, quindi il contesto torna al tuo lavoro.
        // Senza, il tab continuerebbe a mostrare un worktree che l'agente puo' ripulire.
        if (this.context.agent === r.agentName) this.context.backToUser();
        this.snackBar.open(res.message, 'OK', { duration: 10000 });
        this.refresh(); this.changed.emit();
      },
      error: () => { this.busyId = null; this.refresh(); this.changed.emit(); },
    });
  }

  iconFor(change: ChangedFile['change']): string {
    switch (change) {
      case 'added': return 'add_circle';
      case 'deleted': return 'remove_circle';
      case 'renamed': return 'drive_file_move';
      default: return 'edit';
    }
  }

  countOf(r: MergeRequest, change: ChangedFile['change']): number {
    return (r.files || []).filter(f => f.change === change).length;
  }

  trackById = (_: number, r: MergeRequest) => r.id;

  private toast(key: string): void {
    this.snackBar.open(this.translate.instant(key), 'OK', { duration: 6000 });
  }
}
