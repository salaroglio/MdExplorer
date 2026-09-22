import { Component, OnInit, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { GITService } from '../../services/gitservice.service';
import {
  GitSetupRemoteGenericDialogData,
  ParseRemoteUrlResponse,
  PROVIDER_INFO
} from '../../models/remote-setup.models';

/**
 * «Collega a un repository remoto». Una sola maschera per GitHub, GitLab, Bitbucket e i
 * server aziendali: incolli l'URL, MdExplorer ricava host, proprietario e nome, ti propone
 * l'account git per quell'host (con più account sullo stesso host git deve saperlo) e fa il
 * primo push. Nessuna credenziale passa di qui: se serve un login lo fa il git di sistema
 * col suo credential manager, e se manca una credenziale l'errore di git compare qui sotto,
 * così com'è, con un'indicazione su cosa fare.
 */
@Component({
  selector: 'app-git-setup-remote-generic-dialog',
  templateUrl: './git-setup-remote-generic-dialog.component.html',
  styleUrls: ['./git-setup-remote-generic-dialog.component.scss']
})
export class GitSetupRemoteGenericDialogComponent implements OnInit {
  remoteUrl: string = '';
  accountUsername: string = '';
  remoteName: string = 'origin';
  pushAfterAdd: boolean = true;

  isParsing: boolean = false;
  isChecking: boolean = false;
  isSetting: boolean = false;
  /** Errore della maschera (URL non valido, campi mancanti). */
  error: string | null = null;
  /** Lo stderr di git così com'è, quando il push o la verifica falliscono. */
  gitError: string | null = null;

  urlInfo: ParseRemoteUrlResponse | null = null;
  checkResult: { isReachable: boolean; error?: string; isAuthenticationError?: boolean } | null = null;

  readonly providerInfo = PROVIDER_INFO;
  private accountTouched = false;
  private parseTimer: any = null;

  constructor(
    public dialogRef: MatDialogRef<GitSetupRemoteGenericDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitSetupRemoteGenericDialogData,
    private gitService: GITService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    if (this.data?.prefilledRemoteUrl) {
      this.remoteUrl = this.data.prefilledRemoteUrl;
      this.parseUrl();
    }
  }

  /** L'URL cambia: dopo una breve pausa lo facciamo leggere al backend. */
  onUrlChange(): void {
    this.error = null;
    this.gitError = null;
    this.checkResult = null;
    if (this.parseTimer) { clearTimeout(this.parseTimer); }
    if (!this.remoteUrl || this.remoteUrl.trim().length < 10) {
      this.urlInfo = null;
      return;
    }
    this.parseTimer = setTimeout(() => this.parseUrl(), 300);
  }

  private parseUrl(): void {
    this.isParsing = true;
    this.gitService.parseRemoteUrl(this.remoteUrl.trim()).subscribe({
      next: (result: ParseRemoteUrlResponse) => {
        this.urlInfo = result;
        this.isParsing = false;
        if (!result.isValid) {
          this.error = result.error || this.translate.instant('GIT_REMOTE.URL_INVALID');
          return;
        }
        // L'account proposto è il proprietario nell'URL: su GitHub e GitLab è quasi sempre
        // l'utente giusto; su un server aziendale l'utente può cambiarlo.
        if (!this.accountTouched && result.protocol === 'https') {
          this.accountUsername = result.owner || '';
        }
      },
      error: (err) => {
        console.error('Error parsing URL:', err);
        this.isParsing = false;
        this.urlInfo = null;
      }
    });
  }

  onAccountChange(): void {
    this.accountTouched = true;
  }

  /** `git ls-remote` con la credenziale che git ha già: dice se il remoto risponde, senza scrivere niente. */
  checkRemote(): void {
    if (!this.remoteUrl || !this.urlInfo?.isValid) { return; }
    this.isChecking = true;
    this.gitError = null;
    this.checkResult = null;
    this.gitService.validateRemoteUrl(this.remoteUrl.trim()).subscribe({
      next: (result) => {
        this.isChecking = false;
        this.checkResult = result;
        if (!result.isReachable) {
          this.gitError = result.error || this.translate.instant('GIT_REMOTE.REMOTE_NOT_REACHABLE');
        }
      },
      error: (err) => {
        this.isChecking = false;
        this.gitError = err?.message || this.translate.instant('GIT_REMOTE.REMOTE_NOT_REACHABLE');
      }
    });
  }

  getProviderInfo(): { name: string; icon: string; color: string } | null {
    if (!this.urlInfo?.provider) { return null; }
    return this.providerInfo[this.urlInfo.provider] || this.providerInfo['generic'];
  }

  isFormValid(): boolean {
    return !!this.remoteUrl && !!this.urlInfo?.isValid && !this.isParsing;
  }

  /**
   * L'indicazione da dare quando git non ha una credenziale: su Windows la chiede il Git
   * Credential Manager al primo push; su Linux e Mac un push dal terminale, una volta.
   */
  loginHintKey(): string | null {
    const e = (this.gitError || '').toLowerCase();
    if (e.includes('could not read') || e.includes('terminal prompts') || e.includes('authentication failed')
      || e.includes('401') || e.includes('403')) {
      return 'GIT_REMOTE.LOGIN_HINT';
    }
    return null;
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onSetup(): void {
    if (!this.isFormValid()) {
      this.error = this.translate.instant('GIT_REMOTE.URL_INVALID');
      return;
    }
    this.isSetting = true;
    this.error = null;
    this.gitError = null;

    this.gitService.setupRemoteGeneric({
      repositoryPath: this.data.projectPath,
      remoteUrl: this.remoteUrl.trim(),
      remoteName: this.remoteName || 'origin',
      accountUsername: this.accountUsername?.trim() || undefined,
      pushAfterAdd: this.pushAfterAdd === true
    }).subscribe({
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
          // Il remote può essere stato scritto anche se il push è fallito: lo diciamo, e
          // mostriamo l'errore di git così com'è.
          this.error = response.message || this.translate.instant('GIT_REMOTE.REMOTE_ERROR');
          this.gitError = response.error || null;
        }
      },
      error: (err) => {
        this.isSetting = false;
        this.error = this.translate.instant('GIT_REMOTE.REMOTE_ERROR');
        this.gitError = err?.error?.error || err?.message || null;
        console.error('Error setting up remote:', err);
      }
    });
  }
}
