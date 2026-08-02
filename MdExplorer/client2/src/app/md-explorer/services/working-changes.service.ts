import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Come è cambiato un file rispetto al ramo di partenza. */
export type ChangeKind = 'added' | 'modified' | 'deleted' | 'renamed' | 'untracked';

export interface WorkingChange {
  change: ChangeKind;
  path: string;
  oldPath?: string;
}

/**
 * Cosa è cambiato in un contesto: il tuo lavoro nel progetto, oppure quello di un agente
 * nel suo posto di lavoro. La domanda è la stessa, quindi la risposta ha la stessa forma.
 */
export interface WorkingChangesView {
  contextKind: 'user' | 'agent';
  contextLabel: string;
  rootPath: string;
  branch: string | null;
  baseBranch: string | null;
  files: WorkingChange[];
  /** Cartella senza git: va detto, non mostrato come "nessuna modifica". */
  notAGitRepository: boolean;
  /** Condizione che l'utente può risolvere (nessun progetto, agente senza posto). */
  problem: string | null;
}

@Injectable({ providedIn: 'root' })
export class WorkingChangesService {
  constructor(private http: HttpClient) {}

  /**
   * `agent` assente = il lavoro dell'utente. I dati arrivano da GIT, non dal
   * FileSystemWatcher: modificando un file con un editor esterno la vista non se ne accorge
   * da sola, si aggiorna all'apertura, col pulsante rinfresca e dopo ogni azione.
   */
  list(projectPath: string, agent?: string | null): Observable<WorkingChangesView> {
    const params: any = { projectPath };
    if (agent) params.agent = agent;
    return this.http.get<WorkingChangesView>('../api/WorkingChanges/list', { params });
  }

  diff(projectPath: string, agent: string | null, path: string): Observable<{ path: string; diff: string }> {
    const params: any = { projectPath, path };
    if (agent) params.agent = agent;
    return this.http.get<{ path: string; diff: string }>('../api/WorkingChanges/diff', { params });
  }

  /** Irreversibile: nessun commit trattiene ciò che si butta via. */
  discard(projectPath: string, agent: string | null, path: string): Observable<{ path: string; outcome: string }> {
    return this.http.post<{ path: string; outcome: string }>('../api/WorkingChanges/discard', {
      projectPath, agent: agent || null, path,
    });
  }
}
