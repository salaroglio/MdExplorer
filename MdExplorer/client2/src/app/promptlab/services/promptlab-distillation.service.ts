import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
import { AiChatService } from '../../services/ai-chat.service';
import {
  ChatMessage,
  DistillationResult,
  PromptLabParameter,
  ParameterType
} from '../models/promptlab.models';

@Injectable({ providedIn: 'root' })
export class PromptLabDistillationService implements OnDestroy {

  /** Per-card result streams */
  private resultSubjects = new Map<string, Subject<DistillationResult>>();

  /** Per-card debounce timers */
  private debounceTimers = new Map<string, any>();

  /** Per-card stream subscriptions (so we can cancel a running distillation) */
  private activeSubscriptions = new Map<string, Subscription>();

  private destroy$ = new Subject<void>();

  constructor(
    private aiChatService: AiChatService
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.debounceTimers.forEach(t => clearTimeout(t));
    this.activeSubscriptions.forEach(s => s.unsubscribe());
  }

  // ---------------------------------------------------------------------------
  // Public API
  // ---------------------------------------------------------------------------

  /**
   * Trigger distillation for a card. Debounced at 1500 ms.
   * If called again before the timer fires, the previous call is cancelled.
   */
  triggerDistillation(
    cardId: string,
    conversation: ChatMessage[],
    currentPrompt: string,
    model?: string
  ): void {
    // Cancel any pending debounce
    if (this.debounceTimers.has(cardId)) {
      clearTimeout(this.debounceTimers.get(cardId));
    }

    const timer = setTimeout(() => {
      this.debounceTimers.delete(cardId);
      this.executeDistillation(cardId, conversation, currentPrompt, model);
    }, 1500);

    this.debounceTimers.set(cardId, timer);
  }

  /**
   * Returns an Observable that emits whenever distillation completes for the
   * given card.
   */
  getDistillationResult$(cardId: string): Subject<DistillationResult> {
    if (!this.resultSubjects.has(cardId)) {
      this.resultSubjects.set(cardId, new Subject<DistillationResult>());
    }
    return this.resultSubjects.get(cardId)!;
  }

  /**
   * Clean up resources for a card that no longer exists.
   */
  disposeCard(cardId: string): void {
    if (this.debounceTimers.has(cardId)) {
      clearTimeout(this.debounceTimers.get(cardId));
      this.debounceTimers.delete(cardId);
    }
    if (this.activeSubscriptions.has(cardId)) {
      this.activeSubscriptions.get(cardId)!.unsubscribe();
      this.activeSubscriptions.delete(cardId);
    }
    if (this.resultSubjects.has(cardId)) {
      this.resultSubjects.get(cardId)!.complete();
      this.resultSubjects.delete(cardId);
    }
  }

  // ---------------------------------------------------------------------------
  // Private
  // ---------------------------------------------------------------------------

  private async executeDistillation(
    cardId: string,
    conversation: ChatMessage[],
    currentPrompt: string,
    model?: string
  ): Promise<void> {
    // Cancel any running distillation for this card
    if (this.activeSubscriptions.has(cardId)) {
      this.activeSubscriptions.get(cardId)!.unsubscribe();
      this.activeSubscriptions.delete(cardId);
    }

    const channelId = `card-${cardId}-distill`;

    // Clear previous distillation history on the backend
    this.aiChatService.clearChannelHistory(channelId);

    // Build the distillation prompt
    const distillationPrompt = this.buildDistillationPrompt(conversation, currentPrompt);

    // Collect the full response from the distillation channel
    let responseBuffer = '';

    const sub = this.aiChatService.getChannelStream$(channelId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        switch (event.type) {
          case 'chunk':
            responseBuffer += event.data;
            break;

          case 'complete':
            const result = this.parseDistillationResponse(responseBuffer);
            if (result) {
              this.getDistillationResult$(cardId).next(result);
            }
            responseBuffer = '';
            // Unsubscribe — one-shot per distillation
            if (this.activeSubscriptions.has(cardId)) {
              this.activeSubscriptions.get(cardId)!.unsubscribe();
              this.activeSubscriptions.delete(cardId);
            }
            break;

          case 'error':
            console.error(`[PromptLabDistillation] Error for card ${cardId}:`, event.data);
            responseBuffer = '';
            if (this.activeSubscriptions.has(cardId)) {
              this.activeSubscriptions.get(cardId)!.unsubscribe();
              this.activeSubscriptions.delete(cardId);
            }
            break;
        }
      });

    this.activeSubscriptions.set(cardId, sub);

    // Ensure chat mode is set before sending
    const provider = model?.toLowerCase().includes('llama') ? 'local' : 'copilotcli';
    const modelId = model || 'claude-sonnet-4';
    await this.aiChatService.setProviderAsync(provider, modelId);

    // Send the distillation request to the LLM
    this.aiChatService.sendMessageToChannel(distillationPrompt, channelId);
  }

  private buildDistillationPrompt(
    conversation: ChatMessage[],
    currentPrompt: string
  ): string {
    const conversationText = conversation
      .map(m => {
        // Strip the [System Instructions]...[End System Instructions] block
        // that gets prepended to the first user message — the distillation
        // LLM has its own instructions and doesn't need the chat system prompt.
        let content = m.content;
        if (m.role === 'user') {
          content = content.replace(
            /\[System Instructions\][\s\S]*?\[End System Instructions\]\s*/,
            ''
          ).trim();
        }
        return `[${m.role.toUpperCase()}]: ${content}`;
      })
      .filter(line => line.length > 0)
      .join('\n\n');

    return `You are a prompt refinement assistant. The user is having a conversation with an LLM to iteratively build and improve a prompt. Your job is to produce an updated version of the prompt that incorporates all the refinements discussed in the conversation.

Rules:
- Start from the current prompt (below) and apply the changes discussed in the conversation.
- If there is no current prompt, create one from scratch based on the conversation.
- The prompt must be standalone and executable without the conversation context.
- Use {{paramName}} syntax for variable parts (input files, output directories, configurable values).
- For each parameter, specify its type: "file" (a document to read), "directory" (an output location), or "text" (a free-form value).
- Generate a short title (max 6 words) describing what the prompt does.
- Write the prompt in the same language the user is using in the conversation.

Respond in this EXACT format (no extra text outside the markers):
---TITLE---
[title here]
---PARAMETERS---
[paramName]|[type]
[paramName]|[type]
---PROMPT---
[the updated prompt with {{paramName}} placeholders]

Current prompt (if any):
${currentPrompt || '(none)'}

Conversation:
${conversationText}`;
  }

  /**
   * Parse the LLM response using the ---TITLE--- / ---PARAMETERS--- / ---PROMPT--- markers.
   * Handles extra whitespace and minor formatting variations.
   */
  private parseDistillationResponse(raw: string): DistillationResult | null {
    try {
      const titleMatch = raw.match(/---TITLE---\s*([\s\S]*?)\s*---PARAMETERS---/i);
      const paramsMatch = raw.match(/---PARAMETERS---\s*([\s\S]*?)\s*---PROMPT---/i);
      const promptMatch = raw.match(/---PROMPT---\s*([\s\S]*)/i);

      if (!titleMatch || !promptMatch) {
        console.warn('[PromptLabDistillation] Could not parse response — markers not found.');
        return null;
      }

      const generatedTitle = titleMatch[1].trim();
      const distilledPrompt = promptMatch[1].trim();

      const parameters: PromptLabParameter[] = [];
      if (paramsMatch) {
        const paramLines = paramsMatch[1]
          .split('\n')
          .map(l => l.trim())
          .filter(l => l.length > 0);

        for (const line of paramLines) {
          const parts = line.split('|').map(p => p.trim());
          if (parts.length >= 2) {
            const paramType = this.normalizeParamType(parts[1]);
            parameters.push({
              name: parts[0],
              value: '',
              type: paramType
            });
          }
        }
      }

      return { distilledPrompt, parameters, generatedTitle };
    } catch (err) {
      console.error('[PromptLabDistillation] Parse error:', err);
      return null;
    }
  }

  private normalizeParamType(raw: string): ParameterType {
    const lower = raw.toLowerCase();
    if (lower === 'file') return 'file';
    if (lower === 'directory' || lower === 'dir' || lower === 'folder') return 'directory';
    return 'text';
  }
}
