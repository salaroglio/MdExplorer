import { Component, HostListener } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { AppCurrentMetadataService } from './services/app-current-metadata.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
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
    private router: Router) {
    
    currentFolder.folderName.subscribe((data: any) => {      
      this.titleService.setTitle(data.currentFolder);
    });
    currentFolder.loadFolderName();
    
  }
}
