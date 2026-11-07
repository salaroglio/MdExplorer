import { TranslateService } from '@ngx-translate/core';
import { Component, HostListener } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { MatLegacySnackBar as MatSnackBar, MatLegacySnackBarRef as MatSnackBarRef, LegacyTextOnlySnackBar as TextOnlySnackBar } from '@angular/material/legacy-snack-bar';
import { of } from 'rxjs';
import { catchError, distinctUntilChanged, filter, map, switchMap } from 'rxjs/operators';
import { slideInAnimation } from './shared/animations';
import { AppCurrentMetadataService } from './services/app-current-metadata.service';
import { AiNotificationService } from './services/ai-notification.service';
import { UrlHandlerService } from './services/url-handler.service';
import { FileChangeNotificationService } from './services/file-change-notification.service';
import { LanguageService } from './services/language.service';
import { ThemeService } from './services/theme.service';
import { ExecutionService } from './services/execution.service';
import { MarkDiagramService } from './mark-assistant/mark-diagram.service';
import { MdServerMessagesService } from './signalR/services/server-messages.service';
import { ProjectsService } from './md-explorer/services/projects.service';
import { DevelopmentConfigStateService } from './md-explorer/services/development-config-state.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  animations: [
    slideInAnimation
  ],
})
export class AppComponent {
  @HostListener('window:unload', ['$event'])
  unloadHandler(event) {
    // E' stato dato il comando di chiusura del tab o di chrome
    // spegni il serverino che si è acceso
    if (performance.navigation.type != performance.navigation.TYPE_RELOAD) {
      //this.currentFolder.killServer();
    }

    //
  }
  title = 'client2';
  constructor(private titleService: Title,
    private currentFolder: AppCurrentMetadataService,
    private route: ActivatedRoute,
    private router: Router,
    private aiNotificationService: AiNotificationService,
    private urlHandlerService: UrlHandlerService,
    private fileChangeNotificationService: FileChangeNotificationService,
    private languageService: LanguageService,
    private themeService: ThemeService,
    private executionService: ExecutionService,
    // Instantiated for its side effect: it installs the "Ask to MarkAgent"
    // postMessage listener before any document can be opened.
    private markDiagramService: MarkDiagramService,
    private serverMessages: MdServerMessagesService,
    private snackBar: MatSnackBar,
    private projectsService: ProjectsService,
    private developmentConfig: DevelopmentConfigStateService,
    private translate: TranslateService) {

    currentFolder.folderName.subscribe((data: any) => {
      this.titleService.setTitle(data.currentFolder);
    });
    currentFolder.loadFolderName();

    // Initialize URL handler service for mdexplorer:// protocol
    this.urlHandlerService.initialize();

    // Initialize file change notification service (taskbar flash)
    this.fileChangeNotificationService.initialize();

    // KG drift surface: when a .md edit invalidates the adjacent .kg.cypher, show
    // a snackbar pointing the user at the file that needs regeneration. The
    // backend (FileSystemWatcherManager.CheckKgDriftBestEffortAsync) already
    // throttles by only emitting on actual mismatch.
    this.serverMessages.kgStale$.subscribe(evt => {
      const filename = (evt?.sourceMdPath ?? '').split(/[\\\/]/).pop() || this.translate.instant('APP.KG_STALE_DOCUMENT');
      this.snackBar.open(
        this.translate.instant('APP.KG_STALE', { filename }),
        'OK',
        { duration: 6000, panelClass: ['kg-stale-snack'] }
      );
    });

    // Sprint «File di sistema di MDE», S3: ogni modifica di MDE a un file del repository si dice, con il motivo e con che ne è
    // del commit/push. L'evento va a tutte le finestre: lo mostra solo quella del progetto dentro quel repository.
    this.serverMessages.mdeRepoChange$.subscribe(evt => {
      const norm = (p: string) => (p ?? '').replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
      const project = norm(this.projectsService.currentProjects$.getValue()?.path);
      const root = norm(evt?.repositoryRoot);
      if (!project || !root || !(project === root || project.startsWith(root + '/'))) return;
      if (!evt.notices?.length) return;
      const problem = evt.pending && evt.pending !== 'None' && evt.pending !== 'NothingToSave' && !evt.pushed;
      this.snackBar.open(evt.notices.join('\n'), 'OK', {
        duration: problem ? 15000 : 8000,
        panelClass: problem ? ['kg-stale-snack', 'mde-repo-change-snack'] : ['mde-repo-change-snack'],
      });
    });

    // Sprint «Schema di .development.yml», F4 (D4): a ogni apertura di un progetto, se il suo .development.yml ha gruppi spenti,
    // l'avviso resta finché l'utente non lo chiude; dopo un pull o un cambio del file arriva dal servizio, e uno vuoto lo chiude.
    this.projectsService.currentProjects$.pipe(
      map(p => p?.path ?? ''),
      distinctUntilChanged(),
      filter(path => !!path),
      switchMap(path => this.developmentConfig.get(path).pipe(catchError(() => of(null)))),
    ).subscribe(state => this.showDevelopmentConfig(state?.notice ?? ''));
    this.serverMessages.developmentConfigOff$.subscribe(evt => {
      const norm = (p: string) => (p ?? '').replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
      if (!evt || norm(evt.projectPath) !== norm(this.projectsService.currentProjects$.getValue()?.path)) return;
      this.showDevelopmentConfig(evt.notice ?? '');
    });

    // Esito dei run degli agenti *.agent.md (lancio manuale, schedule, hook):
    // toast globale così l'esito arriva anche se il dialog di lancio è chiuso.
    this.serverMessages.agentJobProgress$.subscribe(evt => {
      if (evt.phase === 'completed') {
        this.snackBar.open(this.translate.instant('APP.AGENT_COMPLETED', { agent: evt.agentName }), 'OK', { duration: 5000 });
      } else if (evt.phase === 'failed') {
        this.snackBar.open(
          this.translate.instant('APP.AGENT_FAILED', { agent: evt.agentName, error: evt.error || this.translate.instant('APP.UNKNOWN_ERROR') }),
          'OK',
          { duration: 10000, panelClass: ['kg-stale-snack'] }
        );
      }
    });
  }

  private developmentConfigSnack: MatSnackBarRef<TextOnlySnackBar> | null = null;

  /** L'avviso sui gruppi spenti: resta aperto finché l'utente non lo chiude (D4); un testo vuoto chiude quello aperto. */
  private showDevelopmentConfig(notice: string): void {
    this.developmentConfigSnack?.dismiss();
    this.developmentConfigSnack = null;
    if (!notice) return;
    this.developmentConfigSnack = this.snackBar.open(notice, 'OK', { panelClass: ['kg-stale-snack', 'mde-repo-change-snack'] });
    this.developmentConfigSnack.afterDismissed().subscribe(() => this.developmentConfigSnack = null);
  }
}
