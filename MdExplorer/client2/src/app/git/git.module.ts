import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitAuthComponent } from './components/git-auth/git-auth.component';
import { GitMessagesComponent } from './components/git-messages/git-messages.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    GitAuthComponent,
    GitMessagesComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    FormsModule
  ],
  exports: [GitAuthComponent]
})
export class GitModule { }
