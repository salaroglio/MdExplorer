import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * Lo stato di `.development.yml` del progetto (sprint «Schema di .development.yml», F4): `notice` è l'avviso già nella lingua
 * dell'interfaccia (vuoto se va tutto bene), `off` i percorsi dei gruppi spenti (`harness`, `codeRoots[2]`…).
 */
export interface DevelopmentConfigState {
  projectPath: string;
  notice: string;
  off: string[];
  writtenByNewerMde: boolean;
  fileSchema: number;
  schema: number;
}

@Injectable({ providedIn: 'root' })
export class DevelopmentConfigStateService {
  constructor(private http: HttpClient) { }

  get(projectPath: string): Observable<DevelopmentConfigState> {
    const params = new HttpParams().set('path', projectPath);
    return this.http.get<DevelopmentConfigState>('../api/MdProjects/DevelopmentConfigState', { params });
  }

  /** Il gruppo (o un gruppo che lo contiene) è spento. */
  static isOff(state: DevelopmentConfigState | null, group: string): boolean {
    return !!state?.off?.some(o => group === o || group.startsWith(o + '.') || group.startsWith(o + '['));
  }
}
