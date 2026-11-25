import { ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { GITService, NewBranchPreview, NewBranchResult } from '../../services/gitservice.service';
import { GitDialogRepo } from '../../components/git-repo-picker/git-repo-picker.component';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';

export interface NewBranchDialogData {
  projectName?: string;
  /** Il progetto e i suoi submodule: il ramo nasce in quello scelto. */
  repos: GitDialogRepo[];
}

/**
 * «Nuovo ramo» (sprint 2026-11-25-Nuovo-Ramo-E-Lavoro-All-Agente, A2): dice da dove parte il ramo, quali commit non
 * pubblicati e quali file non registrati vengono con te, poi lo crea, ci passa sopra e lo pubblica. Se la pubblicazione non
 * riesce il ramo c'è lo stesso: lo si dice, e «Pubblica» la rifà. Chiude con true se un ramo è stato creato.
 */
@Component({
  selector: 'app-new-branch-dialog',
  templateUrl: './new-branch-dialog.component.html',
  styleUrls: ['./new-branch-dialog.component.scss'],
})
export class NewBranchDialogComponent implements OnInit {
  repos: GitDialogRepo[] = [];
  activeRepo: GitDialogRepo | null = null;
  preview: NewBranchPreview | null = null;
  name = '';
  loading = false;
  creating = false;
  publishing = false;
  error: string | null = null;
  result: NewBranchResult | null = null;

  constructor(
    public dialogRef: MatDialogRef<NewBranchDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) public data: NewBranchDialogData,
    private git: GITService,
    private serverMessages: MdServerMessagesService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.repos = this.data.repos || [];
    this.activeRepo = this.repos.find(r => r.isRoot) || this.repos[0] || null;
    this.load();
  }

  selectRepo(repo: GitDialogRepo): void {
    this.activeRepo = repo;
    this.load();
  }

  private load(): void {
    if (!this.activeRepo) return;
    this.loading = true;
    this.error = null;
    this.preview = null;
    this.git.newBranchPreview(this.activeRepo.path).subscribe({
      next: p => {
        this.preview = p;
        if (!this.name) this.name = p.suggestedPrefix;
        this.loading = false;
        this.cd.detectChanges();
      },
      error: err => {
        // Senza l'anteprima non si sa che cosa viene con te: non si crea alla cieca.
        this.error = err?.error?.error || err?.message || String(err);
        this.loading = false;
        this.cd.detectChanges();
      },
    });
  }

  get canCreate(): boolean {
    const n = this.name.trim();
    return !!this.preview && !this.preview.blocker && !this.loading && !this.creating && !this.result
      && n.length > 0 && n !== this.preview.suggestedPrefix;
  }

  create(): void {
    if (!this.canCreate || !this.activeRepo) return;
    this.creating = true;
    this.error = null;
    this.git.createNewBranch(this.activeRepo.path, this.name.trim(), this.serverMessages.connectionId).subscribe({
      next: r => { this.result = r; this.creating = false; this.cd.detectChanges(); },
      error: err => { this.error = err?.error?.error || err?.message || String(err); this.creating = false; this.cd.detectChanges(); },
    });
  }

  publish(): void {
    if (!this.result?.branch || !this.activeRepo) return;
    this.publishing = true;
    this.git.publishNewBranch(this.activeRepo.path, this.result.branch).subscribe({
      next: r => {
        this.result = { ...this.result!, published: r.published, publishProblem: r.publishProblem };
        this.publishing = false;
        this.cd.detectChanges();
      },
      error: err => {
        this.result = { ...this.result!, publishProblem: err?.error?.error || err?.message || String(err) };
        this.publishing = false;
        this.cd.detectChanges();
      },
    });
  }

  close(): void {
    this.dialogRef.close(!!this.result?.created);
  }
}
