import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from '@angular/core';
import { AgentDefinition } from '../../models/promptlab.models';

@Component({
  selector: 'app-promptlab-agent-card',
  templateUrl: './promptlab-agent-card.component.html',
  styleUrls: ['./promptlab-agent-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PromptLabAgentCardComponent {

  @Input() agentDefinition: AgentDefinition = {
    identity: '',
    objectives: '',
    rules: '',
    tools: []
  };

  @Output() agentDefinitionChange = new EventEmitter<AgentDefinition>();

  collapsed = false;

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
  }

  onFieldBlur(field: keyof AgentDefinition, value: string): void {
    const updated = { ...this.agentDefinition, [field]: value };
    this.agentDefinitionChange.emit(updated);
  }

  onToolsBlur(value: string): void {
    const tools = value.split(',').map(t => t.trim()).filter(t => t.length > 0);
    const updated = { ...this.agentDefinition, tools };
    this.agentDefinitionChange.emit(updated);
  }
}
