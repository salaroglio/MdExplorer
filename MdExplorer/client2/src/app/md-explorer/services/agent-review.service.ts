import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Come è cambiato un file nel lavoro dell'agente. */
export type FileChange = 'added' | 'modified' | 'deleted' | 'renamed';

export interface ChangedFile {
  change: FileChange;
  path: string;
}

/**
 * Una richiesta di merge: un agente ha finito e chiede di entrare nel ramo principale.
 * I file toccati sono una FOTOGRAFIA presa al momento della richiesta — è ciò su cui
 * l'umano decide, e non deve cambiargli sotto gli occhi mentre lo guarda.
 */
export interface MergeRequest {
  id: string;
  agentName: string;
  branch: string;
  headSha: string;
  createdAt: string;
  status: string;
  note: string | null;
  /** C'è una sessione d'intervento aperta su questo agente: ci stai già lavorando. */
  sessionOpen: boolean;
  files: ChangedFile[];
}

export interface TakeResult {
  worktreePath: string;
  folderOpened: boolean;
  sessionOpen: boolean;
  agentQueued: boolean;
}

@Injectable({ providedIn: 'root' })
export class AgentReviewService {
  constructor(private http: HttpClient) {}

  pending(projectPath: string): Observable<{ requests: MergeRequest[] }> {
    return this.http.get<{ requests: MergeRequest[] }>('../api/AgentReview/requests', {
      params: { projectPath },
    });
  }

  approve(id: string): Observable<MergeRequest> {
    return this.http.post<MergeRequest>(`../api/AgentReview/requests/${id}/approve`, {});
  }

  reject(id: string, note?: string): Observable<MergeRequest> {
    return this.http.post<MergeRequest>(`../api/AgentReview/requests/${id}/reject`, { note });
  }

  /** Apre la sessione d'intervento e la cartella del worktree sul filesystem. */
  take(id: string): Observable<TakeResult> {
    return this.http.post<TakeResult>(`../api/AgentReview/requests/${id}/take`, {});
  }

  /** Chiude la sessione. `discard` = ho annullato: il lavoro torna in coda all'agente. */
  release(id: string, discard: boolean): Observable<{ closed: boolean; requeued: number; message: string }> {
    return this.http.post<any>(`../api/AgentReview/requests/${id}/release`, {}, {
      params: { discard },
    });
  }
}
