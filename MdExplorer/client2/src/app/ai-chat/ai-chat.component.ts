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

  // Edit message state
  editingMessageId: string | null = null;
  editedContent: string = '';

  // Copy message state
  copiedMessageId: string | null = null;

  // Thinking section collapse state (per message id)
  collapsedThinking: Set<string> = new Set();

  private destroy$ = new Subject<void>();
  private shouldScrollToBottom = false;

  constructor(
    private aiService: AiChatService,
    private router: Router,
    private sanitizer: DomSanitizer,
    private layoutService: LayoutService,
    private translate: TranslateService,
    private projectsService: ProjectsService
  ) {}

  ngOnInit(): void {
    
    // Subscribe to messages
    this.aiService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(messages => {
        this.messages = messages;
        this.shouldScrollToBottom = true;
      });
    
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
          return;
        }
        if (config.autoSelect && config.available) {
          const model = config.defaultModel || 'claude-sonnet-4.6';
          console.log('[AiChatComponent] Auto-selecting Copilot CLI with model:', model);
          this.copilotCliUnavailable = false;
          this.aiService.setProvider('copilotcli', model);
          this.aiService.notifyCopilotCliConnected(model);
        } else if (config.autoSelect && !config.available) {
          console.log('[AiChatComponent] Copilot CLI auto-select enabled but CLI not available — locking chat');
          this.copilotCliUnavailable = true;
          this.aiService.notifyCopilotCliDisconnected();
        } else {
          this.copilotCliUnavailable = false;
        }
      });
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  sendMessage(): void {
    if (!this.inputMessage.trim() || !this.isModelLoaded || this.isConfiguringProvider) return;

    this.aiService.sendMessage(this.inputMessage);
    this.inputMessage = '';
    this.focusInput();
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