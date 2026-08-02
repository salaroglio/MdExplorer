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
@Injectable({ providedIn: 'root' })
export class AgentLaunchService {
  constructor(private http: HttpClient) {}

  normalize(projectPath: string, prompt: string): Observable<NormalizeAgentPromptResponse> {
    return this.http.post<NormalizeAgentPromptResponse>('/api/AgentPrompts/normalize', {
      projectPath,
      prompt,
    });
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
  ): Observable<LaunchAgentResponse> {
    return this.http.post<LaunchAgentResponse>('/api/AgentPrompts/launch', {
      projectPath,
      agentFilePath,
      prompt,
      parameterValues,
      useWorktree,
    });
  }
}
