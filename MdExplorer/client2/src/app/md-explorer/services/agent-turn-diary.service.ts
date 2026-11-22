import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

/** Una voce del diario di un turno: un pezzo di risposta, di ragionamento, uno strumento usato, un errore. */
export interface AgentTurnDiaryEntry {
  seq: number;
  kind: 'message' | 'thinking' | 'tool' | 'error';
  text: string;
  atUtc: string;
  revision: number;
}

/** Il diario di un turno (IAgentTurnDiary lato Service), o la sua sola testata negli elenchi (`entries` vuoto). */
export interface AgentTurnDiary {
  id: string;
  agentName: string;
  projectPath: string;
  /** Il motore del turno, come lo si mostra: Copilot, Claude Code, opencode. */
  engine: string;
  startedAtUtc: string;
  /** null finché il turno è in corso. */
  finishedAtUtc: string | null;
  /** null finché il turno è in corso; poi Completed, ProviderError, Cancelled… */
  outcome: string | null;
  diagnostic: string | null;
  version: number;
  truncated: boolean;
  entries: AgentTurnDiaryEntry[];
}

/** L'avviso che un diario è cambiato: solo chi e a che versione, il testo lo chiede chi ha il pannello aperto. */
export interface AgentTurnDiaryChange {
  id: string;
  projectPath: string;
  version: number;
  finished: boolean;
}

/**
 * Che cosa sta facendo, e che cosa ha fatto, un agente in un turno. Il Service avvisa a ogni cambiamento
 * (`agentTurnDiaryChanged`, al più ogni 300 ms per turno); qui si rilegge «dalla versione che ho già».
 */
@Injectable({ providedIn: 'root' })
export class AgentTurnDiaryService {
  changed$: Observable<AgentTurnDiaryChange>;

  constructor(private http: HttpClient, serverMessages: MdServerMessagesService) {
    this.changed$ = serverMessages.agentTurnDiary$.asObservable();
  }

  /** Il diario con le voci cambiate dopo `after` (0 = tutte). 404 se il turno non ha un diario. */
  read(id: string, after: number): Observable<AgentTurnDiary> {
    const params = new HttpParams().set('after', String(after || 0));
    return this.http.get<AgentTurnDiary>(`/api/A2A/activity/diary/${encodeURIComponent(id)}`, { params });
  }

  /** Le testate dei turni del progetto, i più recenti prima: quelli in corso e quelli finiti. */
  recent(projectPath: string, max: number): Observable<AgentTurnDiary[]> {
    const params = new HttpParams().set('projectPath', projectPath).set('max', String(max));
    return this.http.get<AgentTurnDiary[]>('/api/A2A/activity/recent', { params });
  }
}
