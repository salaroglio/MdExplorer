import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidenavComponent } from './components/sidenav/sidenav.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { MainContentComponent } from './components/main-content/main-content.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';
import { MdExplorerComponent } from './md-explorer.component';
import { Routes, RouterModule } from '@angular/router';
import { MdFileService } from './services/md-file.service';
import { HttpClientModule } from '@angular/common/http';
import { SafePipe } from './pipes/safePipe';
import { SettingsComponent } from './components/dialogs/settings/settings.component';
import { RenameFileComponent } from './components/refactoring/rename-file/rename-file.component';
import { RulesComponent } from '../signalR/dialogs/rules/rules.component';
import { NewMarkdownComponent } from './components/dialogs/new-markdown/new-markdown.component';
import { NewDirectoryComponent } from './components/dialogs/new-directory/new-directory.component';
import { MdTreeComponent } from './components/md-tree/md-tree.component';
import { ConnectionLostComponent } from '../signalR/dialogs/connection-lost/connection-lost.component';
import { ParsingProjectComponent } from '../signalR/dialogs/parsing-project/parsing-project.component';
import { ChangeDirectoryComponent } from './components/dialogs/change-directory/change-directory.component';
import { DeleteMarkdownComponent } from './components/dialogs/delete-markdown/delete-markdown.component';
import { PublishMdTreeComponent } from './components/publish-md-tree/publish-md-tree.component';
import { GitlabSettingsComponent } from './components/gitlab-settings/gitlab-settings.component';
import { DocumentSettingsComponent } from './components/document-settings/document-settings.component';
import { CopyFromClipboardComponent } from './components/dialogs/copy-from-clipboard/copy-from-clipboard.component';
import { GitModule } from '../git/git.module';


const routes: Routes = [
  { path: '', component: SidenavComponent },
  {
  path: 'navigation', component: SidenavComponent,
  children: [
    { path: 'document', component: MainContentComponent },
    { path: 'gitlabsettings', component: GitlabSettingsComponent },
    { path: 'documentsettings', component: DocumentSettingsComponent },
  ]
  }];


@NgModule({
  declarations: [
    SidenavComponent,
    ToolbarComponent,
    SafePipe,
    MainContentComponent,
    MdExplorerComponent,
    SettingsComponent,
    RenameFileComponent,
    RulesComponent,
    NewMarkdownComponent,
    NewDirectoryComponent,
    MdTreeComponent,
    ConnectionLostComponent,
    ParsingProjectComponent,
    ChangeDirectoryComponent,
    DeleteMarkdownComponent,
    PublishMdTreeComponent,
    GitlabSettingsComponent,
    DocumentSettingsComponent,
    CopyFromClipboardComponent,
      
  ],
  imports: [
    CommonModule,
    MaterialModule,
    HttpClientModule,
    FormsModule,
    GitModule,
    RouterModule.forChild(routes)
  ],
  providers: [
    MdFileService,
    
  ]
})
export class MdExplorerModule {
  constructor() {
    console.log('constructor MdExplorerModule');
  }
}
