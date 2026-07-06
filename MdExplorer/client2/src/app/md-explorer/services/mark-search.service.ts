import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MarkSearchAnswerDocument {
  relativePath: string;
  fullPath: string;
  fileName: string;
}


/**
 * Persists the AI-generated Mark Search answer as a temporary markdown file
 * under {project}/.md/mark-search/ so the standard viewer pipeline can render it.
 * ConnectionId is appended by the global ConnectionIdInterceptor.
 */
@Injectable({ providedIn: 'root' })
export class MarkSearchService {

  constructor(private http: HttpClient) {}

  saveAnswer(content: string): Observable<MarkSearchAnswerDocument> {
    return this.http.post<MarkSearchAnswerDocument>('../api/marksearch/answer', { content });
  }
}
