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
  /** Archiviato: fuori dall'elenco della posta, ma non cancellato. */
  archived?: boolean;
  /** Il turno di lavoro dell'agente che l'ha scritto: lega il messaggio alla richiesta di approvazione dello stesso turno. */
  runId?: string | null;
  /** Le risposte che l'agente propone: i pulsanti sotto il messaggio. */
  replies?: MailReply[];
  /** La scheda dell'agente dichiara le sue risposte: se il messaggio non ne propone, l'agente non chiede niente. */
  declaresReplies?: boolean;
  /** L'artefatto del turno che ha scritto il messaggio: 'pending' = da decidere, 'rejected' = rifiutato; in entrambi i casi non si risponde. */
  artifact?: string | null;
  /** I lavori che il turno di questo messaggio ha chiesto ad altri agenti, con il loro stato: per chi li aspetta. */
  awaited?: AwaitedWork[];
}

/** Un lavoro chiesto a un altro agente, visto da chi lo aspetta. */
/**
 * Un incarico che il workflow dice di far avviare al responsabile dell'agente (start: ask-owner): aspetta nella posta
 * che la persona lo avvii dalla schermata di lancio, o lo rifiuti con un motivo.
 */
export interface ToStartAssignment {
  id: string;
  conversationId: string;
  fromAgent: string;
  toAgent: string;
  body: string;
  createdAt: string;
  runId?: string | null;
  /** Il passo del workflow, con il suo titolo («Scheda tecnica»). */
  step?: string | null;
  agentFilePath?: string | null;
}

export interface AwaitedWork {
  messageId: string;
  agent: string;
  /**
   * tostart = aspetta che il responsabile lo avvii · declined = il responsabile non l'ha avviato · working = sta lavorando ·
   * approval = artefatto in approvazione · approved · rejected = rifiutato, fermo · reworking · done = concluso senza artefatto · failed
   */
  state: 'tostart' | 'declined' | 'working' | 'approval' | 'approved' | 'rejected' | 'reworking' | 'done' | 'failed';
  /** Il motivo del rifiuto, quando c'è. */
  note?: string | null;
}

/** Una risposta dichiarata dalla scheda dell'agente e proposta con il messaggio. */
export interface MailReply {
  id: string;
  /** Il testo del pulsante. */
  label: string;
  /** Che cosa succede premendo: chi viene contattato e per ottenere cosa. */
  description: string;
  /** Il messaggio che l'agente riceve. */
  message: string;
}

export interface MailboxInbox {
  messages: MailboxMessage[];
  unread: number;
  /** Gli incarichi che aspettano te per partire. */
  toStart?: ToStartAssignment[];
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

  /** `archived` = solo l'archivio; altrimenti ciò che è in posta (tutto, o solo i non letti). */
  inbox(projectPath: string, includeRead = false, archived = false): Observable<MailboxInbox> {
    let params = new HttpParams().set('includeRead', includeRead).set('archived', archived);
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<MailboxInbox>('/api/A2A/mailbox/inbox', { params });
  }

  /** Avvia un incarico in attesa del responsabile, con le sue indicazioni e il motore e il modello scelti. */
  startAssignment(messageId: string, body: { note?: string; provider?: string; model?: string }): Observable<{ messageId: string; toAgent: string }> {
    return this.http.post<{ messageId: string; toAgent: string }>(`/api/A2A/mailbox/to-start/${messageId}/start`, body);
  }

  /** Il responsabile non avvia l'incarico, e dice perché. */
  declineAssignment(messageId: string, reason: string): Observable<{ messageId: string }> {
    return this.http.post<{ messageId: string }>(`/api/A2A/mailbox/to-start/${messageId}/decline`, { reason });
  }

  unreadCount(projectPath: string): Observable<{ unread: number }> {
    let params = new HttpParams();
    if (projectPath) params = params.set('projectPath', projectPath);
    return this.http.get<{ unread: number }>('/api/A2A/mailbox/inbox/count', { params });
  }

  /** Segna come letti tutti i messaggi aperti del progetto: la posta si svuota, niente viene cancellato. */
  markAllRead(projectPath: string): Observable<{ read: number }> {
    const params = new HttpParams().set('projectPath', projectPath);
    return this.http.post<{ read: number }>('/api/A2A/mailbox/inbox/read-all', null, { params });
  }

  /** Archivia un messaggio: esce dall'elenco, non viene cancellato. */
  archive(messageId: string): Observable<{ archived: boolean }> {
    return this.http.post<{ archived: boolean }>(`/api/A2A/mailbox/inbox/${messageId}/archive`, null);
  }

  /** Riporta in posta un messaggio archiviato. */
  unarchive(messageId: string): Observable<{ archived: boolean }> {
    return this.http.post<{ archived: boolean }>(`/api/A2A/mailbox/inbox/${messageId}/unarchive`, null);
  }

  /** Archivia tutti i messaggi in posta del progetto. */
  archiveAll(projectPath: string): Observable<{ archived: number }> {
    const params = new HttpParams().set('projectPath', projectPath);
    return this.http.post<{ archived: number }>('/api/A2A/mailbox/inbox/archive-all', null, { params });
  }

  markRead(messageId: string): Observable<{ read: boolean; readAt: string }> {
    return this.http.post<{ read: boolean; readAt: string }>(
      `/api/A2A/mailbox/inbox/${messageId}/read`, null);
  }

  /** choice = true: body è il messaggio di uno dei pulsanti proposti; false: testo libero, che arriva con il messaggio citato.
   *  messageId: il messaggio a cui si risponde (senza, l'ultimo del thread). */
  reply(conversationId: string, body: string, choice = false, messageId?: string):
    Observable<{ accepted: boolean; taskId: string; conversationId: string; toAgent: string }> {
    return this.http.post<{ accepted: boolean; taskId: string; conversationId: string; toAgent: string }>(
      '/api/A2A/mailbox/reply', { conversationId, body, choice, messageId });
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
