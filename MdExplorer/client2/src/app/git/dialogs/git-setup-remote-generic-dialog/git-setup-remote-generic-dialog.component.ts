import { Component, OnInit, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { GITService } from '../../services/gitservice.service';
import { GitCredentialService } from '../../services/git-credential.service';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import {
  GitSetupRemoteGenericDialogData,
  ParseRemoteUrlResponse,
  ValidateRemoteAuthResponse,
  AuthMethod,
  GitProvider,
  PROVIDER_INFO
} from '../../models/remote-setup.models';

@Component({
  selector: 'app-git-setup-remote-generic-dialog',
  templateUrl: './git-setup-remote-generic-dialog.component.html',
  styleUrls: ['./git-setup-remote-generic-dialog.component.scss']
})
export class GitSetupRemoteGenericDialogComponent implements OnInit {
  // Main form fields
  remoteUrl: string = '';
  username: string = '';
  password: string = '';
  token: string = '';

  // Options
  saveCredentials: boolean = true;
  pushAfterAdd: boolean = true;
  remoteName: string = 'origin';

  // Advanced options
  authMethod: AuthMethod = 'username_password';
  createRemoteRepo: boolean = false;
  repoDescription: string = '';
  isPrivate: boolean = true;

  // State
  isLoading: boolean = false;
  isValidating: boolean = false;
  isSetting: boolean = false;
  showAdvanced: boolean = false;
  showPassword: boolean = false;
  error: string | null = null;

  // URL parsing result
  urlInfo: ParseRemoteUrlResponse | null = null;
  validationResult: ValidateRemoteAuthResponse | null = null;

  // Multi-account support (uses unique credentials)
  savedGitHubCredentials: { id: string; username: string; accountName: string }[] = [];
  selectedCredentialId: string | null = null;

  // Provider info for display
  readonly providerInfo = PROVIDER_INFO;

  constructor(
    public dialogRef: MatDialogRef<GitSetupRemoteGenericDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitSetupRemoteGenericDialogData,
    private gitService: GITService,
    private gitCredentialService: GitCredentialService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // Use pre-filled URL if available (from toolbar when reconfiguring credentials)
    if (this.data?.prefilledRemoteUrl) {
      this.remoteUrl = this.data.prefilledRemoteUrl;
      this.onUrlChange();  // Trigger provider detection and GitHub token check
    }
  }

  /**
   * Handle URL input change - parse and detect provider
   */
  onUrlChange(): void {
    this.error = null;
    this.validationResult = null;

    if (!this.remoteUrl || this.remoteUrl.trim().length < 10) {
      this.urlInfo = null;
      this.savedGitHubCredentials = [];
      return;
    }

    this.isValidating = true;
    this.gitService.parseRemoteUrl(this.remoteUrl.trim()).subscribe({
      next: (result) => {
        this.urlInfo = result;
        this.isValidating = false;

        if (!result.isValid) {
          this.error = result.error || this.translate.instant('GIT_REMOTE.URL_INVALID');
        }

        // If GitHub detected, load saved credentials
        if (result.provider === 'github') {
          this.loadSavedGitHubCredentials();
          // GitHub requires PAT, auto-select it
          this.authMethod = 'pat';
        } else {
          this.savedGitHubCredentials = [];
          this.selectedCredentialId = null;
        }
      },
      error: (err) => {
        console.error('Error parsing URL:', err);
        this.isValidating = false;
        this.urlInfo = null;
      }
    });
  }

  /**
   * Validate connection with credentials
   */
  validateConnection(): void {
    if (!this.remoteUrl || !this.urlInfo?.isValid) {
      return;
    }

    this.isValidating = true;
    this.error = null;
    this.validationResult = null;

    const effectivePassword = this.getEffectivePassword();

    this.gitService.validateRemoteAuth({
      remoteUrl: this.remoteUrl,
      username: this.username,
      password: effectivePassword,
      authMethod: this.authMethod
    }).subscribe({
      next: (result) => {
        this.validationResult = result;
        this.isValidating = false;

        if (result.isReachable && result.credentialsValid) {
          this.snackBar.open(this.translate.instant('GIT_REMOTE.VERIFY_SUCCESS'), 'OK', {
            duration: 3000,
            verticalPosition: 'top'
          });
        } else if (result.error) {
          this.error = result.error;
        }
      },
      error: (err) => {
        console.error('Error validating connection:', err);
        this.isValidating = false;
        this.error = this.translate.instant('GIT_REMOTE.VERIFY_ERROR');
      }
    });
  }

  /**
   * Open token creation URL for the detected provider
   */
  openTokenCreationUrl(): void {
    if (this.urlInfo?.tokenCreationUrl) {
      window.open(this.urlInfo.tokenCreationUrl, '_blank');
    }
  }

  /**
   * Get effective password based on auth method
   */
  getEffectivePassword(): string {
    if (this.authMethod === 'pat' && this.token) {
      return this.token;
    }
    return this.password;
  }

  /**
   * Auto-select PAT auth method when user types in token field
   */
  onTokenChange(): void {
    if (this.token && this.token.length > 0) {
      this.authMethod = 'pat';
    }
  }

  /**
   * Load saved GitHub credentials from credential service
   * Now returns unique credentials directly (no more deduplication needed)
   */
  loadSavedGitHubCredentials(): void {
    this.gitCredentialService.getCredentialsByType('GitHub').subscribe({
      next: (credentials) => {
        this.savedGitHubCredentials = credentials;
      },
      error: (err) => {
        console.error('Error loading saved GitHub credentials:', err);
        this.savedGitHubCredentials = [];
      }
    });
  }

  /**
   * Get display names of saved GitHub credentials
   */
  getSavedCredentialNames(): string {
    return this.savedGitHubCredentials
      .map(c => c.username || c.accountName)
      .join(', ');
  }

  /**
   * Handle credential selection change
   */
  onCredentialSelectionChange(): void {
    if (!this.selectedCredentialId) {
      // When deselected (new credentials), clear manual input fields
      this.username = '';
      this.token = '';
      this.password = '';
    }
  }

  /**
   * Get provider display info
   */
  getProviderInfo(): { name: string; icon: string; color: string } | null {
    if (!this.urlInfo?.provider) return null;
    return this.providerInfo[this.urlInfo.provider] || this.providerInfo['generic'];
  }

  /**
   * Check if form is valid for setup
   */
  isFormValid(): boolean {
    if (!this.remoteUrl || !this.urlInfo?.isValid) {
      return false;
    }

    // If using a saved credential
    if (this.selectedCredentialId) {
      return true;
    }

    // Must have credentials (username + password/token)
    const hasCredentials = this.username &&
      (this.authMethod === 'pat' ? this.token : this.password);

    return !!hasCredentials;
  }

  /**
   * Toggle advanced options panel
   */
  toggleAdvanced(): void {
    this.showAdvanced = !this.showAdvanced;
  }

  /**
   * Cancel and close dialog
   */
  onCancel(): void {
    this.dialogRef.close(false);
  }

  /**
   * Setup the remote
   */
  onSetup(): void {
    if (!this.isFormValid()) {
      this.error = this.translate.instant('GIT_REMOTE.FILL_ALL_REQUIRED');
      return;
    }

    this.isSetting = true;
    this.error = null;

    const effectivePassword = this.getEffectivePassword();
    const useExistingCredential = !!this.selectedCredentialId;

    const request = {
      repositoryPath: this.data.projectPath,
      remoteUrl: this.remoteUrl,
      remoteName: this.remoteName || 'origin',
      authMethod: this.authMethod || 'username_password',
      username: useExistingCredential ? '' : (this.username || ''),
      password: useExistingCredential ? '' : (this.authMethod === 'username_password' ? (this.password || '') : ''),
      token: useExistingCredential ? '' : (this.authMethod === 'pat' ? (this.token || '') : ''),
      saveCredentials: this.saveCredentials === true,
      pushAfterAdd: this.pushAfterAdd === true,
      createRemoteRepo: this.createRemoteRepo === true && this.urlInfo?.supportsAutoCreate === true,
      repoDescription: this.repoDescription || '',
      isPrivate: this.isPrivate !== false,
      useSavedToken: false,
      copyFromCredentialId: this.selectedCredentialId || undefined
    };

    console.log('Setup remote request:', request);

    this.gitService.setupRemoteGeneric(request).subscribe({
      next: (response) => {
        this.isSetting = false;
        if (response.success) {
          this.snackBar.open(
            response.message || this.translate.instant('GIT_REMOTE.REMOTE_SUCCESS'),
            'OK',
            { duration: 5000, verticalPosition: 'top' }
          );
          this.dialogRef.close(true);
        } else {
          this.error = response.error || this.translate.instant('GIT_REMOTE.REMOTE_ERROR');
        }
      },
      error: (err) => {
        this.isSetting = false;
        this.error = this.translate.instant('GIT_REMOTE.REMOTE_ERROR_DETAIL', { error: err.message || 'Errore sconosciuto' });
        console.error('Error setting up remote:', err);
      }
    });
  }
}
