import { Injectable } from '@angular/core';
import { HttpClient, HttpRequest, HttpEvent } from '@angular/common/http';
import { Observable, of, forkJoin } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { StoreCatalog, StoreCatalogApp, InstalledApp, AppStoreRepository } from '../models/app-store.models';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

export interface UpdateCheckResult {
  hasUpdate: boolean;
  installedVersion: string;
  catalogVersion: string;
  catalogApp: StoreCatalogApp | null;
}

@Injectable({
  providedIn: 'root'
})
export class AppStoreService {

  private _platform: string | null = null;
  private _cachedCatalog: StoreCatalog | null = null;
  private _cachedInstalled: InstalledApp[] | null = null;
  private _prefetchInProgress = false;

  constructor(
    private http: HttpClient,
    private mdServerMessages: MdServerMessagesService
  ) {}

  private get connectionId(): string {
    return this.mdServerMessages.connectionId ?? '';
  }

  getPlatform(): Observable<string> {
    if (this._platform) {
      return new Observable(sub => { sub.next(this._platform!); sub.complete(); });
    }
    return this.http.get<{ platform: string }>(`/api/MdAppStore/platform?ConnectionId=${this.connectionId}`)
      .pipe(
        map(r => {
          this._platform = r.platform;
          return r.platform;
        })
      );
  }

  /** Pre-fetch catalog + installed apps into cache. Call on project open. */
  prefetchCatalogAndInstalled(): void {
    if (this._prefetchInProgress) return;
    this._prefetchInProgress = true;
    forkJoin([
      this.http.get<StoreCatalog>(`/api/MdAppStore/catalog?ConnectionId=${this.connectionId}`),
      this.http.get<InstalledApp[]>(`/api/MdAppStore/installed?ConnectionId=${this.connectionId}`)
    ]).subscribe({
      next: ([catalog, installed]) => {
        this._cachedCatalog = catalog;
        this._cachedInstalled = installed;
        this._prefetchInProgress = false;
      },
      error: () => { this._prefetchInProgress = false; }
    });
  }

  /** Invalidate cache (e.g. after install/update). */
  invalidateCache(): void {
    this._cachedCatalog = null;
    this._cachedInstalled = null;
  }

  getCatalog(): Observable<StoreCatalog> {
    if (this._cachedCatalog) return of(this._cachedCatalog);
    return this.http.get<StoreCatalog>(`/api/MdAppStore/catalog?ConnectionId=${this.connectionId}`)
      .pipe(tap(c => this._cachedCatalog = c));
  }

  getInstalled(): Observable<InstalledApp[]> {
    if (this._cachedInstalled) return of(this._cachedInstalled);
    return this.http.get<InstalledApp[]>(`/api/MdAppStore/installed?ConnectionId=${this.connectionId}`)
      .pipe(tap(list => this._cachedInstalled = list));
  }

  /**
   * Check if an installed app has an update available on the catalog.
   * Uses cached data when available.
   */
  checkUpdate(appId: string): Observable<UpdateCheckResult> {
    return forkJoin([this.getCatalog(), this.getInstalled()]).pipe(
      map(([catalog, installed]) => {
        const inst = installed.find(a => a.appId === appId);
        const catApp = catalog.apps.find(a => a.id === appId) || null;
        if (!inst || !catApp) {
          return { hasUpdate: false, installedVersion: inst?.version || '', catalogVersion: catApp?.version || '', catalogApp: catApp };
        }
        const hasUpdate = this.semverGreaterThan(catApp.version, inst.version);
        return { hasUpdate, installedVersion: inst.version, catalogVersion: catApp.version, catalogApp: catApp };
      })
    );
  }

  /** Returns true if versionA > versionB using semantic versioning. */
  private semverGreaterThan(a: string, b: string): boolean {
    const parse = (v: string) => (v || '0.0.0').replace(/^v/i, '').split('.').map(n => parseInt(n, 10) || 0);
    const pa = parse(a);
    const pb = parse(b);
    const len = Math.max(pa.length, pb.length);
    for (let i = 0; i < len; i++) {
      const na = pa[i] || 0;
      const nb = pb[i] || 0;
      if (na > nb) return true;
      if (na < nb) return false;
    }
    return false;
  }

  installApp(app: { id: string; name: string; description?: string; version: string; downloadUrl: string; icon?: string; repoId?: string; executableName?: string }, platform?: string): Observable<{ success: boolean; installDir: string; executableName: string }> {
    return this.http.post<{ success: boolean; installDir: string; executableName: string }>(
      `/api/MdAppStore/install?ConnectionId=${this.connectionId}`,
      {
        appId: app.id,
        name: app.name,
        description: app.description,
        version: app.version,
        downloadUrl: app.downloadUrl,
        icon: app.icon,
        platform: platform || undefined,
        repoId: app.repoId || undefined,
        executableName: app.executableName || undefined
      }
    );
  }

  uninstallApp(appId: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(
      `/api/MdAppStore/uninstall/${appId}?ConnectionId=${this.connectionId}`
    );
  }

  // ── Repositories CRUD ──────────────────────────────

  getRepositories(): Observable<AppStoreRepository[]> {
    return this.http.get<AppStoreRepository[]>(`/api/MdAppStore/repositories?ConnectionId=${this.connectionId}`);
  }

  addRepository(repo: { label: string; url: string; username?: string; password?: string }): Observable<{ success: boolean; id: string }> {
    return this.http.post<{ success: boolean; id: string }>(
      `/api/MdAppStore/repositories?ConnectionId=${this.connectionId}`, repo
    );
  }

  updateRepository(id: string, repo: { label: string; url: string; username?: string; password?: string }): Observable<{ success: boolean }> {
    return this.http.put<{ success: boolean }>(
      `/api/MdAppStore/repositories/${id}?ConnectionId=${this.connectionId}`, repo
    );
  }

  deleteRepository(id: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(
      `/api/MdAppStore/repositories/${id}?ConnectionId=${this.connectionId}`
    );
  }

  // ── Catalog editing ────────────────────────────────

  updateCatalogMetadata(formData: FormData): Observable<{ success: boolean }> {
    return this.http.put<{ success: boolean }>(
      `/api/MdAppStore/catalog/metadata?ConnectionId=${this.connectionId}`, formData
    );
  }

  updateCatalogEntry(formData: FormData): Observable<{ success: boolean }> {
    return this.http.put<{ success: boolean }>(
      `/api/MdAppStore/catalog/entry?ConnectionId=${this.connectionId}`, formData
    );
  }

  // ── Publish ────────────────────────────────────────

  publishApp(file: File, metadata: { name: string; description: string; executableName: string }, customIcon?: File, platform?: string, repoId?: string): Observable<HttpEvent<{ success: boolean; downloadUrl: string; icon?: string }>> {
    const formData = new FormData();
    formData.append('AppPackage', file, file.name);
    formData.append('Name', metadata.name);
    formData.append('Description', metadata.description);
    formData.append('ExecutableName', metadata.executableName);
    if (customIcon) {
      formData.append('CustomIcon', customIcon, customIcon.name);
    }
    if (platform) {
      formData.append('Platform', platform);
    }
    if (repoId) {
      formData.append('RepoId', repoId);
    }
    const req = new HttpRequest('POST',
      `/api/MdAppStore/publish?ConnectionId=${this.connectionId}`,
      formData, { reportProgress: true });
    return this.http.request<{ success: boolean; downloadUrl: string; icon?: string }>(req);
  }
}
