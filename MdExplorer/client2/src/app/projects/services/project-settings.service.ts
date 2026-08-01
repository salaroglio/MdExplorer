import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Da dove viene un valore risolto del relay (catena: progetto → .development.yml → globale). */
export type RelaySettingSource = 'None' | 'Project' | 'DevelopmentYml' | 'Global';

export interface RelaySettings {
  relayUrl: string;
  relayUrlSource: RelaySettingSource;
  /** La chiave non viaggia mai verso il client: si sa solo se c'è. */
  hasApiKey: boolean;
  apiKeySource: RelaySettingSource;
  lastTestedAt: string | null;
  lastTestSuccess: boolean | null;
}

export interface RelayTestResult {
  success: boolean;
  statusCode: number | null;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectSettingsService {

  constructor(private http: HttpClient) { }

  getProjectSettings(): Observable<any[]> {
    const url = '../api/ProjectSettings/GetProjectSettings';
    return this.http.get<any[]>(url);
  }

  saveProjectSetting(setting: any): Observable<any> {
    const url = '../api/ProjectSettings/SaveProjectSetting';
    return this.http.post<any>(url, setting);
  }

  getRule1Setting(): Observable<any> {
    const url = '../api/ProjectSettings/GetRule1Setting';
    return this.http.get<any>(url);
  }

  setRule1Setting(enabled: boolean): Observable<any> {
    const url = '../api/ProjectSettings/SetRule1Setting';
    return this.http.post<any>(url, { enabled });
  }

  getStickyScrollSetting(): Observable<{enabled: boolean}> {
    return this.http.get<{enabled: boolean}>('../api/ProjectSettings/GetStickyScrollSetting');
  }

  setStickyScrollSetting(enabled: boolean): Observable<any> {
    return this.http.post('../api/ProjectSettings/SetStickyScrollSetting', { enabled });
  }

  getLinkIndexingSetting(projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/GetLinkIndexingSetting';
    return this.http.get<any>(url, { params: { projectPath } });
  }

  setLinkIndexingSetting(enabled: boolean, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetLinkIndexingSetting';
    return this.http.post<any>(url, { enabled, projectPath });
  }

  getPlantUmlKeepOriginalColorsSetting(projectPath: string): Observable<{enabled: boolean}> {
    const url = '../api/ProjectSettings/GetPlantUmlKeepOriginalColorsSetting';
    return this.http.get<{enabled: boolean}>(url, { params: { projectPath } });
  }

  setPlantUmlKeepOriginalColorsSetting(enabled: boolean, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetPlantUmlKeepOriginalColorsSetting';
    return this.http.post<any>(url, { enabled, projectPath });
  }

  getCopilotCliAutoSelectSetting(projectPath: string): Observable<{enabled: boolean}> {
    const url = '../api/ProjectSettings/GetCopilotCliAutoSelectSetting';
    return this.http.get<{enabled: boolean}>(url, { params: { projectPath } });
  }

  setCopilotCliAutoSelectSetting(enabled: boolean, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetCopilotCliAutoSelectSetting';
    return this.http.post<any>(url, { enabled, projectPath });
  }

  getExcludeSubmodulesSetting(projectPath: string): Observable<{enabled: boolean}> {
    const url = '../api/ProjectSettings/GetExcludeSubmodulesSetting';
    return this.http.get<{enabled: boolean}>(url, { params: { projectPath } });
  }

  setExcludeSubmodulesSetting(enabled: boolean, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetExcludeSubmodulesSetting';
    return this.http.post<any>(url, { enabled, projectPath });
  }

  getTextIndexingSetting(projectPath: string): Observable<{enabled: boolean, extensions: string, defaultExtensions: string}> {
    const url = '../api/ProjectSettings/GetTextIndexingSetting';
    return this.http.get<{enabled: boolean, extensions: string, defaultExtensions: string}>(url, { params: { projectPath } });
  }

  setTextIndexingSetting(enabled: boolean, extensions: string, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetTextIndexingSetting';
    return this.http.post<any>(url, { enabled, extensions, projectPath });
  }

  /** Forces a full rebuild of the separate text index only (POST api/mdfiles/ReindexTextFiles). */
  reindexTextFiles(connectionId: string): Observable<any> {
    const url = '../api/mdfiles/ReindexTextFiles?connectionId=' + encodeURIComponent(connectionId || '');
    return this.http.post<any>(url, {});
  }

  // RAG Settings
  getRagStatus(): Observable<any> {
    const url = '../api/Rag/status';
    return this.http.get<any>(url);
  }

  enableRag(): Observable<any> {
    const url = '../api/Rag/enable';
    return this.http.post<any>(url, {});
  }

  disableRag(): Observable<any> {
    const url = '../api/Rag/disable';
    return this.http.post<any>(url, {});
  }

  reindexRag(projectPath: string): Observable<any> {
    const url = '../api/Rag/reindex';
    return this.http.post<any>(url, { projectPath });
  }

  /** Full project reindex: ignores the incremental fingerprints (links, FTS, embeddings). */
  reindexProject(connectionId: string): Observable<any> {
    const url = `../api/mdfiles/ReindexProject?ConnectionId=${connectionId}`;
    return this.http.post<any>(url, {});
  }

  clearRagIndex(projectPath: string): Observable<any> {
    const url = '../api/Rag/clear';
    return this.http.post<any>(url, { projectPath });
  }

  indexRagFile(filePath: string, projectPath: string, forceReindex = false): Observable<any> {
    const url = '../api/Rag/index-file';
    return this.http.post<any>(url, { filePath, projectPath, forceReindex });
  }

  indexRagDirectory(directoryPath: string, projectPath: string, forceReindex = false): Observable<any> {
    const url = '../api/Rag/index-directory';
    return this.http.post<any>(url, { directoryPath, projectPath, forceReindex });
  }

  // ============================================================
  //   Agent City / Federation activation (.development.yml, §12.4)
  // ============================================================
  getAgentCity(projectPath: string): Observable<{ enabled: boolean; ownershipDoc: string | null; relayUrl: string | null; hasRoomSecret: boolean; useAgentWorktrees: boolean; autoMergeAgentDeliverables: boolean }> {
    return this.http.get<any>('../api/MdProjects/AgentCity', { params: { path: projectPath } });
  }

  setAgentCity(projectPath: string, body: { enabled: boolean; ownershipDoc?: string; relayUrl?: string; useAgentWorktrees?: boolean; autoMergeAgentDeliverables?: boolean }): Observable<any> {
    return this.http.post<any>('../api/MdProjects/SetAgentCity', body, { params: { path: projectPath } });
  }

  // ------------------------------------------------------------
  //   Relay della federazione: indirizzo + API key, PER PROGETTO.
  //   La chiave non attraversa mai il filo in uscita dal server:
  //   il GET dice solo se c'è e da dove arriva.
  // ------------------------------------------------------------
  getRelaySettings(projectPath: string): Observable<RelaySettings> {
    return this.http.get<RelaySettings>('../api/MdProjects/RelaySettings', { params: { path: projectPath } });
  }

  setRelaySettings(projectPath: string, body: { relayUrl?: string; apiKey?: string; clearApiKey?: boolean }): Observable<RelaySettings> {
    return this.http.post<RelaySettings>('../api/MdProjects/SetRelaySettings', body, { params: { path: projectPath } });
  }

  testRelaySettings(projectPath: string): Observable<RelayTestResult> {
    return this.http.post<RelayTestResult>('../api/MdProjects/TestRelaySettings', {}, { params: { path: projectPath } });
  }

  // ============================================================
  //   Knowledge Graph (Neo4j) settings + sync
  // ============================================================
  getKgSettings(projectId: string): Observable<any> {
    return this.http.get<any>(`../api/kg/settings/${encodeURIComponent(projectId)}`);
  }

  saveKgSettings(projectId: string, body: {
    enabled: boolean;
    uri: string;
    database: string;
    username: string;
    password: string;
    syncOnTocGeneration: boolean;
    syncOnKgFileSave: boolean;
  }): Observable<any> {
    return this.http.put<any>(`../api/kg/settings/${encodeURIComponent(projectId)}`, body);
  }

  testKgConnection(body: {
    projectId?: string;
    uri: string;
    database: string;
    username: string;
    password: string;
  }): Observable<{ success: boolean; error?: string; latencyMs: number }> {
    return this.http.post<any>('../api/kg/test-connection', body);
  }

  syncKgProject(projectId: string): Observable<any> {
    return this.http.post<any>('../api/kg/ingest/project', { projectId });
  }

  syncKgFolder(projectId: string, relativeFolderPath: string): Observable<any> {
    return this.http.post<any>('../api/kg/ingest/folder', { projectId, relativeFolderPath });
  }

  syncKgFile(projectId: string, relativeKgPath: string): Observable<any> {
    return this.http.post<any>('../api/kg/ingest/file', { projectId, relativeKgPath });
  }

  resetKg(projectId: string): Observable<any> {
    return this.http.post<any>('../api/kg/reset', { projectId, confirm: true });
  }

  getKgState(projectId: string): Observable<any> {
    return this.http.get<any>(`../api/kg/state/${encodeURIComponent(projectId)}`);
  }

  // ============================================================
  //   Apache Jena Fuseki settings (parallelo a KG/Neo4j)
  // ============================================================
  getFusekiSettings(projectId: string): Observable<any> {
    return this.http.get<any>(`../api/fs/settings/${encodeURIComponent(projectId)}`);
  }

  saveFusekiSettings(projectId: string, body: {
    enabled: boolean;
    uri: string;
    dataset: string;
    username: string;
    password: string;
    syncOnTocGeneration: boolean;
    syncOnKgFileSave: boolean;
  }): Observable<any> {
    return this.http.put<any>(`../api/fs/settings/${encodeURIComponent(projectId)}`, body);
  }

  testFusekiConnection(body: {
    projectId?: string;
    uri: string;
    dataset: string;
    username: string;
    password: string;
    autoCreateDataset?: boolean;
  }): Observable<{
    success: boolean;
    serverReachable: boolean;
    datasetExists: boolean;
    datasetCreated: boolean;
    dataset: string;
    error?: string;
    latencyMs: number;
  }> {
    return this.http.post<any>('../api/fs/test-connection', body);
  }

  ensureFusekiDataset(projectId: string): Observable<{ ok: boolean; dataset: string }> {
    return this.http.post<any>('../api/fs/ensure-dataset', { projectId });
  }

  // ============================================================
  //   Atlassian (Jira/Confluence) settings
  //   Shared config (base url, project keys, planning folder) -> .development.yml
  //   Personal token -> UserDB (encrypted). Both saved in one PUT.
  // ============================================================
  getAtlassianSettings(projectId: string): Observable<any> {
    return this.http.get<any>(`../api/atlassian/settings/${encodeURIComponent(projectId)}`);
  }

  saveAtlassianSettings(projectId: string, body: {
    enabled: boolean;
    jiraBaseUrl: string;
    jiraProjectKeys: string[];
    confluenceBaseUrl?: string;
    confluenceSpaceKeys?: string[];
    email: string;
    apiToken: string;
  }): Observable<any> {
    return this.http.put<any>(`../api/atlassian/settings/${encodeURIComponent(projectId)}`, body);
  }

  testAtlassianConnection(body: {
    projectId?: string;
    jiraBaseUrl: string;
    email: string;
    apiToken: string;
  }): Observable<{ success: boolean; error?: string; displayName?: string; latencyMs: number }> {
    return this.http.post<any>('../api/atlassian/test-connection', body);
  }
}