import { Component, Inject, OnDestroy } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { CodeRootsService, RemoteFoundEntry } from '../../../services/code-roots.service';

export interface CodeRootFindDialogData {
  projectRoot: string;
  /** Dove si cerca: <c>@alias</c> (tutta la radice) o una sua cartella <c>@alias/…</c>. */
  path: string;
}

/**
 * «Cerca file» in una radice esterna (20/11/2026): la ricerca per nome la fa MDE Remote sul server (o le stesse regole sul
 * disco), anche nelle cartelle che l'albero non ha mai aperto. Chiude con il file scelto: l'albero apre il percorso,
 * accende l'occhio sulla sua cartella se serve, lo mostra e lo apre.
 */
@Component({
  selector: 'app-code-root-find-dialog',
  template: `
    <div mat-dialog-title class="dialog-header">
      <mat-icon class="dialog-icon">search</mat-icon>
      <span>{{ 'REMOTE_TREE.FIND_TITLE' | translate }}</span>
    </div>
    <mat-dialog-content>
      <div class="dialog-container find-container">
        <p class="intro">{{ 'REMOTE_TREE.FIND_WHERE' | translate: { path: data.path } }}</p>
        <label class="field-label" for="code-root-find">{{ 'REMOTE_TREE.FIND_LABEL' | translate }}</label>
        <div class="find-row">
          <mat-form-field appearance="outline" class="alias-field">
            <input matInput id="code-root-find" [(ngModel)]="query" (keydown.enter)="search()" autocomplete="off" cdkFocusInitial
                   [placeholder]="'REMOTE_TREE.FIND_PLACEHOLDER' | translate">
          </mat-form-field>
          <button mat-flat-button color="primary" (click)="search()" [disabled]="!query.trim() || searching">
            <mat-spinner diameter="16" *ngIf="searching"></mat-spinner>
            <mat-icon *ngIf="!searching">search</mat-icon>{{ 'REMOTE_TREE.FIND_GO' | translate }}
          </button>
        </div>
        <div class="find-error" *ngIf="errorMessage"><mat-icon>error</mat-icon><span>{{ errorMessage }}</span></div>
        <ng-container *ngIf="results">
          <div class="find-summary">
            {{ (results.length === 0 ? 'REMOTE_TREE.FIND_NONE' : results.length === 1 ? 'REMOTE_TREE.FIND_ONE' : 'REMOTE_TREE.FIND_COUNT') | translate: { count: results.length } }}
            <span class="find-warn" *ngIf="truncated"><mat-icon>warning</mat-icon>{{ 'REMOTE_TREE.FIND_TRUNCATED' | translate: { count: results.length } }}</span>
          </div>
          <div class="find-list">
            <button type="button" class="find-item" *ngFor="let r of results" (click)="choose(r)" [title]="r.path">
              <mat-icon>{{ r.isText ? 'description' : 'insert_drive_file' }}</mat-icon>
              <span class="find-name">{{ r.name }}</span>
              <span class="find-folder">{{ folderOf(r.path) }}</span>
            </button>
          </div>
        </ng-container>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">{{ 'CODE_ROOTS.CANCEL' | translate }}</button>
    </mat-dialog-actions>`,
  styleUrls: ['./code-roots-dialog.component.scss'],
  styles: [`
    .find-container { min-width: 0; width: 560px; max-width: 100%; }
    .find-row { display: flex; gap: 8px; align-items: flex-start; }
    .find-row mat-form-field { flex: 1; min-width: 0; }
    .find-row button { margin-top: 4px; }
    .find-row mat-spinner { display: inline-block; margin-right: 6px; }
    .find-error, .find-warn { display: flex; align-items: center; gap: 4px; color: var(--mde-warn, #c62828); }
    .find-warn { color: var(--mde-warning, #b26a00); margin-left: 8px; }
    .find-summary { display: flex; align-items: center; flex-wrap: wrap; margin: 4px 0 6px; font-size: 13px; }
    .find-list { max-height: 50vh; overflow-y: auto; border: 1px solid var(--mde-border, rgba(127,127,127,.3)); border-radius: 4px; }
    .find-item { display: flex; align-items: center; gap: 8px; width: 100%; padding: 6px 8px; border: 0; background: transparent;
                 color: inherit; text-align: left; cursor: pointer; font: inherit; }
    .find-item:hover, .find-item:focus { background: var(--mde-hover, rgba(127,127,127,.12)); outline: none; }
    .find-item mat-icon { flex: none; font-size: 18px; width: 18px; height: 18px; opacity: .7; }
    .find-name { flex: none; font-weight: 500; }
    .find-folder { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; opacity: .65; font-size: 12px; }
  `]
})
export class CodeRootFindDialogComponent implements OnDestroy {
  public query = '';
  public searching = false;
  public results: RemoteFoundEntry[] | null = null;
  public truncated = false;
  public errorMessage = '';
  private sub?: Subscription;

  constructor(@Inject(MAT_DIALOG_DATA) public data: CodeRootFindDialogData,
              public dialogRef: MatDialogRef<CodeRootFindDialogComponent, RemoteFoundEntry | undefined>,
              private codeRoots: CodeRootsService,
              private translate: TranslateService) { }

  search(): void {
    const q = this.query.trim();
    if (!q) { return; }
    this.sub?.unsubscribe();
    this.searching = true;
    this.errorMessage = '';
    this.sub = this.codeRoots.file(this.data.projectRoot, 'find', this.data.path, { query: q }).subscribe({
      next: r => {
        this.searching = false;
        this.results = r.entries ?? [];
        this.truncated = !!r.truncated;
      },
      error: err => {
        this.searching = false;
        this.results = null;
        this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR');
      },
    });
  }

  /** La cartella del risultato, senza il nome del file: «@alias/sorgenti/comuni». */
  folderOf(path: string): string { const i = path.lastIndexOf('/'); return i > 0 ? path.substring(0, i) : path; }

  choose(r: RemoteFoundEntry): void { this.dialogRef.close(r); }

  ngOnDestroy(): void { this.sub?.unsubscribe(); }
}
