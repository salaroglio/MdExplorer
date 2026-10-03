import { Component, HostListener } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
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
    private snackBar: MatSnackBar) {

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
      const filename = (evt?.sourceMdPath ?? '').split(/[\\\/]/).pop() || 'documento';
      this.snackBar.open(
        `⚠️  Knowledge Graph non aggiornato per "${filename}". Rigenera il grafo.`,
        'OK',
        { duration: 6000, panelClass: ['kg-stale-snack'] }
      );
    });

    // Esito dei run degli agenti *.agent.md (lancio manuale, schedule, hook):
    // toast globale così l'esito arriva anche se il dialog di lancio è chiuso.
    this.serverMessages.agentJobProgress$.subscribe(evt => {
      if (evt.phase === 'completed') {
        this.snackBar.open(`🤖 Agente "${evt.agentName}" completato.`, 'OK', { duration: 5000 });
      } else if (evt.phase === 'failed') {
        this.snackBar.open(
          `🤖 Agente "${evt.agentName}" fallito: ${evt.error || 'errore sconosciuto'}`,
          'OK',
          { duration: 10000, panelClass: ['kg-stale-snack'] }
        );
      }
    });
  }
}
