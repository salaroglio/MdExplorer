import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface AgentQueueMessage {
  id: string;
  conversationId: string;
  fromAgent: string;
  state: string;
  deferredReason: string | null;   // resources | maintenance | user | null
  topics: string[];
  bodyPreview: string;
  attempts: number;
  createdAt: string;
  nextAttemptAt: string | null;
}

export interface AgentQueueFederated {
  id: string;
  fromOwner: string;
  fromAgent: string;
  scope: string;
  message: string;
  createdAt: string;
}

export interface AgentQueue {
  agent: string;
  messages: AgentQueueMessage[];
  federatedPending: AgentQueueFederated[];
}

/**
 * La coda di lavoro di un agente (§12.5/§12.6, Fase 6d): messaggi non conclusi (inclusi i
 * parcheggiati, col motivo) + richieste federate in attesa di gate. Speculare a
 * AgentQueueController (/api/A2A/agents).
 */
@Injectable({ providedIn: 'root' })
export class AgentQueueService {
  constructor(private http: HttpClient) {}

  queue(agentName: string, projectPath?: string): Observable<AgentQueue> {
    let params = new HttpParams();
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<AgentQueue>(`/api/A2A/agents/${encodeURIComponent(agentName)}/queue`, { params });
  }

  force(messageId: string): Observable<{ forced: boolean }> {
    return this.http.post<{ forced: boolean }>(`/api/A2A/agents/queue/${messageId}/force`, null);
  }

  discard(messageId: string): Observable<{ discarded: boolean }> {
    return this.http.post<{ discarded: boolean }>(`/api/A2A/agents/queue/${messageId}/discard`, null);
  }
}
