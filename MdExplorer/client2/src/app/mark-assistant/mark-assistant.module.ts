import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { MarkAssistantComponent } from './mark-assistant.component';

@NgModule({
  declarations: [MarkAssistantComponent],
  imports: [CommonModule, FormsModule, TranslateModule],
  exports: [MarkAssistantComponent],
})
export class MarkAssistantModule {}
