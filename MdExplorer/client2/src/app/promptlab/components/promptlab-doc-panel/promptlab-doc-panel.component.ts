import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { PromptLabCard } from '../../models/promptlab.models';

@Component({
  selector: 'app-promptlab-doc-panel',
  templateUrl: './promptlab-doc-panel.component.html',
  styleUrls: ['./promptlab-doc-panel.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PromptLabDocPanelComponent {

  @Input() cards: PromptLabCard[] = [];
  @Input() templateName = '';

  collapsed = false;

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
  }
}
