import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitMessagesComponent } from './components/git-messages/git-messages.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    GitMessagesComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    FormsModule
  ],
  exports: []
})
export class GitModule { }
