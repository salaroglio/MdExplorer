import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import {
  LeaveAgentCopyDialogComponent, NewBranchAnswer, PendingInCopy, PROCEED_WITHOUT_SAVING,
} from '../components/leave-agent-copy-dialog/leave-agent-copy-dialog.component';
import { GITService } from '../../git/services/gitservice.service';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

/** Come parte l'agente: `baseBranch` = il ramo nuovo con dentro il lavoro aperto (la casella), assente = come sempre. */
export interface StartDecision {
  baseBranch?: string;
}

/**
 * Prima che un agente parta. La copia in cui lavora nasce da ciò che è pubblicato su `origin`: i file che la
 * persona non ha committato e i commit che non ha pubblicato, l'agente non li vede. Qui lo si dice a chi sta
 * per farlo partire, e gli si offre di committare e pubblicare prima — oppure di partire lo stesso. Nel lancio a
 * mano c'è in più la casella «nel ramo nuovo» (sprint 2026-11-25-Nuovo-Ramo-E-Lavoro-All-Agente, B3).
 */
@Injectable({ providedIn: 'root' })
export class AgentStartGuardService {
  constructor(
    private http: HttpClient,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    private git: GITService,
    private serverMessages: MdServerMessagesService,
  ) {}

  /** Emette `true` se l'agente può partire, `false` se la persona ha annullato o il salvataggio non è riuscito. */
  beforeStart(projectPath: string, agent: string): Observable<boolean> {
    return this.guard(projectPath, agent, false).pipe(map(decision => decision !== null));
  }

  /**
   * Il lancio a mano con la copia isolata: come {@link beforeStart}, e con la casella spuntata il lavoro aperto va
   * in un ramo nuovo (commit della persona, pubblicato) da cui l'agente parte. Emette null se l'agente non parte.
   */
  beforeManualLaunch(projectPath: string, agent: string): Observable<StartDecision | null> {
    return this.guard(projectPath, agent, true);
  }

  private guard(projectPath: string, agent: string, offerNewBranch: boolean): Observable<StartDecision | null> {
    return this.http.post<PendingInCopy>('../api/AgentWorkspace/project-pending', { projectPath }).pipe(
      map(pending => pending as PendingInCopy | null),
      // Se non si sa com'è la cartella non si parte alla cieca: lo si dice, e l'agente non parte.
      catchError(err => {
        this.fail(err?.error?.error || this.translate.instant('BEFORE_START.PENDING_FAILED'));
        return of(null);
      }),
      switchMap(pending => {
        if (pending === null) return of(null);
        if (!pending.uncommitted.length && !pending.unpublished.length) return of({});
        return this.dialog.open(LeaveAgentCopyDialogComponent, {
          data: { agent, pending, mode: 'start', offerNewBranch }, autoFocus: false,
        })
          .afterClosed().pipe(
            switchMap((answer: string | NewBranchAnswer | null | undefined) => {
              if (answer === null || answer === undefined) return of(null);
              if (answer === PROCEED_WITHOUT_SAVING) return of({});
              if (typeof answer !== 'string') return this.intoNewBranch(projectPath, answer);
              return this.http.post('../api/AgentWorkspace/project-save', { projectPath, commitMessage: answer }).pipe(
                map(() => ({}) as StartDecision),
                catchError(err => {
                  this.fail(err?.error?.error || this.translate.instant('BEFORE_START.SAVE_FAILED'));
                  return of(null);
                }));
            }));
      }),
    );
  }

  /**
   * «Nuovo ramo» con il messaggio: il ramo nasce dal punto in cui si è, il lavoro aperto vi si registra con il commit
   * della persona e si pubblica. L'agente parte da `origin/<ramo>`, quindi senza commit o senza pubblicazione non parte.
   */
  private intoNewBranch(projectPath: string, answer: NewBranchAnswer): Observable<StartDecision | null> {
    return this.git.createNewBranch(projectPath, answer.branch, this.serverMessages.connectionId, answer.message).pipe(
      map(result => {
        const problem = !result.created ? result.error
          : !result.committed ? (result.commitProblem || this.translate.instant('BEFORE_START.NOTHING_COMMITTED'))
          : !result.published ? result.publishProblem
          : null;
        if (problem) {
          this.fail(this.translate.instant('BEFORE_START.NEW_BRANCH_FAILED', { problem }));
          return null;
        }
        // La barra in alto legge il ramo da currentBranch$: la persona adesso è sul ramo nuovo, senza file aperti.
        this.git.modernGetBranchStatus(projectPath).subscribe({ next: b => this.git.currentBranch$.next(b), error: () => {} });
        return { baseBranch: result.branch || answer.branch };
      }),
      catchError(err => {
        this.fail(this.translate.instant('BEFORE_START.NEW_BRANCH_FAILED',
          { problem: err?.error?.error || err?.message || '' }));
        return of(null);
      }));
  }

  private fail(text: string): void {
    this.snackBar.open(text, this.translate.instant('COMMON.CLOSE'), { duration: 12000 });
  }
}
