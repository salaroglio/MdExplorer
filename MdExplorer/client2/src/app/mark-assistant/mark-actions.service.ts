import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * HTTP client for Mark's context actions on a folder (MarkActionsController).
 * The actual progress comes back over SignalR (markFolderProgress) — these
 * calls only start / cancel the background job.
 */
@Injectable({ providedIn: 'root' })
export class MarkActionsService {
  private readonly baseUrl = '/api/markactions';

  constructor(private http: HttpClient) { }

  /** Starts the recursive "Riassumi documentazione" job for a folder subtree. */
  summarizeFolder(folderFullPath: string, connectionId: string): Observable<{ started: boolean }> {
    return this.http.post<{ started: boolean }>(`${this.baseUrl}/summarize-folder`, {
      folderFullPath,
      connectionId,
    });
  }

  /** Requests cancellation of the running job for this connection. */
  cancel(connectionId: string): Observable<{ cancelled: boolean }> {
    return this.http.post<{ cancelled: boolean }>(`${this.baseUrl}/cancel`, {
      connectionId,
    });
  }
}
