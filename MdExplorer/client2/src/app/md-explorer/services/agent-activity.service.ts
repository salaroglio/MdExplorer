import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

/** Un turno di un agente in corso adesso (AgentActivityController lato Service). */
export interface RunningAgent {
  id: string;
  agentName: string;
  /** Il motore del turno, come lo si mostra: Copilot, Claude Code, opencode. */
  engine: string;
  startedAt: string;
}

interface AgentActivityView {
  projectPath: string;
  running: RunningAgent[];
}

/**
 * Chi sta lavorando adesso nel progetto aperto: gli agenti lanciati a mano, quelli di uno schedule e
 * quelli svegliati da un collega. Il Service manda l'elenco intero a ogni inizio e a ogni fine
 * (`agentActivityChanged`), quindi qui non si contano né si tolgono voci: si sostituisce l'elenco.
 */
@Injectable({ providedIn: 'root' })
export class AgentActivityService {
  private runningSubject = new BehaviorSubject<RunningAgent[]>([]);
  private currentPath: string = '';

  running$: Observable<RunningAgent[]> = this.runningSubject.asObservable();

  constructor(private http: HttpClient, serverMessages: MdServerMessagesService) {
    serverMessages.agentActivity$.subscribe(view => {
      if (this.samePath(view?.projectPath, this.currentPath)) this.runningSubject.next(view.running || []);
    });
  }

  /** Segue un progetto: rilegge chi lavora lì. Nessun progetto aperto → nessuno al lavoro, senza chiamata. */
  follow(projectPath: string): void {
    this.currentPath = projectPath || '';
    this.runningSubject.next([]);
    if (!this.currentPath) return;
    const asked = this.currentPath;
    const params = new HttpParams().set('projectPath', asked);
    this.http.get<AgentActivityView>('/api/A2A/activity/running', { params }).subscribe({
      next: view => {
        if (asked === this.currentPath) this.runningSubject.next(view?.running || []);
      },
      // Stato ignoto: l'indicatore resta spento e lo si dichiara, invece di mostrare un elenco inventato.
      error: err => console.warn('[AgentActivity] chi lavora non è leggibile, indicatore spento:', err),
    });
  }

  /** Windows scrive lo stesso percorso con barre e maiuscole diverse a seconda di chi lo dice. */
  private samePath(a: string, b: string): boolean {
    const normalize = (p: string) => (p || '').replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    return !!a && !!b && normalize(a) === normalize(b);
  }
}
