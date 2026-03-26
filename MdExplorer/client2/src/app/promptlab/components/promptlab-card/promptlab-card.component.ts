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
import { PromptLabCard, PromptLabParameter, ChatMessage } from '../../models/promptlab.models';
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

  /** Index of the parameter currently being inline-edited (text type only) */
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
      case 'directory': return '\uD83D\uDCC2';
      case 'text': return '\u270E';
      default: return '';
    }
  }

  getParamDisplayClass(param: PromptLabParameter): string {
    const base = param.type === 'directory' ? 'directory' : param.type;
    const state = param.value ? 'filled' : 'empty';
    return `${base} ${state}`;
  }

  onParameterClick(param: PromptLabParameter, index: number): void {
    if (param.type === 'text') {
      this.editingParamIndex = index;
      this.editingParamValue = param.value || '';
      this.cdr.markForCheck();
    } else {
      // file or directory — open MdExplorer file system dialog
      const data = new ShowFileMetadata();
      // Start from project root to keep selection within workspace
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
    this.isDiagramLoading = true;
    this.cdr.markForCheck();
    this.generateDiagram(type);
  }

  private async generateDiagram(type: 'sequence' | 'workflow'): Promise<void> {
    this.cleanupDiagramSubscription();

    const channelId = `card-${this.card.id}-diagram`;
    this.aiChatService.clearChannelHistory(channelId);

    const colorDirectives = `
Use a clean, professional color scheme with these PlantUML skinparam directives at the top:
skinparam backgroundColor #FEFEFE
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam roundCorner 8`;

    const systemPrompt = type === 'sequence'
      ? `Generate a PlantUML sequence diagram that shows the interaction flow described in this prompt. Show actors (User, LLM), messages exchanged, and data flow. Include parameter values if available. ${colorDirectives}\nUse colored participants: actor User #E3F2FD, participant LLM #FFF3E0, participant FileSystem #E8F5E9. Return ONLY the PlantUML code between @startuml and @enduml, nothing else.`
      : `Generate a PlantUML activity diagram that shows the workflow steps described in this prompt. Show input, processing steps, decisions, and output. Include parameter values if available. ${colorDirectives}\nUse colored partitions: #E3F2FD for input steps, #FFF3E0 for processing, #E8F5E9 for output. Use start/stop nodes. Return ONLY the PlantUML code between @startuml and @enduml, nothing else.`;

    const message = `${systemPrompt}\n\n--- Prompt ---\n${this.card.distilledPrompt || '(nessun prompt distillato)'}`;

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

    // Ensure chat mode is set before sending
    await this.promptLabService.ensureChatModePublic();
    this.aiChatService.sendMessageToChannel(message, channelId);
  }

  private renderPlantUmlToSvg(plantUml: string): void {
    if (!plantUml) {
      this.isDiagramLoading = false;
      this.cdr.markForCheck();
      return;
    }

    this.http.post<{ svg: string }>('/api/plantumlextensions/RenderSvg', {
      plantUmlCode: plantUml
    }).subscribe({
      next: (response) => {
        if (response?.svg) {
          this.diagramSvg = this.sanitizer.bypassSecurityTrustHtml(response.svg);
        }
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('[PromptLabCard] Error rendering PlantUML:', err);
        // Fallback: show raw PlantUML code
        this.diagramSvg = '';
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  private extractPlantUml(text: string): string {
    const match = text.match(/@startuml[\s\S]*?@enduml/);
    return match ? match[0] : text.trim();
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
