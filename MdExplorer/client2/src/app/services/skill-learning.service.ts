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
  /** The ChatLearning row to approve or reject; null for a proposal that is not valid. */
  recordId: string | null;
}

/** What happened to a decision: the skill written (where, which version), or rejected. */
export interface LearnDecision {
  recordId: string;
  outcome: 'approved' | 'corrected' | 'rejected';
  targetPath: string | null;
  version: number | null;
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

  review(projectPath: string, threadId: string, from: number, to: number, answer: string): Observable<LearnReview> {
    return this.http.post<LearnReview>(`${this.baseUrl}/review`, { projectPath, threadId, from, to, answer });
  }

  /** «Approva» (U5): the skill is written now, and MDE commits it. content = the text corrected by the user, if any. */
  approve(projectPath: string, recordId: string, content: string | null): Observable<LearnDecision> {
    return this.http.post<LearnDecision>(`${this.baseUrl}/approve`, { projectPath, recordId, content });
  }

  /** «Rifiuta» (U5): remembered now, for the project. */
  reject(projectPath: string, recordId: string): Observable<LearnDecision> {
    return this.http.post<LearnDecision>(`${this.baseUrl}/reject`, { projectPath, recordId });
  }
}
