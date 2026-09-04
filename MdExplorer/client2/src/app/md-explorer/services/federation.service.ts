import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

/** Una richiesta di intervento federata in attesa del gate umano (§12.6). */
export interface FederationRequest {
  id: string;
  federationId: string;
  projectPath: string;
  fromOwner: string;
  fromAgent: string;
  targetAgent: string;
  scope: string;
  message: string;
  topics: string[];
  status: string;         // pending | approved | rejected
  createdAt: string;
  decidedAt: string | null;
}

/** Una città locale attiva sul relay (§12.5). */
export interface FederationCity {
  projectPath: string;
  projectName: string;
  roomId: string;
  relayUrl: string;
}

/**
 * La porta dell'umano sulla federazione (§12.5/§12.6): le città accese e — cuore della
 * 6c — il gate delle richieste federate (approva/rifiuta). Speculare a FederationController
 * (/api/A2A/federation).
 */
@Injectable({ providedIn: 'root' })
export class FederationService {
  constructor(private http: HttpClient) {}

  cities(projectPath?: string): Observable<{ local: FederationCity[]; remote: any[]; relayConnected: boolean }> {
    let params = new HttpParams();
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<any>('/api/A2A/federation/cities', { params });
  }

  requests(projectPath?: string, includeDecided = false): Observable<{ requests: FederationRequest[] }> {
    let params = new HttpParams().set('includeDecided', includeDecided);
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<{ requests: FederationRequest[] }>('/api/A2A/federation/requests', { params });
  }

  approve(id: string, targetAgent?: string): Observable<{ approved: boolean; conversationId: string; targetAgent: string }> {
    return this.http.post<any>(`/api/A2A/federation/requests/${id}/approve`, { targetAgent: targetAgent || null });
  }

  reject(id: string): Observable<{ rejected: boolean }> {
    return this.http.post<{ rejected: boolean }>(`/api/A2A/federation/requests/${id}/reject`, null);
  }

  // ---- Impersonazione utente (test della città) ----

  cityUsers(projectPath: string): Observable<{ testMode: boolean; users: CityUser[] }> {
    const params = new HttpParams().set('projectPath', projectPath || '');
    return this.http.get<{ testMode: boolean; users: CityUser[] }>('/api/A2A/federation/users', { params });
  }

  impersonationStatus(projectPath: string): Observable<ImpersonationStatus> {
    const params = new HttpParams().set('projectPath', projectPath || '');
    return this.http.get<ImpersonationStatus>('/api/A2A/federation/impersonate', { params });
  }

  setTestMode(enabled: boolean): Observable<{ testMode: boolean }> {
    return this.http.post<{ testMode: boolean }>('/api/A2A/federation/impersonate/test-mode', { enabled });
  }

  impersonate(projectPath: string, email: string): Observable<ImpersonationStatus> {
    return this.http.post<ImpersonationStatus>('/api/A2A/federation/impersonate', { projectPath, email });
  }

  stopImpersonation(projectPath: string): Observable<ImpersonationStatus> {
    const params = new HttpParams().set('projectPath', projectPath || '');
    return this.http.delete<ImpersonationStatus>('/api/A2A/federation/impersonate', { params });
  }
}

export interface CityUser {
  email: string;
  ownerId: string;
  displayName: string;
  isMe: boolean;
  /** true se la città REMOTA di questo padrone è accesa ora sulla stanza (badge presenza). */
  online: boolean;
}

export interface ImpersonationStatus {
  testMode: boolean;
  email: string;
  ownerId: string;
  impersonated: boolean;
}
