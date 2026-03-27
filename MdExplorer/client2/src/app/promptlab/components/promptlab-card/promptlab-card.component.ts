import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  ElementRef,
  ViewChild,
  OnInit,
  OnDestroy,
  AfterViewChecked
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../commons/components/show-file-system/show-file-metadata';
import { PromptLabCard, PromptLabParameter, ChatMessage, DEFAULT_SEQUENCE_PROMPT, DEFAULT_WORKFLOW_PROMPT } from '../../models/promptlab.models';
import { PromptLabService } from '../../services/promptlab.service';
import { PromptLabDistillationService } from '../../services/promptlab-distillation.service';
import { AiChatService } from '../../../services/ai-chat.service';
import { ProjectsService } from '../../../md-explorer/services/projects.service';

@Component({
  selector: 'app-promptlab-card',
  templateUrl: './promptlab-card.component.html',
  styleUrls: ['./promptlab-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PromptLabCardComponent implements OnInit, OnDestroy, AfterViewChecked {

  @Input() card!: PromptLabCard;
  @Input() isSingleCard = false;

  @Output() cardDeleted = new EventEmitter<string>();
  @Output() cardChanged = new EventEmitter<PromptLabCard>();

  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

  chatCollapsed = false;
  chatInputText = '';
  chatColumnWidth: number | null = null;
  isDragging = false;

  /** Streaming state */
  isStreaming = false;
  streamingContent = '';

  /** Index of the parameter currently being inline-edited */
  editingParamIndex: number | null = null;
  editingParamValue = '';

  private destroy$ = new Subject<void>();
  private needsScroll = false;
  private moveListener: ((e: MouseEvent) => void) | null = null;
  private upListener: ((e: MouseEvent) => void) | null = null;

  /** True while the distillation LLM call is in progress */
  isDistilling = false;

  /** Prompt rendering & editing */
  isEditingPrompt = false;
  editingPromptText = '';
  renderedPrompt: SafeHtml = '';

  /** Play execution state (Task 8.2) */
  isExecuting = false;
  executeStartTime = 0;
  executeAccumulatedOutput = '';

  /** Ultimo Run panel toggle (Task 8.2) */
  showLastRun = false;

  /** Diagram generation state (Task 9.1) */
  activeDiagram: 'sequence' | 'workflow' | null = null;
  diagramPlantUml = '';
  diagramSvg: SafeHtml = '';
  isDiagramLoading = false;
  diagramRenderError = false;
  diagramRetryCount = 0;
  diagramStatusMessage = '';
  private readonly DIAGRAM_MAX_RETRIES = 3;
  private diagramSubscription: Subscription | null = null;

  constructor(
    private cdr: ChangeDetectorRef,
    private elRef: ElementRef,
    private sanitizer: DomSanitizer,
    private http: HttpClient,
    private dialog: MatDialog,
    private promptLabService: PromptLabService,
    private distillationService: PromptLabDistillationService,
    private aiChatService: AiChatService,
    private projectsService: ProjectsService
  ) {}

  ngOnInit(): void {
    this.updateRenderedPrompt();
    this.subscribeToCardStream();
    this.subscribeToDistillation();
  }

  ngAfterViewChecked(): void {
    if (this.needsScroll) {
      this.scrollToBottom();
      this.needsScroll = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.cleanupDragListeners();
    this.cleanupDiagramSubscription();
    this.distillationService.disposeCard(this.card.id);
  }

  // ── Stream subscription ──

  private subscribeToCardStream(): void {
    console.log('[PromptLabCard] subscribeToCardStream for card:', this.card.id);
    this.promptLabService.getCardStream$(this.card.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        console.log('[PromptLabCard] stream event:', event.type, event.data?.substring?.(0, 50) || event.data);
        switch (event.type) {
          case 'chunk':
            if (!this.isStreaming) {
              this.isStreaming = true;
              this.streamingContent = '';
            }
            this.streamingContent += event.data;
            // Accumulate output for Play execution tracking
            if (this.isExecuting) {
              this.executeAccumulatedOutput += event.data;
            }
            this.needsScroll = true;
            this.cdr.markForCheck();
            break;

          case 'complete':
            if (this.streamingContent) {
              const assistantMsg: ChatMessage = {
                id: this.generateId(),
                role: 'assistant',
                content: this.streamingContent,
                timestamp: new Date()
              };
              this.card.conversation.push(assistantMsg);
              this.cardChanged.emit(this.card);

              // After each LLM response, trigger distillation to update
              // the prompt on the right. Skip if this was a Play execution.
              if (!this.isExecuting) {
                this.isDistilling = true;
                this.distillationService.triggerDistillation(
                  this.card.id,
                  this.card.conversation,
                  this.card.distilledPrompt,
                  this.promptLabService.getCurrentModel()
                );
              }
            }

            // Complete Play execution — create lastRun (Task 8.2)
            if (this.isExecuting) {
              this.promptLabService.completeCardExecution(
                this.card.id,
                this.executeAccumulatedOutput,
                this.executeStartTime
              );
              this.isExecuting = false;
              this.executeAccumulatedOutput = '';
            }

            this.isStreaming = false;
            this.streamingContent = '';
            this.needsScroll = true;
            this.cdr.markForCheck();
            break;

          case 'error':
            const errorMsg: ChatMessage = {
              id: this.generateId(),
              role: 'assistant',
              content: `Errore: ${event.data}`,
              timestamp: new Date()
            };
            this.card.conversation.push(errorMsg);
            this.isStreaming = false;
            this.streamingContent = '';
            // Reset Play execution on error
            if (this.isExecuting) {
              this.isExecuting = false;
              this.executeAccumulatedOutput = '';
            }
            this.cardChanged.emit(this.card);
            this.needsScroll = true;
            this.cdr.markForCheck();
            break;

          case 'thinking':
            // Ignore thinking events for now
            break;
        }
      });
  }

  private subscribeToDistillation(): void {
    this.distillationService.getDistillationResult$(this.card.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        this.card.distilledPrompt = result.distilledPrompt;
        this.card.parameters = result.parameters;
        this.card.generatedTitle = result.generatedTitle;
        this.isDistilling = false;
        this.updateRenderedPrompt();
        this.cardChanged.emit(this.card);
        this.cdr.markForCheck();
      });
  }

  // ── Header actions ──

  onTitleEdit(event: Event): void {
    const el = event.target as HTMLElement;
    this.card.generatedTitle = el.innerText.trim();
    this.cardChanged.emit(this.card);
  }

  deleteCard(): void {
    const title = this.card.generatedTitle || 'questa card';
    if (confirm(`Eliminare "${title}"?\n\nQuesta azione non può essere annullata.`)) {
      this.cardDeleted.emit(this.card.id);
    }
  }

  // ── Parameters ──

  getParamIcon(type: string): string {
    switch (type) {
      case 'file': return '\uD83D\uDCC4';
      case 'output_file': return '\uD83D\uDCBE';
      case 'directory': return '\uD83D\uDCC2';
      case 'text': return '\u270E';
      default: return '';
    }
  }

  getParamDisplayClass(param: PromptLabParameter): string {
    const base = param.type === 'output_file' ? 'output-file' : (param.type === 'directory' ? 'directory' : param.type);
    const state = param.value ? 'filled' : 'empty';
    return `${base} ${state}`;
  }

  onParameterClick(param: PromptLabParameter, index: number): void {
    if (param.type === 'text') {
      this.editingParamIndex = index;
      this.editingParamValue = param.value || '';
      this.cdr.markForCheck();
    } else if (param.type === 'output_file') {
      // Output file — "Save As" style: pick folder, then type filename
      this.openSaveAsDialog(param);
    } else {
      // file or directory — open MdExplorer file system dialog
      const data = new ShowFileMetadata();
      const project = this.projectsService.currentProjects$.getValue();
      data.start = project?.path || 'project';

      if (param.type === 'directory') {
        data.title = `Seleziona cartella per "${param.name}"`;
        data.typeOfSelection = 'Folders';
        data.buttonText = 'Seleziona cartella';
      } else {
        data.title = `Seleziona file per "${param.name}"`;
        data.typeOfSelection = 'FoldersAndFiles';
        data.buttonText = 'Seleziona file';
      }

      const dialogRef = this.dialog.open(ShowFileSystemComponent, {
        width: '800px',
        height: '600px',
        panelClass: 'resizable-dialog-container',
        data: data
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result && result.data) {
          param.value = this.toRelativePath(result.data);
          this.cardChanged.emit(this.card);
          this.cdr.markForCheck();
        }
      });
    }
  }

  private openSaveAsDialog(param: PromptLabParameter): void {
    const data = new ShowFileMetadata();
    const project = this.projectsService.currentProjects$.getValue();
    data.start = project?.path || 'project';
    data.title = `Salva come — "${param.name}"`;
    data.typeOfSelection = 'Folders';
    data.buttonText = 'Salva';
    data.saveAs = true;
    // Suggest current filename if already set
    const currentName = param.value ? param.value.split(/[/\\]/).pop() : '';
    data.defaultFileName = currentName || 'output.md';

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.data) {
        param.value = this.toRelativePath(result.data);
        this.cardChanged.emit(this.card);
        this.cdr.markForCheck();
      }
    });
  }

  confirmParamEdit(param: PromptLabParameter): void {
    param.value = this.editingParamValue;
    this.editingParamIndex = null;
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();
  }

  cancelParamEdit(): void {
    this.editingParamIndex = null;
    this.cdr.markForCheck();
  }

  onParamEditKeydown(event: KeyboardEvent, param: PromptLabParameter): void {
    if (event.key === 'Enter') {
      this.confirmParamEdit(param);
    } else if (event.key === 'Escape') {
      this.cancelParamEdit();
    }
  }

  // ── Play & Last Run (Task 8.2) ──

  onPlay(): void {
    if (this.isStreaming || this.isExecuting) return;
    if (!this.card.distilledPrompt?.trim()) return;

    // 1. Copy the distilled prompt into the chat as the first message of this execution
    const promptMsg: ChatMessage = {
      id: this.generateId(),
      role: 'user',
      content: this.card.distilledPrompt,
      timestamp: new Date()
    };
    this.card.conversation.push(promptMsg);
    this.needsScroll = true;

    // 2. Start execution
    this.isExecuting = true;
    this.executeStartTime = Date.now();
    this.executeAccumulatedOutput = '';
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();

    console.log('[PromptLabCard] Play — sending distilled prompt to LLM');
    this.promptLabService.executeCard(this.card.id);
  }

  toggleLastRun(): void {
    this.showLastRun = !this.showLastRun;
    this.cdr.markForCheck();
  }

  formatDuration(ms: number): string {
    if (ms < 1000) return `${ms}ms`;
    const seconds = Math.round(ms / 1000);
    if (seconds < 60) return `${seconds}s`;
    const minutes = Math.floor(seconds / 60);
    const remaining = seconds % 60;
    return `${minutes}m ${remaining}s`;
  }

  formatRunDate(date: Date): string {
    if (!date) return '';
    const d = new Date(date);
    const day = d.getDate().toString().padStart(2, '0');
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const year = d.getFullYear();
    const hours = d.getHours().toString().padStart(2, '0');
    const minutes = d.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }

  getRunParamKeys(): string[] {
    if (!this.card.lastRun) return [];
    return Object.keys(this.card.lastRun.resolvedParameters);
  }

  // ── Diagram generation (Task 9.1) ──

  toggleDiagram(type: 'sequence' | 'workflow'): void {
    if (this.activeDiagram === type) {
      this.activeDiagram = null;
      this.cleanupDiagramSubscription();
      this.cdr.markForCheck();
      return;
    }

    this.activeDiagram = type;
    this.diagramPlantUml = '';
    this.diagramSvg = '';
    this.diagramRenderError = false;
    this.diagramRetryCount = 0;
    this.diagramStatusMessage = '';

    // Check cached diagram
    const cache = type === 'sequence' ? this.card.sequenceDiagram : this.card.workflowDiagram;
    const currentHash = this.hashPrompt(this.card.distilledPrompt || '');
    if (cache?.svgPath && cache.promptHash === currentHash) {
      // Cache hit — load SVG from file
      this.isDiagramLoading = true;
      this.diagramStatusMessage = 'Caricamento dalla cache...';
      this.cdr.markForCheck();
      this.loadCachedSvg(cache.svgPath);
    } else {
      // Cache miss or prompt changed — regenerate via LLM
      this.isDiagramLoading = true;
      this.cdr.markForCheck();
      this.generateDiagram(type);
    }
  }

  /**
   * Load a cached SVG file via the backend file API.
   * The svgPath is relative to the template's directory.
   */
  private loadCachedSvg(svgPath: string): void {
    const session = this.promptLabService.currentSession();
    if (!session?.templatePath) {
      this.isDiagramLoading = false;
      this.cdr.markForCheck();
      return;
    }

    // templatePath is absolute (e.g. "C:\...\promptlab\file.md")
    // svgPath is relative to template dir (e.g. "assets/card-xxx-workflow.svg")
    // The API needs a path relative to the project root.
    // templatePath relative to project = toRelativePath(templatePath) → "promptlab\file.md"
    // We replace the filename with the svgPath.
    const relativeTemplatePath = this.toRelativePath(session.templatePath);
    const relativeDir = relativeTemplatePath.replace(/[/\\][^/\\]+$/, '');
    const fileName = svgPath.split('/').pop() || svgPath;
    const relativeSvgPath = `${relativeDir}/assets/${fileName}`;
    const url = `/api/MdExplorerEditorReact/${relativeSvgPath}`;

    this.http.get(url, { responseType: 'text' }).subscribe({
      next: (svgContent) => {
        if (svgContent) {
          this.diagramSvg = this.sanitizer.bypassSecurityTrustHtml(svgContent);
          this.diagramStatusMessage = '';
        }
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        // File not found — regenerate
        console.warn('[PromptLabCard] Cached SVG not found, regenerating...');
        this.generateDiagram(this.activeDiagram!);
      }
    });
  }

  private async generateDiagram(type: 'sequence' | 'workflow'): Promise<void> {
    this.cleanupDiagramSubscription();

    const channelId = `card-${this.card.id}-diagram`;
    this.aiChatService.clearChannelHistory(channelId);

    // Get diagram prompts from the session (configurable via Settings)
    const session = this.promptLabService.currentSession();
    const diagramPrompt = type === 'sequence'
      ? (session?.sequencePrompt || DEFAULT_SEQUENCE_PROMPT)
      : (session?.workflowPrompt || DEFAULT_WORKFLOW_PROMPT);

    const message = `${diagramPrompt}\n\n--- Prompt ---\n${this.card.distilledPrompt || '(nessun prompt distillato)'}`;

    let accumulated = '';

    this.diagramSubscription = this.aiChatService.getChannelStream$(channelId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        switch (event.type) {
          case 'chunk':
            accumulated += event.data;
            break;

          case 'complete':
            const plantUml = this.extractPlantUml(accumulated);
            this.diagramPlantUml = plantUml;
            this.cleanupDiagramSubscription();
            // Render PlantUML to SVG via backend
            this.renderPlantUmlToSvg(plantUml);
            break;

          case 'error':
            this.diagramPlantUml = '';
            this.diagramSvg = '';
            this.isDiagramLoading = false;
            this.cdr.markForCheck();
            this.cleanupDiagramSubscription();
            break;
        }
      });

    // Use diagram-specific model if configured, otherwise session default
    const diagramModel = type === 'sequence'
      ? (session?.sequencePromptModel || '')
      : (session?.workflowPromptModel || '');

    if (diagramModel) {
      const provider = diagramModel.toLowerCase().includes('llama') ? 'local' : 'copilotcli';
      await this.aiChatService.setProviderAsync(provider, diagramModel);
    } else {
      await this.promptLabService.ensureChatModePublic();
    }
    this.aiChatService.sendMessageToChannel(message, channelId);
  }

  private renderPlantUmlToSvg(plantUml: string): void {
    if (!plantUml) {
      this.isDiagramLoading = false;
      this.cdr.markForCheck();
      return;
    }

    // Build save path for caching the SVG to disk
    const savePath = this.buildSvgSavePath();

    this.http.post<{ svg: string }>('/api/plantumlextensions/RenderSvg', {
      plantUmlCode: plantUml,
      savePath: savePath
    }).subscribe({
      next: (response) => {
        if (response?.svg) {
          this.diagramSvg = this.sanitizer.bypassSecurityTrustHtml(response.svg);
          this.diagramStatusMessage = '';
          // Update card cache reference
          this.updateDiagramCache();
        }
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('[PromptLabCard] Error rendering PlantUML:', err);
        const errorMessage = err?.error?.error || err?.message || 'Unknown rendering error';

        if (this.diagramRetryCount < this.DIAGRAM_MAX_RETRIES) {
          // Auto-retry: send the error back to the LLM to fix
          this.diagramRetryCount++;
          this.diagramStatusMessage = `Errore di sintassi (tentativo ${this.diagramRetryCount}/${this.DIAGRAM_MAX_RETRIES})...`;
          this.cdr.markForCheck();
          this.requestDiagramFix(plantUml, errorMessage);
        } else {
          // Max retries reached — show error + code
          this.diagramSvg = '';
          this.diagramRenderError = true;
          this.diagramStatusMessage = `Errore dopo ${this.DIAGRAM_MAX_RETRIES} tentativi di correzione.`;
          this.isDiagramLoading = false;
          this.cdr.markForCheck();
        }
      }
    });
  }

  /**
   * Send the broken PlantUML + error message back to the LLM on the diagram channel,
   * asking it to fix the syntax error. Reuses the same channel (history preserved).
   */
  private async requestDiagramFix(brokenCode: string, errorMessage: string): Promise<void> {
    this.cleanupDiagramSubscription();

    const channelId = `card-${this.card.id}-diagram`;
    const fixMessage = `The PlantUML code you generated has a syntax error. Here is the error from the PlantUML renderer:\n\n${errorMessage}\n\nHere is the broken code:\n\`\`\`\n${brokenCode}\n\`\`\`\n\nPlease fix the syntax error and return ONLY the corrected PlantUML code between @startuml and @enduml, nothing else.`;

    let accumulated = '';

    this.diagramSubscription = this.aiChatService.getChannelStream$(channelId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        switch (event.type) {
          case 'chunk':
            accumulated += event.data;
            break;
          case 'complete':
            const plantUml = this.extractPlantUml(accumulated);
            this.diagramPlantUml = plantUml;
            this.cleanupDiagramSubscription();
            this.renderPlantUmlToSvg(plantUml);
            break;
          case 'error':
            this.diagramSvg = '';
            this.diagramRenderError = true;
            this.diagramStatusMessage = 'Errore nella comunicazione con il LLM.';
            this.isDiagramLoading = false;
            this.cdr.markForCheck();
            this.cleanupDiagramSubscription();
            break;
        }
      });

    // Use same model as the diagram generation
    const session = this.promptLabService.currentSession();
    const diagramModel = this.activeDiagram === 'sequence'
      ? (session?.sequencePromptModel || '')
      : (session?.workflowPromptModel || '');

    if (diagramModel) {
      const provider = diagramModel.toLowerCase().includes('llama') ? 'local' : 'copilotcli';
      await this.aiChatService.setProviderAsync(provider, diagramModel);
    } else {
      await this.promptLabService.ensureChatModePublic();
    }
    this.aiChatService.sendMessageToChannel(fixMessage, channelId);
  }

  /**
   * Simple hash of the prompt text for cache comparison.
   * DJB2 algorithm — fast, good distribution, no crypto needed.
   */
  private hashPrompt(text: string): string {
    let hash = 5381;
    for (let i = 0; i < text.length; i++) {
      hash = ((hash << 5) + hash) + text.charCodeAt(i);
      hash |= 0; // Convert to 32-bit integer
    }
    return (hash >>> 0).toString(36);
  }

  /**
   * Build the absolute save path for the current diagram's SVG file.
   * Returns null if session has no templatePath.
   */
  private buildSvgSavePath(): string | null {
    if (!this.activeDiagram) return null;
    const session = this.promptLabService.currentSession();
    if (!session?.templatePath) return null;

    const fileName = `${this.card.id}-${this.activeDiagram}.svg`;
    const templateDir = session.templatePath.replace(/[/\\][^/\\]+$/, '');
    const separator = templateDir.includes('/') ? '/' : '\\';
    return `${templateDir}${separator}assets${separator}${fileName}`;
  }

  /**
   * Update the card's diagram cache reference (hash + relative SVG path).
   * Triggers cardChanged → auto-save → persisted as ![hash](assets/...) in the .md.
   */
  private updateDiagramCache(): void {
    if (!this.activeDiagram) return;

    const promptHash = this.hashPrompt(this.card.distilledPrompt || '');
    const fileName = `${this.card.id}-${this.activeDiagram}.svg`;
    const cache = { promptHash, svgPath: `assets/${fileName}` };
    if (this.activeDiagram === 'sequence') {
      this.card.sequenceDiagram = cache;
    } else {
      this.card.workflowDiagram = cache;
    }
    this.cardChanged.emit(this.card);
  }

  private extractPlantUml(text: string): string {
    const match = text.match(/@startuml[\s\S]*?@enduml/);
    return match ? match[0] : text.trim();
  }

  regenerateDiagram(): void {
    if (this.activeDiagram) {
      const type = this.activeDiagram;
      this.activeDiagram = null;
      this.cleanupDiagramSubscription();
      // Re-trigger
      setTimeout(() => this.toggleDiagram(type));
    }
  }

  copyDiagram(): void {
    if (this.diagramPlantUml) {
      navigator.clipboard.writeText(this.diagramPlantUml).catch(err => {
        console.error('Failed to copy diagram to clipboard:', err);
      });
    }
  }

  private cleanupDiagramSubscription(): void {
    if (this.diagramSubscription) {
      this.diagramSubscription.unsubscribe();
      this.diagramSubscription = null;
    }
  }

  // ── Chat ──

  toggleChat(): void {
    this.chatCollapsed = !this.chatCollapsed;
    this.cdr.markForCheck();
  }

  resetChat(): void {
    this.promptLabService.resetCardChat(this.card.id);
    this.card.conversation = [];
    this.isStreaming = false;
    this.streamingContent = '';
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();
  }

  sendMessage(): void {
    const text = this.chatInputText.trim();
    if (!text || this.isStreaming) { return; }

    const msg: ChatMessage = {
      id: this.generateId(),
      role: 'user',
      content: text,
      timestamp: new Date()
    };
    this.card.conversation.push(msg);
    this.chatInputText = '';
    this.cardChanged.emit(this.card);
    this.needsScroll = true;
    this.cdr.markForCheck();

    // Send to LLM — the chat is a conversational editor for the prompt.
    // The user talks to the LLM to refine the distilled prompt.
    // After the LLM responds, distillation updates the prompt on the right.
    this.promptLabService.sendCardMessage(this.card.id, text);
  }

  onChatInputKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  formatTime(date: Date): string {
    if (!date) { return ''; }
    const d = new Date(date);
    return d.getHours().toString().padStart(2, '0') + ':' + d.getMinutes().toString().padStart(2, '0');
  }

  // ── Auto-scroll ──

  private scrollToBottom(): void {
    if (this.messagesContainer?.nativeElement) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }

  // ── Distilled Prompt ──

  /** Convert markdown to simple HTML for display */
  updateRenderedPrompt(): void {
    const md = this.card.distilledPrompt || '';
    const lines = md.split('\n');
    const out: string[] = [];
    let inList = false;

    for (const raw of lines) {
      let line = raw
        .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

      // Inline formatting
      line = line
        .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
        .replace(/\*(.+?)\*/g, '<em>$1</em>')
        .replace(/`([^`]+)`/g, '<code>$1</code>')
        .replace(/\{\{(\w+)\}\}/g, '<span class="param-highlight">{{$1}}</span>');

      // Headings
      if (/^### (.+)/.test(line)) {
        if (inList) { out.push('</ul>'); inList = false; }
        out.push(line.replace(/^### (.+)/, '<h4>$1</h4>'));
      } else if (/^## (.+)/.test(line)) {
        if (inList) { out.push('</ul>'); inList = false; }
        out.push(line.replace(/^## (.+)/, '<h3>$1</h3>'));
      } else if (/^# (.+)/.test(line)) {
        if (inList) { out.push('</ul>'); inList = false; }
        out.push(line.replace(/^# (.+)/, '<h2>$1</h2>'));
      }
      // List items
      else if (/^[-*] (.+)/.test(line)) {
        if (!inList) { out.push('<ul>'); inList = true; }
        out.push(line.replace(/^[-*] (.+)/, '<li>$1</li>'));
      } else if (/^\d+\. (.+)/.test(line)) {
        if (!inList) { out.push('<ul>'); inList = true; }
        out.push(line.replace(/^\d+\. (.+)/, '<li>$1</li>'));
      }
      // Empty line
      else if (line.trim() === '') {
        if (inList) { out.push('</ul>'); inList = false; }
        out.push('<br>');
      }
      // Normal text
      else {
        if (inList) { out.push('</ul>'); inList = false; }
        out.push('<div>' + line + '</div>');
      }
    }
    if (inList) out.push('</ul>');

    this.renderedPrompt = this.sanitizer.bypassSecurityTrustHtml(out.join('\n'));
  }

  startEditingPrompt(): void {
    this.isEditingPrompt = true;
    this.editingPromptText = this.card.distilledPrompt || '';
    this.cdr.markForCheck();
    // Focus the textarea after render
    setTimeout(() => {
      const ta = this.elRef.nativeElement.querySelector('.prompt-editor');
      if (ta) ta.focus();
    });
  }

  finishEditingPrompt(): void {
    this.isEditingPrompt = false;
    this.card.distilledPrompt = this.editingPromptText;
    this.updateRenderedPrompt();
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();
  }

  // ── Splitter ──

  onSplitterMouseDown(event: MouseEvent): void {
    event.preventDefault();
    const cardBody = (event.target as HTMLElement).closest('.card-body') as HTMLElement;
    if (!cardBody) { return; }

    const chatCol = cardBody.querySelector('.chat-column') as HTMLElement;
    if (!chatCol) { return; }

    const startX = event.clientX;
    const startWidth = chatCol.offsetWidth;
    this.isDragging = true;

    this.moveListener = (e: MouseEvent) => {
      const dx = e.clientX - startX;
      const newWidth = Math.max(200, startWidth + dx);
      // also limit so prompt column keeps at least 200px
      const bodyWidth = cardBody.offsetWidth;
      const splitterWidth = 5;
      const maxChatWidth = bodyWidth - splitterWidth - 200;
      this.chatColumnWidth = Math.min(newWidth, maxChatWidth);
      this.cdr.markForCheck();
      e.preventDefault();
    };

    this.upListener = () => {
      this.isDragging = false;
      this.cleanupDragListeners();
      this.cdr.markForCheck();
    };

    document.addEventListener('mousemove', this.moveListener);
    document.addEventListener('mouseup', this.upListener);
  }

  getChatColumnStyle(): { [key: string]: string } {
    if (this.chatColumnWidth !== null) {
      return { flex: `0 0 ${this.chatColumnWidth}px` };
    }
    return {};
  }

  // ── Helpers ──

  private cleanupDragListeners(): void {
    if (this.moveListener) {
      document.removeEventListener('mousemove', this.moveListener);
      this.moveListener = null;
    }
    if (this.upListener) {
      document.removeEventListener('mouseup', this.upListener);
      this.upListener = null;
    }
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2, 5);
  }

  private toRelativePath(absolutePath: string): string {
    const project = this.projectsService.currentProjects$.getValue();
    if (!project?.path) return absolutePath;
    const root = project.path.replace(/[\/\\]$/, '');
    const normalized = absolutePath.replace(/\//g, '\\');
    const normalizedRoot = root.replace(/\//g, '\\');
    if (normalized.toLowerCase().startsWith(normalizedRoot.toLowerCase())) {
      return normalized.substring(normalizedRoot.length).replace(/^[\\\/]/, '');
    }
    return absolutePath;
  }
}
