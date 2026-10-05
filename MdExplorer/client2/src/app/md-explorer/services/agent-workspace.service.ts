import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { LeaveAgentCopyDialogComponent, PendingInCopy } from '../components/leave-agent-copy-dialog/leave-agent-copy-dialog.component';
import { map, switchMap, tap } from 'rxjs/operators';
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
 * città (posta, registro, differenze). Uscendo si torna alla propria cartella; se nella copia c'è lavoro da
 * salvare, la persona autorizza commit e pubblicazione in un gesto solo, oppure resta.
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
    private dialog: MatDialog,
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
   * Torna al proprio lavoro. Fuori da una copia è solo l'uscita dalla revisione. Dentro una copia, se c'è
   * lavoro da salvare si apre la finestra di uscita: la persona vede cosa verrà committato e pubblicato e lo
   * autorizza, oppure resta. Emette ciò che è stato pubblicato; `null` = è rimasta nella copia.
   */
  leave(): Observable<string[] | null> {
    const agentName = this.inside;
    if (!agentName) {
      this.review.backToUser();
      return of([]);
    }
    return this.http.post<PendingInCopy>('../api/AgentWorkspace/pending', this.body(agentName)).pipe(
      switchMap(pending => {
        if (!pending.uncommitted.length && !pending.unpublished.length) return this.postLeave(agentName, false, null);
        return this.dialog.open(LeaveAgentCopyDialogComponent, { data: { agent: agentName, pending }, autoFocus: false })
          .afterClosed().pipe(
            switchMap(message => message === null || message === undefined
              ? of(null)
              : this.postLeave(agentName, true, message)));
      }),
    );
  }

  private postLeave(agentName: string, authorized: boolean, commitMessage: string | null): Observable<string[]> {
    return this.http.post<{ published?: string[] }>('../api/AgentWorkspace/leave',
      { ...this.body(agentName), authorized, commitMessage }).pipe(
      map(r => r?.published || []),
      tap(() => {
        this.inside$.next(null);
        this.review.backToUser();
        this.reloadEverything();
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
