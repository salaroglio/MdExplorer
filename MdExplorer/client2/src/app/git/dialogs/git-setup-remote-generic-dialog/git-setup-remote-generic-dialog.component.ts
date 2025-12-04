import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { GITService } from '../../services/gitservice.service';
import { MatSnackBar } from '@angular/material/snack-bar';
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

  // Saved GitHub token info
  hasSavedGitHubToken: boolean = false;
  savedTokenUsername: string = '';
  savedTokenValid: boolean = false;
  useSavedToken: boolean = true;

  // Provider info for display
  readonly providerInfo = PROVIDER_INFO;

  constructor(
    public dialogRef: MatDialogRef<GitSetupRemoteGenericDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitSetupRemoteGenericDialogData,
    private gitService: GITService,
    private snackBar: MatSnackBar
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
      this.hasSavedGitHubToken = false;
      return;
    }

    this.isValidating = true;
    this.gitService.parseRemoteUrl(this.remoteUrl.trim()).subscribe({
      next: (result) => {
        this.urlInfo = result;
        this.isValidating = false;

        if (!result.isValid) {
          this.error = result.error || 'URL non valido';
        }

        // If GitHub detected, check for saved token
        if (result.provider === 'github') {
          this.checkSavedGitHubToken();
        } else {
          this.hasSavedGitHubToken = false;
        }
      },
      error: (err) => {
        console.error('Error parsing URL:', err);
        this.isValidating = false;
        this.urlInfo = null;
        this.hasSavedGitHubToken = false;
      }
    });
  }

  /**
   * Check if there's a saved GitHub token
   */
  checkSavedGitHubToken(): void {
    this.gitService.getGitHubToken().subscribe({
      next: (result) => {
        this.hasSavedGitHubToken = result.hasToken;
        this.savedTokenUsername = result.username || '';
        this.savedTokenValid = result.tokenValid;
        // Default to using saved token if it exists and is valid
        this.useSavedToken = result.hasToken && result.tokenValid;
      },
      error: () => {
        this.hasSavedGitHubToken = false;
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
          this.snackBar.open('Connessione verificata con successo!', 'OK', {
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
        this.error = 'Errore durante la verifica della connessione';
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

    // If using saved GitHub token, no need for manual credentials
    if (this.urlInfo?.provider === 'github' && this.useSavedToken && this.hasSavedGitHubToken) {
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
      this.error = 'Compila tutti i campi richiesti';
      return;
    }

    this.isSetting = true;
    this.error = null;

    const effectivePassword = this.getEffectivePassword();
    const shouldUseSavedToken = this.urlInfo?.provider === 'github' && this.useSavedToken && this.hasSavedGitHubToken;

    const request = {
      repositoryPath: this.data.projectPath,
      remoteUrl: this.remoteUrl,
      remoteName: this.remoteName || 'origin',
      authMethod: this.authMethod || 'username_password',
      username: shouldUseSavedToken ? '' : (this.username || ''),
      password: shouldUseSavedToken ? '' : (this.authMethod === 'username_password' ? (this.password || '') : ''),
      token: shouldUseSavedToken ? '' : (this.authMethod === 'pat' ? (this.token || '') : ''),
      saveCredentials: this.saveCredentials === true,
      pushAfterAdd: this.pushAfterAdd === true,
      createRemoteRepo: this.createRemoteRepo === true && this.urlInfo?.supportsAutoCreate === true,
      repoDescription: this.repoDescription || '',
      isPrivate: this.isPrivate !== false,
      useSavedToken: shouldUseSavedToken
    };

    console.log('Setup remote request:', request);

    this.gitService.setupRemoteGeneric(request).subscribe({
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
