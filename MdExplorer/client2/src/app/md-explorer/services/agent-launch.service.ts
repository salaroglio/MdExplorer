import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Parameter detected in a normalized agent prompt (backend ParameterExtractor). */
export interface AgentParam {
  name: string;
  description?: string;
  defaultValue?: string;
  /** 'file' | 'dir' | 'out-file' | null (null → plain text input) */
  picker?: string | null;
}

export interface NormalizeAgentPromptResponse {
  success: boolean;
  normalizedPrompt?: string;
  error?: string;
  parameters?: AgentParam[];
}

export interface LaunchAgentResponse {
  success: boolean;
  runId?: string;
  error?: string;
}

/**
 * HTTP client for the AgentPrompts backend (normalize / extract-params / launch)
 * backing the *.agent.md launch dialog.
 */
/** Il motore dichiarato dalla scheda (runtime:) e quello del progetto (tab MarkAgent). */
export interface AgentEngineInfo {
  card: { provider?: string; engine?: string; model?: string; valid: boolean } | null;
  project: { engine?: string; model?: string };
}

@Injectable({ providedIn: 'root' })
export class AgentLaunchService {
  constructor(private http: HttpClient) {}

  /**
   * «Normalize», step 1: the prompt MarkAgent answers in the MarkAgent tab's session (sprint
   * 2026-09-29-Motore-LLM-Unico, F3: the project's engine, not a Copilot chosen here).
   */
  normalizePrompt(projectPath: string, prompt: string): Observable<{ success: boolean; prompt?: string; error?: string }> {
    return this.http.post<{ success: boolean; prompt?: string; error?: string }>('/api/AgentPrompts/normalize-prompt', {
      projectPath,
      prompt,
    });
  }

  /** «Normalize», step 2: the answer cleaned, with its parameters. */
  normalizeClean(raw: string): Observable<NormalizeAgentPromptResponse> {
    return this.http.post<NormalizeAgentPromptResponse>('/api/AgentPrompts/normalize-clean', { raw });
  }

  extractParams(prompt: string): Observable<{ parameters: AgentParam[] }> {
    return this.http.post<{ parameters: AgentParam[] }>('/api/AgentPrompts/extract-params', {
      prompt,
    });
  }

  /** Substitutes parameter values server-side and returns the ready-to-run prompt. */
  prepare(
    prompt: string,
    parameterValues: { [name: string]: string },
  ): Observable<{ success: boolean; preparedPrompt?: string; error?: string }> {
    return this.http.post<{ success: boolean; preparedPrompt?: string; error?: string }>(
      '/api/AgentPrompts/prepare',
      { prompt, parameterValues },
    );
  }

  launch(
    projectPath: string,
    agentFilePath: string,
    prompt: string,
    parameterValues: { [name: string]: string },
    /** `true` = posto di lavoro isolato, `false` = nel progetto. Assente = come da impostazione. */
    useWorktree?: boolean,
    /** Motore scelto nella finestra (claude | copilot | opencode): vince sul runtime: della scheda (D12). */
    engine?: string,
    model?: string,
  ): Observable<LaunchAgentResponse> {
    return this.http.post<LaunchAgentResponse>('/api/AgentPrompts/launch', {
      projectPath,
      agentFilePath,
      prompt,
      parameterValues,
      useWorktree,
      engine,
      model,
    });
  }

  /** Da dove parte il selettore del motore: il runtime: della scheda, altrimenti il motore del progetto. */
  engineInfo(projectPath: string, agentFilePath: string): Observable<AgentEngineInfo> {
    return this.http.get<AgentEngineInfo>('/api/AgentPrompts/engine', { params: { projectPath, agentFilePath } });
  }
}
