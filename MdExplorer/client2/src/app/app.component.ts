import { Component, HostListener } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { slideInAnimation } from './shared/animations';
import { AppCurrentMetadataService } from './services/app-current-metadata.service';
import { AiNotificationService } from './services/ai-notification.service';
import { UrlHandlerService } from './services/url-handler.service';
import { FileChangeNotificationService } from './services/file-change-notification.service';
import { LanguageService } from './services/language.service';

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
    private languageService: LanguageService) {

    currentFolder.folderName.subscribe((data: any) => {
      this.titleService.setTitle(data.currentFolder);
    });
    currentFolder.loadFolderName();

    // Initialize URL handler service for mdexplorer:// protocol
    this.urlHandlerService.initialize();

    // Initialize file change notification service (taskbar flash)
    this.fileChangeNotificationService.initialize();
  }
}
