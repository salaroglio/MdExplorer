import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest } from 'rxjs';
import { distinctUntilChanged, map } from 'rxjs/operators';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { ProjectsService } from './projects.service';

/**
 * N6 della valutazione del 07/11, la soluzione semplice: mentre MDE fa le sue operazioni git (il commit automatico dei suoi file)
 * sul repository del progetto aperto, i pulsanti git dell'utente restano disattivati, così le due sequenze non si accavallano sul
 * blocco di git. `busy$` vale per il progetto corrente (che sta nel repository o in una sua sottocartella).
 */
@Injectable({ providedIn: 'root' })
export class MdeGitBusyService {
  private readonly roots = new BehaviorSubject<ReadonlySet<string>>(new Set());

  readonly busy$ = combineLatest([this.roots, this.projects.currentProjects$]).pipe(
    map(([roots, project]) => {
      const path = MdeGitBusyService.norm(project?.path);
      return !!path && [...roots].some(r => path === r || path.startsWith(r + '/'));
    }),
    distinctUntilChanged(),
  );

  constructor(serverMessages: MdServerMessagesService, private projects: ProjectsService) {
    serverMessages.mdeGitBusy$.subscribe(evt => {
      const root = MdeGitBusyService.norm(evt?.repositoryRoot);
      if (!root) return;
      const next = new Set(this.roots.value);
      if (evt.busy) next.add(root); else next.delete(root);
      this.roots.next(next);
    });
  }

  private static norm(p: string | null | undefined): string {
    return (p ?? '').replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
  }
}
