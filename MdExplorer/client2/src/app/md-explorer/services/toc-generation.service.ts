import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TocGenerationResult {
  success: boolean;
  tocPath?: string;
  message?: string;
  content?: string;
}

export interface TocStatusResult {
  exists: boolean;
  path?: string;
  lastModified?: Date;
  canRefresh: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TocGenerationService {
  private baseUrl = '/api/toc';

  constructor(private http: HttpClient) { }

  generateToc(directoryPath: string): Observable<TocGenerationResult> {
    return this.http.post<TocGenerationResult>(`${this.baseUrl}/generate`, {
      directoryPath: directoryPath
    });
  }

  refreshToc(tocFilePath: string): Observable<TocGenerationResult> {
    return this.http.post<TocGenerationResult>(`${this.baseUrl}/refresh`, {
      tocFilePath: tocFilePath
    });
  }

  generateQuickToc(directoryPath: string): Observable<TocGenerationResult> {
    return this.http.post<TocGenerationResult>(`${this.baseUrl}/quick`, {
      directoryPath: directoryPath
    });
  }

  getTocStatus(directoryPath: string): Observable<TocStatusResult> {
    return this.http.get<TocStatusResult>(`${this.baseUrl}/status/${encodeURIComponent(directoryPath)}`);
  }
}