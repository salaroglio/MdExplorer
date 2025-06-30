import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitMessagesComponent } from './components/git-messages/git-messages.component';
import { CommitMessageDialogComponent } from './dialogs/commit-message-dialog/commit-message-dialog.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    GitMessagesComponent,
    CommitMessageDialogComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    FormsModule
  ],
  exports: [CommitMessageDialogComponent]
})
export class GitModule { }
