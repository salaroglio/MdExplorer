import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';

/** A chat in the history list (sprint 2026-11-24-Chat-Memorizzate, F6): the row of the user DB. */
export interface ChatThreadSummary {
  id: string;
  /** Given by the model at the end of each turn; null until the first one. */
  title: string | null;
  /** The log of the chat points, one «- …» per line from the oldest: the model sends the new point, MDE appends it. */
  detail: string | null;
  engine: string | null;
  modelId: string | null;
  startedAt: string;
  lastActivityAt: string;
  /** False when the chat file is gone: the row says so instead of showing an empty chat. */
  fileExists: boolean;
}

/** A line of the chat file, as the backend wrote it. */
export interface ChatRecordEvent {
  seq: number;
  turn: number;
  at: string;
  channel: string;
  kind: string;
  engine?: string;
  tool?: string;
  callId?: string;
  isError?: boolean;
  content?: string;
  json?: any;
}

export interface ChatThreadContent {
  id: string;
  title: string | null;
  detail: string | null;
  events: ChatRecordEvent[];
  /** Lines of the file that could not be read, with the line number. */
  problems: string[];
}

/**
 * The MarkAgent chat history: the list of a project's chats, a chat's content, deleting one; and whether the history
 * panel is open in the big pane (the button sits in the MarkAgent tab, the panel where the document is).
 */
@Injectable({ providedIn: 'root' })
export class ChatHistoryService {
  private readonly baseUrl = '/api/chats';

  /** true = the history panel is shown in the big pane instead of the document. */
  readonly open$ = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) {}

  toggle(): void {
    this.open$.next(!this.open$.value);
  }

  close(): void {
    if (this.open$.value) this.open$.next(false);
  }

  list(projectPath: string): Observable<ChatThreadSummary[]> {
    return this.http.get<ChatThreadSummary[]>(this.baseUrl, { params: { projectPath } });
  }

  content(id: string): Observable<ChatThreadContent> {
    return this.http.get<ChatThreadContent>(`${this.baseUrl}/${id}/events`);
  }

  delete(id: string): Observable<{ deleted: boolean; fileDeleted: boolean }> {
    return this.http.delete<{ deleted: boolean; fileDeleted: boolean }>(`${this.baseUrl}/${id}`);
  }
}
