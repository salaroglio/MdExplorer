import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitMessagesComponent } from './components/git-messages/git-messages.component';
import { CommitMessageDialogComponent } from './dialogs/commit-message-dialog/commit-message-dialog.component';
import { GitHistoryDialogComponent } from './dialogs/git-history-dialog/git-history-dialog.component';
import { GitBranchDialogComponent } from './dialogs/git-branch-dialog/git-branch-dialog.component';
import { GitSetupRemoteDialogComponent } from './dialogs/git-setup-remote-dialog/git-setup-remote-dialog.component';
import { GitSetupRemoteGenericDialogComponent } from './dialogs/git-setup-remote-generic-dialog/git-setup-remote-generic-dialog.component';
import { GitTokenDialogComponent } from './dialogs/git-token-dialog/git-token-dialog.component';
import { GitAccountManagementDialogComponent } from './dialogs/git-account-management-dialog/git-account-management-dialog.component';
import { GitInitWizardDialogComponent } from './dialogs/git-init-wizard/git-init-wizard-dialog.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';



@NgModule({
  declarations: [
    GitMessagesComponent,
    CommitMessageDialogComponent,
    GitHistoryDialogComponent,
    GitBranchDialogComponent,
    GitSetupRemoteDialogComponent,
    GitSetupRemoteGenericDialogComponent,
    GitTokenDialogComponent,
    GitAccountManagementDialogComponent,
    GitInitWizardDialogComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    FormsModule,
    TranslateModule
  ],
  exports: [
    CommitMessageDialogComponent,
    GitHistoryDialogComponent,
    GitBranchDialogComponent,
    GitSetupRemoteDialogComponent,
    GitSetupRemoteGenericDialogComponent,
    GitTokenDialogComponent,
    GitAccountManagementDialogComponent,
    GitInitWizardDialogComponent
  ]
})
export class GitModule { }
