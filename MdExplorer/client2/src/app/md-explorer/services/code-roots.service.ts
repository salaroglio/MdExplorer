import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Lo stato di una radice di codice esterna su questa macchina (sprint 2026-10-12-Radici-Di-Codice-Esterne). */
export type CodeRootStatus = 'Linked' | 'NotLinked' | 'NotFound' | 'Mismatch' | 'Unreachable' | 'ConsentRequired';

export interface CodeRoot {
  alias: string;
  repository: string | null;
  subpath: string;
  tags: string[];
  languages: string[];
  localPath: string | null;
  rootPath: string | null;
  status: CodeRootStatus;
  reason: string | null;
  actualRemotes: string[];
  /** Radice remota (sprint 2026-10-21-Cartella-Remota-Via-SSH): la cartella analizzata sul server, ssh://host/…; rootPath è null. */
  remote: string | null;
}

export interface CodeRootChanges {
  alias: string;
  isGit: boolean;
  branch: string | null;
  files: { status: string; path: string }[];
}

/** Un figlio di una cartella di una radice, per l'albero in sola lettura (R9). */
export interface CodeRootEntry {
  name: string;
  /** Il percorso logico @alias/…: si apre con questo e con questo si chiedono i figli. */
  path: string;
  fullPath: string;
  isFolder: boolean;
  isText: boolean;
  /** Perché il file non si è potuto leggere; null se letto. */
  unreadable: string | null;
}

/** Le sottocartelle di una cartella di un server, per scegliere che cosa collegare (sprint 2026-10-21, F3). */
export interface ServerBrowse {
  host: string;
  path: string;
  home: string;
  /** ssh://host/percorso: quello che si collega. */
  location: string;
  parent: string | null;
  folders: { name: string; path: string }[];
}

/** Le scelte per un server: cartella del serverino (P11) e consenso a caricarlo (P13). */
export interface ServerSettings {
  host: string;
  destination: string;
  agentFolder: string | null;
  defaultAgentFolder: string;
  uploadAllowed: boolean;
}

/** Il 409 del backend: il serverino va caricato e serve il sì dell'utente. */
export interface UploadConsent { destination: string; size: number; folder: string; }

/** Un'operazione su un file o una cartella di una radice remota (D7). */
export type RemoteFileOp = 'read' | 'write' | 'create' | 'mkdir' | 'delete' | 'move' | 'grep';

export interface RemoteFileResult {
  path: string;
  text?: string;
  stamp?: string;
  bom?: boolean;
  truncated?: boolean;
  hits?: { path: string; line: number; text: string }[];
}

/** Il consenso richiesto, se l'errore HTTP è il 409 del serverino da caricare; altrimenti null. */
export function uploadConsentOf(err: any): UploadConsent | null {
  return err?.status === 409 && err?.error?.consent ? err.error.consent as UploadConsent : null;
}

/**
 * Le radici di codice esterne: il codice che sta FUORI dalla cartella del progetto di documentazione. L'identità
 * (alias, repository) è nel .development.yml, su Git; dove sta su questa macchina è nel database dell'utente.
 */
@Injectable({ providedIn: 'root' })
export class CodeRootsService {
  private readonly api = '../api/coderoots';

  constructor(private http: HttpClient) { }

  list(projectPath: string): Observable<CodeRoot[]> {
    return this.http.get<CodeRoot[]>(this.api, { params: new HttpParams().set('projectPath', projectPath) });
  }

  link(projectPath: string, alias: string, localPath: string): Observable<CodeRoot> {
    return this.http.post<CodeRoot>(`${this.api}/link`, { projectPath, alias, localPath });
  }

  unlink(projectPath: string, alias: string): Observable<{ alias: string; linked: boolean }> {
    return this.http.post<{ alias: string; linked: boolean }>(`${this.api}/unlink`, { projectPath, alias });
  }

  /** Le radici non utilizzabili qui per cui l'utente non ha detto «non ricordarmelo più». */
  reminder(projectPath: string): Observable<CodeRoot[]> {
    return this.http.get<CodeRoot[]>(`${this.api}/reminder`, { params: new HttpParams().set('projectPath', projectPath) });
  }

  dismissReminder(projectPath: string, aliases: string[]): Observable<{ dismissed: string[] }> {
    return this.http.post<{ dismissed: string[] }>(`${this.api}/reminder/dismiss`, { projectPath, aliases });
  }

  /**
   * L'avviso all'apertura compare una volta per progetto in questa sessione dell'app: una riconnessione SignalR o
   * il ritorno all'albero non lo ripropongono. «Non ricordarmelo più» invece sta nel database dell'utente.
   */
  private readonly remindedThisSession = new Set<string>();

  shouldRemind(projectPath: string): boolean {
    const key = (projectPath || '').replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (!key || this.remindedThisSession.has(key)) { return false; }
    this.remindedThisSession.add(key);
    return true;
  }

  /** I figli diretti di una cartella di una radice (@alias o @alias/…): cartelle prima, poi file. */
  browse(projectPath: string, path: string): Observable<CodeRootEntry[]> {
    return this.http.get<CodeRootEntry[]>(`${this.api}/browse`, { params: new HttpParams().set('projectPath', projectPath).set('path', path) });
  }

  changes(projectPath: string, alias: string): Observable<CodeRootChanges> {
    return this.http.get<CodeRootChanges>(`${this.api}/changes`, { params: new HttpParams().set('projectPath', projectPath).set('alias', alias) });
  }

  // ---------------------------------------------------------------- cartelle remote via SSH (sprint 2026-10-21, F3)

  /** Gli alias di ~/.ssh/config, da proporre: si può scrivere qualunque host. */
  sshHosts(): Observable<string[]> {
    return this.http.get<string[]>(`${this.api}/ssh-hosts`);
  }

  /** Le sottocartelle di una cartella del server; path vuoto = la home. 409 = serve il consenso (uploadConsentOf). */
  serverBrowse(host: string, path: string | null): Observable<ServerBrowse> {
    let params = new HttpParams().set('host', host);
    if (path) { params = params.set('path', path); }
    return this.http.get<ServerBrowse>(`${this.api}/server-browse`, { params });
  }

  serverSettings(host: string): Observable<ServerSettings> {
    return this.http.get<ServerSettings>(`${this.api}/server-settings`, { params: new HttpParams().set('host', host) });
  }

  saveServerSettings(host: string, agentFolder: string | null, allowUpload: boolean): Observable<ServerSettings> {
    return this.http.post<ServerSettings>(`${this.api}/server-settings`, { host, agentFolder, allowUpload });
  }

  /** Un'operazione sui file di una radice remota: path e to sono percorsi logici @alias/…. */
  file(projectPath: string, op: RemoteFileOp, path: string, extra: { to?: string; text?: string; stamp?: string; recursive?: boolean; pattern?: string; ignoreCase?: boolean } = {}): Observable<RemoteFileResult> {
    return this.http.post<RemoteFileResult>(`${this.api}/files`, { projectPath, op, path, ...extra });
  }
}
