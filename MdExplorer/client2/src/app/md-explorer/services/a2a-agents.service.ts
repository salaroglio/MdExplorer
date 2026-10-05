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

/** Di chi è un agente, visto da questo computer (AgentOwnersController). */
export interface AgentOwner {
  agentName: string;
  kind: 'mine' | 'someoneElse' | 'unassigned' | 'contested';
  ownerName?: string;
  ownerEmail?: string;
  contestedBy?: string[];
  /** Un agente lavora solo sul computer di chi ne risponde. */
  canWorkHere: boolean;
  /** Perché qui non lavora e cosa fare; assente se lavora. */
  explanation?: string;
}

/** Chi risponde di ogni agente del progetto. */
export interface AgentOwnersView {
  /** false a città spenta: lì la regola non vale e non si chiede niente. */
  applies: boolean;
  /** Chi sei per il progetto: la tua email git. */
  me?: string;
  /** Il documento delle responsabilità, se il progetto ne dichiara uno. */
  document?: string;
  /** Perché il documento dichiarato non è in uso. */
  documentProblem?: string;
  agents: AgentOwner[];
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

  getOwners(projectPath: string): Observable<AgentOwnersView> {
    const params = new HttpParams().set('projectPath', projectPath);
    return this.http.get<AgentOwnersView>('/api/A2A/owners', { params });
  }

  /** «È mio» / «Sono tutti miei»: le righe entrano nel documento delle responsabilità in una scrittura sola. */
  assignToMe(projectPath: string, agentNames: string[]): Observable<{ agents: AgentOwner[] }> {
    return this.http.post<{ agents: AgentOwner[] }>('/api/A2A/owners/assign-to-me', { projectPath, agentNames });
  }

  untrust(projectPath: string, agentName: string): Observable<AgentRegistryEntry> {
    return this.http.post<AgentRegistryEntry>('/api/A2A/agents/untrust', { projectPath, agentName });
  }
}
