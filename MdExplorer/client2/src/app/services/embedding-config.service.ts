import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EmbeddingConfig {
  selectedModel: string;
  contextSize: number;
  batchSize: number;
  maxChunkChars: number;
  maxEmbeddingChars: number;
}

export interface EmbeddingModelInfo {
  id: string;
  name: string;
  description: string;
  fileName: string;
  fileSize: number;
  isInstalled: boolean;
  localPath?: string;
  contextLength: number;
  parameters: string;
}

export interface EmbeddingConfigResponse {
  config: EmbeddingConfig;
  presets: { [modelId: string]: EmbeddingConfig };
  embeddingModels: EmbeddingModelInfo[];
  modelLoaded: boolean;
  embeddingDimension: number;
  currentModelPath: string;
}

export interface EmbeddingSaveResponse {
  success: boolean;
  modelReloaded: boolean;
  reloadSuccess: boolean;
  reindexRequired: boolean;
  message: string;
}

export interface EmbeddingStatus {
  modelLoaded: boolean;
  embeddingDimension: number;
  currentModelPath: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmbeddingConfigService {
  private baseUrl = '/api/EmbeddingConfig';

  constructor(private http: HttpClient) {}

  getConfig(): Observable<EmbeddingConfigResponse> {
    return this.http.get<EmbeddingConfigResponse>(this.baseUrl);
  }

  saveConfig(config: EmbeddingConfig): Observable<EmbeddingSaveResponse> {
    return this.http.post<EmbeddingSaveResponse>(this.baseUrl, config);
  }

  getPresets(): Observable<{ [modelId: string]: EmbeddingConfig }> {
    return this.http.get<{ [modelId: string]: EmbeddingConfig }>(`${this.baseUrl}/presets`);
  }

  getStatus(): Observable<EmbeddingStatus> {
    return this.http.get<EmbeddingStatus>(`${this.baseUrl}/status`);
  }
}
