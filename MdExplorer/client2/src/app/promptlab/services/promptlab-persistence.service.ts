import { Injectable } from '@angular/core';
import {
  PromptLabSession,
  PromptLabCard,
  PromptLabParameter,
  PromptLabMode,
  ParameterType,
  AgentDefinition,
  DEFAULT_SYSTEM_PROMPT
} from '../models/promptlab.models';

@Injectable({ providedIn: 'root' })
export class PromptLabPersistenceService {

  // ---------------------------------------------------------------------------
  // Public API
  // ---------------------------------------------------------------------------

  /**
   * Parse a Markdown template into a PromptLabSession model.
   */
  loadTemplate(markdown: string): PromptLabSession {
    const frontMatter = this.parseFrontMatter(markdown);
    const body = this.stripFrontMatter(markdown);

    const mode: PromptLabMode = frontMatter['mode'] === 'agent' ? 'agent' : 'prompt';

    // Parse system prompt section (if present)
    const systemPromptResult = this.parseSystemPromptSection(body);
    let remainingBody = systemPromptResult.remaining;

    let agentDefinition: AgentDefinition | undefined;

    if (mode === 'agent') {
      const agentResult = this.parseAgentSection(remainingBody);
      agentDefinition = agentResult.agent;
      remainingBody = agentResult.remaining;
    }

    const cards = this.parseCards(remainingBody);

    return {
      id: this.generateUUID(),
      title: frontMatter['title'] || '',
      model: frontMatter['model'] || '',
      mode,
      systemPrompt: systemPromptResult.systemPrompt,
      agentDefinition,
      cards,
      createdAt: frontMatter['created'] ? new Date(frontMatter['created']) : new Date(),
      updatedAt: frontMatter['updated'] ? new Date(frontMatter['updated']) : new Date(),
      templatePath: ''
    };
  }

  /**
   * Serialize a PromptLabSession model back to Markdown template format.
   */
  saveTemplate(session: PromptLabSession): string {
    const lines: string[] = [];

    // Front matter
    lines.push('---');
    lines.push('promptlab: true');
    lines.push('version: 1');
    lines.push(`title: ${session.title}`);
    lines.push(`model: ${session.model}`);
    lines.push(`mode: ${session.mode}`);
    lines.push(`created: ${this.formatDate(session.createdAt)}`);
    lines.push(`updated: ${this.formatDate(session.updatedAt)}`);
    lines.push('---');
    lines.push('');

    // System prompt section (only if non-default or explicitly customized)
    if (session.systemPrompt) {
      lines.push('## System Prompt');
      lines.push('');
      lines.push(session.systemPrompt);
      lines.push('');
      lines.push('---');
      lines.push('');
    }

    // Agent section
    if (session.mode === 'agent' && session.agentDefinition) {
      lines.push(...this.serializeAgentSection(session.agentDefinition));
      lines.push('---');
      lines.push('');
    }

    // Cards
    session.cards.forEach((card, index) => {
      if (index > 0) {
        lines.push('---');
        lines.push('');
      }
      lines.push(...this.serializeCard(card));
    });

    return lines.join('\n');
  }

  /**
   * Build a ready-to-execute Markdown string from a PromptLabSession.
   * Parameters in {{param}} placeholders are replaced with their actual values.
   */
  buildSession(session: PromptLabSession): string {
    const sections: string[] = [];

    // Agent system prompt
    if (session.mode === 'agent' && session.agentDefinition) {
      sections.push(this.buildAgentSystemPrompt(session.agentDefinition));
    }

    // Resolved cards
    for (const card of session.cards) {
      const resolved = this.resolveParameters(card.distilledPrompt, card.parameters);
      sections.push(resolved);
    }

    return sections.join('\n\n---\n\n');
  }

  // ---------------------------------------------------------------------------
  // Front matter helpers
  // ---------------------------------------------------------------------------

  private parseFrontMatter(markdown: string): Record<string, string> {
    const result: Record<string, string> = {};
    const match = markdown.match(/^---\r?\n([\s\S]*?)\r?\n---/);
    if (!match) {
      return result;
    }

    const yamlBlock = match[1];
    for (const line of yamlBlock.split(/\r?\n/)) {
      const colonIndex = line.indexOf(':');
      if (colonIndex === -1) continue;
      const key = line.substring(0, colonIndex).trim();
      const value = line.substring(colonIndex + 1).trim();
      result[key] = value;
    }
    return result;
  }

  private stripFrontMatter(markdown: string): string {
    return markdown.replace(/^---\r?\n[\s\S]*?\r?\n---\r?\n?/, '');
  }

  // ---------------------------------------------------------------------------
  // System Prompt section parsing
  // ---------------------------------------------------------------------------

  private parseSystemPromptSection(body: string): { systemPrompt: string; remaining: string } {
    const headingRegex = /^## System Prompt\s*$/m;
    const match = headingRegex.exec(body);

    if (!match) {
      // No system prompt section found — use default
      return { systemPrompt: DEFAULT_SYSTEM_PROMPT, remaining: body };
    }

    const afterHeading = body.substring(match.index + match[0].length);
    // Content ends at the first `---` separator or first `## ` heading
    const endMatch = afterHeading.match(/\r?\n---\r?\n/);
    const nextH2 = afterHeading.match(/\r?\n## /);

    let endIndex: number;
    if (endMatch && nextH2) {
      endIndex = Math.min(endMatch.index!, nextH2.index!);
    } else {
      endIndex = endMatch?.index ?? nextH2?.index ?? afterHeading.length;
    }

    const content = afterHeading.substring(0, endIndex).trim();
    const remaining = endMatch && endMatch.index === endIndex
      ? afterHeading.substring(endIndex + endMatch[0].length)
      : afterHeading.substring(endIndex);

    return {
      systemPrompt: content || DEFAULT_SYSTEM_PROMPT,
      remaining: body.substring(0, match.index) + remaining
    };
  }

  // ---------------------------------------------------------------------------
  // Agent section parsing
  // ---------------------------------------------------------------------------

  private parseAgentSection(body: string): { agent: AgentDefinition; remaining: string } {
    const agentHeadingRegex = /^## Agent\s*$/m;
    const agentMatch = agentHeadingRegex.exec(body);

    if (!agentMatch) {
      return {
        agent: { identity: '', objectives: '', rules: '', tools: [] },
        remaining: body
      };
    }

    // Find where the agent section ends: at the first `---` separator or first `## Card:`
    const afterAgent = body.substring(agentMatch.index + agentMatch[0].length);
    const endMatch = afterAgent.match(/\r?\n---\r?\n/);
    const agentContent = endMatch
      ? afterAgent.substring(0, endMatch.index)
      : afterAgent;
    const remaining = endMatch
      ? afterAgent.substring(endMatch.index + endMatch[0].length)
      : '';

    const identity = this.extractSubSection(agentContent, "Identita'");
    const objectives = this.extractSubSection(agentContent, 'Obiettivi');
    const rules = this.extractSubSection(agentContent, 'Regole');
    const toolsRaw = this.extractSubSection(agentContent, 'Strumenti');

    const tools = toolsRaw
      .split(/\r?\n/)
      .map(line => line.replace(/^\s*-\s*/, '').trim())
      .filter(line => line.length > 0);

    return {
      agent: { identity, objectives, rules, tools },
      remaining
    };
  }

  private extractSubSection(content: string, heading: string): string {
    const regex = new RegExp(`^### ${this.escapeRegex(heading)}\\s*$`, 'm');
    const match = regex.exec(content);
    if (!match) return '';

    const after = content.substring(match.index + match[0].length);
    // Content goes until the next ### heading or end of string
    const nextHeading = after.match(/\r?\n###\s/);
    const text = nextHeading ? after.substring(0, nextHeading.index) : after;
    return text.trim();
  }

  // ---------------------------------------------------------------------------
  // Card parsing
  // ---------------------------------------------------------------------------

  private parseCards(body: string): PromptLabCard[] {
    const cards: PromptLabCard[] = [];
    // Split on `## Card: ` headings, keeping the heading text
    const cardRegex = /^## Card:\s*(.+)$/gm;
    const matches: { title: string; index: number }[] = [];
    let m: RegExpExecArray | null;

    while ((m = cardRegex.exec(body)) !== null) {
      matches.push({ title: m[1].trim(), index: m.index });
    }

    for (let i = 0; i < matches.length; i++) {
      const start = matches[i].index;
      const end = i + 1 < matches.length ? matches[i + 1].index : body.length;
      let cardContent = body.substring(start, end);

      // Strip trailing separator
      cardContent = cardContent.replace(/\r?\n---\s*$/, '');

      const title = matches[i].title;
      const id = this.extractCardId(cardContent, i + 1);
      const parameters = this.parseParameterTable(cardContent);
      const distilledPrompt = this.extractPromptText(cardContent);

      // Cross-check: find {{param}} in prompt and ensure they exist in parameters
      const promptParams = this.extractParamPlaceholders(distilledPrompt);
      for (const pName of promptParams) {
        if (!parameters.find(p => p.name === pName)) {
          parameters.push({ name: pName, value: '', type: 'text' });
        }
      }

      cards.push({
        id,
        generatedTitle: title,
        parameters,
        distilledPrompt,
        conversation: [],
        lastRun: undefined
      });
    }

    return cards;
  }

  private extractCardId(cardContent: string, fallbackIndex: number): string {
    const match = cardContent.match(/<!--\s*promptlab-card-id:\s*([\w-]+)\s*-->/);
    if (match) return match[1];
    // Generate a fallback ID
    const idx = String(fallbackIndex).padStart(3, '0');
    return `card-${idx}`;
  }

  private parseParameterTable(cardContent: string): PromptLabParameter[] {
    const params: PromptLabParameter[] = [];
    // Find ### Parametri section
    const sectionMatch = cardContent.match(/^### Parametri\s*$/m);
    if (!sectionMatch) return params;

    const after = cardContent.substring(sectionMatch.index! + sectionMatch[0].length);
    // Find the table rows (skip header row and separator row)
    const lines = after.split(/\r?\n/);
    let tableStarted = false;
    let headerSkipped = false;

    for (const line of lines) {
      const trimmed = line.trim();
      // Stop at next heading or end
      if (trimmed.startsWith('###') || trimmed.startsWith('## ')) break;

      if (!trimmed.startsWith('|')) {
        if (tableStarted) break; // Table ended
        continue;
      }

      tableStarted = true;

      // Skip header row
      if (!headerSkipped) {
        headerSkipped = true;
        continue;
      }

      // Skip separator row (|---|---|---|)
      if (/^\|[\s-|]+\|$/.test(trimmed)) continue;

      // Parse data row
      const cells = trimmed
        .split('|')
        .map(c => c.trim())
        .filter(c => c.length > 0);

      if (cells.length >= 2) {
        const name = cells[0];
        const type = this.parseParameterType(cells[1]);
        const value = cells.length >= 3 ? cells[2] : '';
        params.push({ name, type, value });
      }
    }

    return params;
  }

  private parseParameterType(raw: string): ParameterType {
    const normalized = raw.trim().toLowerCase();
    if (normalized === 'file') return 'file';
    if (normalized === 'directory') return 'directory';
    return 'text';
  }

  private extractPromptText(cardContent: string): string {
    const promptMatch = cardContent.match(/^### Prompt\s*$/m);
    if (!promptMatch) return '';

    const after = cardContent.substring(promptMatch.index! + promptMatch[0].length);
    // Prompt text goes until next `## `, `### `, or `---` separator, or end
    const endMatch = after.match(/\r?\n(?:---|## |### )/);
    const text = endMatch ? after.substring(0, endMatch.index) : after;
    return text.trim();
  }

  private extractParamPlaceholders(text: string): string[] {
    const result: string[] = [];
    const regex = /\{\{(\w+)\}\}/g;
    let match: RegExpExecArray | null;
    while ((match = regex.exec(text)) !== null) {
      if (!result.includes(match[1])) {
        result.push(match[1]);
      }
    }
    return result;
  }

  // ---------------------------------------------------------------------------
  // Serialization helpers
  // ---------------------------------------------------------------------------

  private serializeAgentSection(agent: AgentDefinition): string[] {
    const lines: string[] = [];
    lines.push("## Agent");
    lines.push('');
    lines.push("### Identita'");
    lines.push('');
    lines.push(agent.identity);
    lines.push('');
    lines.push('### Obiettivi');
    lines.push('');
    lines.push(agent.objectives);
    lines.push('');
    lines.push('### Regole');
    lines.push('');
    lines.push(agent.rules);
    lines.push('');
    lines.push('### Strumenti');
    lines.push('');
    for (const tool of agent.tools) {
      lines.push(`- ${tool}`);
    }
    lines.push('');
    return lines;
  }

  private serializeCard(card: PromptLabCard): string[] {
    const lines: string[] = [];
    lines.push(`## Card: ${card.generatedTitle}`);
    lines.push('');
    lines.push(`<!-- promptlab-card-id: ${card.id} -->`);
    lines.push('');
    lines.push('### Parametri');
    lines.push('');
    lines.push('| Nome | Tipo | Valore |');
    lines.push('|------|------|--------|');
    for (const param of card.parameters) {
      lines.push(`| ${param.name} | ${param.type} | ${param.value} |`);
    }
    lines.push('');
    lines.push('### Prompt');
    lines.push('');
    lines.push(card.distilledPrompt);
    lines.push('');
    return lines;
  }

  private formatDate(date: Date): string {
    if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
      return new Date().toISOString().replace(/\.\d{3}Z$/, '');
    }
    return date.toISOString().replace(/\.\d{3}Z$/, '');
  }

  // ---------------------------------------------------------------------------
  // Build helpers
  // ---------------------------------------------------------------------------

  /**
   * Public accessor for buildAgentSystemPrompt, used by PromptLabService
   * when building prompts for card execution.
   */
  buildAgentSystemPromptPublic(agent: AgentDefinition): string {
    return this.buildAgentSystemPrompt(agent);
  }

  private buildAgentSystemPrompt(agent: AgentDefinition): string {
    const parts: string[] = [];

    if (agent.identity) {
      parts.push(agent.identity);
    }

    if (agent.objectives) {
      parts.push(`Obiettivi:\n${agent.objectives}`);
    }

    if (agent.rules) {
      parts.push(`Regole:\n${agent.rules}`);
    }

    if (agent.tools && agent.tools.length > 0) {
      const toolsList = agent.tools.map(t => `- ${t}`).join('\n');
      parts.push(`Strumenti disponibili:\n${toolsList}`);
    }

    return parts.join('\n\n');
  }

  private resolveParameters(prompt: string, parameters: PromptLabParameter[]): string {
    let resolved = prompt;
    for (const param of parameters) {
      const regex = new RegExp(`\\{\\{${this.escapeRegex(param.name)}\\}\\}`, 'g');
      resolved = resolved.replace(regex, param.value);
    }
    return resolved;
  }

  // ---------------------------------------------------------------------------
  // Utilities
  // ---------------------------------------------------------------------------

  private generateUUID(): string {
    // Simple UUID v4 generator (no crypto dependency needed)
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  private escapeRegex(str: string): string {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }
}
