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
}
