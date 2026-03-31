import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { AppStoreService } from '../../services/app-store.service';
import { MdFileService } from '../../services/md-file.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { StoreCatalog, StoreCatalogApp, InstalledApp, AppStoreRepository, RepoInfo } from '../../models/app-store.models';

/**
 * Compares two semantic version strings numerically (e.g. "1.10.0" > "1.9.0").
 * Returns -1 if a < b, 0 if equal, 1 if a > b.
 * Handles different lengths ("1.0" == "1.0.0") and null/undefined values.
 */
function compareVersions(a: string | null | undefined, b: string | null | undefined): number {
  if (!a && !b) return 0;
  if (!a) return -1;
  if (!b) return 1;
  const pa = a.split('.').map(s => parseInt(s, 10) || 0);
  const pb = b.split('.').map(s => parseInt(s, 10) || 0);
  const len = Math.max(pa.length, pb.length);
  for (let i = 0; i < len; i++) {
    const na = pa[i] ?? 0;
    const nb = pb[i] ?? 0;
    if (na < nb) return -1;
    if (na > nb) return 1;
  }
  return 0;
}

@Component({
  selector: 'app-store',
  templateUrl: './app-store.component.html',
  styleUrls: ['./app-store.component.scss']
})
export class AppStoreComponent implements OnInit, OnDestroy {

  private publishProgressSub: Subscription;
  private reposChangedSub: Subscription;

  catalog: StoreCatalog | null = null;
  catalogApps: StoreCatalogApp[] = [];
  installedApps: InstalledApp[] = [];
  repos: AppStoreRepository[] = [];
  repoInfos: RepoInfo[] = [];
  searchQuery: string = '';

  // Repo metadata editing
  repoMetadata: { repoName: string; repoDescription: string; repoCompany: string } = { repoName: '', repoDescription: '', repoCompany: '' };
  repoLogoFile: File | null = null;
  repoLogoPreview: string | null = null;
  isSavingMetadata = false;

  // Project association
  projectApps: any[] = [];

  isLoadingCatalog = false;
  catalogLoadError = false;
  isLoadingInstalled = false;
  installingAppIds = new Set<string>();
  uninstallingAppIds = new Set<string>();

  // Publish
  selectedRepoId: string = '';  // selected repo for publish/manage

  // Platform
  currentPlatform: string = 'windows';
  publishPlatform: string = 'windows';

  // Edit catalog entry (Gestisci tab)
  editingEntry: StoreCatalogApp | null = null;
  editForm: { name: string; description: string; version: string } = { name: '', description: '', version: '' };
  editIconFile: File | null = null;
  editIconPreview: string | null = null;
  isSavingEntry = false;
  editInstallerFile: File | null = null;
  editInstallerPlatform: string = 'windows';
  editFilenameError: string | null = null;
  editPublishProgress: number | null = null;
  editPublishPhase: string = '';

  // Publish panel toggle (Gestisci tab)
  showPublishPanel = false;

  // Publish
  selectedFile: File | null = null;
  isPublishing = false;
  publishProgress: number | null = null;
  publishPhase: string = '';
  publishFilenameError: string | null = null;
  publishMetadata: { appId: string; version: string; name: string; description: string; executableName: string } | null = null;
  customIconFile: File | null = null;
  customIconPreview: string | null = null;

  private static readonly WIN_REGEX = /^(.+)-setup-(.+)\.exe$/i;
  private static readonly LINUX_REGEX = /^(.+)-(.+)\.AppImage$/i;

  constructor(
    private appStoreService: AppStoreService,
    private mdFileService: MdFileService,
    private http: HttpClient,
    private mdServerMessages: MdServerMessagesService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  private get connectionId(): string {
    return this.mdServerMessages.connectionId ?? '';
  }

  ngOnInit(): void {
    this.loadRepos();
    this.loadInstalled();
    this.loadProjectApps();
    this.appStoreService.getPlatform().subscribe({
      next: p => {
        this.currentPlatform = p;
        this.publishPlatform = p;
      },
      error: () => {}
    });

    // Reload when repos are changed from settings dialog
    this.reposChangedSub = this.appStoreService.reposChanged$.subscribe(() => {
      this.loadRepos();
      this.loadCatalog();
    });

    // Listen for real publish progress from backend (backend → Nexus upload)
    this.publishProgressSub = this.mdServerMessages.publishProgress$.subscribe(data => {
      if (data.phase === 'uploading') {
        // Update publish progress (new app publish panel)
        if (this.isPublishing) {
          this.publishProgress = data.percent;
          this.publishPhase = data.percent >= 100 ? 'Elaborazione sul server...' : `Upload verso Nexus: ${data.percent}%`;
        }
        // Update edit progress (edit entry panel)
        if (this.editPublishProgress !== null) {
          this.editPublishProgress = data.percent;
          this.editPublishPhase = data.percent >= 100 ? 'Elaborazione sul server...' : `Upload verso Nexus: ${data.percent}%`;
        }
      } else if (data.phase === 'catalog') {
        if (this.isPublishing) {
          this.publishPhase = 'Aggiornamento catalogo...';
        }
        if (this.editPublishProgress !== null) {
          this.editPublishPhase = 'Aggiornamento catalogo...';
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.publishProgressSub?.unsubscribe();
    this.reposChangedSub?.unsubscribe();
  }

  loadRepos(): void {
    this.appStoreService.getRepositories().subscribe({
      next: repos => {
        this.repos = repos;
        if (repos.length > 0 && !this.selectedRepoId) {
          this.selectedRepoId = repos[0].id;
        }
        if (repos.length > 0 && this.catalogApps.length === 0) {
          this.loadCatalog();
        }
      },
      error: () => {}
    });
  }

  loadInstalled(): void {
    this.isLoadingInstalled = true;
    this.appStoreService.getInstalled().subscribe({
      next: apps => {
        this.installedApps = apps;
        this.isLoadingInstalled = false;
      },
      error: () => { this.isLoadingInstalled = false; }
    });
  }

  loadCatalog(): void {
    if (this.repos.length === 0) {
      this.snackBar.open(this.translate.instant('APP_STORE.ADD_REPO_SETTINGS'), 'OK', { duration: 4000 });
      return;
    }
    this.appStoreService.invalidateCache();
    this.isLoadingCatalog = true;
    this.catalogLoadError = false;
    this.catalogApps = [];
    this.appStoreService.getCatalog().subscribe({
      next: catalog => {
        this.catalog = catalog;
        this.catalogApps = catalog?.apps ?? [];
        this.repoInfos = catalog?.repos ?? [];
        this.catalogLoadError = (catalog?.failedRepos ?? 0) > 0 && this.catalogApps.length === 0;
        // Initialize repo metadata from first repo info
        const firstRepo = this.repoInfos[0];
        if (firstRepo) {
          this.repoMetadata = {
            repoName: firstRepo.repoName || '',
            repoDescription: firstRepo.repoDescription || '',
            repoCompany: firstRepo.repoCompany || ''
          };
        }
        this.isLoadingCatalog = false;
      },
      error: () => {
        this.isLoadingCatalog = false;
        this.catalogLoadError = true;
      }
    });
  }

  get filteredCatalogApps(): StoreCatalogApp[] {
    if (!this.searchQuery.trim()) return this.catalogApps;
    const q = this.searchQuery.toLowerCase().trim();
    return this.catalogApps.filter(app =>
      app.name?.toLowerCase().includes(q) || app.description?.toLowerCase().includes(q)
    );
  }

  isInstalled(app: StoreCatalogApp): boolean {
    return this.installedApps.some(a => a.appId === app.id);
  }

  getInstalledVersion(app: StoreCatalogApp): string | null {
    return this.installedApps.find(a => a.appId === app.id)?.version ?? null;
  }

  hasUpdate(app: StoreCatalogApp): boolean {
    const installed = this.installedApps.find(a => a.appId === app.id);
    if (!installed) return false;
    const catalogVersion = this.getVersionForPlatform(app);
    return compareVersions(installed.version, catalogVersion) < 0;
  }

  // ── Platform helpers ────────────────────────────────
  isAvailableForPlatform(app: StoreCatalogApp): boolean {
    if (app.platforms && Object.keys(app.platforms).length > 0) {
      return !!app.platforms[this.currentPlatform];
    }
    // Legacy: no platforms dict → treat as Windows-only
    return this.currentPlatform === 'windows';
  }

  getAppPlatforms(app: StoreCatalogApp): string[] {
    if (app.platforms && Object.keys(app.platforms).length > 0) {
      return Object.keys(app.platforms);
    }
    return ['windows'];
  }

  getDownloadUrlForPlatform(app: StoreCatalogApp): string {
    if (app.platforms && app.platforms[this.currentPlatform]) {
      return app.platforms[this.currentPlatform].downloadUrl;
    }
    return app.downloadUrl;
  }

  getVersionForPlatform(app: StoreCatalogApp): string {
    const platformBuild = app.platforms?.[this.currentPlatform];
    return platformBuild?.version || app.version;
  }

  async installApp(app: StoreCatalogApp): Promise<void> {
    this.installingAppIds.add(app.id);

    // Terminate the app if it's running (file locks would cause NSIS to fail)
    await this.terminateAppIfRunning(app.id);

    const downloadUrl = this.getDownloadUrlForPlatform(app);
    this.appStoreService.installApp(
      { ...app, downloadUrl, repoId: app.repoId },
      this.currentPlatform
    ).subscribe({
      next: () => {
        this.installingAppIds.delete(app.id);
        this.snackBar.open(this.translate.instant('APP_STORE.INSTALLED_SUCCESS', { name: app.name }), 'OK', { duration: 3000 });
        this.loadInstalled();
        this.refreshTree();
      },
      error: (err) => {
        this.installingAppIds.delete(app.id);
        const msg = err?.error?.error ?? this.translate.instant('APP_STORE.INSTALL_FAILED');
        this.snackBar.open(msg, 'OK', { duration: 5000 });
      }
    });
  }

  async uninstallApp(appId: string, appName: string): Promise<void> {
    if (!confirm(`Uninstall "${appName}"?`)) return;
    this.uninstallingAppIds.add(appId);

    // Terminate the app if it's running (file locks would cause uninstaller to fail)
    await this.terminateAppIfRunning(appId);

    this.appStoreService.uninstallApp(appId).subscribe({
      next: () => {
        this.uninstallingAppIds.delete(appId);
        this.snackBar.open(this.translate.instant('APP_STORE.UNINSTALLED', { name: appName }), 'OK', { duration: 3000 });
        this.loadInstalled();
        this.refreshTree();
      },
      error: () => {
        this.uninstallingAppIds.delete(appId);
        this.snackBar.open(this.translate.instant('APP_STORE.UNINSTALL_FAILED'), 'OK', { duration: 3000 });
      }
    });
  }

  // ── Project association ────────────────────────────
  loadProjectApps(): void {
    this.http.get<any[]>(`/api/MdExternalApps?ConnectionId=${this.connectionId}`).subscribe({
      next: apps => this.projectApps = apps || [],
      error: () => {}
    });
  }

  isAssociatedToProject(app: StoreCatalogApp): boolean {
    return this.projectApps.some(a => a.id === app.id);
  }

  associateToProject(app: StoreCatalogApp): void {
    this.http.post<{ success: boolean }>(`/api/MdExternalApps/Add?ConnectionId=${this.connectionId}`, {
      id: app.id,
      name: app.name,
      description: app.description || '',
      icon: app.icon || ''
    }).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('APP_STORE.ASSOCIATED', { name: app.name }), '', { duration: 2000 });
        this.loadProjectApps();
        this.refreshTree();
      },
      error: () => this.snackBar.open(this.translate.instant('APP_STORE.ASSOCIATE_ERROR'), 'OK', { duration: 3000 })
    });
  }

  dissociateFromProject(app: StoreCatalogApp): void {
    this.http.delete<{ success: boolean }>(`/api/MdExternalApps/${app.id}?ConnectionId=${this.connectionId}`).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('APP_STORE.REMOVED_FROM_PROJECT', { name: app.name }), '', { duration: 2000 });
        this.loadProjectApps();
        this.refreshTree();
      },
      error: () => this.snackBar.open(this.translate.instant('APP_STORE.REMOVE_ERROR'), 'OK', { duration: 3000 })
    });
  }

  private async terminateAppIfRunning(appId: string): Promise<void> {
    const electronAPI = (window as any).electronAPI;
    if (electronAPI?.externalApp?.terminate) {
      electronAPI.externalApp.terminate(appId);
      // Wait for the process to release file locks
      await new Promise(resolve => setTimeout(resolve, 1500));
    }
  }

  private refreshTree(): void {
    this.mdFileService.loadAll(null, null);
  }

  // ── Repo metadata ─────────────────────────────────
  onRepoLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.repoLogoFile = file;
    const reader = new FileReader();
    reader.onload = () => { this.repoLogoPreview = reader.result as string; };
    reader.readAsDataURL(file);
  }

  removeRepoLogo(): void {
    this.repoLogoFile = null;
    this.repoLogoPreview = null;
  }

  saveCatalogMetadata(): void {
    this.isSavingMetadata = true;
    const formData = new FormData();
    formData.append('RepoName', this.repoMetadata.repoName);
    formData.append('RepoDescription', this.repoMetadata.repoDescription);
    formData.append('RepoCompany', this.repoMetadata.repoCompany);
    if (this.repoLogoFile) {
      formData.append('LogoFile', this.repoLogoFile, this.repoLogoFile.name);
    }
    if (this.selectedRepoId) {
      formData.append('RepoId', this.selectedRepoId);
    }
    this.appStoreService.updateCatalogMetadata(formData).subscribe({
      next: () => {
        this.isSavingMetadata = false;
        this.repoLogoFile = null;
        this.repoLogoPreview = null;
        this.snackBar.open(this.translate.instant('APP_STORE.METADATA_SAVED'), '', { duration: 2000 });
        this.loadCatalog();
      },
      error: () => {
        this.isSavingMetadata = false;
        this.snackBar.open(this.translate.instant('APP_STORE.METADATA_SAVE_ERROR'), 'OK', { duration: 3000 });
      }
    });
  }

  // ── Edit catalog entry (Gestisci) ──────────────────
  startEdit(app: StoreCatalogApp): void {
    this.editingEntry = app;
    this.editForm = { name: app.name, description: app.description || '', version: app.version };
    this.editIconFile = null;
    this.editIconPreview = null;
    this.editInstallerFile = null;
    this.editInstallerPlatform = this.currentPlatform;
    this.editFilenameError = null;
    this.editPublishProgress = null;
    this.editPublishPhase = '';
  }

  cancelEdit(): void {
    this.editingEntry = null;
    this.editIconFile = null;
    this.editIconPreview = null;
    this.editInstallerFile = null;
    this.editFilenameError = null;
    this.editPublishProgress = null;
    this.editPublishPhase = '';
  }

  onEditInstallerSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.editInstallerFile = input.files?.[0] ?? null;
    this.editFilenameError = null;

    if (!this.editInstallerFile || !this.editingEntry) return;

    const regex = this.editInstallerPlatform === 'linux'
      ? AppStoreComponent.LINUX_REGEX
      : AppStoreComponent.WIN_REGEX;
    const match = regex.exec(this.editInstallerFile.name);

    if (!match) {
      const expectedFormat = this.editInstallerPlatform === 'linux'
        ? '{appId}-{version}.AppImage'
        : '{appId}-setup-{version}.exe';
      this.editFilenameError = this.translate.instant('APP_STORE.FILENAME_FORMAT_ERROR', { format: expectedFormat });
      return;
    }

    const appId = match[1];
    const version = match[2];

    if (appId.toLowerCase() !== this.editingEntry.id.toLowerCase()) {
      this.editFilenameError = this.translate.instant('APP_STORE.APPID_MISMATCH', { fileId: appId, editId: this.editingEntry.id });
      this.editInstallerFile = null;
      return;
    }

    this.editForm.version = version;
  }

  onEditIconSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.editIconFile = file;
    const reader = new FileReader();
    reader.onload = () => { this.editIconPreview = reader.result as string; };
    reader.readAsDataURL(file);
  }

  saveEdit(): void {
    if (!this.editingEntry) return;
    this.isSavingEntry = true;

    if (this.editInstallerFile) {
      // Upload installer first, then update metadata
      this.editPublishProgress = 0;
      this.editPublishPhase = 'Invio al server...';
      this.appStoreService.publishApp(
        this.editInstallerFile,
        { name: this.editForm.name, description: this.editForm.description, executableName: '' },
        this.editIconFile || undefined,
        this.editInstallerPlatform,
        this.editingEntry.repoId || this.selectedRepoId || undefined
      ).subscribe({
        next: (event) => {
          // Progress is now tracked via SignalR (publishProgress$ in ngOnInit).
          // Here we only handle the final HTTP response.
          if (event.type === HttpEventType.Response) {
            this.editPublishProgress = null;
            this.editPublishPhase = '';
            // Now update metadata (name, description, version, icon)
            this.saveEditMetadata();
          }
        },
        error: (err) => {
          this.isSavingEntry = false;
          this.editPublishProgress = null;
          this.editPublishPhase = '';
          const msg = err?.error?.error ?? this.translate.instant('APP_STORE.UPLOAD_ERROR');
          this.snackBar.open(msg, 'OK', { duration: 5000 });
        }
      });
    } else {
      this.saveEditMetadata();
    }
  }

  private saveEditMetadata(): void {
    if (!this.editingEntry) return;
    const formData = new FormData();
    formData.append('Id', this.editingEntry.id);
    formData.append('Name', this.editForm.name);
    formData.append('Description', this.editForm.description);
    formData.append('Version', this.editForm.version);
    if (this.editIconFile) {
      formData.append('IconFile', this.editIconFile, this.editIconFile.name);
    }
    if (this.editingEntry.repoId) {
      formData.append('RepoId', this.editingEntry.repoId);
    }
    this.appStoreService.updateCatalogEntry(formData).subscribe({
      next: () => {
        this.isSavingEntry = false;
        this.snackBar.open(this.translate.instant('APP_STORE.CATALOG_UPDATED'), '', { duration: 2000 });
        this.cancelEdit();
        this.loadCatalog();
      },
      error: (err) => {
        this.isSavingEntry = false;
        const msg = err?.error?.error ?? this.translate.instant('APP_STORE.CATALOG_UPDATE_ERROR');
        this.snackBar.open(msg, 'OK', { duration: 4000 });
      }
    });
  }

  // ── Publish ────────────────────────────────────────
  onPlatformChange(): void {
    this.selectedFile = null;
    this.publishMetadata = null;
    this.publishFilenameError = null;
    this.customIconFile = null;
    this.customIconPreview = null;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
    this.publishFilenameError = null;
    this.publishMetadata = null;
    this.customIconFile = null;
    this.customIconPreview = null;

    if (!this.selectedFile) return;

    const regex = this.publishPlatform === 'linux'
      ? AppStoreComponent.LINUX_REGEX
      : AppStoreComponent.WIN_REGEX;
    const match = regex.exec(this.selectedFile.name);

    if (!match) {
      const expectedFormat = this.publishPlatform === 'linux'
        ? '{appId}-{version}.AppImage'
        : '{appId}-setup-{version}.exe';
      this.publishFilenameError = this.translate.instant('APP_STORE.FILENAME_FORMAT_ERROR', { format: expectedFormat });
      return;
    }

    const appId = match[1];
    const version = match[2];
    const name = appId.replace(/(^|-)(\w)/g, (_m, sep, ch) =>
      (sep === '-' ? ' ' : '') + ch.toUpperCase()
    );
    const executableName = this.publishPlatform === 'linux'
      ? this.selectedFile!.name   // AppImage: same filename
      : `${name}.exe`;            // Windows: derive from name (editable)
    this.publishMetadata = { appId, version, name, description: '', executableName };
  }

  onIconSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.customIconFile = file;
    const reader = new FileReader();
    reader.onload = () => {
      this.customIconPreview = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  removeCustomIcon(): void {
    this.customIconFile = null;
    this.customIconPreview = null;
  }

  get isSettingsConfigured(): boolean {
    return this.repos.length > 0;
  }

  get publishFileAccept(): string {
    return this.publishPlatform === 'linux' ? '.AppImage' : '.exe';
  }

  publishApp(): void {
    if (!this.isSettingsConfigured) {
      this.snackBar.open(this.translate.instant('APP_STORE.ADD_REPO_SETTINGS'), 'OK', { duration: 4000 });
      return;
    }
    if (!this.selectedFile || !this.publishMetadata) {
      this.snackBar.open(this.translate.instant('APP_STORE.SELECT_FILE'), 'OK', { duration: 3000 });
      return;
    }
    if (this.publishFilenameError) {
      this.snackBar.open(this.publishFilenameError, 'OK', { duration: 4000 });
      return;
    }
    if (!this.publishMetadata.description?.trim()) {
      this.snackBar.open(this.translate.instant('APP_STORE.DESC_REQUIRED'), 'OK', { duration: 3000 });
      return;
    }
    if (!this.publishMetadata.executableName?.trim()) {
      this.snackBar.open(this.translate.instant('APP_STORE.EXECUTABLE_REQUIRED'), 'OK', { duration: 3000 });
      return;
    }
    this.isPublishing = true;
    this.publishProgress = 0;
    this.publishPhase = 'Invio al server...';
    this.appStoreService.publishApp(
      this.selectedFile,
      { name: this.publishMetadata.name, description: this.publishMetadata.description, executableName: this.publishMetadata.executableName },
      this.customIconFile || undefined,
      this.publishPlatform,
      this.selectedRepoId || undefined
    ).subscribe({
      next: (event) => {
        // Progress is now tracked via SignalR (publishProgress$ in ngOnInit).
        // Here we only handle the final HTTP response.
        if (event.type === HttpEventType.Response) {
          this.isPublishing = false;
          this.publishProgress = null;
          this.publishPhase = '';
          this.snackBar.open(this.translate.instant('APP_STORE.PUBLISH_SUCCESS'), 'OK', { duration: 4000 });
          this.selectedFile = null;
          this.publishFilenameError = null;
          this.publishMetadata = null;
          this.customIconFile = null;
          this.customIconPreview = null;
          this.showPublishPanel = false;
          this.loadCatalog();
        }
      },
      error: (err) => {
        this.isPublishing = false;
        this.publishProgress = null;
        this.publishPhase = '';
        const msg = err?.error?.error ?? this.translate.instant('APP_STORE.PUBLISH_ERROR');
        this.snackBar.open(msg, 'OK', { duration: 5000 });
      }
    });
  }
}
