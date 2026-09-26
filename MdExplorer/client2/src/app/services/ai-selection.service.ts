import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MarkdownFragmentResponse {
  fragment: string;
  startLine: number;
  endLine: number;
  totalLines: number;
  lineEnding: string;
}

export interface ReplaceMarkdownSectionRequest {
  path: string;
  startLine: number;
  endLine: number;
  expectedOriginalText: string;
  newText: string;
  connectionId?: string;
}

/**
 * HTTP wrapper for the "Usa AI" selection endpoints.
 * Line numbers are 1-based and come from the data-mde-line-* attributes
 * emitted by the server-side source map.
 */
@Injectable({ providedIn: 'root' })
export class AiSelectionService {

  constructor(private http: HttpClient) { }

  // ConnectionId in query: MdControllerBase resolves the per-client project
  // context (project path, filesystem watcher) from Request.Query["ConnectionId"].
  getFragment(path: string, startLine: number, endLine: number, connectionId: string): Observable<MarkdownFragmentResponse> {
    const params = new HttpParams()
      .set('path', path)
      .set('startLine', startLine)
      .set('endLine', endLine)
      .set('ConnectionId', connectionId);
    return this.http.get<MarkdownFragmentResponse>('../api/aiselection/fragment', { params });
  }

  replaceSection(request: ReplaceMarkdownSectionRequest): Observable<{ newEndLine: number }> {
    const params = new HttpParams().set('ConnectionId', request.connectionId || '');
    return this.http.post<{ newEndLine: number }>('../api/aiselection/replace', request, { params });
  }
}
