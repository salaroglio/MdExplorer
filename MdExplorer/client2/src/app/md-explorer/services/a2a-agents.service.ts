import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Una skill dichiarata nella Agent Card. */
export interface AgentSkill {
  id: string;
  description: string;
}

/** Una cosa che l'agente può (o esplicitamente non può) fare sul computer; la frase sta nelle traduzioni. */
export interface AgentEffect {
  id: string;
  granted: boolean;
  /** Esce dalla sola lettura: la UI la evidenzia. */
  danger: boolean;
}

/**
 * Una voce del catalogo del registry ("Pagine Gialle" del progetto, §6).
 * `registrationError` non-null ⇒ voce esclusa (fail-loud, visibile in UI).
 */
export interface AgentRegistryEntry {
  name: string;
  kind: string; // 'llm' | 'algorithmic'
  agentFilePath?: string;
  role?: string;
  /** Che cosa fa, scritto dall'autore (a2a.summary): una dichiarazione, non verificata. */
  summary?: string;
  skills: AgentSkill[];
  tools: string[];
  /** Cosa può fare sul computer: calcolato dall'app dagli strumenti dichiarati. */
  effects?: AgentEffect[];
  /** A chi passa il lavoro «Autorizza» (a2a.on_approval_notify). */
  onApprovalNotify?: string[];
  trusted: boolean;
  enabled: boolean;
  trustDecayed: boolean;
  registrationError?: string;
  identityId?: string;
  isCitizen: boolean;
  isExcluded: boolean;
}

/** HTTP client per il registry degli agenti (endpoint /api/A2A). Loopback-only. */
@Injectable({ providedIn: 'root' })
export class A2aAgentsService {
  constructor(private http: HttpClient) {}

  getAgents(projectPath: string): Observable<AgentRegistryEntry[]> {
    const params = new HttpParams().set('projectPath', projectPath);
    return this.http.get<AgentRegistryEntry[]>('/api/A2A/agents', { params });
  }

  trust(projectPath: string, agentName: string): Observable<AgentRegistryEntry> {
    return this.http.post<AgentRegistryEntry>('/api/A2A/agents/trust', { projectPath, agentName });
  }

  untrust(projectPath: string, agentName: string): Observable<AgentRegistryEntry> {
    return this.http.post<AgentRegistryEntry>('/api/A2A/agents/untrust', { projectPath, agentName });
  }
}
