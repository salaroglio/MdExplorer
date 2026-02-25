import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

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

  addApp(app: MdeAppDefinition): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(
      `/api/MdExternalApps/Add?ConnectionId=${this.connectionId}`,
      app
    );
  }

  deleteApp(id: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(
      `/api/MdExternalApps/${encodeURIComponent(id)}?ConnectionId=${this.connectionId}`
    );
  }
}
