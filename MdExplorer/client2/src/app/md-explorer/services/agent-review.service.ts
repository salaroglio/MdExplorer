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
  /** Che cosa fa l'agente che ha consegnato (dalla sua scheda); assente se non lo dichiara. */
  agentSummary?: string | null;
  /** A chi può passare il lavoro chi approva (scheda dell'agente: `on_approval_notify`). Vuota = nessun avviso. */
  notifyCandidates: NotifyCandidate[];
  /** Esito dell'avviso, presente solo nella risposta a «Autorizza». */
  notice?: { notified: boolean; recipient: string; error: string | null } | null;
}

/** Un collega a cui «Autorizza» può passare il lavoro, e se oggi è raggiungibile. */
export interface NotifyCandidate {
  name: string;
  role: string | null;
  available: boolean;
  /** Perché non è raggiungibile (non esiste, non è fidato…); null se lo è. */
  reason: string | null;
}

/** La scelta di chi approva: un collega, oppure nessuno. */
export interface ApproveChoice {
  notify?: string;
  nobody?: boolean;
}

/** Un documento citato in un messaggio, e dove sta: in una consegna da approvare, nel progetto, da nessuna parte. */
export interface MailArtifact {
  path: string;
  /** `toPull` = approvato, sul ramo principale di origin, non ancora scaricato nella cartella. */
  state: 'pending' | 'inProject' | 'toPull' | 'missing';
  /** La richiesta da approvare che lo contiene, quando è `pending`. */
  requestId: string | null;
  agentName: string | null;
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

  /** Dove stanno i documenti che un messaggio cita. */
  artifacts(projectPath: string, agent: string, paths: string[]): Observable<{ artifacts: MailArtifact[] }> {
    return this.http.post<{ artifacts: MailArtifact[] }>('../api/AgentReview/artifacts', { projectPath, agent, paths });
  }

  approve(id: string, choice: ApproveChoice = {}): Observable<MergeRequest> {
    return this.http.post<MergeRequest>(`../api/AgentReview/requests/${id}/approve`, choice);
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
