import { Component, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { GITService } from '../../services/gitservice.service';

@Component({
  selector: 'app-git-add-submodule-dialog',
  templateUrl: './git-add-submodule-dialog.component.html',
  styleUrls: ['./git-add-submodule-dialog.component.scss']
})
export class GitAddSubmoduleDialogComponent {
  repoUrl: string = '';
  destinationPath: string = '';
  branch: string = '';

  isAdding: boolean = false;
  isValidating: boolean = false;
  error: string = '';
  validationMessage: string = '';
  validationOk: boolean = false;

  // Once the user edits the destination manually, stop auto-deriving it from the URL
  private destinationTouched: boolean = false;

  private readonly urlPattern = /^(https?:\/\/.+|git@[^:]+:.+|ssh:\/\/.+)$/;

  constructor(
    public dialogRef: MatDialogRef<GitAddSubmoduleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { projectPath: string; connectionId: string },
    private gitService: GITService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) { }

  onUrlChange(): void {
    this.error = '';
    this.validationMessage = '';
    this.validationOk = false;

    if (!this.destinationTouched && this.isUrlValid()) {
      this.destinationPath = this.deriveRepoName(this.repoUrl);
    }
  }

  onDestinationInput(): void {
    this.destinationTouched = true;
    this.error = '';
  }

  isUrlValid(): boolean {
    return this.urlPattern.test(this.repoUrl.trim());
  }

  isDestinationValid(): boolean {
    const dest = this.destinationPath.trim();
    if (!dest) { return false; }
    // No absolute paths (drive letter or leading slash/backslash), no .. segments
    if (/^[A-Za-z]:/.test(dest) || /^[/\\]/.test(dest)) { return false; }
    return !dest.split(/[/\\]/).some(segment => segment === '..');
  }

  isFormValid(): boolean {
    return this.isUrlValid() && this.isDestinationValid();
  }

  validateUrl(): void {
    if (!this.repoUrl.trim()) { return; }

    this.isValidating = true;
    this.validationMessage = '';
    this.validationOk = false;

    this.gitService.validateRemoteUrl(this.repoUrl.trim()).subscribe(result => {
      this.isValidating = false;
      if (result.isReachable) {
        this.validationOk = true;
        this.validationMessage = this.translate.instant('GIT_SUBMODULE.URL_REACHABLE', { count: result.referenceCount || 0 });
      } else {
        this.validationMessage = result.error || this.translate.instant('GIT_SUBMODULE.URL_UNREACHABLE');
      }
    });
  }

  onAdd(): void {
    if (!this.isFormValid() || this.isAdding) { return; }

    this.isAdding = true;
    this.error = '';

    this.gitService.addSubmodule(
      this.data.projectPath,
      this.repoUrl.trim(),
      this.destinationPath.trim(),
      this.branch.trim() || undefined,
      this.data.connectionId
    ).subscribe(response => {
      this.isAdding = false;

      if (response.success) {
        this.snackBar.open(
          response.message || this.translate.instant('GIT_SUBMODULE.SUCCESS'),
          'OK',
          { duration: 4000, verticalPosition: 'top', panelClass: ['success-snackbar'] }
        );
        this.dialogRef.close(true);
      } else {
        this.error = response.error || this.translate.instant('COMMON.ERROR');
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  private deriveRepoName(url: string): string {
    // Last segment after '/' (or after ':' for scp-style git@host:org/repo.git), without .git
    const trimmed = url.trim().replace(/\/+$/, '');
    const lastSlash = Math.max(trimmed.lastIndexOf('/'), trimmed.lastIndexOf(':'));
    const segment = lastSlash >= 0 ? trimmed.substring(lastSlash + 1) : trimmed;
    return segment.replace(/\.git$/i, '');
  }
}
