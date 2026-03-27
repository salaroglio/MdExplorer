import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { debounceTime, skip, takeUntil, filter, take } from 'rxjs/operators';
import { AiChatService } from '../../services/ai-chat.service';
import { PromptLabPersistenceService } from './promptlab-persistence.service';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import {
  PromptLabSession,
  PromptLabCard,
  PromptLabMode,
  AgentDefinition,
  DEFAULT_SYSTEM_PROMPT,
  DEFAULT_SEQUENCE_PROMPT,
  DEFAULT_WORKFLOW_PROMPT
} from '../models/promptlab.models';

@Injectable({ providedIn: 'root' })
export class PromptLabService implements OnDestroy {

  private _session$ = new BehaviorSubject<PromptLabSession | null>(null);
  session$ = this._session$.asObservable();

  /** Emits true while a save is in progress. */
  private _isSaving$ = new BehaviorSubject<boolean>(false);
  isSaving$ = this._isSaving$.asObservable();

  private destroy$ = new Subject<void>();

  /**
   * Counter that tracks how many emissions to skip for auto-save.
   * Incremented when loadSession() is called (the initial load should NOT
   * trigger a save back to disk).
   */
  private skipNextSave = 0;

  constructor(
    private aiChatService: AiChatService,
    private persistenceService: PromptLabPersistenceService,
    private http: HttpClient,
    private serverMessages: MdServerMessagesService
  ) {
    this.initAutoSave();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ---------------------------------------------------------------------------
  // Auto-save (Task 7.2)
  // ---------------------------------------------------------------------------

  private initAutoSave(): void {
    this._session$.pipe(
      takeUntil(this.destroy$),
      // Skip the very first null emission from BehaviorSubject constructor
      skip(1),
      // Only save if we have a session with a valid templatePath
      filter(session => {
        if (!session || !session.templatePath) return false;

        // If this emission came from loadSession(), skip it
        if (this.skipNextSave > 0) {
          this.skipNextSave--;
          return false;
        }
        return true;
      }),
      debounceTime(3000)
    ).subscribe(session => {
      if (session) {
        this.saveSessionToFile(session);
      }
    });
  }

  private saveSessionToFile(session: PromptLabSession): void {
    const markdown = this.persistenceService.saveTemplate(session);
    const connectionId = this.serverMessages.connectionId || '';
    const url = `/api/MdExplorerEditorReact/UpdateMarkdown?ConnectionId=${encodeURIComponent(connectionId)}`;

    this._isSaving$.next(true);

    this.http.post(url, {
      FilePath: session.templatePath,
      MarkdownContent: markdown
    }).subscribe({
      next: () => {
        console.log('[PromptLab] Auto-save completato per:', session.templatePath);
        this._isSaving$.next(false);
      },
      error: (err) => {
        console.error('[PromptLab] Errore auto-save:', err);
        this._isSaving$.next(false);
      }
    });
  }

  // ---------------------------------------------------------------------------
  // Session management
  // ---------------------------------------------------------------------------

  /**
   * Create a new empty session with the given title.
   */
  createSession(title: string): void {
    const session: PromptLabSession = {
      id: this.generateId(),
      title,
      model: '',
      mode: 'prompt',
      systemPrompt: DEFAULT_SYSTEM_PROMPT,
      systemPromptModel: '',
      sequencePrompt: DEFAULT_SEQUENCE_PROMPT,
      sequencePromptModel: '',
      workflowPrompt: DEFAULT_WORKFLOW_PROMPT,
      workflowPromptModel: '',
      agentDefinition: undefined,
      cards: [],
      createdAt: new Date(),
      updatedAt: new Date(),
      templatePath: ''
    };
    this._session$.next(session);
  }

  /**
   * Parse a markdown template and load it as the active session.
   */
  loadSession(markdown: string, templatePath: string): void {
    // Mark next emission to be skipped by auto-save
    this.skipNextSave++;
    const session = this.persistenceService.loadTemplate(markdown);
    session.templatePath = templatePath;
    this._session$.next(session);
  }

  // ---------------------------------------------------------------------------
  // Card management
  // ---------------------------------------------------------------------------

  /**
   * Add an empty card to the current session.
   */
  addCard(): void {
    const session = this.currentSession();
    if (!session) return;

    const newCard: PromptLabCard = {
      id: this.generateId(),
      generatedTitle: 'Nuova Card',
      parameters: [],
      distilledPrompt: '',
      conversation: []
    };

    session.cards = [...session.cards, newCard];
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  /**
   * Remove a card by id from the current session.
   * Also clears the channel history on the backend.
   */
  removeCard(cardId: string): void {
    const session = this.currentSession();
    if (!session) return;

    // Clear backend channel history for this card
    this.aiChatService.clearChannelHistory(this.bodyChannelId(cardId));
    this.aiChatService.clearChannelHistory(this.distillChannelId(cardId));

    session.cards = session.cards.filter(c => c.id !== cardId);
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  /**
   * Update a card in-place inside the current session.
   */
  updateCard(card: PromptLabCard): void {
    const session = this.currentSession();
    if (!session) return;

    const idx = session.cards.findIndex(c => c.id === card.id);
    if (idx === -1) return;

    session.cards[idx] = card;
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  /**
   * Push an externally modified session object to trigger auto-save.
   */
  updateSession(session: PromptLabSession): void {
    this._session$.next({ ...session });
  }

  // ---------------------------------------------------------------------------
  // Model & mode
  // ---------------------------------------------------------------------------

  setModel(model: string): void {
    const session = this.currentSession();
    if (!session) return;

    session.model = model;
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  setMode(mode: PromptLabMode): void {
    const session = this.currentSession();
    if (!session) return;

    session.mode = mode;
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  setSystemPrompt(prompt: string): void {
    const session = this.currentSession();
    if (!session) return;
    session.systemPrompt = prompt;
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  setSequencePrompt(prompt: string): void {
    const session = this.currentSession();
    if (!session) return;
    session.sequencePrompt = prompt;
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  setWorkflowPrompt(prompt: string): void {
    const session = this.currentSession();
    if (!session) return;
    session.workflowPrompt = prompt;
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  setAgentDefinition(def: AgentDefinition): void {
    const session = this.currentSession();
    if (!session) return;

    session.agentDefinition = def;
    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  // ---------------------------------------------------------------------------
  // Chat per card
  // ---------------------------------------------------------------------------

  /**
   * Ensure the hub has the right chat mode set for the current session model.
   * Must be called before sending any message.
   */
  async ensureChatModePublic(): Promise<void> {
    return this.ensureChatMode();
  }

  private async ensureChatMode(): Promise<void> {
    const session = this.currentSession();
    if (!session) return;

    // Default to claude-sonnet-4 if no model is set
    if (!session.model) {
      session.model = 'claude-sonnet-4';
    }

    const model = session.model.toLowerCase();

    // Local LLama models use the 'local' provider, everything else goes through copilotcli
    const provider = model.includes('llama') ? 'local' : 'copilotcli';
    const modelId = session.model;

    await this.aiChatService.setProviderAsync(provider, modelId);
  }

  /**
   * Send a message on the card's body channel.
   * The channelId follows the pattern `card-{cardId}-body`.
   *
   * On the FIRST message of a conversation (empty history on backend),
   * the session's system prompt is prepended so the LLM knows its role
   * is to help design prompts — not to execute instructions.
   */
  async sendCardMessage(cardId: string, message: string): Promise<void> {
    const session = this.currentSession();
    if (!session) return;

    const channelId = this.bodyChannelId(cardId);
    const card = session.cards.find(c => c.id === cardId);

    // Prepend system prompt on the first user message in this conversation.
    // We check the local conversation length: if it has only 1 message
    // (the one just pushed by the card component), it's the first turn.
    const isFirstMessage = card && card.conversation.length <= 1;
    let messageToSend = message;

    if (isFirstMessage) {
      const systemPrompt = session.systemPrompt || DEFAULT_SYSTEM_PROMPT;
      messageToSend = `[System Instructions]\n${systemPrompt}\n[End System Instructions]\n\n${message}`;
    }

    await this.ensureChatMode();
    this.aiChatService.sendMessageToChannel(messageToSend, channelId);
  }

  /**
   * Get the stream of events for a specific card's body channel.
   * Returns events with { type: 'chunk' | 'thinking' | 'complete' | 'error', data: any }.
   */
  getCardStream$(cardId: string): Observable<{ type: string; data: any }> {
    return this.aiChatService.getChannelStream$(this.bodyChannelId(cardId));
  }

  /**
   * Get the stream of events for a specific card's distillation channel.
   */
  getCardDistillStream$(cardId: string): Observable<{ type: string; data: any }> {
    return this.aiChatService.getChannelStream$(this.distillChannelId(cardId));
  }

  /**
   * Clear the backend conversation history for a card's body channel.
   */
  resetCardChat(cardId: string): void {
    this.aiChatService.clearChannelHistory(this.bodyChannelId(cardId));

    // Also clear the local conversation in the session model
    const session = this.currentSession();
    if (!session) return;

    const card = session.cards.find(c => c.id === cardId);
    if (card) {
      card.conversation = [];
      session.updatedAt = new Date();
      this._session$.next({ ...session });
    }
  }

  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------

  /**
   * Resolve parameters for a single card and return the ready-to-send prompt.
   */
  buildCard(cardId: string): string {
    const session = this.currentSession();
    if (!session) return '';

    const card = session.cards.find(c => c.id === cardId);
    if (!card) return '';

    return this.resolveParameters(card.distilledPrompt, card.parameters);
  }

  /**
   * Build the full session (all cards resolved) via the persistence service.
   */
  buildSession(): string {
    const session = this.currentSession();
    if (!session) return '';

    return this.persistenceService.buildSession(session);
  }

  // ---------------------------------------------------------------------------
  // Execute
  // ---------------------------------------------------------------------------

  /** Tracks which card IDs are currently executing (Play). */
  private _executingCards = new Set<string>();

  /** Emits true while executeAll() is running. */
  private _isExecutingAll$ = new BehaviorSubject<boolean>(false);
  isExecutingAll$ = this._isExecutingAll$.asObservable();

  /** Returns the full prompt that was sent to the LLM for a Play execution. */
  getLastSentPrompt(cardId: string): string | undefined {
    return this._lastSentPrompts.get(cardId);
  }
  private _lastSentPrompts = new Map<string, string>();

  /** Check if a card is currently executing. */
  isCardExecuting(cardId: string): boolean {
    return this._executingCards.has(cardId);
  }

  /**
   * Build a single card's prompt (with agent preamble if applicable),
   * resolve parameters, send it to the LLM, and track execution state.
   */
  executeCard(cardId: string): void {
    const session = this.currentSession();
    if (!session) return;

    const card = session.cards.find(c => c.id === cardId);
    if (!card) return;

    // Build the resolved prompt
    let builtPrompt = this.resolveParameters(card.distilledPrompt, card.parameters);

    // If agent mode, prepend the agent system prompt
    if (session.mode === 'agent' && session.agentDefinition) {
      const agentPreamble = this.persistenceService.buildAgentSystemPromptPublic(session.agentDefinition);
      if (agentPreamble) {
        builtPrompt = agentPreamble + '\n\n---\n\n' + builtPrompt;
      }
    }

    // Track execution state
    this._executingCards.add(cardId);
    this._lastSentPrompts.set(cardId, builtPrompt);

    // Ensure provider is set, then send to LLM
    this.ensureChatMode().then(() => {
      this.aiChatService.sendMessageToChannel(builtPrompt, this.bodyChannelId(cardId));
    });
  }

  /**
   * Called by the card component when a Play execution completes.
   * Creates the lastRun object and updates the session.
   */
  completeCardExecution(cardId: string, output: string, startTime: number): void {
    this._executingCards.delete(cardId);

    const session = this.currentSession();
    if (!session) return;

    const card = session.cards.find(c => c.id === cardId);
    if (!card) return;

    const promptSent = this._lastSentPrompts.get(cardId) || '';
    this._lastSentPrompts.delete(cardId);

    const resolvedParams: Record<string, string> = {};
    for (const p of card.parameters) {
      resolvedParams[p.name] = p.value;
    }

    card.lastRun = {
      executedAt: new Date(),
      duration: Date.now() - startTime,
      provider: session.model,
      model: session.model,
      resolvedParameters: resolvedParams,
      promptSent,
      output
    };

    session.updatedAt = new Date();
    this._session$.next({ ...session });
  }

  /**
   * Execute all cards sequentially, waiting for each to complete before
   * starting the next one.
   */
  executeAll(): void {
    const session = this.currentSession();
    if (!session || session.cards.length === 0) return;

    this._isExecutingAll$.next(true);
    this.executeCardsSequentially([...session.cards], 0);
  }

  private executeCardsSequentially(cards: PromptLabCard[], index: number): void {
    if (index >= cards.length) {
      this._isExecutingAll$.next(false);
      return;
    }

    const card = cards[index];
    const channelId = this.bodyChannelId(card.id);

    // Listen for completion on this card's channel, then proceed to next
    this.aiChatService.getChannelStream$(channelId).pipe(
      filter(e => e.type === 'complete' || e.type === 'error'),
      take(1)
    ).subscribe(() => {
      this.executeCardsSequentially(cards, index + 1);
    });

    this.executeCard(card.id);
  }

  // ---------------------------------------------------------------------------
  // Channel ID helpers
  // ---------------------------------------------------------------------------

  private bodyChannelId(cardId: string): string {
    return `card-${cardId}-body`;
  }

  private distillChannelId(cardId: string): string {
    return `card-${cardId}-distill`;
  }

  // ---------------------------------------------------------------------------
  // Private helpers
  // ---------------------------------------------------------------------------

  getCurrentModel(): string {
    return this.currentSession()?.model || 'claude-sonnet-4';
  }

  currentSession(): PromptLabSession | null {
    return this._session$.getValue();
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2, 5);
  }

  private resolveParameters(
    prompt: string,
    parameters: { name: string; value: string }[]
  ): string {
    let resolved = prompt;
    for (const param of parameters) {
      const regex = new RegExp(
        `\\{\\{${param.name.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\}\\}`,
        'g'
      );
      resolved = resolved.replace(regex, param.value);
    }
    return resolved;
  }
}
