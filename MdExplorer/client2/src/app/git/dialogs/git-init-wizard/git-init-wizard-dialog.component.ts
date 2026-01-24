import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { GITService } from '../../services/gitservice.service';
import { InitRepositoryRequest, InitRepositoryResponse, GITIGNORE_TEMPLATES, GitignoreTemplate } from '../../models/git-init.models';
import { GitSetupRemoteGenericDialogComponent } from '../git-setup-remote-generic-dialog/git-setup-remote-generic-dialog.component';

@Component({
  selector: 'app-git-init-wizard-dialog',
  templateUrl: './git-init-wizard-dialog.component.html',
  styleUrls: ['./git-init-wizard-dialog.component.scss']
})
export class GitInitWizardDialogComponent implements OnInit {
  // Wizard state
  currentStep: number = 0;
  isLoading: boolean = false;

  // Step 1: Git Init
  repositoryPath: string;
  initialBranch: string = 'main';
  gitignoreTemplate: string = 'mdexplorer';
  gitignoreTemplates: GitignoreTemplate[] = GITIGNORE_TEMPLATES;

  // Results
  initSuccess: boolean = false;
  initMessage: string = '';

  constructor(
    public dialogRef: MatDialogRef<GitInitWizardDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { repositoryPath: string },
    private gitService: GITService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.repositoryPath = data.repositoryPath;
  }

  ngOnInit(): void {
    console.log('[GitInitWizard] Initialized for path:', this.repositoryPath);
  }

  /**
   * Initialize Git repository (Step 1)
   */
  initializeGit(): void {
    if (!this.repositoryPath) {
      this.snackBar.open('Repository path is required', 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    this.isLoading = true;

    const request: InitRepositoryRequest = {
      repositoryPath: this.repositoryPath,
      initialBranch: this.initialBranch,
      gitignoreTemplate: this.gitignoreTemplate as any
    };

    console.log('[GitInitWizard] Initializing Git repository:', request);

    this.gitService.initRepository(request).subscribe(
      (response: InitRepositoryResponse) => {
        this.isLoading = false;

        if (response.success) {
          this.initSuccess = true;
          this.initMessage = response.message;
          console.log('[GitInitWizard] ✅ Git repository initialized successfully');

          this.snackBar.open('Git repository initialized successfully!', 'OK', {
            duration: 3000,
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });

          // Move to step 2 (Remote Setup)
          this.currentStep = 1;
        } else {
          console.error('[GitInitWizard] ❌ Initialization failed:', response.message);
          this.snackBar.open(`Initialization failed: ${response.message}`, 'OK', {
            duration: 5000,
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          });
        }
      },
      (error) => {
        this.isLoading = false;
        console.error('[GitInitWizard] ❌ Error initializing repository:', error);

        const errorMessage = error?.error?.message || error?.message || 'Unknown error';
        this.snackBar.open(`Error: ${errorMessage}`, 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    );
  }

  /**
   * Open remote setup dialog (Step 2)
   */
  setupRemote(): void {
    const projectName = this.repositoryPath.split(/[/\\]/).pop() || 'repository';

    const remoteDialogRef = this.dialog.open(GitSetupRemoteGenericDialogComponent, {
      width: '650px',
      data: {
        projectPath: this.repositoryPath,
        projectName: projectName
      }
    });

    remoteDialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('[GitInitWizard] Remote setup completed');
        // Close wizard after successful remote setup
        this.dialogRef.close(true);
      }
    });
  }

  /**
   * Skip remote setup and close wizard
   */
  skipRemoteSetup(): void {
    console.log('[GitInitWizard] Skipping remote setup');
    this.dialogRef.close(true);
  }

  /**
   * Cancel wizard
   */
  cancel(): void {
    this.dialogRef.close(false);
  }

  /**
   * Get selected template description
   */
  getSelectedTemplateDescription(): string {
    const template = this.gitignoreTemplates.find(t => t.value === this.gitignoreTemplate);
    return template ? template.description : '';
  }
}
