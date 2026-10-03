import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { AgentReviewService, ApproveChoice, ChangedFile, MergeRequest, NotifyCandidate } from '../../services/agent-review.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { ProjectsService } from '../../services/projects.service';
import { ReviewContextService } from '../../services/review-context.service';

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
export class AgentReviewComponent implements OnInit, OnDestroy {
  requests: MergeRequest[] = [];
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

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.projectSub?.unsubscribe();
  }

  refresh(): void {
    if (!this.projectPath) return;
    this.loading = true;
    this.review.pending(this.projectPath).subscribe({
      next: (res) => { this.requests = res?.requests || []; this.preselect(); this.loading = false; },
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
        this.refresh();
      },
      error: (err) => {
        this.busyId = null;
        // Nessuna scelta o scelta non valida: non si è fuso niente. Si aggiorna l'elenco e si dice il perché.
        if (err?.error?.code === 'choose-recipient' || err?.error?.code?.startsWith('recipient-')) {
          this.snackBar.open(err.error.error, 'OK', { duration: 12000 });
          this.refresh();
          return;
        }
        // Autorizzata ma non fusa (tipicamente un conflitto): dirlo, non nasconderlo.
        const note = err?.error?.note || err?.error?.error || this.translate.instant('AGENT_REVIEW.MERGE_FAILED');
        this.snackBar.open(note, 'OK', { duration: 12000 });
        this.refresh();
      },
    });
  }

  reject(r: MergeRequest): void {
    this.busyId = r.id;
    this.review.reject(r.id).subscribe({
      next: () => { this.busyId = null; this.toast('AGENT_REVIEW.REJECTED'); this.refresh(); },
      error: () => { this.busyId = null; this.refresh(); },
    });
  }

  take(r: MergeRequest): void {
    this.busyId = r.id;
    this.review.take(r.id).subscribe({
      next: (res) => {
        this.busyId = null;
        // Da qui in poi il tab delle differenze, gli aggregati e il commit parlano di lui:
        // stai lavorando nel suo posto, non nel tuo.
        this.context.enterAgent(r.agentName);
        this.snackBar.open(
          this.translate.instant(
            res.folderOpened ? 'AGENT_REVIEW.TAKEN' : 'AGENT_REVIEW.TAKEN_NO_FOLDER',
            { path: res.worktreePath }),
          'OK', { duration: 12000 });
        this.refresh();
      },
      error: (err) => {
        this.busyId = null;
        this.snackBar.open(err?.error?.error || 'Errore', 'OK', { duration: 8000 });
      },
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
        this.refresh();
      },
      error: () => { this.busyId = null; this.refresh(); },
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
