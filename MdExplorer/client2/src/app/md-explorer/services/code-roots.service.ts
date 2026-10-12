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

  changes(projectPath: string, alias: string): Observable<CodeRootChanges> {
    return this.http.get<CodeRootChanges>(`${this.api}/changes`, { params: new HttpParams().set('projectPath', projectPath).set('alias', alias) });
  }
}
