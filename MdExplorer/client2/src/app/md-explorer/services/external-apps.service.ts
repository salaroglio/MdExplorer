import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { MdeAppsConfig, MdeTreeNode } from '../models/mde-apps-tree.models';

export interface MdeAppDefinition {
  id: string;
  name: string;
  description?: string;
  icon?: string;
  executable: string;
  args?: string[];
  treePosition?: 'top' | 'bottom';
  singleton?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ExternalAppsService {
  constructor(
    private http: HttpClient,
    private serverMessages: MdServerMessagesService
  ) {}

  private get connectionId(): string {
    return this.serverMessages.connectionId || '';
  }

  getApps(): Observable<MdeAppDefinition[]> {
    return this.http.get<MdeAppDefinition[]>(
      `/api/MdExternalApps?ConnectionId=${this.connectionId}`
    );
  }

  addApp(app: MdeAppDefinition, projectPath?: string): Observable<{ success: boolean }> {
    const params: any = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.post<{ success: boolean }>(
      `/api/MdExternalApps/Add`,
      app,
      { params }
    );
  }

  deleteApp(id: string, projectPath?: string): Observable<{ success: boolean }> {
    const params: any = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.delete<{ success: boolean }>(
      `/api/MdExternalApps/${encodeURIComponent(id)}`,
      { params }
    );
  }

  getConfig(projectPath?: string): Observable<MdeAppsConfig> {
    const params: any = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.get<MdeAppsConfig>(`/api/MdExternalApps/config`, { params });
  }

  saveTree(tree: MdeTreeNode[], projectPath?: string): Observable<{ success: boolean }> {
    const params: any = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.put<{ success: boolean }>(`/api/MdExternalApps/tree`, tree, { params });
  }

  saveConfig(config: MdeAppsConfig, projectPath?: string): Observable<{ success: boolean }> {
    const params: any = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.put<{ success: boolean }>(`/api/MdExternalApps/config`, config, { params });
  }
}
