import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** The prompt the service built for the tab's engine (sprint 2026-11-24-Skill-Che-Migliorano, F3). */
export interface LearnRequest {
  threadId: string;
  prompt: string;
  from: number;
  to: number;
  harness: string;
  skillsFolder: string;
  /** The tab channel the request travels on: it stays in the chat history, apart from the rest. */
  channel: string;
}

export interface DiffLine {
  kind: 'same' | 'del' | 'add';
  text: string;
}

export interface ReviewedProposal {
  index: number;
  action: 'create' | 'update' | string;
  skill: string | null;
  why: string | null;
  evidence: number[];
  content: string | null;
  /** Why it cannot be approved; empty = valid. */
  problems: string[];
  isValid: boolean;
  targetPath: string | null;
  currentContent: string | null;
  diff: DiffLine[];
}

export interface LearnReview {
  prose: string;
  /** The block is missing, not closed or unreadable: no proposals, and why. */
  blockProblem: string | null;
  proposals: ReviewedProposal[];
}

/** «Impara da questa attività»: the backend builds the prompt and reads the answer; the tab's engine answers in between. */
@Injectable({ providedIn: 'root' })
export class SkillLearningService {
  private readonly baseUrl = '/api/skills/learn';

  constructor(private http: HttpClient) {}

  prepare(projectPath: string, threadId: string): Observable<LearnRequest> {
    return this.http.post<LearnRequest>(`${this.baseUrl}/prepare`, { projectPath, threadId });
  }

  review(projectPath: string, from: number, to: number, answer: string): Observable<LearnReview> {
    return this.http.post<LearnReview>(`${this.baseUrl}/review`, { projectPath, from, to, answer });
  }
}
