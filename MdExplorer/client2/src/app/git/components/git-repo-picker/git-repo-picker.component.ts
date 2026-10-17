import { Component, EventEmitter, Input, Output } from '@angular/core';

/**
 * Un repository fra cui scegliere in un dialogo git: il progetto, o uno dei suoi submodule.
 * Un submodule è un repository a tutti gli effetti: cronologia e rami sono i SUOI, e per
 * leggerli basta il suo percorso.
 */
export interface GitDialogRepo {
  /** Percorso assoluto sul disco: è ciò che il backend vuole. */
  path: string;
  label: string;
  /** `null` con HEAD staccato. */
  branch: string | null;
  detached: boolean;
  isRoot: boolean;
  /** Il commit che il progetto registra per questo submodule. Assente sulla radice. */
  recordedCommit?: string | null;
}

/**
 * Il selettore del repository in testa ai dialoghi di cronologia e rami: una voce per
 * repository, col suo ramo. Senza, i due dialoghi mostravano solo il progetto e i submodule —
 * ognuno col suo ramo e la sua storia — restavano invisibili.
 */
@Component({
  selector: 'app-git-repo-picker',
  templateUrl: './git-repo-picker.component.html',
  styleUrls: ['./git-repo-picker.component.scss']
})
export class GitRepoPickerComponent {
  @Input() repos: GitDialogRepo[] = [];
  @Input() selected: GitDialogRepo | null = null;
  @Input() disabled = false;
  @Output() selectedChange = new EventEmitter<GitDialogRepo>();

  trackByPath = (_: number, r: GitDialogRepo) => r.path;

  pick(repo: GitDialogRepo): void {
    if (this.disabled || repo === this.selected) return;
    this.selectedChange.emit(repo);
  }
}
