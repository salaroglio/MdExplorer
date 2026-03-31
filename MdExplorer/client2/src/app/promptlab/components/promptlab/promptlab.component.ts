import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
import { PromptLabMode, PromptLabCard, AgentDefinition, DEFAULT_SYSTEM_PROMPT, DEFAULT_SEQUENCE_PROMPT, DEFAULT_WORKFLOW_PROMPT } from '../../models/promptlab.models';
import { PromptLabService } from '../../services/promptlab.service';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { AiChatService } from '../../../services/ai-chat.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-promptlab',
  templateUrl: './promptlab.component.html',
  styleUrls: ['./promptlab.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PromptLabComponent implements OnInit, OnDestroy {

  mode: PromptLabMode = 'prompt';
  selectedModel = 'claude-sonnet-4';
  sessionTitle = '';
  cards: PromptLabCard[] = [];
  templateName = 'template.md';

  agentDefinition: AgentDefinition = {
    identity: '',
    objectives: '',
    rules: '',
    tools: []
  };

  /** Build overlay state (Task 8.1) */
  buildOutput: string | null = null;
  buildCopied = false;

  /** Execute All state (Task 8.3) */
  isExecutingAll = false;

  /** Settings panel */
  showSettings = false;
  systemPrompt = DEFAULT_SYSTEM_PROMPT;
  systemPromptModel = '';
  sequencePrompt = DEFAULT_SEQUENCE_PROMPT;
  sequencePromptModel = '';
  workflowPrompt = DEFAULT_WORKFLOW_PROMPT;
  workflowPromptModel = '';

  models: { value: string; label: string }[] = [];
  isLoadingModels = true;

  private destroy$ = new Subject<void>();

  constructor(
    private cdr: ChangeDetectorRef,
    private http: HttpClient,
    private promptLabService: PromptLabService,
    private mdFileService: MdFileService,
    private aiChatService: AiChatService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // Set default session title using translate
    if (!this.sessionTitle) {
      this.sessionTitle = this.translate.instant('PROMPTLAB.NEW_SESSION');
    }

    // 1. Subscribe to session$ to keep local state in sync
    this.promptLabService.session$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(session => {
      if (session) {
        this.sessionTitle = session.title || this.translate.instant('PROMPTLAB.NEW_SESSION');
        this.selectedModel = session.model || 'gpt-4o';
        this.mode = session.mode || 'prompt';
        this.cards = session.cards || [];
        this.templateName = session.templatePath
          ? session.templatePath.split(/[/\\]/).pop() || 'template.md'
          : 'template.md';
        this.systemPrompt = session.systemPrompt || DEFAULT_SYSTEM_PROMPT;
        this.sequencePrompt = session.sequencePrompt || DEFAULT_SEQUENCE_PROMPT;
        this.workflowPrompt = session.workflowPrompt || DEFAULT_WORKFLOW_PROMPT;
        this.systemPromptModel = session.systemPromptModel || '';
        this.sequencePromptModel = session.sequencePromptModel || '';
        this.workflowPromptModel = session.workflowPromptModel || '';
        this.agentDefinition = session.agentDefinition || {
          identity: '',
          objectives: '',
          rules: '',
          tools: []
        };
      }
      this.cdr.markForCheck();
    });

    // 1b. Subscribe to executeAll state
    this.promptLabService.isExecutingAll$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(val => {
      this.isExecutingAll = val;
      this.cdr.markForCheck();
    });

    // 2. When a file is selected in the tree, load its content as a PromptLab session
    //    BUT skip if the file is already loaded (avoids wiping transient state
    //    like conversation[] when auto-save triggers a FileSystemWatcher event)
    this.mdFileService.selectedMdFileFromSideNav.pipe(
      takeUntil(this.destroy$),
      filter(file => !!file && !!file.fullPath),
      filter(file => {
        const currentSession = this.promptLabService.currentSession();
        return !currentSession || currentSession.templatePath !== file.fullPath;
      })
    ).subscribe(file => {
      this.loadFileAsSession(file.fullPath);
    });

    // 3. Also try the currently selected file (if already set before navigation)
    const currentFile = this.mdFileService.currentSelectedMdFile;
    if (currentFile && currentFile.fullPath) {
      this.loadFileAsSession(currentFile.fullPath);
    }

    // 4. Load available models from Copilot CLI
    this.loadModels();

  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  private loadModels(): void {
    // 1. Try in-memory cache from the service (instant, no HTTP)
    const memoryCached = this.aiChatService.cachedModels;
    if (memoryCached?.length) {
      this.models = memoryCached;
      this.isLoadingModels = false;
      this.syncSelectedModel();
      this.cdr.markForCheck();
    } else {
      this.isLoadingModels = true;
    }

    // 2. Load from DB cache (fast, <50ms) — populates combo immediately
    this.aiChatService.getCachedModels().subscribe({
      next: (models) => {
        if (models.length) {
          this.models = models;
          this.isLoadingModels = false;
          this.syncSelectedModel();
          this.cdr.markForCheck();
        }
      },
      error: () => { /* ignore — will try refresh next */ }
    });

    // 3. Background refresh via Copilot CLI discovery (~5s) — updates DB + memory cache
    this.aiChatService.refreshCopilotCliModels().subscribe({
      next: () => {
        const refreshed = this.aiChatService.cachedModels;
        if (refreshed?.length) {
          this.models = refreshed;
          this.syncSelectedModel();
        }
        this.isLoadingModels = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingModels = false;
        this.cdr.markForCheck();
      }
    });
  }

  private syncSelectedModel(): void {
    if (this.models.length && !this.models.find(m => m.value === this.selectedModel)) {
      this.selectedModel = this.models[0].value;
      this.promptLabService.setModel(this.selectedModel);
    }
  }

  private loadFileAsSession(fullPath: string): void {
    const url = `/api/MdExplorerEditorReact/${fullPath}`;
    this.http.get(url, { responseType: 'text' }).subscribe({
      next: (markdown) => {
        this.promptLabService.loadSession(markdown, fullPath);
      },
      error: (err) => {
        console.error('[PromptLab] Errore nel caricare il file:', err);
      }
    });
  }

  // ---------------------------------------------------------------------------
  // Template actions — delegate to PromptLabService
  // ---------------------------------------------------------------------------

  toggleMode(newMode: PromptLabMode): void {
    this.promptLabService.setMode(newMode);
  }

  addCard(): void {
    this.promptLabService.addCard();
  }

  onCardDeleted(cardId: string): void {
    this.promptLabService.removeCard(cardId);
  }

  onCardChanged(updatedCard: PromptLabCard): void {
    this.promptLabService.updateCard(updatedCard);
  }

  build(): void {
    const result = this.promptLabService.buildSession();
    this.buildOutput = result;
    this.buildCopied = false;
    this.cdr.markForCheck();
  }

  closeBuildOverlay(): void {
    this.buildOutput = null;
    this.cdr.markForCheck();
  }

  copyBuildOutput(): void {
    if (this.buildOutput) {
      navigator.clipboard.writeText(this.buildOutput).then(() => {
        this.buildCopied = true;
        this.cdr.markForCheck();
        setTimeout(() => {
          this.buildCopied = false;
          this.cdr.markForCheck();
        }, 2000);
      });
    }
  }

  onBuildBackdropClick(event: MouseEvent): void {
    // Only close if clicking the backdrop itself, not the card
    if ((event.target as HTMLElement).classList.contains('build-overlay-backdrop')) {
      this.closeBuildOverlay();
    }
  }

  executeAll(): void {
    this.promptLabService.executeAll();
  }

  trackByCardId(index: number, card: PromptLabCard): string {
    return card.id;
  }

  onAgentDefinitionChange(def: AgentDefinition): void {
    this.promptLabService.setAgentDefinition(def);
  }

  onModelChange(model: string): void {
    this.promptLabService.setModel(model);
  }

  openSettings(): void {
    this.showSettings = true;
    this.cdr.markForCheck();
  }

  closeSettings(): void {
    this.showSettings = false;
    this.cdr.markForCheck();
  }

  onSystemPromptChange(value: string): void {
    this.systemPrompt = value;
    this.promptLabService.setSystemPrompt(value);
  }

  resetSystemPrompt(): void {
    this.systemPrompt = DEFAULT_SYSTEM_PROMPT;
    this.promptLabService.setSystemPrompt(DEFAULT_SYSTEM_PROMPT);
    this.cdr.markForCheck();
  }

  onSequencePromptChange(value: string): void {
    this.sequencePrompt = value;
    this.promptLabService.setSequencePrompt(value);
  }

  resetSequencePrompt(): void {
    this.sequencePrompt = DEFAULT_SEQUENCE_PROMPT;
    this.promptLabService.setSequencePrompt(DEFAULT_SEQUENCE_PROMPT);
    this.cdr.markForCheck();
  }

  onWorkflowPromptChange(value: string): void {
    this.workflowPrompt = value;
    this.promptLabService.setWorkflowPrompt(value);
  }

  resetWorkflowPrompt(): void {
    this.workflowPrompt = DEFAULT_WORKFLOW_PROMPT;
    this.promptLabService.setWorkflowPrompt(DEFAULT_WORKFLOW_PROMPT);
    this.cdr.markForCheck();
  }

  onPromptModelChange(promptKey: 'system' | 'sequence' | 'workflow', value: string): void {
    const session = this.promptLabService.currentSession();
    if (!session) return;

    switch (promptKey) {
      case 'system':
        this.systemPromptModel = value;
        session.systemPromptModel = value;
        break;
      case 'sequence':
        this.sequencePromptModel = value;
        session.sequencePromptModel = value;
        break;
      case 'workflow':
        this.workflowPromptModel = value;
        session.workflowPromptModel = value;
        break;
    }
    session.updatedAt = new Date();
    this.promptLabService.updateSession(session);
  }

  onSettingsBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.closeSettings();
    }
  }
}
