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
/** La traccia di una descrizione del codice (punti 2c-2d del 14/10/2026). */
export interface DescriptionRun {
  programFolder: string;
  status: 'running' | 'done' | 'cancelled' | 'failed';
  engine: string;
  startedAt: string;
  updatedAt: string;
  fileIndex: number;
  fileTotal: number;
  currentFile?: string;
  filesDescribed: number;
  error?: string;
  /** Solo per un lavoro vivo: da quando e da quale file è partita questa sessione (per la stima del tempo). */
  sessionStartedAt?: string;
  sessionStartIndex?: number;
}

export interface BackgroundWork {
  descriptions: { state: 'running' | 'interrupted' | 'failed' | 'stopped'; run: DescriptionRun }[];
  scans: { folder: string; startedAt: string }[];
}

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

  describe(projectPath: string, folderPath: string, connectionId: string, force = false): Observable<any> {
    return this.http.post('../api/codegraph/describe', { projectPath, folderPath, connectionId, force });
  }

  /** Il pannello dei lavori in sfondo: descrizioni in corso o da riprendere, scansioni in corso (punto 2d). */
  background(projectPath: string): Observable<BackgroundWork> {
    return this.http.get<BackgroundWork>('../api/codegraph/background', { params: new HttpParams().set('projectPath', projectPath) });
  }

  /** «Ferma» una descrizione in corso, da qualunque finestra: la traccia resta, da riprendere. */
  stopDescribe(projectPath: string, folder: string): Observable<any> {
    return this.http.post('../api/codegraph/describe/stop', { projectPath, folder });
  }

  /** «Riprendi»: riparte dal file a cui era arrivata. */
  resumeDescribe(projectPath: string, folder: string, connectionId: string): Observable<any> {
    return this.http.post('../api/codegraph/describe/resume', { projectPath, folder, connectionId });
  }

  /** «Lascia stare»: la traccia si toglie. */
  dismissDescribe(projectPath: string, folder: string): Observable<any> {
    return this.http.post('../api/codegraph/describe/dismiss', { projectPath, folder });
  }

  cancelDescribe(connectionId: string): Observable<any> {
    return this.http.post('../api/codegraph/describe/cancel', { connectionId });
  }

  downloadGrammar(grammarId: string): Observable<any> {
    return this.http.post('../api/codegraph/grammars/download', { grammarId });
  }

  status(projectPath: string): Observable<CodeGraphStatus> {
    const params = new HttpParams().set('projectPath', projectPath);
    return this.http.get<CodeGraphStatus>('../api/codegraph/status', { params });
  }
}
