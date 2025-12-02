import { Component, OnInit, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { GitMessagesComponent } from '../../../git/components/git-messages/git-messages.component';
import { GitTokenDialogComponent } from '../../../git/dialogs/git-token-dialog/git-token-dialog.component';
import { GITService } from '../../../git/services/gitservice.service';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { ProjectsService } from '../../../md-explorer/services/projects.service';
import { ShowFileMetadata } from '../../../commons/components/show-file-system/show-file-metadata';

interface ModernCloneRequest {
  url: string;
  localPath: string;
  branchName?: string;
  useToken?: boolean;
  useSSH?: boolean;
}

@Component({
  selector: 'app-modern-clone-project',
  templateUrl: './modern-clone-project.component.html',
  styleUrls: ['./modern-clone-project.component.scss']
})
export class ModernCloneProjectComponent implements OnInit {
  public cloneRequest: ModernCloneRequest = {
    url: '',
    localPath: '',
    branchName: '',
    useToken: true,  // Default to token authentication
    useSSH: false
  };

  public hasGitHubToken = false;
  public tokenStatus = '';
  public tokenUsername = '';
  public tokenValid = false;
  public useSavedToken = true;
  public isDeleting = false;
  public isGitHubRepo = false;
  public authMethod: 'automatic' | 'manual' = 'automatic';

  // For manual authentication (non-GitHub repos)
  public manualCredentials = {
    username: '',
    password: ''
  };

  // For Share Project feature: when basePath is provided, the path is auto-computed
  public isPrefilledFromShare = false;

  // URL validation state
  public isValidatingUrl = false;
  public urlValidationResult: { isValid: boolean; error?: string } | null = null;

  constructor(
    private dialog: MatDialog,
    private mdFileService: MdFileService,
    private gitService: GITService,
    private dialogRef: MatDialogRef<ModernCloneProjectComponent>,
    private waitingDialog: WaitingDialogService,
    private projectService: ProjectsService,
    private router: Router,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit(): void {
    // Check if we have prefilled data from URL handler (Share Project feature)
    if (this.data?.prefilledUrl) {
      console.log('[ModernClone] Using prefilled URL from URL handler:', this.data.prefilledUrl);
      this.cloneRequest.url = this.data.prefilledUrl;
      if (this.data.prefilledBranch) {
        this.cloneRequest.branchName = this.data.prefilledBranch;
      }
      if (this.data.prefilledUser) {
        this.manualCredentials.username = this.data.prefilledUser;
      }

      // Handle basePath from Share Project feature
      if (this.data.prefilledBasePath) {
        console.log('[ModernClone] Using prefilled basePath:', this.data.prefilledBasePath);
        this.isPrefilledFromShare = true;
        // Auto-compute the full path: basePath + repoName
        const repoName = this.extractRepoName(this.data.prefilledUrl);
        if (repoName) {
          this.cloneRequest.localPath = `${this.data.prefilledBasePath}\\${repoName}`;
        } else {
          this.cloneRequest.localPath = this.data.prefilledBasePath;
        }
        console.log('[ModernClone] Auto-computed localPath:', this.cloneRequest.localPath);
      }

      this.checkIfGitHubRepo();
    } else {
      // Get URL from clipboard (default behavior)
      this.mdFileService.getTextFromClipboard().subscribe(clipboard => {
        if (clipboard?.url) {
          this.cloneRequest.url = clipboard.url;
          this.checkIfGitHubRepo();
        }
      });
    }

    // Check GitHub token status
    this.checkGitHubToken();

    // When the project changes, navigate to the main environment
    this.projectService.currentProjects$.subscribe(project => {
      if (project != null && project != undefined) {
        this.router.navigate(['/main/navigation/document']);
        this.dialogRef.close();
      }
    });
  }

  checkGitHubToken(): void {
    this.gitService.getGitHubToken().subscribe(response => {
      this.hasGitHubToken = response.hasToken;
      this.tokenUsername = response.username || '';
      this.tokenValid = response.tokenValid;

      if (response.hasToken) {
        if (this.tokenUsername) {
          this.tokenStatus = `${response.maskedToken}`;
        } else {
          this.tokenStatus = `Token: ${response.maskedToken}`;
        }
      } else {
        this.tokenStatus = 'No GitHub token configured';
      }

      // Default to using saved token only if valid
      this.useSavedToken = response.hasToken && response.tokenValid;
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
        this.showMessage('Token eliminato con successo');
        this.hasGitHubToken = false;
        this.tokenStatus = 'No GitHub token configured';
        this.tokenUsername = '';
        this.tokenValid = false;
        this.useSavedToken = false;
        this.isDeleting = false;
      },
      error: (err) => {
        console.error('Error deleting token:', err);
        this.showMessage('Errore nell\'eliminazione del token');
        this.isDeleting = false;
      }
    });
  }

  onUrlChange(): void {
    this.checkIfGitHubRepo();
  }

  checkIfGitHubRepo(): void {
    const url = this.cloneRequest.url.toLowerCase();
    this.isGitHubRepo = url.includes('github.com');

    // Auto-select authentication method based on URL
    if (this.isGitHubRepo && this.hasGitHubToken) {
      this.authMethod = 'automatic';
    } else if (!this.isGitHubRepo) {
      this.authMethod = 'manual';
    }
  }

  openFileSystem(): void {
    let data = new ShowFileMetadata();
    data.start = null;
    data.title = "Select Clone Destination";
    data.typeOfSelection = "Folders";
    data.buttonText = "Select folder";

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.data) {
        // Add repository name to the path
        const repoName = this.extractRepoName(this.cloneRequest.url);
        if (repoName) {
          this.cloneRequest.localPath = `${result.data}\\${repoName}`;
        } else {
          this.cloneRequest.localPath = result.data;
        }
      }
    });
  }

  extractRepoName(url: string): string {
    if (!url) return '';

    // Handle various Git URL formats
    let repoName = url;

    // Remove .git extension if present
    repoName = repoName.replace(/\.git$/, '');

    // Extract from HTTPS URL: https://github.com/user/repo
    if (repoName.includes('github.com/')) {
      const parts = repoName.split('github.com/')[1]?.split('/');
      if (parts && parts.length >= 2) {
        return parts[1];
      }
    }

    // Extract from SSH URL: git@github.com:user/repo
    if (repoName.includes('git@github.com:')) {
      const parts = repoName.split(':')[1]?.split('/');
      if (parts && parts.length >= 2) {
        return parts[1];
      }
    }

    // Fallback: get last part of URL
    const parts = repoName.split('/');
    return parts[parts.length - 1] || 'repository';
  }

  async performClone(): Promise<void> {
    if (!this.cloneRequest.url || !this.cloneRequest.localPath) {
      this.showMessage('Please fill in all required fields');
      return;
    }

    // Step 1: Validate URL reachability before cloning
    const info = new WaitingDialogInfo();
    info.message = "Validating repository URL...";
    this.waitingDialog.showMessageBox(info);
    this.isValidatingUrl = true;

    try {
      // Validate URL first
      const validationResult = await this.gitService.validateRemoteUrl(this.cloneRequest.url).toPromise();

      if (!validationResult?.isReachable) {
        this.waitingDialog.closeMessageBox();
        this.isValidatingUrl = false;
        this.urlValidationResult = { isValid: false, error: validationResult?.error || 'Repository not reachable' };

        if (validationResult?.isAuthenticationError) {
          this.showMessage('Authentication required. Please check your credentials.');
        } else {
          this.showMessage(`Repository URL not reachable: ${validationResult?.error || 'Unknown error'}`);
        }
        return;
      }

      this.urlValidationResult = { isValid: true };
      console.log('[ModernClone] URL validation passed, proceeding with clone');

      // Step 2: Proceed with clone
      info.message = "Cloning repository...";

      // Validate manual credentials if not using saved token
      if (this.isGitHubRepo && !this.useSavedToken && (!this.manualCredentials.username || !this.manualCredentials.password)) {
        this.waitingDialog.closeMessageBox();
        this.isValidatingUrl = false;
        this.showMessage('Inserisci username e password/token per l\'autenticazione');
        return;
      }

      // Use modern clone endpoint
      const request = {
        url: this.cloneRequest.url,
        localPath: this.cloneRequest.localPath,
        branchName: this.cloneRequest.branchName || null,
        useSavedToken: this.isGitHubRepo ? this.useSavedToken : true,
        username: (!this.useSavedToken && this.manualCredentials.username) ? this.manualCredentials.username : null,
        password: (!this.useSavedToken && this.manualCredentials.password) ? this.manualCredentials.password : null
      };

      // Log the request for debugging
      console.log('[ModernClone] Sending clone request:', request);

      // Call the modern Git service clone method
      this.gitService.modernClone(request).subscribe(
        response => {
          this.waitingDialog.closeMessageBox();
          this.isValidatingUrl = false;

          if (response.success) {
            // Set the new project folder
            this.projectService.setNewFolderProject(this.cloneRequest.localPath);
            this.showMessage('Repository cloned successfully!');
            this.dialogRef.close(this.cloneRequest);
          } else {
            this.showMessage(response.error || 'Clone failed');
          }
        },
        error => {
          this.waitingDialog.closeMessageBox();
          this.isValidatingUrl = false;
          this.showMessage(error.error?.error || 'Clone failed: ' + error.message);
        }
      );
    } catch (error: any) {
      this.waitingDialog.closeMessageBox();
      this.isValidatingUrl = false;
      this.showMessage('Unexpected error: ' + (error?.message || 'Unknown error'));
    }
  }

  showMessage(message: string): void {
    const dialogRef = this.dialog.open(GitMessagesComponent, {
      width: '400px',
      data: { message: message }
    });
  }

  openTokenSettings(): void {
    const dialogRef = this.dialog.open(GitTokenDialogComponent, {
      width: '500px',
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Token was saved successfully, refresh token status
        this.checkGitHubToken();
      }
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}