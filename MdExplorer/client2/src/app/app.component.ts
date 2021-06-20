import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AppCurrentFolderService } from './services/app-current-folder.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'client2';
  constructor(private titleService: Title, private currentFolder: AppCurrentFolderService) {
    currentFolder.folderName.subscribe((data:any) => {
      debugger;
      this.titleService.setTitle(data.currentFolder);
    });
    currentFolder.loadFolderName();
    
  }
}
