import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { ProjectsService } from '../../services/projects.service';
import { ReviewContextService } from '../../services/review-context.service';
import { ChangeKind, RepoChanges, WorkingChange, WorkingChangesService, WorkingChangesView } from '../../services/working-changes.service';

/**
 * Il tab delle differenze, fratello dell'albero dei documenti.
 *
 * Da una parte i file del progetto, dall'altra quelli toccati — e cliccandoli, il diff, che è
 * il motivo per cui questo tab esiste. Sempre presente e sempre lo stesso gesto: «cosa è
 * cambiato qui». Un tab che apparisse e sparisse sposterebbe gli altri sotto il dito e, quando
 * via, negherebbe l'accesso alla storia.
 *
 * Segue il contesto: il tuo lavoro, oppure quello di un agente quando entri in revisione.
 *
 * I file sono raggruppati **per repository** — il progetto e i suoi submodule — perché il
 * repository è l'unità in cui si committa: mescolarli darebbe un elenco in cui due righe vicine
 * finiscono in posti diversi.
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

  /** File aperto nel riquadro del diff: identificato da repository E percorso, non dal solo percorso. */
  openedPath: string | null = null;
  openedRepo: string | null = null;
  diffText = '';
  diffLoading = false;

  /**
   * Chi ha aperto o chiuso cosa, per percorso di repository. Serve perché la vista si rilegge
   * dopo ogni azione: senza, i gruppi tornerebbero al loro stato di partenza a ogni scarto.
   */
  private readonly chosen = new Map<string, boolean>();

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

  open(repo: RepoChanges, file: WorkingChange): void {
    if (this.openedPath === file.path && this.openedRepo === repo.path) { this.closeDiff(); return; }
    this.openedPath = file.path;
    this.openedRepo = repo.path;
    this.diffText = '';
    this.diffLoading = true;
    this.changes.diff(this.projectPath, this.agent, file.path, repo.path).subscribe({
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
    this.openedRepo = null;
    this.diffText = '';
  }

  // ---- i gruppi ----

  /** C'è qualcosa da guardare qui dentro? Decide come si presenta un gruppo la prima volta. */
  hasSomething(repo: RepoChanges): boolean {
    return repo.files.length > 0 || repo.pointerMoved || repo.notInitialized;
  }

  /** Chiuso di partenza se non c'è niente: aprire cinque gruppi vuoti non aiuta a capire. */
  isCollapsed(repo: RepoChanges): boolean {
    const choice = this.chosen.get(repo.path);
    return choice !== undefined ? choice : !this.hasSomething(repo);
  }

  toggle(repo: RepoChanges): void {
    this.chosen.set(repo.path, !this.isCollapsed(repo));
  }

  /** Quanti file di un tipo in QUESTO repository. */
  countIn(repo: RepoChanges, change: ChangeKind): number {
    return repo.files.filter(f => f.change === change).length;
  }

  trackByRepo = (_: number, r: RepoChanges) => r.path;

  /** Si scarta DOPO aver visto il diff, non alla cieca: per questo sta qui e non nel commit. */
  discard(repo: RepoChanges, file: WorkingChange, event: MouseEvent): void {
    event.stopPropagation();
    // Il nome del repository sta nella domanda: due file omonimi in due submodule diversi
    // sarebbero indistinguibili, e questo gesto non si annulla.
    const where = repo.path ? repo.path + '/' + file.path : file.path;
    const question = this.translate.instant('CHANGES.DISCARD_CONFIRM', { path: where });
    if (!window.confirm(question)) return;

    this.changes.discard(this.projectPath, this.agent, file.path, repo.path).subscribe({
      next: () => {
        if (this.openedPath === file.path && this.openedRepo === repo.path) this.closeDiff();
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
