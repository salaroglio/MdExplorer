import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { GITService } from '../../services/gitservice.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { GitTokenDialogComponent } from '../git-token-dialog/git-token-dialog.component';

export interface GitSetupRemoteDialogData {
  projectPath: string;
  projectName: string;
}

@Component({
  selector: 'app-git-setup-remote-dialog',
  templateUrl: './git-setup-remote-dialog.component.html',
  styleUrls: ['./git-setup-remote-dialog.component.scss']
})
export class GitSetupRemoteDialogComponent implements OnInit {
  organization: string = '';
  repositoryName: string = '';
  repositoryDescription: string = '';
  isPrivate: boolean = true;
  saveOrganization: boolean = true;
  pushAfterAdd: boolean = true;
  hasToken: boolean = false;

  isLoading = false;
  isSetting = false;
  error: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<GitSetupRemoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitSetupRemoteDialogData,
    private gitService: GITService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    // Initialize repository name with project name
    this.repositoryName = this.extractProjectName(data.projectName);
  }

  ngOnInit(): void {
    this.loadSavedOrganization();
    this.checkToken();
  }

  loadSavedOrganization(): void {
    this.isLoading = true;
    this.gitService.getGitHubOrganization().subscribe({
      next: (org) => {
        this.organization = org;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading organization:', err);
        this.isLoading = false;
      }
    });
  }

  checkToken(): void {
    this.gitService.getGitHubToken().subscribe({
      next: (result) => {
        this.hasToken = result.hasToken && result.tokenValid;
      },
      error: (err) => {
        console.error('Error checking token:', err);
        this.hasToken = false;
      }
    });
  }

  openTokenSettings(): void {
    const dialogRef = this.dialog.open(GitTokenDialogComponent, {
      width: '650px',
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // Token was configured successfully, update the status
        this.checkToken();
        this.snackBar.open('Token configurato con successo!', 'OK', {
          duration: 3000,
          verticalPosition: 'top'
        });
      }
    });
  }

  extractProjectName(fullPath: string): string {
    // Extract the folder name from the full path
    const parts = fullPath.replace(/\\/g, '/').split('/');
    return parts[parts.length - 1] || 'repository';
  }

  getGitHubUrl(): string {
    if (this.organization && this.repositoryName) {
      return `https://github.com/${this.organization}/${this.repositoryName}.git`;
    }
    return '';
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onSetup(): void {
    if (!this.organization || !this.repositoryName) {
      this.error = 'Organizzazione e nome repository sono richiesti';
      return;
    }

    this.isSetting = true;
    this.error = null;

    // Save organization if requested
    if (this.saveOrganization && this.organization) {
      this.gitService.saveGitHubOrganization(this.organization).subscribe();
    }

    // Setup the remote with additional parameters
    this.gitService.setupGitHubRemote(
      this.data.projectPath,
      this.organization,
      this.repositoryName,
      this.saveOrganization,
      this.pushAfterAdd,
      this.repositoryDescription,
      this.isPrivate
    ).subscribe({
      next: (response) => {
        this.isSetting = false;
        if (response.success) {
          this.snackBar.open(
            response.message || 'Remote configurato con successo',
            'OK',
            { duration: 5000, verticalPosition: 'top' }
          );
          this.dialogRef.close(true);
        } else {
          this.error = response.error || 'Errore durante la configurazione del remote';
        }
      },
      error: (err) => {
        this.isSetting = false;
        this.error = 'Errore durante la configurazione: ' + (err.message || 'Errore sconosciuto');
        console.error('Error setting up remote:', err);
      }
    });
  }
}