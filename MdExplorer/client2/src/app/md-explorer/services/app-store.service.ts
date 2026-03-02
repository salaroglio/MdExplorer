import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { StoreCatalog, InstalledApp, AppStoreRepository } from '../models/app-store.models';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

@Injectable({
  providedIn: 'root'
})
export class AppStoreService {

  private _platform: string | null = null;

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

  getCatalog(): Observable<StoreCatalog> {
    return this.http.get<StoreCatalog>(`/api/MdAppStore/catalog?ConnectionId=${this.connectionId}`);
  }

  getInstalled(): Observable<InstalledApp[]> {
    return this.http.get<InstalledApp[]>(`/api/MdAppStore/installed?ConnectionId=${this.connectionId}`);
  }

  installApp(app: { id: string; name: string; description?: string; version: string; downloadUrl: string; icon?: string; repoId?: string }, platform?: string): Observable<{ success: boolean; installDir: string }> {
    return this.http.post<{ success: boolean; installDir: string }>(
      `/api/MdAppStore/install?ConnectionId=${this.connectionId}`,
      {
        appId: app.id,
        name: app.name,
        description: app.description,
        version: app.version,
        downloadUrl: app.downloadUrl,
        icon: app.icon,
        platform: platform || undefined,
        repoId: app.repoId || undefined
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

  publishApp(file: File, metadata: { name: string; description: string }, customIcon?: File, platform?: string, repoId?: string): Observable<{ success: boolean; downloadUrl: string; icon?: string }> {
    const formData = new FormData();
    formData.append('AppPackage', file, file.name);
    formData.append('Name', metadata.name);
    formData.append('Description', metadata.description);
    if (customIcon) {
      formData.append('CustomIcon', customIcon, customIcon.name);
    }
    if (platform) {
      formData.append('Platform', platform);
    }
    if (repoId) {
      formData.append('RepoId', repoId);
    }
    return this.http.post<{ success: boolean; downloadUrl: string; icon?: string }>(
      `/api/MdAppStore/publish?ConnectionId=${this.connectionId}`, formData
    );
  }
}
