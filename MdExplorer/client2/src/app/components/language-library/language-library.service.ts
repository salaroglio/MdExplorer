import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Un pacchetto di linguaggio del grafo come lo mostra la libreria (12/11/2026). */
export interface LanguageLibraryEntry {
  id: string;
  displayName: string;
  version: number | null;
  extensions: string[];
  database: boolean;
  pages: boolean;
  skills: string[];
  findings: number;
  error: string | null;
}

/** Lo stesso visto da un progetto: in libreria, attivo nel progetto, chi lo usa. */
export interface ProjectLanguageEntry extends LanguageLibraryEntry {
  inLibrary: boolean;
  libraryVersion: number | null;
  active: boolean;
  projectVersion: number | null;
  usedBy: string[];
  updateAvailable: boolean;
}

/** La libreria dei linguaggi del grafo: caricati una volta in MdExplorer, attivati nei progetti che li usano. */
@Injectable({ providedIn: 'root' })
export class LanguageLibraryService {
  constructor(private http: HttpClient) { }

  list(): Observable<{ folder: string; packages: LanguageLibraryEntry[] }> {
    return this.http.get<{ folder: string; packages: LanguageLibraryEntry[] }>('../api/language-library');
  }

  import(sourcePath: string, replace: boolean): Observable<{ id: string; displayName: string; version: number; replaced: boolean }> {
    return this.http.post<{ id: string; displayName: string; version: number; replaced: boolean }>('../api/language-library/import', { sourcePath, replace });
  }

  remove(id: string): Observable<{ removed: boolean }> {
    return this.http.post<{ removed: boolean }>('../api/language-library/remove', { id });
  }

  forProject(projectPath: string): Observable<{ folder: string; packages: ProjectLanguageEntry[] }> {
    return this.http.get<{ folder: string; packages: ProjectLanguageEntry[] }>('../api/language-library/project', { params: new HttpParams().set('projectPath', projectPath) });
  }

  activate(projectPath: string, id: string, replace: boolean): Observable<{ version: number; installed: string[] }> {
    return this.http.post<{ version: number; installed: string[] }>('../api/language-library/activate', { projectPath, id, replace });
  }

  deactivate(projectPath: string, id: string): Observable<{ deactivated: boolean }> {
    return this.http.post<{ deactivated: boolean }>('../api/language-library/deactivate', { projectPath, id });
  }

  adopt(projectPath: string, id: string, replace: boolean): Observable<{ version: number }> {
    return this.http.post<{ version: number }>('../api/language-library/adopt', { projectPath, id, replace });
  }
}
