import { Component, HostListener } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AppCurrentFolderService } from './services/app-current-folder.service';

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
    debugger;
    this.currentFolder.killServer();
  }
  title = 'client2';
  constructor(private titleService: Title, private currentFolder: AppCurrentFolderService) {
    currentFolder.folderName.subscribe((data:any) => {      
      this.titleService.setTitle(data.currentFolder);
    });
    currentFolder.loadFolderName();
    
  }
}
