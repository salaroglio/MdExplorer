import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { AppStoreService } from '../../../services/app-store.service';
import { StoreCatalogApp } from '../../../models/app-store.models';
import { MdFileService } from '../../../services/md-file.service';

export interface InstallWizardData {
  appId: string;
  appName: string;
  appDescription: string;
  appIcon: string;
  /** If set, dialog opens in "update" mode */
  mode?: 'install' | 'update';
  installedVersion?: string;
  catalogApp?: StoreCatalogApp;
}

@Component({
  selector: 'app-install-wizard',
  templateUrl: './install-wizard.component.html',
  styleUrls: ['./install-wizard.component.scss']
})
export class InstallWizardDialogComponent implements OnInit, OnDestroy {
  catalogApp: StoreCatalogApp | null = null;
  loading = true;
  installing = false;
  installed = false;
  errorMessage = '';
  platform = '';
  isUpdateMode = false;

  // Progress tracking
  installPhase: 'idle' | 'downloading' | 'installing' | 'registering' | 'done' = 'idle';
  progressValue = 0;
  private progressTimer: any = null;

  private isElectron = !!(window as any).electronAPI?.externalApp;

  constructor(
    public dialogRef: MatDialogRef<InstallWizardDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: InstallWizardData,
    private appStoreService: AppStoreService,
    private mdFileService: MdFileService
  ) {}

  ngOnInit(): void {
    this.isUpdateMode = this.data.mode === 'update';
    this.appStoreService.getPlatform().subscribe(p => this.platform = p);

    if (this.isUpdateMode && this.data.catalogApp) {
      this.catalogApp = this.data.catalogApp;
      this.loading = false;
    } else {
      this.appStoreService.getCatalog().subscribe({
        next: (catalog) => {
          this.catalogApp = catalog.apps.find(a => a.id === this.data.appId) || null;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading catalog:', err);
          this.errorMessage = 'Failed to load catalog.';
          this.loading = false;
        }
      });
    }
  }

  ngOnDestroy(): void {
    this.stopProgress();
  }

  get phaseLabel(): string {
    switch (this.installPhase) {
      case 'downloading': return 'Downloading package...';
      case 'installing': return this.isUpdateMode ? 'Updating...' : 'Running installer...';
      case 'registering': return 'Registering app...';
      case 'done': return 'Complete!';
      default: return '';
    }
  }

  install(): void {
    if (!this.catalogApp) return;

    this.installing = true;
    this.errorMessage = '';
    this.installPhase = 'downloading';
    this.progressValue = 0;
    this.startProgress();

    // If updating, terminate the running process first
    if (this.isUpdateMode && this.isElectron) {
      (window as any).electronAPI.externalApp.terminate(this.data.appId);
    }

    let downloadUrl = this.catalogApp.downloadUrl;
    let executableName = this.catalogApp.executableName;
    if (this.catalogApp.platforms && this.platform && this.catalogApp.platforms[this.platform]) {
      const platformBuild = this.catalogApp.platforms[this.platform];
      downloadUrl = platformBuild.downloadUrl;
      if (platformBuild.executableName) {
        executableName = platformBuild.executableName;
      }
    }

    this.appStoreService.installApp({
      id: this.catalogApp.id,
      name: this.catalogApp.name,
      description: this.catalogApp.description,
      version: this.catalogApp.version,
      downloadUrl: downloadUrl,
      icon: this.catalogApp.icon,
      repoId: this.catalogApp.repoId,
      executableName: executableName
    }, this.platform).subscribe({
      next: () => {
        this.finishProgress(() => {
          this.installing = false;
          this.installed = true;
          this.appStoreService.invalidateCache();
          this.mdFileService.loadAll(null, null);
        });
      },
      error: (err) => {
        console.error('Error installing app:', err);
        this.stopProgress();
        this.installPhase = 'idle';
        this.progressValue = 0;
        this.errorMessage = err.error?.error || (this.isUpdateMode ? 'Update failed. Please try again.' : 'Installation failed. Please try again.');
        this.installing = false;
      }
    });
  }

  /** Skip update and open the app anyway */
  skip(): void {
    this.dialogRef.close('skip');
  }

  private startProgress(): void {
    this.progressTimer = setInterval(() => {
      if (this.progressValue < 20) {
        // Fast initial ramp: 0→20% in ~0.7s
        this.progressValue += 3;
      } else if (this.progressValue < 40) {
        this.installPhase = 'installing';
        this.progressValue += 1.5;
      } else if (this.progressValue < 70) {
        this.progressValue += 0.8;
      } else if (this.progressValue < 85) {
        this.installPhase = 'registering';
        this.progressValue += 0.3;
      } else if (this.progressValue < 92) {
        // Slow crawl near the end — waits for response
        this.progressValue += 0.1;
      }
    }, 80);
  }

  /** Animate smoothly from current value to 100%, then call onDone. */
  private finishProgress(onDone: () => void): void {
    this.stopProgress();
    this.installPhase = 'done';
    const animateToEnd = () => {
      if (this.progressValue < 100) {
        this.progressValue = Math.min(this.progressValue + 4, 100);
        requestAnimationFrame(animateToEnd);
      } else {
        onDone();
      }
    };
    requestAnimationFrame(animateToEnd);
  }

  private stopProgress(): void {
    if (this.progressTimer) {
      clearInterval(this.progressTimer);
      this.progressTimer = null;
    }
  }

  close(): void {
    this.dialogRef.close(this.installed ? 'updated' : false);
  }
}
