import { Component, OnInit, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { GitAccountService } from '../../services/git-account.service';
import { GitAccount, CreateGitAccountRequest } from '../../models/git-account.model';

export interface GitAccountManagementDialogData {
  repositoryPath: string;
  repositoryName: string;
}

@Component({
  selector: 'app-git-account-management-dialog',
  templateUrl: './git-account-management-dialog.component.html',
  styleUrls: ['./git-account-management-dialog.component.scss']
})
export class GitAccountManagementDialogComponent implements OnInit {
  isLoading = false;
  currentAccount: GitAccount | null = null;
  showCreateForm = false;

  // Form fields
  accountName = '';
  accountType: 'GitHub' | 'GitLab' | 'Bitbucket' | 'Generic' = 'GitHub';
  gitHubPAT = '';
  gitLabToken = '';
  bitbucketAppPassword = '';
  httpsPassword = '';
  authUsername = '';
  preferredAuthMethod: 'token' | 'username_password' = 'token';
  username = '';
  email = '';
  notes = '';
  hideToken = true;

  accountTypes = [
    { value: 'GitHub', label: 'GitHub' },
    { value: 'GitLab', label: 'GitLab' },
    { value: 'Bitbucket', label: 'Bitbucket' },
    { value: 'Generic', label: 'Generic Git' }
  ];

  constructor(
    public dialogRef: MatDialogRef<GitAccountManagementDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitAccountManagementDialogData,
    private gitAccountService: GitAccountService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadCurrentAccount();
  }

  loadCurrentAccount(): void {
    this.isLoading = true;
    this.gitAccountService.getAccountForRepository(this.data.repositoryPath).subscribe({
      next: (account) => {
        this.currentAccount = account;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading account:', err);
        this.isLoading = false;
        // No account is OK, user can create one
      }
    });
  }

  showCreate(): void {
    this.showCreateForm = true;
    // Pre-fill with defaults
    this.accountName = `${this.accountType} Account`;
  }

  cancelCreate(): void {
    this.showCreateForm = false;
    this.resetForm();
  }

  resetForm(): void {
    this.accountName = '';
    this.accountType = 'GitHub';
    this.gitHubPAT = '';
    this.gitLabToken = '';
    this.bitbucketAppPassword = '';
    this.httpsPassword = '';
    this.authUsername = '';
    this.preferredAuthMethod = 'token';
    this.username = '';
    this.email = '';
    this.notes = '';
  }

  saveAccount(): void {
    // Validation
    if (!this.accountName.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.ACCOUNT_NAME_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    if (this.accountType === 'GitHub' && !this.gitHubPAT.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.GITHUB_TOKEN_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    if (this.accountType === 'GitLab' && !this.gitLabToken.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.GITLAB_TOKEN_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    if (this.accountType === 'Bitbucket') {
      if (this.preferredAuthMethod === 'token' && !this.bitbucketAppPassword.trim()) {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.BITBUCKET_PASSWORD_REQUIRED'), 'OK', {
          duration: 3000,
          verticalPosition: 'top'
        });
        return;
      }
      if (this.preferredAuthMethod === 'username_password' &&
          (!this.authUsername.trim() || !this.httpsPassword.trim())) {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.USER_PASS_REQUIRED'), 'OK', {
          duration: 3000,
          verticalPosition: 'top'
        });
        return;
      }
    }

    if (this.accountType === 'Generic' && (!this.authUsername.trim() || !this.httpsPassword.trim())) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.GENERIC_CREDS_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    const request: CreateGitAccountRequest = {
      repositoryPath: this.data.repositoryPath,
      accountName: this.accountName.trim(),
      accountType: this.accountType,
      gitHubPAT: this.accountType === 'GitHub' ? this.gitHubPAT.trim() : undefined,
      gitLabToken: this.accountType === 'GitLab' ? this.gitLabToken.trim() : undefined,
      bitbucketAppPassword: this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'token'
        ? this.bitbucketAppPassword.trim() : undefined,
      httpsPassword: (this.accountType === 'Generic' ||
        (this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'username_password'))
        ? this.httpsPassword.trim() : undefined,
      authUsername: (this.accountType === 'Generic' || this.accountType === 'Bitbucket')
        ? this.authUsername.trim() : undefined,
      preferredAuthMethod: (this.accountType === 'Bitbucket' || this.accountType === 'Generic')
        ? this.preferredAuthMethod : undefined,
      username: this.username.trim() || undefined,
      email: this.email.trim() || undefined,
      notes: this.notes.trim() || undefined,
      isActive: true
    };

    this.isLoading = true;
    this.gitAccountService.createAccount(request).subscribe({
      next: (account) => {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.CREATED_SUCCESS'), 'OK', {
          duration: 3000,
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        this.currentAccount = account;
        this.showCreateForm = false;
        this.resetForm();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error creating account:', err);
        const message = err.error?.error || err.error?.title || this.translate.instant('GIT_ACCOUNT.CREATE_ERROR');
        this.snackBar.open(message, 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
        this.isLoading = false;
      }
    });
  }

  deleteAccount(): void {
    if (!this.currentAccount || !this.currentAccount.id) return;

    const confirmed = confirm(`Vuoi davvero eliminare l'account "${this.currentAccount.accountName}"?`);
    if (!confirmed) return;

    this.isLoading = true;
    this.gitAccountService.deleteAccount(this.currentAccount.id).subscribe({
      next: (result) => {
        if (result.success) {
          this.snackBar.open(this.translate.instant('GIT_ACCOUNT.DELETED_SUCCESS'), 'OK', {
            duration: 3000,
            verticalPosition: 'top'
          });
          this.currentAccount = null;
          this.isLoading = false;
        }
      },
      error: (err) => {
        console.error('Error deleting account:', err);
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.DELETE_ERROR'), 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
        this.isLoading = false;
      }
    });
  }

  editAccount(): void {
    if (!this.currentAccount) return;

    // Fill form with current account data
    this.accountName = this.currentAccount.accountName;
    this.accountType = this.currentAccount.accountType;
    this.authUsername = this.currentAccount.authUsername || '';
    // SSH is handled automatically, so default to 'token' if ssh is set
    const method = this.currentAccount.preferredAuthMethod;
    this.preferredAuthMethod = (method === 'token' || method === 'username_password') ? method : 'token';
    this.username = this.currentAccount.username || '';
    this.email = this.currentAccount.email || '';
    this.notes = this.currentAccount.notes || '';
    // Don't pre-fill tokens/passwords for security
    this.showCreateForm = true;
  }

  updateAccount(): void {
    if (!this.currentAccount || !this.currentAccount.id) return;

    // Validation similar to saveAccount
    if (!this.accountName.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.ACCOUNT_NAME_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    const request: any = {
      repositoryPath: this.data.repositoryPath,
      accountName: this.accountName.trim(),
      accountType: this.accountType,
      authUsername: this.authUsername.trim() || undefined,
      preferredAuthMethod: (this.accountType === 'Bitbucket' || this.accountType === 'Generic')
        ? this.preferredAuthMethod : undefined,
      username: this.username.trim() || undefined,
      email: this.email.trim() || undefined,
      notes: this.notes.trim() || undefined,
      isActive: true
    };

    // Only include credentials if user provided new ones
    if (this.accountType === 'GitHub' && this.gitHubPAT.trim()) {
      request.gitHubPAT = this.gitHubPAT.trim();
    }
    if (this.accountType === 'GitLab' && this.gitLabToken.trim()) {
      request.gitLabToken = this.gitLabToken.trim();
    }
    if (this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'token' && this.bitbucketAppPassword.trim()) {
      request.bitbucketAppPassword = this.bitbucketAppPassword.trim();
    }
    if ((this.accountType === 'Generic' || (this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'username_password'))
        && this.httpsPassword.trim()) {
      request.httpsPassword = this.httpsPassword.trim();
    }

    this.isLoading = true;
    this.gitAccountService.updateAccount(this.currentAccount.id, request).subscribe({
      next: (account) => {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.UPDATED_SUCCESS'), 'OK', {
          duration: 3000,
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        this.currentAccount = account;
        this.showCreateForm = false;
        this.resetForm();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error updating account:', err);
        const message = err.error?.error || this.translate.instant('GIT_ACCOUNT.UPDATE_ERROR');
        this.snackBar.open(message, 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
        this.isLoading = false;
      }
    });
  }

  openTokenCreationPage(): void {
    if (this.accountType === 'GitHub') {
      window.open('https://github.com/settings/tokens/new?scopes=repo', '_blank');
    } else if (this.accountType === 'GitLab') {
      window.open('https://gitlab.com/-/profile/personal_access_tokens', '_blank');
    } else if (this.accountType === 'Bitbucket') {
      window.open('https://bitbucket.org/account/settings/app-passwords/', '_blank');
    }
  }

  close(): void {
    this.dialogRef.close(this.currentAccount !== null);
  }
}
