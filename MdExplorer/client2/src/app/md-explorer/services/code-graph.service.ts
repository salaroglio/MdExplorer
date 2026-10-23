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
  /** Linguaggio definito nel progetto: il file della definizione (`.mde-languages/<id>.lang.yml`). */
  definitionPath?: string | null;
}

/** Una definizione di linguaggio del progetto che non si può usare: la finestra la mostra col motivo. */
export interface LanguageDefinitionProblem {
  id: string;
  definitionPath: string;
  message: string;
}

export interface CodeFolderLanguages {
  folder: string;
  isProgramFolder: boolean;
  selected: string[];
  detected: DetectedLanguage[];
  /** 'git' oppure 'hash' (cartella fuori da un repository). */
  scanMode: string;
  /** 'all' = la descrizione parte da sola dopo le scansioni; 'onDemand' = solo a richiesta (punto 2b del 14/10). */
  describe: 'all' | 'onDemand';
  /** Quante descrizioni insieme, dalle preferenze dell'utente. */
  descriptionParallel: number;
  /** Le definizioni in `.mde-languages/` che non si possono usare, col motivo. */
  definitionProblems?: LanguageDefinitionProblem[];
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

/** La stima del peso di una cartella nel database: (MB del dataset del codice − preallocati) × triple della cartella / triple totali. */
export interface FolderWeight {
  folderTriples: number;
  totalTriples: number;
  datasetMb: number;
  /** Lo spazio che TDB2 riserva comunque (8 MB per file): non si ripartisce fra le cartelle. */
  preallocatedMb: number;
  share: number;
  estimatedMb: number;
}

/** Il modello di «Descrivi con Mark» (22/10/2026): l'harness è quello del progetto, il modello si sceglie a parte. */
export interface DescriptionModelInfo {
  /** 'claude' | 'copilot' | 'opencode' | 'none'. */
  engine: string;
  engineName: string;
  /** Il modello scelto per descrivere, null = il predefinito per descrivere (o quello del tab se il motore non ne ha). */
  model: string | null;
  /** Il predefinito per descrivere del motore: haiku (Claude Code), gpt-6-luna (Copilot), null (opencode). */
  defaultModel: string | null;
  tabModel: string | null;
}

/** Gli script AST scritti da un estrattore precedente (D25): il grafo di quei file è quello vecchio. */
export interface OutdatedScripts {
  count: number;
  /** Per linguaggio: da quale estrattore a quale, quanti file. */
  changes: { language: string; from: string; to: string; files: number }[];
  scripts: { folder: string; source: string; language: string }[];
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
  /** Le domande finite: il numero da mostrare (fileIndex è la prima non finita, per riprendere). Assente nelle tracce di prima del 22/10/2026. */
  questionsDone?: number;
  error?: string;
  /** Solo per un lavoro vivo: da quando e da quale file è partita questa sessione (per la stima del tempo). */
  sessionStartedAt?: string;
  sessionStartIndex?: number;
}

export interface BackgroundWork {
  descriptions: { state: 'running' | 'interrupted' | 'failed' | 'stopped'; run: DescriptionRun }[];
  scans: { folder: string; startedAt: string }[];
}

/** Quanto occupa il database del grafo (Fuseki, unico per tutti i progetti): per dataset, con le generazioni TDB2. */
export interface GraphStorage {
  dataDirectory: string;
  totalMb: number;
  datasets: { name: string; generations: string[]; databaseMb: number; textIndexMb: number }[];
}

/** L'anteprima di un giro di descrizioni (F12b), come la calcola il backend (CodeDocPreview). */
export interface DescribePreview {
  folder: string;
  questions: number;
  levels: number;
  missing: number;
  signatureChanged: number;
  bodyChanged: number;
  /** Descrizioni incomplete (manca il significato di un parametro o del valore restituito): si rifanno. */
  incomplete: number;
  reviewContext: number;
  reviewPrompt: number;
  reviewNoFingerprint: number;
  fresh: number;
  trivial: number;
  bodyUnknown: number;
  /** I file con qualcosa da chiedere all'LLM in questo giro. */
  files: number;
  /** I file della cartella analizzati (con uno script AST). */
  analyzedFiles: number;
  /** Il peso su disco degli script del grafo della cartella (.mde-code). */
  scriptsBytes: number;
}

@Injectable({ providedIn: 'root' })
export class CodeGraphService {
  constructor(private http: HttpClient) { }

  getLanguages(projectPath: string, folderPath: string): Observable<CodeFolderLanguages> {
    const params = new HttpParams().set('projectPath', projectPath).set('folderPath', folderPath);
    return this.http.get<CodeFolderLanguages>('../api/codegraph/languages', { params });
  }

  saveLanguages(projectPath: string, folderPath: string, languages: string[], describe?: 'all' | 'onDemand' | null): Observable<any> {
    return this.http.post('../api/codegraph/languages', { projectPath, folderPath, languages, describe: describe ?? null });
  }

  analyze(projectPath: string, folderPath?: string): Observable<{ folder?: CodeFolderAnalysisOutcome; folders?: CodeFolderAnalysisOutcome[]; status: CodeGraphStatus }> {
    return this.http.post<any>('../api/codegraph/analyze', { projectPath, folderPath });
  }

  generate(projectPath: string): Observable<any> {
    return this.http.post('../api/codegraph/generate', { projectPath });
  }

  describe(projectPath: string, folderPath: string, connectionId: string, force = false, review = false): Observable<any> {
    return this.http.post('../api/codegraph/describe', { projectPath, folderPath, connectionId, force, review });
  }

  /** Quanto occupa il database del grafo su disco (non avvia Fuseki). */
  storage(): Observable<GraphStorage> {
    return this.http.get<GraphStorage>('../api/codegraph/storage');
  }

  /** Quanto della cartella sta nel database del grafo, stimato dalle triple (22/10/2026). */
  folderWeight(projectPath: string, folderPath: string): Observable<FolderWeight> {
    return this.http.get<FolderWeight>('../api/codegraph/folder-weight', { params: { projectPath, folderPath } });
  }

  /** Il motore del progetto e il modello di «Descrivi con Mark» (null = come il tab MarkAgent), con quello del tab. */
  descriptionModel(projectPath: string): Observable<DescriptionModelInfo> {
    return this.http.get<DescriptionModelInfo>('../api/codegraph/description-model', { params: { projectPath } });
  }

  /** Salva il modello di «Descrivi con Mark» per il motore in vigore; null = come il tab MarkAgent. */
  saveDescriptionModel(projectPath: string, model: string | null): Observable<{ model: string | null }> {
    return this.http.post<{ model: string | null }>('../api/codegraph/description-model', { projectPath, model });
  }

  /** Gli script della cartella scritti da un estrattore precedente (D25); `folder` relativa alla radice del progetto o «@alias». */
  outdated(projectPath: string, folder: string): Observable<OutdatedScripts> {
    return this.http.get<OutdatedScripts>('../api/codegraph/outdated', { params: { projectPath, folder } });
  }

  /** «Aggiorna la struttura»: riestrae solo i file dell'estrattore precedente. */
  upgradeOutdated(projectPath: string, folder: string): Observable<{ reparsed: number }> {
    return this.http.post<{ reparsed: number }>('../api/codegraph/outdated/upgrade', { projectPath, folder });
  }

  /** L'anteprima di «Descrivi con Mark» (F12b): da descrivere, da rivedere e perché. Non scrive niente. */
  describePreview(projectPath: string, folderPath: string): Observable<DescribePreview> {
    return this.http.get<DescribePreview>('../api/codegraph/describe/preview', { params: { projectPath, folderPath } });
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
