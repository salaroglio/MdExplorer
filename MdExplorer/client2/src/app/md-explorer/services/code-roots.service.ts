import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Lo stato di una radice di codice esterna su questa macchina (sprint 2026-10-12-Radici-Di-Codice-Esterne). */
export type CodeRootStatus = 'Linked' | 'NotLinked' | 'NotFound' | 'Mismatch';

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
}
