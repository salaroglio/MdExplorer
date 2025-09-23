import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { GITService } from '../../services/gitservice.service';
import { MatSnackBar } from '@angular/material/snack-bar';

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
  hasExistingToken: boolean = false;
  existingMaskedToken: string = '';
  tokenValid: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<GitTokenDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private gitService: GITService,
    private snackBar: MatSnackBar
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
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error checking token:', err);
        this.isLoading = false;
      }
    });
  }

  testToken(): void {
    if (!this.token) {
      this.snackBar.open('Inserisci un token da testare', 'OK', {
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
          this.snackBar.open('Token valido e configurato con successo!', 'OK', {
            duration: 3000,
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.dialogRef.close(true);
        } else {
          this.snackBar.open('Token non valido. Verifica di aver copiato correttamente il token.', 'OK', {
            duration: 5000,
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          });
        }
        this.isSaving = false;
      },
      error: (err) => {
        console.error('Error setting token:', err);
        this.snackBar.open('Errore nel salvataggio del token', 'OK', {
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