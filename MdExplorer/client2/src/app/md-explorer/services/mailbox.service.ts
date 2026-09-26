import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

/** Un messaggio della inbox dell'umano (agente→user), §13 Fase 4a. */
export interface MailboxMessage {
  id: string;
  conversationId: string;
  fromAgent: string;
  projectPath: string;
  body: string;
  bodyPreview: string;
  topics: string[];
  createdAt: string;
  readAt: string | null;
  read: boolean;
}

export interface MailboxInbox {
  messages: MailboxMessage[];
  unread: number;
}

/** Riepilogo di un thread di conversazione (§8), per l'osservabilità/governo (Fase 4b). */
export interface ConversationSummary {
  id: string;
  projectPath: string;
  startedBy: string;
  status: string;              // active | completed | killed | exhausted
  hopCount: number;
  hopLimit: number;
  messageCount: number;
  participants: string[];
  startedAt: string;
  lastActivityAt: string;
  federationId?: string | null;
  remoteOwner?: string | null;
  remoteAgent?: string | null;
  federated?: boolean;
}

/** Un messaggio dentro un thread (vista dettaglio). */
export interface ConversationMessage {
  id: string;
  fromAgent: string;
  toAgent: string;
  body: string;
  topics: string[];
  state: string;
  createdAt: string;
  processedAt: string | null;
  readAt: string | null;
  error: string | null;
}

export interface ConversationThread {
  conversation: ConversationSummary;
  messages: ConversationMessage[];
}

/**
 * La porta dell'umano sulla mailbox della città (§13 Fase 4a): legge i messaggi
 * indirizzati a `user`, li marca letti e risponde risvegliando l'agente nella stessa
 * conversazione. Speculare a MailboxController lato Service (/api/A2A/mailbox).
 */
@Injectable({ providedIn: 'root' })
export class MailboxService {
  constructor(private http: HttpClient) {}

  inbox(projectPath: string, includeRead = false): Observable<MailboxInbox> {
    let params = new HttpParams().set('includeRead', includeRead);
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<MailboxInbox>('/api/A2A/mailbox/inbox', { params });
  }

  unreadCount(projectPath: string): Observable<{ unread: number }> {
    let params = new HttpParams();
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<{ unread: number }>('/api/A2A/mailbox/inbox/count', { params });
  }

  markRead(messageId: string): Observable<{ read: boolean; readAt: string }> {
    return this.http.post<{ read: boolean; readAt: string }>(
      `/api/A2A/mailbox/inbox/${messageId}/read`, null);
  }

  reply(conversationId: string, body: string):
    Observable<{ accepted: boolean; taskId: string; conversationId: string; toAgent: string }> {
    return this.http.post<{ accepted: boolean; taskId: string; conversationId: string; toAgent: string }>(
      '/api/A2A/mailbox/reply', { conversationId, body });
  }

  // ---- 4b: osservabilità e governo dei thread ----

  conversations(projectPath: string): Observable<{ conversations: ConversationSummary[] }> {
    let params = new HttpParams();
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<{ conversations: ConversationSummary[] }>(
      '/api/A2A/mailbox/conversations', { params });
  }

  conversationMessages(conversationId: string): Observable<ConversationThread> {
    return this.http.get<ConversationThread>(
      `/api/A2A/mailbox/conversations/${conversationId}/messages`);
  }

  kill(conversationId: string): Observable<{ status: string }> {
    return this.http.post<{ status: string }>(
      `/api/A2A/mailbox/conversations/${conversationId}/kill`, null);
  }

  reopen(conversationId: string): Observable<{ status: string; hopCount: number }> {
    return this.http.post<{ status: string; hopCount: number }>(
      `/api/A2A/mailbox/conversations/${conversationId}/reopen`, null);
  }

  // ---- Consolidamento memoria (Fase 7f) ----

  /** Fatti in memoria del progetto (tutti gli agenti + shared), per la scelta di promozione. */
  memoryFacts(projectPath: string): Observable<{ facts: MemFact[] }> {
    const params = new HttpParams().set('projectPath', projectPath || '');
    return this.http.get<{ facts: MemFact[] }>('/api/mem/facts', { params });
  }

  /** Consolida una conversazione: promuove i fatti scelti nel .agent.md e decade il resto. */
  consolidate(conversationId: string, projectPath: string, promote: { factUri: string; graph: string; statement: string }[])
    : Observable<{ consolidated: boolean; memoryDisabled?: boolean; promoted?: number; decayed?: number; deleted?: number; agents?: string[] }> {
    return this.http.post<any>(
      `/api/mem/conversations/${conversationId}/consolidate`, { projectPath, promote });
  }
}

/** Un fatto in memoria (proiezione di /api/mem/facts). */
export interface MemFact {
  factUri: string;
  graph: string;
  agent: string;
  statement: string;
  confidence: number;
  tags: string[];
  shared: boolean;
}
