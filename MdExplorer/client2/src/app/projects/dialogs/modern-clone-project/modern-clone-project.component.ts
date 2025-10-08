import { Component, OnInit, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { GitMessagesComponent } from '../../../git/components/git-messages/git-messages.component';
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
  public isGitHubRepo = false;
  public authMethod: 'automatic' | 'manual' = 'automatic';

  // For manual authentication (non-GitHub repos)
  public manualCredentials = {
    username: '',
    password: ''
  };

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
    // Get URL from clipboard
    this.mdFileService.getTextFromClipboard().subscribe(clipboard => {
      if (clipboard?.url) {
        this.cloneRequest.url = clipboard.url;
        this.checkIfGitHubRepo();
      }
    });

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
      if (response.hasToken) {
        this.tokenStatus = `Token configured: ${response.maskedToken}`;
      } else {
        this.tokenStatus = 'No GitHub token configured';
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

    const info = new WaitingDialogInfo();
    info.message = "Cloning repository...";
    this.waitingDialog.showMessageBox(info);

    try {
      // Use modern clone endpoint
      const request = {
        url: this.cloneRequest.url,
        localPath: this.cloneRequest.localPath,
        branchName: this.cloneRequest.branchName || null
      };

      // Log the request for debugging
      console.log('[ModernClone] Sending clone request:', request);

      // Call the modern Git service clone method
      this.gitService.modernClone(request).subscribe(
        response => {
          this.waitingDialog.closeMessageBox();

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
          this.showMessage(error.error?.error || 'Clone failed: ' + error.message);
        }
      );
    } catch (error) {
      this.waitingDialog.closeMessageBox();
      this.showMessage('Unexpected error during clone');
    }
  }

  showMessage(message: string): void {
    const dialogRef = this.dialog.open(GitMessagesComponent, {
      width: '400px',
      data: { message: message }
    });
  }

  openTokenSettings(): void {
    // TODO: Open settings dialog to configure GitHub token
    this.showMessage('Please configure your GitHub Personal Access Token in Settings');
  }

  cancel(): void {
    this.dialogRef.close();
  }
}