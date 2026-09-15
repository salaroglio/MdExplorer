import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Un fatto in memoria, con le coordinate (fatto-URI + grafo) per la curatela (§11 Fase 5d). */
export interface MemoryFact {
  factUri: string;
  graph: string;
  agent: string;
  statement: string;
  confidence: number;
  tags: string[];
  createdAt?: string;
  shared: boolean;
}

/**
 * HTTP client per la vista umana della memoria degli agenti (endpoint /api/mem, loopback).
 * L'umano ispeziona e cura i fatti; l'agente usa invece /api/A2A/memory (RunToken).
 */
@Injectable({ providedIn: 'root' })
export class AgentMemoryService {
  constructor(private http: HttpClient) {}

  listFacts(projectPath: string, agent?: string): Observable<{ facts: MemoryFact[] }> {
    let params = new HttpParams().set('projectPath', projectPath);
    if (agent) { params = params.set('agent', agent); }
    return this.http.get<{ facts: MemoryFact[] }>('/api/mem/facts', { params });
  }

  setConfidence(projectPath: string, graph: string, factUri: string, confidence: number): Observable<any> {
    return this.http.post('/api/mem/facts/confidence', { projectPath, graph, factUri, confidence });
  }

  deleteFact(projectPath: string, graph: string, factUri: string): Observable<any> {
    const params = new HttpParams()
      .set('projectPath', projectPath).set('graph', graph).set('factUri', factUri);
    return this.http.delete('/api/mem/facts', { params });
  }

  getDiary(projectPath: string, agent?: string): Observable<string> {
    let params = new HttpParams().set('projectPath', projectPath);
    if (agent) { params = params.set('agent', agent); }
    return this.http.get('/api/mem/diary', { params, responseType: 'text' });
  }
}
