import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { AppStoreService } from '../../services/app-store.service';
import { MdFileService } from '../../services/md-file.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { StoreCatalog, StoreCatalogApp, InstalledApp, AppStoreRepository, RepoInfo } from '../../models/app-store.models';

@Component({
  selector: 'app-store',
  templateUrl: './app-store.component.html',
  styleUrls: ['./app-store.component.scss']
})
export class AppStoreComponent implements OnInit {

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

  // Publish
  selectedFile: File | null = null;
  isPublishing = false;
  publishFilenameError: string | null = null;
  publishMetadata: { appId: string; version: string; name: string; description: string } | null = null;
  customIconFile: File | null = null;
  customIconPreview: string | null = null;

  private static readonly WIN_REGEX = /^(.+)-setup-(.+)\.exe$/i;
  private static readonly LINUX_REGEX = /^(.+)-(.+)\.AppImage$/i;

  constructor(
    private appStoreService: AppStoreService,
    private mdFileService: MdFileService,
    private http: HttpClient,
    private mdServerMessages: MdServerMessagesService,
    private snackBar: MatSnackBar
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
  }

  loadRepos(): void {
    this.appStoreService.getRepositories().subscribe({
      next: repos => {
        this.repos = repos;
        if (repos.length > 0 && !this.selectedRepoId) {
          this.selectedRepoId = repos[0].id;
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
      this.snackBar.open('Aggiungi un repository nelle Impostazioni > Application.', 'OK', { duration: 4000 });
      return;
    }
    this.isLoadingCatalog = true;
    this.catalogApps = [];
    this.appStoreService.getCatalog().subscribe({
      next: catalog => {
        this.catalog = catalog;
        this.catalogApps = catalog?.apps ?? [];
        this.repoInfos = catalog?.repos ?? [];
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
        this.snackBar.open('Failed to load catalog.', 'OK', { duration: 3000 });
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
    return !!installed && installed.version !== app.version;
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

  installApp(app: StoreCatalogApp): void {
    this.installingAppIds.add(app.id);
    const downloadUrl = this.getDownloadUrlForPlatform(app);
    this.appStoreService.installApp(
      { ...app, downloadUrl, repoId: app.repoId },
      this.currentPlatform
    ).subscribe({
      next: () => {
        this.installingAppIds.delete(app.id);
        this.snackBar.open(`"${app.name}" installed successfully.`, 'OK', { duration: 3000 });
        this.loadInstalled();
        this.refreshTree();
      },
      error: (err) => {
        this.installingAppIds.delete(app.id);
        const msg = err?.error?.error ?? 'Installation failed.';
        this.snackBar.open(msg, 'OK', { duration: 5000 });
      }
    });
  }

  uninstallApp(appId: string, appName: string): void {
    if (!confirm(`Uninstall "${appName}"?`)) return;
    this.uninstallingAppIds.add(appId);
    this.appStoreService.uninstallApp(appId).subscribe({
      next: () => {
        this.uninstallingAppIds.delete(appId);
        this.snackBar.open(`"${appName}" uninstalled.`, 'OK', { duration: 3000 });
        this.loadInstalled();
        this.refreshTree();
      },
      error: () => {
        this.uninstallingAppIds.delete(appId);
        this.snackBar.open('Uninstall failed.', 'OK', { duration: 3000 });
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
        this.snackBar.open(`"${app.name}" associata al progetto.`, '', { duration: 2000 });
        this.loadProjectApps();
        this.refreshTree();
      },
      error: () => this.snackBar.open('Errore nell\'associazione.', 'OK', { duration: 3000 })
    });
  }

  dissociateFromProject(app: StoreCatalogApp): void {
    this.http.delete<{ success: boolean }>(`/api/MdExternalApps/${app.id}?ConnectionId=${this.connectionId}`).subscribe({
      next: () => {
        this.snackBar.open(`"${app.name}" rimossa dal progetto.`, '', { duration: 2000 });
        this.loadProjectApps();
        this.refreshTree();
      },
      error: () => this.snackBar.open('Errore nella rimozione.', 'OK', { duration: 3000 })
    });
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
        this.snackBar.open('Metadata repository salvati.', '', { duration: 2000 });
        this.loadCatalog();
      },
      error: () => {
        this.isSavingMetadata = false;
        this.snackBar.open('Errore nel salvataggio dei metadata.', 'OK', { duration: 3000 });
      }
    });
  }

  // ── Edit catalog entry (Gestisci) ──────────────────
  startEdit(app: StoreCatalogApp): void {
    this.editingEntry = app;
    this.editForm = { name: app.name, description: app.description || '', version: app.version };
    this.editIconFile = null;
    this.editIconPreview = null;
  }

  cancelEdit(): void {
    this.editingEntry = null;
    this.editIconFile = null;
    this.editIconPreview = null;
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
        this.snackBar.open('Voce catalog aggiornata.', '', { duration: 2000 });
        this.cancelEdit();
        this.loadCatalog();
      },
      error: (err) => {
        this.isSavingEntry = false;
        const msg = err?.error?.error ?? 'Errore nell\'aggiornamento.';
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
      this.publishFilenameError = `Il nome del file non corrisponde al formato richiesto: ${expectedFormat}`;
      return;
    }

    const appId = match[1];
    const version = match[2];
    const name = appId.replace(/(^|-)(\w)/g, (_m, sep, ch) =>
      (sep === '-' ? ' ' : '') + ch.toUpperCase()
    );
    this.publishMetadata = { appId, version, name, description: '' };
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
      this.snackBar.open('Nessun repository configurato. Aggiungi un repository in Settings.', 'OK', { duration: 4000 });
      return;
    }
    if (!this.selectedFile || !this.publishMetadata) {
      this.snackBar.open('Seleziona un file da pubblicare.', 'OK', { duration: 3000 });
      return;
    }
    if (this.publishFilenameError) {
      this.snackBar.open(this.publishFilenameError, 'OK', { duration: 4000 });
      return;
    }
    if (!this.publishMetadata.description?.trim()) {
      this.snackBar.open('La descrizione è obbligatoria.', 'OK', { duration: 3000 });
      return;
    }
    this.isPublishing = true;
    this.appStoreService.publishApp(
      this.selectedFile,
      { name: this.publishMetadata.name, description: this.publishMetadata.description },
      this.customIconFile || undefined,
      this.publishPlatform,
      this.selectedRepoId || undefined
    ).subscribe({
      next: () => {
        this.isPublishing = false;
        this.snackBar.open('App pubblicata con successo!', 'OK', { duration: 4000 });
        this.selectedFile = null;
        this.publishFilenameError = null;
        this.publishMetadata = null;
        this.customIconFile = null;
        this.customIconPreview = null;
      },
      error: (err) => {
        this.isPublishing = false;
        const msg = err?.error?.error ?? 'Errore durante la pubblicazione.';
        this.snackBar.open(msg, 'OK', { duration: 5000 });
      }
    });
  }
}
