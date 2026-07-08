import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AgentSchedule {
  id: string;
  projectPath: string;
  agentFilePath: string;
  agentName: string;
  name: string;
  preparedPrompt: string;
  triggerType: 'cron' | 'commit' | 'projectOpen';
  cronExpression?: string;
  enabled: boolean;
  trusted: boolean;
  disabledReason?: string;
  createdAt: string;
  updatedAt: string;
  lastRunAt?: string;
  lastRunStatus?: string;
  lastRunError?: string;
}

export interface AgentScheduleRequest {
  projectPath: string;
  agentFilePath: string;
  name: string;
  preparedPrompt: string;
  triggerType: string;
  cronExpression?: string;
  enabled: boolean;
  trusted: boolean;
}

export interface AgentExecution {
  id: string;
  scheduleId?: string;
  projectPath: string;
  agentFilePath: string;
  agentName: string;
  triggerSource: string;
  executedBy: string;
  startedAt: string;
  finishedAt?: string;
  status: string;
  outputSummary?: string;
  error?: string;
}

/** HTTP client for per-user *.agent.md schedules (UserDB) and their execution history. */
@Injectable({ providedIn: 'root' })
export class AgentScheduleService {
  constructor(private http: HttpClient) {}

  list(projectPath?: string, agentFilePath?: string): Observable<{ schedules: AgentSchedule[] }> {
    let params = new HttpParams();
    if (projectPath) params = params.set('projectPath', projectPath);
    if (agentFilePath) params = params.set('agentFilePath', agentFilePath);
    return this.http.get<{ schedules: AgentSchedule[] }>('/api/AgentSchedules', { params });
  }

  create(request: AgentScheduleRequest): Observable<{ schedule: AgentSchedule }> {
    return this.http.post<{ schedule: AgentSchedule }>('/api/AgentSchedules', request);
  }

  update(id: string, request: AgentScheduleRequest): Observable<{ schedule: AgentSchedule }> {
    return this.http.put<{ schedule: AgentSchedule }>(`/api/AgentSchedules/${id}`, request);
  }

  delete(id: string): Observable<{ deleted: boolean }> {
    return this.http.delete<{ deleted: boolean }>(`/api/AgentSchedules/${id}`);
  }

  executions(filter: {
    scheduleId?: string;
    projectPath?: string;
    agentFilePath?: string;
    take?: number;
  }): Observable<{ executions: AgentExecution[] }> {
    let params = new HttpParams();
    if (filter.scheduleId) params = params.set('scheduleId', filter.scheduleId);
    if (filter.projectPath) params = params.set('projectPath', filter.projectPath);
    if (filter.agentFilePath) params = params.set('agentFilePath', filter.agentFilePath);
    if (filter.take) params = params.set('take', filter.take);
    return this.http.get<{ executions: AgentExecution[] }>('/api/AgentSchedules/executions', { params });
  }

  getDraft(projectPath: string, agentFilePath: string): Observable<{ draft: { prompt: string; parameterValuesJson?: string; updatedAt: string } | null }> {
    const params = new HttpParams().set('projectPath', projectPath).set('agentFilePath', agentFilePath);
    return this.http.get<{ draft: any }>('/api/AgentPrompts/draft', { params });
  }

  saveDraft(projectPath: string, agentFilePath: string, prompt: string, parameterValues: { [name: string]: string }): Observable<{ saved: boolean }> {
    return this.http.put<{ saved: boolean }>('/api/AgentPrompts/draft', {
      projectPath,
      agentFilePath,
      prompt,
      parameterValuesJson: JSON.stringify(parameterValues || {}),
    });
  }
}
