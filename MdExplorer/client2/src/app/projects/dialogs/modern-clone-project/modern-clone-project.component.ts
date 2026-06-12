import { Component, OnInit, Inject } from '@angular/core';
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { Router } from '@angular/router';
import { skip } from 'rxjs/operators';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { GitMessagesComponent } from '../../../git/components/git-messages/git-messages.component';
import { GitTokenDialogComponent } from '../../../git/dialogs/git-token-dialog/git-token-dialog.component';
import { GITService } from '../../../git/services/gitservice.service';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { ProjectsService } from '../../../md-explorer/services/projects.service';
import { ShowFileMetadata } from '../../../commons/components/show-file-system/show-file-metadata';
import { TranslateService } from '@ngx-translate/core';

interface ModernCloneRequest {
  url: string;
  localPath: string;
  branchName?: string;
  useToken?: boolean;
  useSSH?: boolean;
}

// Provider types for authentication
type GitProvider = 'github' | 'gitlab' | 'azure' | 'bitbucket' | 'scm-manager' | 'gitea' | 'generic';
type AuthType = 'oauth' | 'basic';

// OAuth providers: GCM handles authentication via browser
const OAUTH_PROVIDERS: GitProvider[] = ['github', 'gitlab', 'azure', 'bitbucket'];
// Basic Auth providers: always require manual credentials
const BASIC_AUTH_PROVIDERS: GitProvider[] = ['scm-manager', 'gitea', 'generic'];

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

  // Provider-based authentication
  public detectedProvider: GitProvider = 'generic';
  public authType: AuthType = 'basic';
  public showCredentialForm = true;  // Show form for basic auth providers

  // For manual authentication (non-GitHub repos)
  public manualCredentials = {
    username: '',
    password: ''
  };

  // Account selector for multi-account support
  public availableAccounts: Array<{ id: string; username: string; accountName: string }> = [];
  public filteredAccounts: Array<{ id: string; username: string; accountName: string }> = [];
  public isLoadingAccounts = false;

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
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
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
          this.cloneRequest.localPath = this.joinPath(this.data.prefilledBasePath, repoName);
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
        this.tokenStatus = this.translate.instant('CLONE.NO_GITHUB_TOKEN');
      }

      // Default to using saved token only if valid
      this.useSavedToken = response.hasToken && response.tokenValid;
    });
  }

  deleteToken(): void {
    const message = this.tokenUsername
      ? this.translate.instant('CLONE.DELETE_TOKEN_CONFIRM', { username: this.tokenUsername })
      : this.translate.instant('CLONE.DELETE_TOKEN_CONFIRM_GENERIC');

    const confirmed = confirm(message);
    if (!confirmed) return;

    this.isDeleting = true;
    this.gitService.deleteGitHubToken().subscribe({
      next: () => {
        this.showMessage(this.translate.instant('CLONE.TOKEN_DELETED'));
        this.hasGitHubToken = false;
        this.tokenStatus = this.translate.instant('CLONE.NO_GITHUB_TOKEN');
        this.tokenUsername = '';
        this.tokenValid = false;
        this.useSavedToken = false;
        this.isDeleting = false;
      },
      error: (err) => {
        console.error('Error deleting token:', err);
        this.showMessage(this.translate.instant('CLONE.TOKEN_DELETE_ERROR'));
        this.isDeleting = false;
      }
    });
  }

  onUrlChange(): void {
    this.detectProviderFromUrl();
  }

  /**
   * Detects the Git provider from URL and sets authentication type accordingly.
   * OAuth providers (GitHub, GitLab, Azure, Bitbucket): GCM handles auth via browser
   * Basic Auth providers (SCM-Manager, Gitea, Generic): Manual credentials required
   */
  detectProviderFromUrl(): void {
    const url = this.cloneRequest.url.toLowerCase();

    // Detect provider from URL
    if (url.includes('github.com')) {
      this.detectedProvider = 'github';
    } else if (url.includes('gitlab.com') || url.includes('gitlab')) {
      this.detectedProvider = 'gitlab';
    } else if (url.includes('dev.azure.com') || url.includes('visualstudio.com')) {
      this.detectedProvider = 'azure';
    } else if (url.includes('bitbucket.org') || url.includes('bitbucket')) {
      this.detectedProvider = 'bitbucket';
    } else if (url.includes('scm-manager') || url.includes('/scm/')) {
      this.detectedProvider = 'scm-manager';
    } else if (url.includes('gitea') || url.includes(':3000/')) {
      this.detectedProvider = 'gitea';
    } else {
      this.detectedProvider = 'generic';
    }

    // Set legacy flag for backward compatibility
    this.isGitHubRepo = this.detectedProvider === 'github';

    // Determine auth type based on provider
    if (OAUTH_PROVIDERS.includes(this.detectedProvider)) {
      this.authType = 'oauth';
      this.showCredentialForm = false;  // GCM handles OAuth via browser
      this.authMethod = 'automatic';
    } else {
      this.authType = 'basic';
      this.showCredentialForm = true;   // Always show form for basic auth
      this.authMethod = 'manual';
    }

    console.log(`[ModernClone] Detected provider: ${this.detectedProvider}, authType: ${this.authType}, showForm: ${this.showCredentialForm}`);

    // Load available accounts for this provider
    this.loadAccountsForProvider();
  }

  // Keep old method name for backward compatibility
  checkIfGitHubRepo(): void {
    this.detectProviderFromUrl();
  }

  /**
   * Loads available accounts for the detected provider
   */
  loadAccountsForProvider(): void {
    if (!this.detectedProvider || this.detectedProvider === 'generic') {
      this.availableAccounts = [];
      this.filteredAccounts = [];
      return;
    }

    // Map provider to account type
    const accountTypeMap: Record<GitProvider, string> = {
      'github': 'GitHub',
      'gitlab': 'GitLab',
      'azure': 'Azure',
      'bitbucket': 'Bitbucket',
      'scm-manager': 'Generic',  // SCM-Manager uses Generic type
      'gitea': 'Generic',
      'generic': 'Generic'
    };

    const accountType = accountTypeMap[this.detectedProvider];
    this.isLoadingAccounts = true;

    this.gitService.getUsernamesByType(accountType).subscribe({
      next: (accounts) => {
        this.availableAccounts = accounts;
        this.filteredAccounts = [...accounts];
        this.isLoadingAccounts = false;
        console.log(`[ModernClone] Loaded ${accounts.length} accounts for ${accountType}`);
      },
      error: (err) => {
        console.error('[ModernClone] Error loading accounts:', err);
        this.availableAccounts = [];
        this.filteredAccounts = [];
        this.isLoadingAccounts = false;
      }
    });
  }

  /**
   * Filters accounts based on user input
   */
  filterAccounts(value: string): void {
    if (!value) {
      this.filteredAccounts = [...this.availableAccounts];
      return;
    }
    const filterValue = value.toLowerCase();
    this.filteredAccounts = this.availableAccounts.filter(
      account => account.username.toLowerCase().includes(filterValue)
    );
  }

  /**
   * Selects an account from the dropdown
   */
  selectAccount(username: string): void {
    this.manualCredentials.username = username;
  }

  /**
   * Display function for autocomplete
   */
  displayAccountFn(value: string): string {
    return value || '';
  }

  /**
   * Deletes an account from the list
   */
  deleteAccount(account: { id: string; username: string; accountName: string }, event: Event): void {
    event.stopPropagation();  // Prevent dropdown from selecting the item

    const confirmed = confirm(this.translate.instant('CLONE.DELETE_ACCOUNT_CONFIRM', { username: account.username }));
    if (!confirmed) return;

    this.gitService.deleteGitAccount(account.id).subscribe({
      next: (response) => {
        if (response.success) {
          // Remove from local lists
          this.availableAccounts = this.availableAccounts.filter(a => a.id !== account.id);
          this.filteredAccounts = this.filteredAccounts.filter(a => a.id !== account.id);

          // Clear username if it was the deleted one
          if (this.manualCredentials.username === account.username) {
            this.manualCredentials.username = '';
          }

          this.showMessage(this.translate.instant('CLONE.ACCOUNT_DELETED'));
        } else {
          this.showMessage(response.message || this.translate.instant('CLONE.ACCOUNT_DELETE_ERROR'));
        }
      },
      error: (err) => {
        console.error('[ModernClone] Error deleting account:', err);
        this.showMessage(this.translate.instant('CLONE.ACCOUNT_DELETE_ERROR'));
      }
    });
  }

  openFileSystem(): void {
    let data = new ShowFileMetadata();
    data.start = null;
    data.title = this.translate.instant('CLONE.SELECT_CLONE_DEST');
    data.typeOfSelection = "Folders";
    data.buttonText = this.translate.instant('PROJECTS.SELECT_FOLDER_BTN');

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
          this.cloneRequest.localPath = this.joinPath(result.data, repoName);
        } else {
          this.cloneRequest.localPath = result.data;
        }
      }
    });
  }

  /** Join a base directory and a name using the separator the base already uses. */
  private joinPath(base: string, name: string): string {
    const sep = base.includes('/') ? '/' : '\\';
    const trimmed = base.endsWith('/') || base.endsWith('\\') ? base.slice(0, -1) : base;
    return `${trimmed}${sep}${name}`;
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
      this.showMessage(this.translate.instant('CLONE.FILL_REQUIRED'));
      return;
    }

    const info = new WaitingDialogInfo();
    this.isValidatingUrl = true;

    try {
      // Step 1: Validate URL reachability before cloning
      // Skip validation for Basic Auth providers (SCM-Manager, Gitea, Generic)
      // because they require credentials even for ls-remote
      const skipValidation = this.authType === 'basic';

      if (!skipValidation) {
        info.message = this.translate.instant('CLONE.VALIDATING_URL');
        this.waitingDialog.showMessageBox(info);

        // Validate URL first (only for OAuth providers)
        const validationResult = await this.gitService.validateRemoteUrl(this.cloneRequest.url).toPromise();

        if (!validationResult?.isReachable) {
          this.waitingDialog.closeMessageBox();
          this.isValidatingUrl = false;
          this.urlValidationResult = { isValid: false, error: validationResult?.error || 'Repository not reachable' };

          if (validationResult?.isAuthenticationError) {
            this.showMessage(this.translate.instant('CLONE.AUTH_REQUIRED'));
          } else {
            this.showMessage(this.translate.instant('CLONE.URL_NOT_REACHABLE', { error: validationResult?.error || 'Unknown error' }));
          }
          return;
        }

        this.urlValidationResult = { isValid: true };
        console.log('[ModernClone] URL validation passed, proceeding with clone');
      } else {
        console.log('[ModernClone] Skipping URL validation for Basic Auth provider, proceeding directly with clone');
        this.waitingDialog.showMessageBox(info);
      }

      // Step 2: Proceed with clone
      const useAutomaticAuth = this.authType === 'oauth' && !this.showCredentialForm;
      if (useAutomaticAuth) {
        info.message = this.translate.instant('CLONE.CLONING_OAUTH', { provider: this.getProviderDisplayName() });
      } else {
        info.message = this.translate.instant('CLONE.CLONING');
      }

      // Determine if we need manual credentials
      const needsManualCredentials = this.showCredentialForm || this.authType === 'basic';

      // Validate manual credentials when required
      if (needsManualCredentials && (!this.manualCredentials.username || !this.manualCredentials.password)) {
        this.waitingDialog.closeMessageBox();
        this.isValidatingUrl = false;
        this.showMessage(this.translate.instant('CLONE.ENTER_CREDENTIALS'));
        return;
      }

      // Build clone request based on auth type
      const request = {
        url: this.cloneRequest.url,
        localPath: this.cloneRequest.localPath,
        branchName: this.cloneRequest.branchName || null,
        // For OAuth providers without manual override, let GCM handle authentication
        useSavedToken: useAutomaticAuth || (this.isGitHubRepo && this.useSavedToken),
        // Pass credentials if manual auth is required
        username: needsManualCredentials ? this.manualCredentials.username : null,
        password: needsManualCredentials ? this.manualCredentials.password : null
      };

      // Log the request for debugging
      console.log(`[ModernClone] Sending clone request (provider: ${this.detectedProvider}, authType: ${this.authType}, manual: ${needsManualCredentials}):`, request);

      // Call the modern Git service clone method
      this.gitService.modernClone(request).subscribe(
        response => {
          this.waitingDialog.closeMessageBox();
          this.isValidatingUrl = false;

          if (response.success) {
            // Set the new project folder
            this.projectService.setNewFolderProject(this.cloneRequest.localPath);
            this.showMessage(this.translate.instant('CLONE.CLONE_SUCCESS'));
            this.dialogRef.close(this.cloneRequest);
          } else {
            this.showMessage(response.error || this.translate.instant('CLONE.CLONE_FAILED'));
          }
        },
        error => {
          this.waitingDialog.closeMessageBox();
          this.isValidatingUrl = false;
          this.showMessage(error.error?.error || this.translate.instant('CLONE.CLONE_FAILED') + ': ' + error.message);
        }
      );
    } catch (error: any) {
      this.waitingDialog.closeMessageBox();
      this.isValidatingUrl = false;
      this.showMessage(this.translate.instant('CLONE.UNEXPECTED_ERROR', { error: error?.message || 'Unknown error' }));
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

  /**
   * Gets a user-friendly display name for the detected provider
   */
  getProviderDisplayName(): string {
    const names: Record<GitProvider, string> = {
      'github': 'GitHub',
      'gitlab': 'GitLab',
      'azure': 'Azure DevOps',
      'bitbucket': 'Bitbucket',
      'scm-manager': 'SCM-Manager',
      'gitea': 'Gitea',
      'generic': 'Git Server'
    };
    return names[this.detectedProvider] || 'Git Server';
  }

  /**
   * Toggle credential form visibility for OAuth providers
   * (Allows manual override if user wants to enter credentials manually)
   */
  toggleCredentialForm(): void {
    this.showCredentialForm = !this.showCredentialForm;
    if (this.showCredentialForm) {
      this.authMethod = 'manual';
    } else {
      this.authMethod = 'automatic';
    }
  }
}