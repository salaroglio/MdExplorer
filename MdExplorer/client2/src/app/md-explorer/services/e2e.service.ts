import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** A setting with the place it comes from (a file relative to the project), or null for MdExplorer's default. */
export interface E2eSetting {
  value: boolean;
  source: string | null;
}

export interface E2eOwnSettings {
  dedicatedSession: boolean | null;
  commitAfterRun: boolean | null;
  headless: boolean | null;
}

export interface E2eSettingsState {
  target: 'file' | 'folder';
  path: string;
  settingsFile: string;
  settingsFileExists: boolean;
  own: E2eOwnSettings;
  effective: { dedicatedSession: E2eSetting; commitAfterRun: E2eSetting; headless: E2eSetting };
}

export interface E2eScriptInfo {
  test: number;
  file: string;
  state: string | null;
  stale: boolean;
}

export interface E2ePlanItem {
  file: string;
  scripts: E2eScriptInfo[];
  runFolder: string;
  tests: number;
  dedicatedSession: E2eSetting;
  commitAfterRun: E2eSetting;
  headless: E2eSetting;
  /** The tests project (E2eTests.csproj) and whether its packages are downloaded; null before the first run. */
  replayPackages: { project: string; restored: boolean } | null;
}

export interface E2ePlan {
  dotnet: E2eRequirement;
  canRun: boolean;
  errors: string[];
  warnings: string[];
  items: E2ePlanItem[];
}

export interface E2eRequirement {
  id: string;
  ok: boolean;
  detail: string;
  remedy: string | null;
  installable: boolean;
}

export interface E2eReplayResult {
  file: string;
  runFolder: string | null;
  outcomes: { test: number; script: string; passed: boolean; message: string | null }[];
  stale: string[];
  problem: string | null;
  needsRestore: boolean;
  commit: { committed: boolean; sha: string | null; message: string | null; reason: string | null } | null;
  /** registro.T<n>.md in runFolder: network calls and console of each replayed test. */
  logs: string[];
}

export interface E2ePrerequisites {
  electron: E2eRequirement;
  browser: E2eRequirement;
  playwrightMcp: E2eRequirement;
  browserArgument: string | null;
  readyToRun: boolean;
}

/**
 * End-to-end tests written in markdown (sprint docs-internal/Sprints/2026-09-26-Test-E2E-Da-Markdown.md):
 * settings of a test or a folder, the preview of a launch, and the prerequisites with their installation.
 * The launch itself goes through the AI chat hub (AiChatService.runE2eTests).
 */
@Injectable({ providedIn: 'root' })
export class E2eService {
  private readonly base = '../api/e2e';

  constructor(private http: HttpClient) {}

  getSettings(path: string, projectPath: string): Observable<E2eSettingsState> {
    return this.http.get<E2eSettingsState>(`${this.base}/settings`, { params: { path, projectPath } });
  }

  putSettings(path: string, projectPath: string, own: E2eOwnSettings): Observable<E2eSettingsState> {
    return this.http.put<E2eSettingsState>(`${this.base}/settings`, { path, projectPath, ...own });
  }

  getPlan(path: string, projectPath: string): Observable<E2ePlan> {
    return this.http.get<E2ePlan>(`${this.base}/plan`, { params: { path, projectPath } });
  }

  /** Adds to the project's .gitignore the lines that exclude the credentials files of this test/folder (D12). */
  addToGitIgnore(path: string, projectPath: string): Observable<{ added: string[] }> {
    return this.http.post<{ added: string[] }>(`${this.base}/gitignore`, { path, projectPath });
  }

  replay(path: string, projectPath: string): Observable<E2eReplayResult[]> {
    return this.http.post<E2eReplayResult[]>(`${this.base}/replay`, { path, projectPath });
  }

  /** Downloads the replay packages (dotnet restore): only when the user asks, never on its own (D3, D7). */
  restoreReplayPackages(path: string, projectPath: string): Observable<{ restored: string[] }> {
    return this.http.post<{ restored: string[] }>(`${this.base}/replay-restore`, { path, projectPath });
  }

  getPrerequisites(): Observable<E2ePrerequisites> {
    return this.http.get<E2ePrerequisites>(`${this.base}/prerequisites`);
  }

  installPlaywrightMcp(): Observable<{ detail: string; report: E2ePrerequisites }> {
    return this.http.post<{ detail: string; report: E2ePrerequisites }>(`${this.base}/prerequisites/playwright-mcp`, {});
  }

  installChromium(): Observable<{ detail: string; report: E2ePrerequisites }> {
    return this.http.post<{ detail: string; report: E2ePrerequisites }>(`${this.base}/prerequisites/chromium`, {});
  }
}
