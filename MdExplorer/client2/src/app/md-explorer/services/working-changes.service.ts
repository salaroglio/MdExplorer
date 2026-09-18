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
 * Un repository dentro il contesto: il progetto stesso, o uno dei suoi submodule.
 * È l'unità di cui si parla perché è l'unità in cui si **committa**.
 */
export interface RepoChanges {
  /** Vuoto per la radice; per i submodule il percorso relativo ad essa. */
  path: string;
  label: string;
  /** 0 = radice, 1 = submodule, 2 = submodule dentro un submodule. Solo per il rientro. */
  depth: number;

  /** `null` se `detached`. */
  branch: string | null;
  detached: boolean;
  upstream: string | null;
  baseBranch: string | null;
  ahead: number;
  behind: number;

  /** Il padre registra un commit diverso: è lavoro **del padre**, non di questo repository. */
  pointerMoved: boolean;
  /** Dichiarato ma mai scaricato: invisibile a `git status`, quindi va detto qui. */
  notInitialized: boolean;

  /**
   * Il commit che il **progetto registra** per questo submodule non è su nessun remoto: è il
   * segnale del disastro. Non coincide con `ahead` — con HEAD staccato `ahead` è 0.
   */
  recordedCommitUnpublished: boolean;
  /** Perché non si è potuto stabilirlo. `null` = si è stabilito. */
  recordedCommitUnknown: string | null;

  files: WorkingChange[];
  /** Commit locali che il ramo di riferimento non ha ancora: da pushare. */
  unpushed?: WorkingChange[];
  /** Ciò che il ramo di riferimento ha e tu no: da scaricare. Non è lavoro tuo. */
  incoming?: WorkingChange[];

  /** Perché qui non si può committare. `null` = si può. Mai disabilitare senza dirlo. */
  commitBlocker: string | null;
  /** Perché pushare questo repository romperebbe qualcosa per gli altri. */
  pushWarnings: string[];
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
  /** `repos[0]` è **sempre** la radice, poi i submodule in ordine di percorso. */
  repos: RepoChanges[];
  /** Cartella senza git: va detto, non mostrato come "nessuna modifica". */
  notAGitRepository: boolean;
  /** Condizione che l'utente può risolvere (nessun progetto, agente senza posto). */
  problem: string | null;
}

/** Un passo del pubblica-tutto: quale repository, com'è andata. */
export interface PushStep {
  repo: string;
  label: string;
  ok: boolean;
  outcome: string;
}

export interface SafePushResult {
  success: boolean;
  /** Perché non si è nemmeno partiti: `null` = si è partiti. */
  refused: string | null;
  steps: PushStep[];
  /** Cosa non è stato pubblicato perché non committato. */
  leftBehind: string[];
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

  /**
   * `repo` vuoto = la radice; altrimenti il percorso del submodule, e `path` è relativo a
   * **quello**. Un file dentro un submodule appartiene a un altro repository: chiedere il suo
   * diff alla radice non darebbe niente.
   */
  diff(projectPath: string, agent: string | null, path: string, repo = '', oldPath = ''): Observable<{ path: string; diff: string }> {
    const params: any = { projectPath, path };
    if (agent) params.agent = agent;
    if (repo) params.repo = repo;
    // Solo per le rinomine: git accoppia i due lati solo se li vede entrambi.
    if (oldPath) params.oldPath = oldPath;
    return this.http.get<{ path: string; diff: string }>('../api/WorkingChanges/diff', { params });
  }

  /** Irreversibile: nessun commit trattiene ciò che si butta via. */
  /**
   * Pubblica il progetto e i suoi submodule. **I figli prima, il padre per ultimo**: qualunque
   * fallimento a monte lascia il remoto vecchio ma coerente, mai rotto.
   */
  pushAll(projectPath: string, agent: string | null): Observable<SafePushResult> {
    return this.http.post<SafePushResult>('../api/WorkingChanges/push-all', {
      projectPath, agent: agent || null,
    });
  }

  discard(projectPath: string, agent: string | null, path: string, repo = ''): Observable<{ path: string; outcome: string }> {
    return this.http.post<{ path: string; outcome: string }>('../api/WorkingChanges/discard', {
      projectPath, agent: agent || null, path, repo: repo || null,
    });
  }
}
