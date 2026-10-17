import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Come è cambiato un file rispetto al ramo di partenza. */
export type ChangeKind = 'added' | 'modified' | 'deleted' | 'renamed' | 'untracked';

export interface WorkingChange {
  change: ChangeKind;
  path: string;
  oldPath?: string;
}

export type SubmoduleRelation = 'same' | 'ahead' | 'behind' | 'diverged' | 'unknown';

/** Com'è andata un'azione su un repository (pubblica, scarica, allinea, annulla l'unione). */
/** La «sorgente» del progetto: un remoto `upstream` da cui si prendono soltanto aggiornamenti. */
export interface UpstreamStatus {
  /** false per quasi tutti i progetti: niente da mostrare. */
  hasUpstream: boolean;
  url: string | null;
  branch: string | null;
  /** Quanti aggiornamenti ha la sorgente che il progetto non ha. */
  behind: number;
  problem: string | null;
}

export interface RepoActionResult {
  success: boolean;
  /** Perché non si è nemmeno partiti: il motivo che la riga mostrava già sul pulsante spento. */
  refused: string | null;
  message: string | null;
  /** Cose da dire a operazione riuscita: un submodule lasciato dov'era, e perché. */
  warnings: string[];
  changedFiles: string[];
  contentChanged: boolean;
}

export interface RepoFetchOutcome {
  repo: string;
  label: string;
  ok: boolean;
  skipped: boolean;
  error: string | null;
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
  /** Solo ciò che `git status` vede: da committare. `files` include anche i commit non pubblicati. */
  uncommitted?: WorkingChange[];
  /** Ciò che il ramo di riferimento ha e tu no: da scaricare. Non è lavoro tuo. */
  incoming?: WorkingChange[];

  /** Il commit che il repository contenitore registra per questo submodule. `null` sulla radice. */
  recordedCommit?: string | null;
  /** Il commit in checkout qui. */
  headCommit?: string | null;
  /**
   * Dove sta il commit in checkout rispetto a quello registrato. `pointerMoved` da solo non dice
   * il verso: `ahead` = versione nuova da registrare, `behind` = va allineato.
   */
  relation?: SubmoduleRelation | null;
  /** I submodule che hanno una versione nuova da registrare QUI con un commit. */
  pointersToRegister?: string[];
  /** Un'unione rimasta a metà dopo uno scaricamento. */
  mergeInProgress?: boolean;
  conflicts?: string[];
  /** Con HEAD staccato: il ramo su cui «Aggiorna all'ultima» rimetterebbe il submodule. */
  detachedTarget?: string | null;
  /** Perché l'ultima interrogazione del remoto non è riuscita. */
  remoteProblem?: string | null;

  /** Perché qui non si può committare. `null` = si può. Mai disabilitare senza dirlo. */
  commitBlocker: string | null;
  /** Perché da qui non si può pubblicare. `null` = si può. */
  pushBlocker?: string | null;
  /** Perché qui non si può scaricare dal remoto. `null` = si può. */
  pullBlocker?: string | null;
  /** Perché questo submodule non si può allineare alla versione registrata. `null` = si può. */
  alignBlocker?: string | null;
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

  // ---- le azioni per riga dei pannelli git: una per repository ----

  /** Chiede a ogni remoto cosa c'è di nuovo: solo dopo i submodule sanno di essere indietro. */
  fetchAll(projectPath: string): Observable<RepoFetchOutcome[]> {
    return this.http.post<RepoFetchOutcome[]>('../api/RepoSync/fetch-all', { projectPath });
  }

  pushRepo(projectPath: string, agent: string | null, repo: string): Observable<RepoActionResult> {
    return this.http.post<RepoActionResult>('../api/RepoSync/push', { projectPath, agent: agent || null, repo: repo || null });
  }

  /** Sulla radice: scarica il progetto e allinea i submodule. Su un submodule: «Aggiorna all'ultima». */
  pullRepo(projectPath: string, repo: string, connectionId: string): Observable<RepoActionResult> {
    return this.http.post<RepoActionResult>('../api/RepoSync/pull', { projectPath, repo: repo || null, connectionId });
  }

  /** Porta un submodule alla versione che il progetto registra, solo in avanti. */
  alignRepo(projectPath: string, repo: string, connectionId: string): Observable<RepoActionResult> {
    return this.http.post<RepoActionResult>('../api/RepoSync/align', { projectPath, repo: repo || null, connectionId });
  }

  /** Scarica il progetto, se c'è da scaricare, e allinea i submodule. */
  pullAll(projectPath: string, connectionId: string): Observable<RepoActionResult> {
    return this.http.post<RepoActionResult>('../api/RepoSync/pull-all', { projectPath, connectionId });
  }

  /** Cosa ha di nuovo la sorgente (`upstream`). Con `fetch` la interroga prima. */
  upstreamStatus(projectPath: string, fetch: boolean): Observable<UpstreamStatus> {
    const params = new HttpParams().set('projectPath', projectPath).set('fetch', String(fetch));
    return this.http.get<UpstreamStatus>('../api/RepoSync/upstream', { params });
  }

  /** «Scarica gli aggiornamenti» dalla sorgente, e riallinea origin. */
  pullUpstream(projectPath: string, connectionId: string): Observable<RepoActionResult> {
    return this.http.post<RepoActionResult>('../api/RepoSync/pull-upstream', { projectPath, connectionId });
  }

  /** Annulla un'unione rimasta a metà: si torna a prima dello scaricamento. */
  abortMerge(projectPath: string, repo: string, connectionId: string): Observable<RepoActionResult> {
    return this.http.post<RepoActionResult>('../api/RepoSync/abort-merge', { projectPath, repo: repo || null, connectionId });
  }

  discard(projectPath: string, agent: string | null, path: string, repo = ''): Observable<{ path: string; outcome: string }> {
    return this.http.post<{ path: string; outcome: string }>('../api/WorkingChanges/discard', {
      projectPath, agent: agent || null, path, repo: repo || null,
    });
  }
}
