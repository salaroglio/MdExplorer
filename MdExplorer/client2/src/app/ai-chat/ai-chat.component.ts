import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Input } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { AiChatService, ChatMessage } from '../services/ai-chat.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Router } from '@angular/router';
import { marked } from 'marked';
import { LayoutService } from '../md-explorer/services/layout.service';
import { TranslateService } from '@ngx-translate/core';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { ProjectSettingsService } from '../projects/services/project-settings.service';

type CopilotModelChoice = { id: string; name: string; unavailable: boolean };
type ClaudeModel = { id: string; name: string; description: string | null };
type ClaudeModelChoice = ClaudeModel & { unavailable: boolean };

/** Consumi di Copilot, come li manda `ReceiveCopilotUsage`. */
type CopilotUsage = {
  quotaType: string | null; quotaUnlimited: boolean;
  quotaUsedPercent: number | null; quotaUsed: number | null; quotaEntitlement: number | null; quotaResetDate: string | null;
  sessionRequests: number; sessionPercent: number | null;
  contextTokens: number | null; contextLimit: number | null; contextPercent: number | null;
};

@Component({
  selector: 'app-ai-chat',
  templateUrl: './ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss']
})
export class AiChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('scrollContainer', { static: false }) private scrollContainer: ElementRef;
  @ViewChild('messageInput', { static: false }) private messageInput: ElementRef;
  @ViewChild('modelManagerPanel', { static: false }) private modelManagerPanel: ElementRef;
  @Input() compactMode: boolean = false;
  
  messages: ChatMessage[] = [];
  inputMessage = '';
  isModelLoaded = false;
  isConfiguringProvider = false;
  currentModel: string | null = null;
  currentDocument: string | null = null;
  showModelManager = false;
  isChatFullScreen = false;
  copilotCliUnavailable = false;
  isStreaming = false;
  // Cosa sta facendo l'agente adesso, mentre risponde. null fuori da un turno.
  toolActivity: string | null = null;

  // Copilot CLI auto-select: when active, expose a Sonnet 5 / Opus 4.7 picker
  // next to the injected-file chip. Hidden for every other provider.
  copilotCliAutoSelected = false;
  selectedCopilotModel: string | null = null;
  // I modelli che QUESTA installazione ha davvero, da models.list (tabella AvailableModel).
  // Prima qui c'erano due modelli scritti a mano, 'claude-sonnet-5' e 'claude-opus-4.7': su
  // un'installazione che non li ha, il CLI risponde con un altro modello senza dirlo.
  copilotModels: { id: string; name: string }[] = [];
  private copilotModelChoicesCache: {
    models: { id: string; name: string }[];
    selected: string | null;
    choices: CopilotModelChoice[];
  } | null = null;
  copilotModelsLoading = false;
  copilotModelsError: string | null = null;

  // Claude Code auto-select: stessa riga del modello di Copilot. L'elenco è quello che il CLI
  // dichiara per questo account (initialize, salvato in AvailableModel): prima qui c'erano tre
  // chip scritte a mano, e mancavano Default e Fable.
  claudeCodeUnavailable = false;
  claudeCodeAutoSelected = false;
  /** opencode è il motore di questo progetto e il CLI c'è. */
  openCodeAutoSelected = false;
  /** …è il motore, ma opencode non è installato su questa macchina: la chat resta bloccata. */
  openCodeUnavailable = false;
  selectedOpenCodeModel: string | null = null;
  selectedClaudeCodeModel: string | null = null;
  claudeModels: ClaudeModel[] = [];
  private claudeModelChoicesCache: {
    models: ClaudeModel[];
    selected: string | null;
    choices: ClaudeModelChoice[];
  } | null = null;
  claudeModelsLoading = false;
  claudeModelsError: string | null = null;

  /**
   * Consuntivo dell'ultimo turno di Claude Code: costo del turno, cumulato della sessione,
   * token e quanta parte delle finestre a 5 ore e 7 giorni è già bruciata.
   *
   * È il dato che con Copilot ACP semplicemente non esiste — là il risultato del prompt
   * porta solo "end_turn" — ed è il motivo per cui l'indicatore consumi era stato
   * accantonato. Qui arriva da solo a fine turno.
   */
  claudeUsage: {
    turnCostUsd: number | null; sessionCostUsd: number | null;
    inputTokens: number | null; outputTokens: number | null; thinkingTokens: number | null;
    durationMs: number | null; model: string | null;
    fiveHourUtilization: number | null; sevenDayUtilization: number | null;
  } | null = null;

  /** Consumi di Copilot accanto alla combo del modello; null finché il backend non li manda. */
  copilotUsage: CopilotUsage | null = null;

  // Edit message state
  editingMessageId: string | null = null;
  editedContent: string = '';

  // Copy message state
  copiedMessageId: string | null = null;

  // Thinking section collapse state (per message id)
  collapsedThinking: Set<string> = new Set();

  private destroy$ = new Subject<void>();
  private shouldScrollToBottom = false;
  // Pixels from the bottom under which the list counts as "at the bottom".
  private static readonly SCROLL_BOTTOM_THRESHOLD = 40;
  // Set on the first messages$ emission after (re)creation: the tab was just
  // (re)opened, so restore the last scroll position instead of snapping down.
  private shouldRestoreScroll = false;
  private isFirstMessagesEmission = true;

  constructor(
    private aiService: AiChatService,
    private router: Router,
    private sanitizer: DomSanitizer,
    private layoutService: LayoutService,
    private translate: TranslateService,
    private projectsService: ProjectsService,
    private projectSettingsService: ProjectSettingsService
  ) {}

  ngOnInit(): void {
    
    // Subscribe to messages
    this.aiService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(messages => {
        this.messages = messages;
        if (this.isFirstMessagesEmission) {
          // Component just (re)created for this tab: restore where the user
          // left off. If they were parked at the bottom, restoring lands there
          // anyway, so the last-message-visible behavior is preserved.
          this.isFirstMessagesEmission = false;
          this.shouldRestoreScroll = true;
        } else {
          // A genuine new/updated message during this session: keep following it.
          this.shouldScrollToBottom = true;
        }
      });

    // Track streaming state to toggle the Send/Stop button.
    this.aiService.toolActivity$
      .pipe(takeUntil(this.destroy$))
      .subscribe(activity => this.toolActivity = activity);

    this.aiService.isStreaming$
      .pipe(takeUntil(this.destroy$))
      .subscribe(streaming => this.isStreaming = streaming);
    
    // Subscribe to model status
    this.aiService.isModelLoaded$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loaded => {
        console.log('[AiChatComponent] Model loaded status changed:', loaded);
        this.isModelLoaded = loaded;
        // Focus input when model is loaded
        if (loaded) {
          this.focusInput();
        }
      });
    
    // Subscribe to current model
    this.aiService.currentModel$
      .pipe(takeUntil(this.destroy$))
      .subscribe(model => {
        console.log('[AiChatComponent] Current model changed:', model);
        this.currentModel = model;
      });

    // Subscribe to provider configuration state (spinner while SetChatMode is in flight)
    this.aiService.isConfiguringProvider$
      .pipe(takeUntil(this.destroy$))
      .subscribe(configuring => {
        this.isConfiguringProvider = configuring;
      });

    // Subscribe to current document
    this.aiService.currentDocument$
      .pipe(takeUntil(this.destroy$))
      .subscribe(doc => {
        console.log('[AiChatComponent] Current document changed:', doc);
        this.currentDocument = doc;
      });

    // Subscribe to fullscreen state
    this.layoutService.chatFullScreen$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isFullScreen => {
        this.isChatFullScreen = isFullScreen;
      });

    // Listen for TruncateMessagesAfter event from backend
    this.aiService['hubConnection'].on('TruncateMessagesAfter', (messageIndex: number) => {
      console.log('[AiChatComponent] Truncating messages after index:', messageIndex);
      // Keep only messages up to and including messageIndex
      this.messages = this.messages.slice(0, messageIndex + 1);
      this.shouldScrollToBottom = true;
    });

    // Honor the per-project "Use Copilot CLI automatically" flag emitted by ProjectsService
    // right after SetFolderProject returns. When the flag is ON and the CLI is installed,
    // silently switch the chat to Copilot CLI. When the flag is ON but the CLI is missing,
    // lock the chat and surface a banner so the user can pick a different model.
    this.projectsService.copilotCliAutoConfig$
      .pipe(takeUntil(this.destroy$))
      .subscribe(config => {
        if (!config) {
          this.copilotCliUnavailable = false;
          this.copilotCliAutoSelected = false;
          this.selectedCopilotModel = null;
          return;
        }
        if (config.autoSelect && config.available) {
          // Il modello scelto per il progetto; null = lo sceglie il CLI. Qui c'era un
          // "|| 'claude-sonnet-5'", uno dei quattro default inventati lungo la strada: su
          // un'installazione che non ha quel modello il CLI ne usa un altro senza dirlo.
          const model = config.defaultModel || null;
          console.log('[AiChatComponent] Auto-selecting Copilot CLI with model:', model);
          this.copilotCliUnavailable = false;
          this.copilotCliAutoSelected = true;
          this.selectedCopilotModel = model;
          this.aiService.setProvider('copilotcli', model);
          this.aiService.notifyCopilotCliConnected(model);
          this.loadCopilotModels();
        } else if (config.autoSelect && !config.available) {
          console.log('[AiChatComponent] Copilot CLI auto-select enabled but CLI not available — locking chat');
          this.copilotCliUnavailable = true;
          this.copilotCliAutoSelected = false;
          this.selectedCopilotModel = null;
          this.aiService.notifyCopilotCliDisconnected();
        } else {
          this.copilotCliUnavailable = false;
          this.copilotCliAutoSelected = false;
          this.selectedCopilotModel = null;
        }
      });

    // Terza gemella, per opencode. Il backend garantisce che al massimo UNO dei tre
    // auto-select arrivi acceso — il motore del progetto è uno solo — quindi queste
    // sottoscrizioni non si contendono la chat.
    this.projectsService.openCodeAutoConfig$
      .pipe(takeUntil(this.destroy$))
      .subscribe(config => {
        if (!config) {
          this.openCodeUnavailable = false;
          this.openCodeAutoSelected = false;
          this.selectedOpenCodeModel = null;
          return;
        }
        if (config.autoSelect && config.available) {
          // null = lo sceglie il server: nessun nome inventato qui.
          const model = config.defaultModel || null;
          console.log('[AiChatComponent] opencode come motore, modello:', model ?? '(default del server)');
          this.openCodeUnavailable = false;
          this.openCodeAutoSelected = true;
          this.selectedOpenCodeModel = model;
          this.aiService.setProvider('opencode', model);
          this.aiService.notifyOpenCodeConnected(model);
        } else if (config.autoSelect && !config.available) {
          console.log('[AiChatComponent] opencode è il motore del progetto ma non è installato — chat bloccata');
          this.openCodeUnavailable = true;
          this.openCodeAutoSelected = false;
          this.selectedOpenCodeModel = null;
          this.aiService.notifyOpenCodeDisconnected();
        } else {
          this.openCodeUnavailable = false;
          this.openCodeAutoSelected = false;
          this.selectedOpenCodeModel = null;
        }
      });

    // Stessa manopola, per Claude Code. Il backend garantisce che al massimo UNO dei due
    // auto-select arrivi acceso, quindi queste due sottoscrizioni non si contendono la chat.
    this.projectsService.claudeCodeAutoConfig$
      .pipe(takeUntil(this.destroy$))
      .subscribe(config => {
        if (!config) {
          this.claudeCodeUnavailable = false;
          this.claudeCodeAutoSelected = false;
          this.selectedClaudeCodeModel = null;
          return;
        }
        if (config.autoSelect && config.available) {
          const model = config.defaultModel || 'sonnet';
          console.log('[AiChatComponent] Auto-selecting Claude Code with model:', model);
          this.claudeCodeUnavailable = false;
          this.claudeCodeAutoSelected = true;
          this.selectedClaudeCodeModel = model;
          this.aiService.setProvider('claudecode', model);
          this.aiService.notifyClaudeCodeConnected(model);
          this.loadClaudeModels();
        } else if (config.autoSelect && !config.available) {
          console.log('[AiChatComponent] Claude Code auto-select acceso ma CLI non disponibile — chat bloccata');
          this.claudeCodeUnavailable = true;
          this.claudeCodeAutoSelected = false;
          this.selectedClaudeCodeModel = null;
          this.aiService.notifyClaudeCodeDisconnected();
        } else {
          this.claudeCodeUnavailable = false;
          this.claudeCodeAutoSelected = false;
          this.selectedClaudeCodeModel = null;
        }
      });

    // Consuntivo di fine turno. Arriva solo da Claude Code: con gli altri provider resta
    // null e la riga non compare.
    this.aiService.claudeUsage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(usage => { this.claudeUsage = usage; });

    this.aiService.copilotUsage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(usage => { this.copilotUsage = usage; });
  }

  ngAfterViewChecked(): void {
    if (this.shouldRestoreScroll && this.scrollContainer) {
      // Restore the position captured before the tab was left. Restore wins
      // over the initial shouldScrollToBottom so reopening the tab no longer
      // jumps the conversation to the top.
      this.shouldRestoreScroll = false;
      this.shouldScrollToBottom = false;
      this.restoreScrollPosition();
      return;
    }
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  /** Persist the current scroll position so it survives a tab switch. */
  onChatScroll(): void {
    const el = this.scrollContainer?.nativeElement;
    // offsetParent is null while the tab body is detached/hidden; ignore those
    // spurious scroll(0) events so we don't overwrite the saved position.
    if (!el || el.offsetParent === null) return;
    const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
    this.aiService.savedScrollTop = el.scrollTop;
    this.aiService.savedAtBottom = distanceFromBottom <= AiChatComponent.SCROLL_BOTTOM_THRESHOLD;
  }

  private restoreScrollPosition(): void {
    try {
      const el = this.scrollContainer.nativeElement;
      if (this.aiService.savedAtBottom) {
        el.scrollTop = el.scrollHeight;
      } else {
        el.scrollTop = this.aiService.savedScrollTop;
      }
    } catch (err) {}
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  sendMessage(): void {
    if (!this.inputMessage.trim() || !this.isModelLoaded || this.isConfiguringProvider) return;
    // A prompt is already streaming: refuse to fire a second turn. Sending now would
    // reassign currentStreamingMessageId (corrupting the in-flight bubble) and start a
    // second session/prompt on top of the live one — killing any running sub-agent.
    // To interrupt, the user must press Stop, which cancels gracefully.
    if (this.isStreaming) return;

    this.aiService.sendMessage(this.inputMessage);
    this.inputMessage = '';
    this.focusInput();
  }

  /** Abort the prompt currently streaming (Stop button). */
  stopStreaming(): void {
    this.aiService.cancelPrompt();
  }

  handleKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  newSession(): void {
    this.aiService.clearMessages();
  }

  toggleFullScreen(): void {
    this.layoutService.setChatFullScreen(!this.isChatFullScreen);
  }

  /**
   * Le voci della combobox. Se il modello salvato per il progetto non e' piu' nell'elenco (tolto
   * dall'abbonamento, o scelto su un'altra installazione) resta visibile e segnalato: sparire in
   * silenzio lascerebbe la combobox vuota proprio mentre il CLI risponde con qualcos'altro.
   *
   * Lo stesso array finche' elenco e scelta non cambiano. Un array nuovo a ogni lettura faceva
   * ricreare a *ngFor tutte le mat-option a ogni change detection; mat-select, vedendo cambiare
   * le sue opzioni, ne programmava un'altra, e cosi' via: pagina bloccata appena aperto un
   * progetto con Copilot CLI (visto il 10/09/26 in Chrome, stack su questo template).
   */
  get copilotModelChoices(): CopilotModelChoice[] {
    const cache = this.copilotModelChoicesCache;
    if (cache && cache.models === this.copilotModels && cache.selected === this.selectedCopilotModel) {
      return cache.choices;
    }
    const choices: CopilotModelChoice[] = this.copilotModels.map(m => ({ ...m, unavailable: false }));
    if (this.selectedCopilotModel && !choices.some(c => c.id === this.selectedCopilotModel)) {
      choices.unshift({ id: this.selectedCopilotModel, name: this.selectedCopilotModel, unavailable: true });
    }
    this.copilotModelChoicesCache = { models: this.copilotModels, selected: this.selectedCopilotModel, choices };
    return choices;
  }

  trackCopilotModel(_: number, choice: CopilotModelChoice): string {
    return choice.id;
  }

  /**
   * Legge i modelli salvati (istantaneo). Se non ce n'e' nessuno, e' la prima volta su questa
   * installazione: li scopre chiedendoli al CLI.
   */
  loadCopilotModels(): void {
    this.copilotModelsLoading = true;
    this.copilotModelsError = null;
    this.aiService.getCopilotChatModels().subscribe({
      next: models => {
        if (models.length) {
          this.copilotModels = models;
          this.copilotModelsLoading = false;
        } else {
          this.refreshCopilotModels();
        }
      },
      error: err => this.failCopilotModels(err)
    });
  }

  /** Chiede l'elenco al CLI (models.list, ~2 s, nessun credito) e lo salva. */
  refreshCopilotModels(): void {
    this.copilotModelsLoading = true;
    this.copilotModelsError = null;
    this.aiService.refreshCopilotCliModels().subscribe({
      next: () => this.aiService.getCopilotChatModels().subscribe({
        next: models => {
          this.copilotModels = models;
          this.copilotModelsLoading = false;
        },
        error: err => this.failCopilotModels(err)
      }),
      error: err => this.failCopilotModels(err)
    });
  }

  private failCopilotModels(err: any): void {
    this.copilotModelsLoading = false;
    this.copilotModelsError = err?.error?.error || err?.message || String(err);
    console.error('[AiChatComponent] Elenco modelli Copilot non disponibile:', this.copilotModelsError);
  }

  async selectCopilotModel(modelId: string): Promise<void> {
    if (!this.copilotCliAutoSelected) return;
    if (this.selectedCopilotModel === modelId) return;
    if (this.isConfiguringProvider) return;
    console.log('[AiChatComponent] User switched Copilot model to:', modelId);
    this.selectedCopilotModel = modelId;
    try {
      await this.aiService.setProviderAsync('copilotcli', modelId);
      this.aiService.notifyCopilotCliConnected(modelId);
    } catch (err) {
      console.error('[AiChatComponent] Failed to switch Copilot model:', err);
      return;
    }

    // La scelta vale per il progetto: alla prossima apertura la chat riparte da questo modello.
    // Il cambio vero avviene alla domanda successiva, sulla sessione viva e senza perdere la
    // conversazione (SetModelAsync, lato backend).
    const projectPath = this.projectsService.currentProjects$.getValue()?.path;
    if (!projectPath) {
      console.warn('[AiChatComponent] Nessun progetto aperto: il modello scelto non viene ricordato');
      return;
    }
    this.projectSettingsService.setCopilotChatModelSetting(modelId, projectPath).subscribe({
      next: () => console.log('[AiChatComponent] Modello Copilot salvato per il progetto:', modelId),
      error: err => console.error('[AiChatComponent] Salvataggio del modello per il progetto fallito:', err)
    });
  }

  /**
   * Le voci della combo di Claude Code. Stesse regole di copilotModelChoices: stesso array finché
   * elenco e scelta non cambiano (un array nuovo a ogni lettura blocca mat-select), e il modello
   * salvato che non è più nell'elenco resta visibile e segnalato. Segnalato solo quando l'elenco
   * c'è: mentre si carica, «non più disponibile» sarebbe falso.
   */
  get claudeModelChoices(): ClaudeModelChoice[] {
    const cache = this.claudeModelChoicesCache;
    if (cache && cache.models === this.claudeModels && cache.selected === this.selectedClaudeCodeModel) {
      return cache.choices;
    }
    const choices: ClaudeModelChoice[] = this.claudeModels.map(m => ({ ...m, unavailable: false }));
    const selected = this.selectedClaudeCodeModel;
    if (selected && !choices.some(c => c.id === selected)) {
      choices.unshift({ id: selected, name: selected, description: null, unavailable: this.claudeModels.length > 0 });
    }
    this.claudeModelChoicesCache = { models: this.claudeModels, selected, choices };
    return choices;
  }

  trackClaudeModel(_: number, choice: ClaudeModelChoice): string {
    return choice.id;
  }

  /** Descrizione del modello scelto (quale modello c'è dietro «Default»), per il tooltip della combo. */
  get selectedClaudeModelDescription(): string {
    return this.claudeModels.find(m => m.id === this.selectedClaudeCodeModel)?.description || '';
  }

  /** Modelli salvati (istantaneo); se non ce ne sono, li chiede al CLI (initialize, ~1 s, nessun token). */
  loadClaudeModels(): void {
    this.claudeModelsLoading = true;
    this.claudeModelsError = null;
    this.aiService.getClaudeCodeChatModels().subscribe({
      next: models => {
        if (models.length) {
          this.claudeModels = models;
          this.claudeModelsLoading = false;
        } else {
          this.refreshClaudeModels();
        }
      },
      error: err => this.failClaudeModels(err)
    });
  }

  refreshClaudeModels(): void {
    this.claudeModelsLoading = true;
    this.claudeModelsError = null;
    this.aiService.refreshClaudeCodeModels().subscribe({
      next: () => this.aiService.getClaudeCodeChatModels().subscribe({
        next: models => {
          this.claudeModels = models;
          this.claudeModelsLoading = false;
        },
        error: err => this.failClaudeModels(err)
      }),
      error: err => this.failClaudeModels(err)
    });
  }

  private failClaudeModels(err: any): void {
    this.claudeModelsLoading = false;
    this.claudeModelsError = err?.error?.error || err?.message || String(err);
    console.error('[AiChatComponent] Elenco modelli Claude Code non disponibile:', this.claudeModelsError);
  }

  async selectClaudeCodeModel(modelId: string): Promise<void> {
    if (!this.claudeCodeAutoSelected) return;
    if (this.selectedClaudeCodeModel === modelId) return;
    if (this.isConfiguringProvider) return;
    console.log('[AiChatComponent] Modello Claude Code cambiato in:', modelId);
    this.selectedClaudeCodeModel = modelId;
    try {
      // Il cambio vero avviene alla domanda successiva, sulla sessione viva (set_model): la
      // conversazione resta, e con lei il consuntivo della sessione, che quindi non si azzera.
      await this.aiService.setProviderAsync('claudecode', modelId);
      this.aiService.notifyClaudeCodeConnected(modelId);
    } catch (err) {
      console.error('[AiChatComponent] Cambio di modello Claude Code fallito:', err);
      return;
    }

    // La scelta vale per il progetto: alla prossima apertura MarkAgent riparte da questo modello.
    const projectPath = this.projectsService.currentProjects$.getValue()?.path;
    if (!projectPath) {
      console.warn('[AiChatComponent] Nessun progetto aperto: il modello scelto non viene ricordato');
      return;
    }
    this.projectSettingsService.setClaudeCodeChatModelSetting(modelId, projectPath).subscribe({
      next: () => console.log('[AiChatComponent] Modello Claude Code salvato per il progetto:', modelId),
      error: err => console.error('[AiChatComponent] Salvataggio del modello Claude Code per il progetto fallito:', err)
    });
  }

  /** Costo in dollari, con abbastanza decimali da non diventare "0,00" su un turno breve. */
  formatUsd(value: number | null): string {
    if (value == null) return '—';
    return '$' + value.toFixed(value < 0.01 ? 4 : 2);
  }

  /** Percentuale già in 0..100, con i separatori della lingua del browser ("9,8%"). */
  formatCopilotPercent(value: number | null, decimals = 1): string {
    if (value == null) return '—';
    return value.toLocaleString(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals }) + '%';
  }

  formatCount(value: number | null): string {
    return value == null ? '—' : value.toLocaleString();
  }

  formatDay(iso: string | null): string {
    return iso ? new Date(iso).toLocaleDateString() : '—';
  }

  /** Frazione 0..1 → percentuale intera. */
  formatPercent(value: number | null): string {
    if (value == null) return '—';
    return Math.round(value * 100) + '%';
  }

  toggleModelManager(): void {
    if (this.compactMode && !this.isChatFullScreen) {
      // In compact mode (not fullscreen), navigate to the model manager in the main router outlet
      this.router.navigate(['/main/navigation/ai-model-manager']);
    } else {
      // In full mode or fullscreen, toggle the embedded model manager
      this.showModelManager = !this.showModelManager;
    }
  }
  
  onModelManagerContentChanged(): void {
    // Event listener for content changes - currently no action needed
  }

  private scrollToBottom(): void {
    try {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    } catch(err) {}
  }

  private focusInput(): void {
    setTimeout(() => {
      if (this.messageInput) {
        this.messageInput.nativeElement.focus();
      }
    }, 100);
  }

  formatMessageContent(content: string): SafeHtml {
    if (!content) return '';
    const html = marked.parse(content, { breaks: true }) as string;
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  getMessageClass(message: ChatMessage): string {
    return `message message-${message.role}`;
  }

  getAvatarIcon(role: string): string {
    switch (role) {
      case 'user':
        return 'person';
      case 'assistant':
        return 'smart_toy';
      case 'system':
        return 'info';
      default:
        return 'chat';
    }
  }

  getCurrentDocumentName(): string {
    if (!this.currentDocument) return '';
    // Extract filename from path (works for both / and \\ separators)
    const parts = this.currentDocument.replace(/\\/g, '/').split('/');
    return parts[parts.length - 1];
  }

  toggleThinking(messageId: string): void {
    if (this.collapsedThinking.has(messageId)) {
      this.collapsedThinking.delete(messageId);
    } else {
      this.collapsedThinking.add(messageId);
    }
  }

  isThinkingCollapsed(messageId: string): boolean {
    return this.collapsedThinking.has(messageId);
  }

  hasThinking(message: ChatMessage): boolean {
    return !!message.thinkingContent && message.thinkingContent.trim().length > 0;
  }

  copyMessageContent(message: ChatMessage): void {
    if (!message.content) return;
    navigator.clipboard.writeText(message.content).then(() => {
      this.copiedMessageId = message.id;
      setTimeout(() => {
        this.copiedMessageId = null;
      }, 2000);
    });
  }

  startEditMessage(message: ChatMessage): void {
    console.log('[AiChatComponent] Starting edit for message:', message.id);
    this.editingMessageId = message.id;
    this.editedContent = message.content;
  }

  cancelEditMessage(): void {
    console.log('[AiChatComponent] Canceling edit');
    this.editingMessageId = null;
    this.editedContent = '';
  }

  saveEditedMessage(message: ChatMessage): void {
    if (!this.editedContent.trim()) {
      console.log('[AiChatComponent] Cannot save empty message');
      return;
    }

    console.log('[AiChatComponent] Saving edited message:', message.id);

    // Find the index of this message in the messages array
    const messageIndex = this.messages.findIndex(m => m.id === message.id);

    if (messageIndex === -1) {
      console.error('[AiChatComponent] Message not found in messages array');
      return;
    }

    // Update the message content locally
    this.messages[messageIndex].content = this.editedContent;

    // Call backend to regenerate from this point
    // The backend uses the conversation history index, which matches our messages array index
    this.aiService.editAndRegenerateMessage(messageIndex, this.editedContent);

    // Reset edit state
    this.editingMessageId = null;
    this.editedContent = '';

    // Create placeholder for new assistant response
    const assistantMessageId = this.generateMessageId();
    this.messages.push({
      id: assistantMessageId,
      role: 'assistant',
      content: '',
      timestamp: new Date(),
      isStreaming: true
    });

    this.shouldScrollToBottom = true;
  }

  private generateMessageId(): string {
    return `msg-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}