import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatLegacyTooltipModule as MatTooltipModule } from '@angular/material/legacy-tooltip';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { ParticipantGemsComponent } from './participant-gems.component';

@NgModule({
  declarations: [ParticipantGemsComponent],
  imports: [CommonModule, MatTooltipModule, MatMenuModule],
  exports: [ParticipantGemsComponent]
})
export class ParticipantGemsModule {}
