import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Un linguaggio rilevato nella cartella: quanti file e se l'estrattore è pronto (sprint «Il grafo del codice», D16). */
export interface DetectedLanguage {
  id: string;
  displayName: string;
  files: number;
  extractorKind: string;
  /** 'ready' oppure il motivo per cui oggi non si analizza. */
  availability: string;
  /** La grammatica tree-sitter da scaricare per renderlo pronto, se è questo il motivo. */
  grammarId?: string | null;
  grammarSizeKb?: number | null;
}

export interface CodeFolderLanguages {
  folder: string;
  isProgramFolder: boolean;
  selected: string[];
  detected: DetectedLanguage[];
  /** 'git' oppure 'hash' (cartella fuori da un repository). */
  scanMode: string;
}

export interface CodeFolderAnalysisOutcome {
  folder: string;
  mode: string;
  reason: string;
  languages: string[];
  filesInPerimeter: number;
  candidates: number;
  reparsed: number;
  scriptsRemoved: number;
  filesWithoutExtractor: { [language: string]: number };
  warnings: string[];
  load?: { loaded: number; skipped: number; removed: number; failed: number; firstError?: string };
  error?: string;
}

export interface CodeGraphStatus {
  scriptsFolderExists: boolean;
  scriptsOnDisk: number;
  scriptsLoaded: number;
  scriptsPending: number;
  files: number;
  symbols: number;
  externalSymbols: number;
  parameters: number;
  edges: number;
  describedSymbols: number;
}

/**
 * Il grafo del codice: parla SOLO col backend .NET (`/api/codegraph`), che a sua volta legge il DB dedicato.
 * `folderPath` può essere assoluto (il fullPath del nodo dell'albero) o relativo alla radice del progetto.
 */
@Injectable({ providedIn: 'root' })
export class CodeGraphService {
  constructor(private http: HttpClient) { }

  getLanguages(projectPath: string, folderPath: string): Observable<CodeFolderLanguages> {
    const params = new HttpParams().set('projectPath', projectPath).set('folderPath', folderPath);
    return this.http.get<CodeFolderLanguages>('../api/codegraph/languages', { params });
  }

  saveLanguages(projectPath: string, folderPath: string, languages: string[]): Observable<any> {
    return this.http.post('../api/codegraph/languages', { projectPath, folderPath, languages });
  }

  analyze(projectPath: string, folderPath?: string): Observable<{ folder?: CodeFolderAnalysisOutcome; folders?: CodeFolderAnalysisOutcome[]; status: CodeGraphStatus }> {
    return this.http.post<any>('../api/codegraph/analyze', { projectPath, folderPath });
  }

  generate(projectPath: string): Observable<any> {
    return this.http.post('../api/codegraph/generate', { projectPath });
  }

  downloadGrammar(grammarId: string): Observable<any> {
    return this.http.post('../api/codegraph/grammars/download', { grammarId });
  }

  status(projectPath: string): Observable<CodeGraphStatus> {
    const params = new HttpParams().set('projectPath', projectPath);
    return this.http.get<CodeGraphStatus>('../api/codegraph/status', { params });
  }
}
