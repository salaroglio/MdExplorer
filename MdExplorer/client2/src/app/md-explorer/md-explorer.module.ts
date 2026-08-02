import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SidenavComponent } from './components/sidenav/sidenav.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { MainContentComponent } from './components/main-content/main-content.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { MdExplorerComponent } from './md-explorer.component';
import { Routes, RouterModule } from '@angular/router';
import { MdFileService } from './services/md-file.service';
// HttpClientModule is imported in AppModule - DO NOT import here or interceptors won't work
import { SafePipe } from './pipes/safePipe';
import { SettingsComponent } from './components/dialogs/settings/settings.component';
import { RenameFileComponent } from './components/refactoring/rename-file/rename-file.component';
import { RulesComponent } from '../signalR/dialogs/rules/rules.component';
import { NewMarkdownComponent } from './components/dialogs/new-markdown/new-markdown.component';
import { NewDirectoryComponent } from './components/dialogs/new-directory/new-directory.component';
import { MdTreeComponent } from './components/md-tree/md-tree.component';
import { AgentReviewComponent } from './components/agent-review/agent-review.component';
import { WorkingChangesComponent } from './components/working-changes/working-changes.component';
import { ConnectionLostComponent } from '../signalR/dialogs/connection-lost/connection-lost.component';
import { ParsingProjectComponent } from '../signalR/dialogs/parsing-project/parsing-project.component';
import { ChangeDirectoryComponent } from './components/dialogs/change-directory/change-directory.component';
import { DeleteMarkdownComponent } from './components/dialogs/delete-markdown/delete-markdown.component';
import { PublishMdTreeComponent } from './components/publish-md-tree/publish-md-tree.component';
import { GitlabSettingsComponent } from './components/gitlab-settings/gitlab-settings.component';
import { DocumentSettingsComponent } from './components/document-settings/document-settings.component';
import { CopyFromClipboardComponent } from './components/dialogs/copy-from-clipboard/copy-from-clipboard.component';
import { GitModule } from '../git/git.module';
import { MoveMdFileComponent } from './components/dialogs/move-md-file/move-md-file.component';
import { AddNewFileToMDEComponent } from './components/dialogs/add-new-file-to-mde/add-new-file-to-mde.component';
import { MilkdownReactHostComponent } from './components/milkdown-react-host/milkdown-react-host.component';
import { DocumentShowComponent } from './components/document-show/document-show.component'; // Added import
import { AiChatModule } from '../ai-chat/ai-chat.module';
// GitChat imports - declared directly to avoid module resolution conflicts
import { GitChatComponent } from '../git-chat/components/git-chat/git-chat.component';
import { GitChatService } from '../git-chat/services/git-chat.service';
import { CHAT_PROVIDER } from '../git-chat/providers/chat-provider.interface';
import { SignalRChatProvider } from '../git-chat/providers/signalr-chat.provider';
import { TocProgressDialogComponent } from './components/dialogs/toc-progress-dialog/toc-progress-dialog.component';
import { TocProgressService } from './services/toc-progress.service';
import { ConfirmDialogComponent } from '../commons/components/confirm-dialog/confirm-dialog.component';
import { ImageAnnotationCanvasComponent } from './components/image-annotation-canvas/image-annotation-canvas.component';
import { ScreenshotAnnotationWizardDialogComponent } from './components/dialogs/screenshot-annotation-wizard/screenshot-annotation-wizard-dialog.component';
import { P2PStatusWidgetComponent } from './components/toolbar/p2p-status-widget/p2p-status-widget.component';
import { ExternalAppComponent } from './components/external-app/external-app.component';
import { ExternalAppsSettingsComponent } from './components/external-apps-settings/external-apps-settings.component';
import { AppStoreComponent } from './components/app-store/app-store.component';
import { InstallWizardDialogComponent } from './components/dialogs/install-wizard/install-wizard.component';
import { BulkExportProgressDialogComponent } from './components/dialogs/bulk-export-progress/bulk-export-progress-dialog.component';
import { AppShowComponent } from './components/app-show/app-show.component';
import { IndexingProgressSnackComponent } from './components/indexing-progress-snack/indexing-progress-snack.component';
import { AiSelectionDialogComponent } from './components/dialogs/ai-selection-dialog/ai-selection-dialog.component';
import { MarkSearchComponent } from './components/mark-search/mark-search.component';
import { AgentLaunchDialogComponent } from './components/agent-launch-dialog/agent-launch-dialog.component';
import { AgentScheduleDialogComponent } from './components/agent-schedule-dialog/agent-schedule-dialog.component';
import { AgentRegistryDialogComponent } from './components/agent-registry-dialog/agent-registry-dialog.component';
import { AgentMemoryDialogComponent } from './components/agent-memory-dialog/agent-memory-dialog.component';
import { MailboxDialogComponent } from './components/mailbox-dialog/mailbox-dialog.component';


const routes: Routes = [
  { path: '', component: SidenavComponent },
  { path: 'app-store', component: AppStoreComponent },
  {
    path: 'navigation',
    component: SidenavComponent,
    children: [
      {
        path: '',
        component: DocumentShowComponent,
        children: [
          { path: 'document', component: MainContentComponent },
          { path: 'gitlabsettings', component: GitlabSettingsComponent },
          { path: 'documentsettings', component: DocumentSettingsComponent },
          { path: 'react-editor', component: MilkdownReactHostComponent },
          { path: 'external-app', component: ExternalAppComponent },
          { path: 'ai-chat', loadChildren: () => import('../ai-chat/ai-chat.module').then(m => m.AiChatModule) },
          { path: 'promptlab', loadChildren: () => import('../promptlab/promptlab.module').then(m => m.PromptLabModule) }
        ]
      },
      {
        path: 'ai-model-manager',
        loadChildren: () => import('../ai-chat/ai-chat.module').then(m => m.AiChatModule)
      }
    ]
  }
];


@NgModule({
  declarations: [
    AgentReviewComponent,
    WorkingChangesComponent,
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
    MoveMdFileComponent, AddNewFileToMDEComponent,
    MilkdownReactHostComponent,
    DocumentShowComponent, // Added to declarations
    TocProgressDialogComponent,
    ConfirmDialogComponent,
    GitChatComponent,
    ImageAnnotationCanvasComponent,
    ScreenshotAnnotationWizardDialogComponent,
    P2PStatusWidgetComponent,
    ExternalAppComponent,
    ExternalAppsSettingsComponent,
    AppStoreComponent,
    InstallWizardDialogComponent,
    BulkExportProgressDialogComponent,
    AppShowComponent,
    IndexingProgressSnackComponent,
    AiSelectionDialogComponent,
    MarkSearchComponent,
    AgentLaunchDialogComponent,
    AgentScheduleDialogComponent,
    AgentRegistryDialogComponent,
    AgentMemoryDialogComponent,
    MailboxDialogComponent
  ],
  imports: [
    CommonModule,
    DragDropModule,
    MaterialModule,
    FormsModule,
    GitModule,
    AiChatModule,
    TranslateModule,
    RouterModule.forChild(routes)
  ],
  providers: [
    // MdFileService è già providedIn: 'root', non va qui
    TocProgressService,
    // GitChat providers
    { provide: CHAT_PROVIDER, useClass: SignalRChatProvider },
    GitChatService
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class MdExplorerModule {
  constructor() {
    console.log('constructor MdExplorerModule');
  }
}
