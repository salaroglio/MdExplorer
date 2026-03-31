import { Component, OnInit, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { GITService } from '../../services/gitservice.service';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-git-token-dialog',
  templateUrl: './git-token-dialog.component.html',
  styleUrls: ['./git-token-dialog.component.scss']
})
export class GitTokenDialogComponent implements OnInit {
  token: string = '';
  hideToken: boolean = true;
  isLoading: boolean = false;
  isSaving: boolean = false;
  isDeleting: boolean = false;
  hasExistingToken: boolean = false;
  existingMaskedToken: string = '';
  tokenValid: boolean = false;
  tokenUsername: string = '';

  constructor(
    public dialogRef: MatDialogRef<GitTokenDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private gitService: GITService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.checkExistingToken();
  }

  checkExistingToken(): void {
    this.isLoading = true;
    this.gitService.getGitHubToken().subscribe({
      next: (result) => {
        this.hasExistingToken = result.hasToken;
        this.existingMaskedToken = result.maskedToken || '';
        this.tokenValid = result.tokenValid;
        this.tokenUsername = result.username || '';
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error checking token:', err);
        this.isLoading = false;
      }
    });
  }

  deleteToken(): void {
    const message = this.tokenUsername
      ? `Vuoi eliminare il token GitHub dell'account "${this.tokenUsername}"?`
      : 'Vuoi davvero eliminare il token GitHub salvato?';

    const confirmed = confirm(message);
    if (!confirmed) return;

    this.isDeleting = true;
    this.gitService.deleteGitHubToken().subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('GIT_TOKEN.TOKEN_DELETED'), 'OK', {
          duration: 3000,
          verticalPosition: 'top'
        });
        this.hasExistingToken = false;
        this.existingMaskedToken = '';
        this.tokenValid = false;
        this.tokenUsername = '';
        this.isDeleting = false;
      },
      error: (err) => {
        console.error('Error deleting token:', err);
        this.snackBar.open(this.translate.instant('GIT_TOKEN.DELETE_ERROR'), 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
        this.isDeleting = false;
      }
    });
  }

  testToken(): void {
    if (!this.token) {
      this.snackBar.open(this.translate.instant('GIT_TOKEN.ENTER_TOKEN'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    this.isSaving = true;
    // First save the token
    this.gitService.setGitHubToken(this.token).subscribe({
      next: (result) => {
        if (result.tokenValid) {
          this.snackBar.open(this.translate.instant('GIT_TOKEN.VALID_SUCCESS'), 'OK', {
            duration: 3000,
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.dialogRef.close(true);
        } else {
          this.snackBar.open(this.translate.instant('GIT_TOKEN.INVALID_TOKEN'), 'OK', {
            duration: 5000,
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          });
        }
        this.isSaving = false;
      },
      error: (err) => {
        console.error('Error setting token:', err);
        this.snackBar.open(this.translate.instant('GIT_TOKEN.SAVE_ERROR'), 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
        this.isSaving = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  openGitHubTokenPage(): void {
    window.open('https://github.com/settings/tokens/new?scopes=repo', '_blank');
  }
}