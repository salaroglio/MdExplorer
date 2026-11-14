import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import {
  LeaveAgentCopyDialogComponent, PendingInCopy, PROCEED_WITHOUT_SAVING,
} from '../components/leave-agent-copy-dialog/leave-agent-copy-dialog.component';

/**
 * Prima che un agente parta. La copia in cui lavora nasce da ciò che è pubblicato su `origin`: i file che la
 * persona non ha committato e i commit che non ha pubblicato, l'agente non li vede. Qui lo si dice a chi sta
 * per farlo partire, e gli si offre di committare e pubblicare prima — oppure di partire lo stesso.
 */
@Injectable({ providedIn: 'root' })
export class AgentStartGuardService {
  constructor(
    private http: HttpClient,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  /** Emette `true` se l'agente può partire, `false` se la persona ha annullato o il salvataggio non è riuscito. */
  beforeStart(projectPath: string, agent: string): Observable<boolean> {
    return this.http.post<PendingInCopy>('../api/AgentWorkspace/project-pending', { projectPath }).pipe(
      // Se non si riesce a sapere com'è la cartella, non è un motivo per fermare l'agente: si parte come prima.
      catchError(() => of({ uncommitted: [], unpublished: [] } as PendingInCopy)),
      switchMap(pending => {
        if (!pending.uncommitted.length && !pending.unpublished.length) return of(true);
        return this.dialog.open(LeaveAgentCopyDialogComponent, { data: { agent, pending, mode: 'start' }, autoFocus: false })
          .afterClosed().pipe(
            switchMap(answer => {
              if (answer === null || answer === undefined) return of(false);
              if (answer === PROCEED_WITHOUT_SAVING) return of(true);
              return this.http.post('../api/AgentWorkspace/project-save', { projectPath, commitMessage: answer }).pipe(
                map(() => true),
                catchError(err => {
                  this.snackBar.open(
                    err?.error?.error || this.translate.instant('BEFORE_START.SAVE_FAILED'),
                    this.translate.instant('COMMON.CLOSE'), { duration: 10000 });
                  return of(false);
                }));
            }));
      }),
    );
  }
}
