import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { ProjectsService } from '../../services/projects.service';
import { ReviewContextService } from '../../services/review-context.service';
import { ChangeKind, WorkingChange, WorkingChangesService, WorkingChangesView } from '../../services/working-changes.service';

/**
 * Il tab delle differenze, fratello dell'albero dei documenti.
 *
 * Da una parte i file del progetto, dall'altra quelli toccati — e cliccandoli, il diff, che è
 * il motivo per cui questo tab esiste. Sempre presente e sempre lo stesso gesto: «cosa è
 * cambiato qui». Un tab che apparisse e sparisse sposterebbe gli altri sotto il dito e, quando
 * via, negherebbe l'accesso alla storia.
 *
 * Segue il contesto: il tuo lavoro, oppure quello di un agente quando entri in revisione.
 */
@Component({
  selector: 'app-working-changes',
  templateUrl: './working-changes.component.html',
  styleUrls: ['./working-changes.component.scss'],
})
export class WorkingChangesComponent implements OnInit, OnDestroy {
  view: WorkingChangesView | null = null;
  loading = false;
  projectPath = '';
  agent: string | null = null;

  /** File aperto nel riquadro del diff. */
  openedPath: string | null = null;
  diffText = '';
  diffLoading = false;

  private subs: Subscription[] = [];

  constructor(
    private changes: WorkingChangesService,
    private projects: ProjectsService,
    private context: ReviewContextService,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.projectPath = this.projects.currentProjects$.value?.path || '';
    this.agent = this.context.agent;
    this.refresh();

    this.subs.push(this.projects.currentProjects$.subscribe(p => {
      const path = p?.path || '';
      if (path === this.projectPath) return;
      this.projectPath = path;
      this.closeDiff();
      this.refresh();
    }));

    // Entrando o uscendo dalla revisione cambia di chi è il lavoro mostrato: il diff aperto
    // parlava dell'altro contesto, quindi va chiuso invece di restare lì a mentire.
    this.subs.push(this.context.agent$.subscribe(agent => {
      if (agent === this.agent) return;
      this.agent = agent;
      this.closeDiff();
      this.refresh();
    }));
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  refresh(): void {
    if (!this.projectPath) { this.view = null; return; }
    this.loading = true;
    this.changes.list(this.projectPath, this.agent).subscribe({
      next: v => { this.view = v; this.loading = false; },
      error: err => {
        // Il corpo dell'errore è comunque una vista: contiene il motivo da mostrare.
        this.view = err?.error?.problem ? err.error : null;
        this.loading = false;
      },
    });
  }

  open(file: WorkingChange): void {
    if (this.openedPath === file.path) { this.closeDiff(); return; }
    this.openedPath = file.path;
    this.diffText = '';
    this.diffLoading = true;
    this.changes.diff(this.projectPath, this.agent, file.path).subscribe({
      next: r => { this.diffText = r.diff || ''; this.diffLoading = false; },
      error: err => {
        this.diffLoading = false;
        this.diffText = '';
        this.snackBar.open(err?.error?.error || this.translate.instant('CHANGES.DIFF_FAILED'), 'OK', { duration: 8000 });
      },
    });
  }

  closeDiff(): void {
    this.openedPath = null;
    this.diffText = '';
  }

  /** Si scarta DOPO aver visto il diff, non alla cieca: per questo sta qui e non nel commit. */
  discard(file: WorkingChange, event: MouseEvent): void {
    event.stopPropagation();
    const question = this.translate.instant('CHANGES.DISCARD_CONFIRM', { path: file.path });
    if (!window.confirm(question)) return;

    this.changes.discard(this.projectPath, this.agent, file.path).subscribe({
      next: () => {
        if (this.openedPath === file.path) this.closeDiff();
        this.refresh();
      },
      error: err => this.snackBar.open(
        err?.error?.error || this.translate.instant('CHANGES.DISCARD_FAILED'), 'OK', { duration: 8000 }),
    });
  }

  /** Le righe del diff, per colorarle senza una libreria. */
  diffLines(): { text: string; kind: string }[] {
    return (this.diffText || '').split('\n').map(text => ({
      text,
      kind: text.startsWith('+++') || text.startsWith('---') ? 'meta'
          : text.startsWith('@@') ? 'hunk'
          : text.startsWith('+') ? 'add'
          : text.startsWith('-') ? 'del'
          : text.startsWith('diff ') || text.startsWith('index ') ? 'meta'
          : 'ctx',
    }));
  }

  countOf(change: ChangeKind): number {
    return (this.view?.files || []).filter(f => f.change === change).length;
  }

  iconFor(change: ChangeKind): string {
    switch (change) {
      case 'added': return 'add_circle';
      case 'untracked': return 'fiber_new';
      case 'deleted': return 'remove_circle';
      case 'renamed': return 'drive_file_move';
      default: return 'edit';
    }
  }

  backToMyWork(): void {
    this.context.backToUser();
  }

  trackByPath = (_: number, f: WorkingChange) => f.path;
}
