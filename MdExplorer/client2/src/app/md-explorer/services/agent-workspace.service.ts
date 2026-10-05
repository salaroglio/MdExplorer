import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { MdFile } from '../models/md-file';
import { MdFileService } from './md-file.service';
import { ProjectsService } from './projects.service';
import { ReviewContextService } from './review-context.service';

/**
 * Lavorare nella copia di un agente «come dopo un cambio di ramo».
 *
 * Entrando, il servizio ripunta QUESTA finestra sulla scrivania dell'agente: l'albero, l'indice, la ricerca e
 * i documenti sono i suoi, e si possono modificare. Il progetto resta lo stesso per tutto ciò che è della
 * città (posta, registro, differenze). Uscendo si torna alla propria cartella; il servizio rifiuta l'uscita
 * finché nella copia resta qualcosa di non committato.
 */
@Injectable({ providedIn: 'root' })
export class AgentWorkspaceService {
  /** L'agente nella cui copia lavora questa finestra (null = il proprio progetto). */
  readonly inside$ = new BehaviorSubject<string | null>(null);

  constructor(
    private http: HttpClient,
    private projects: ProjectsService,
    private messages: MdServerMessagesService,
    private mdFiles: MdFileService,
    private review: ReviewContextService,
  ) {}

  get inside(): string | null {
    return this.inside$.value;
  }

  private body(agentName: string) {
    return {
      projectPath: this.projects.currentProjects$.value?.path || '',
      agentName,
      connectionId: this.messages.connectionId,
    };
  }

  enter(agentName: string): Observable<{ worktreePath: string; branch: string }> {
    return this.http.post<{ worktreePath: string; branch: string }>('../api/AgentWorkspace/enter', this.body(agentName)).pipe(
      tap(() => {
        this.inside$.next(agentName);
        this.review.enterAgent(agentName);
        this.reloadEverything();
      }),
    );
  }

  /**
   * Torna al proprio lavoro. Fuori da una copia è solo l'uscita dalla revisione. Dentro una copia il servizio
   * può rifiutare (file non committati): l'errore arriva a chi ha chiesto, che lo mostra.
   */
  leave(): Observable<void> {
    const agentName = this.inside;
    if (!agentName) {
      this.review.backToUser();
      return of(undefined);
    }
    return this.http.post('../api/AgentWorkspace/leave', this.body(agentName)).pipe(
      map(() => undefined),
      tap(() => {
        this.inside$.next(null);
        this.review.backToUser();
        this.reloadEverything();
      }),
      catchError(err => {
        // Restano file da committare: si porta la persona dove si committa.
        if (err?.status === 409) this.review.showChanges();
        return throwError(() => err);
      }),
    );
  }

  /** La finestra è stata ricaricata mentre si era dentro una copia: lo stato sta nel servizio, e si riprende. */
  resume(): void {
    const projectPath = this.projects.currentProjects$.value?.path;
    if (!projectPath || this.inside) return;
    this.http.get<{ agent: string | null }>('../api/AgentWorkspace/current', { params: { projectPath } }).subscribe({
      next: r => { if (r?.agent && !this.inside) this.enter(r.agent).subscribe({ error: () => { /* la copia non c'è più */ } }); },
      error: () => { /* nessuna copia da riprendere */ },
    });
  }

  /** La cartella di lavoro è cambiata: via il documento aperto (era dell'altra cartella), albero da capo. */
  private reloadEverything(): void {
    const welcome = new MdFile('Welcome to MDExplorer', '/../welcome.html', 0, false);
    welcome.relativePath = '/../../welcome.html';
    this.mdFiles.setSelectedMdFileFromSideNav(welcome);
    this.mdFiles.loadAll(null, null);
  }
}
