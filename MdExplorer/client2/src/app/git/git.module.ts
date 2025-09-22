import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitMessagesComponent } from './components/git-messages/git-messages.component';
import { CommitMessageDialogComponent } from './dialogs/commit-message-dialog/commit-message-dialog.component';
import { GitHistoryDialogComponent } from './dialogs/git-history-dialog/git-history-dialog.component';
import { GitBranchDialogComponent } from './dialogs/git-branch-dialog/git-branch-dialog.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    GitMessagesComponent,
    CommitMessageDialogComponent,
    GitHistoryDialogComponent,
    GitBranchDialogComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    FormsModule
  ],
  exports: [
    CommitMessageDialogComponent,
    GitHistoryDialogComponent,
    GitBranchDialogComponent
  ]
})
export class GitModule { }
