import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatLegacyDialogModule as MatDialogModule } from '@angular/material/legacy-dialog';
import { TranslateModule } from '@ngx-translate/core';
import { PromptLabRoutingModule } from './promptlab-routing.module';
import { PromptLabComponent } from './components/promptlab/promptlab.component';
import { PromptLabDocPanelComponent } from './components/promptlab-doc-panel/promptlab-doc-panel.component';
import { PromptLabAgentCardComponent } from './components/promptlab-agent-card/promptlab-agent-card.component';
import { PromptLabCardComponent } from './components/promptlab-card/promptlab-card.component';

@NgModule({
  declarations: [
    PromptLabComponent,
    PromptLabDocPanelComponent,
    PromptLabAgentCardComponent,
    PromptLabCardComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    TranslateModule,
    PromptLabRoutingModule
  ]
})
export class PromptLabModule { }
