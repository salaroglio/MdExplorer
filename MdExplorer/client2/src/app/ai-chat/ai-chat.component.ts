import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Input } from '@angular/core';
import { AiChatService, ChatMessage } from '../services/ai-chat.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Router } from '@angular/router';

@Component({
  selector: 'app-ai-chat',
  templateUrl: './ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss']
})
export class AiChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('scrollContainer') private scrollContainer: ElementRef;
  @ViewChild('messageInput') private messageInput: ElementRef;
  @ViewChild('modelManagerPanel') private modelManagerPanel: ElementRef;
  @Input() compactMode: boolean = false;
  
  messages: ChatMessage[] = [];
  inputMessage = '';
  isModelLoaded = false;
  currentModel: string | null = null;
  showModelManager = false;
  
  private destroy$ = new Subject<void>();
  private shouldScrollToBottom = false;

  constructor(
    private aiService: AiChatService,
    private router: Router
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
        this.isModelLoaded = loaded;
      });
    
    // Subscribe to current model
    this.aiService.currentModel$
      .pipe(takeUntil(this.destroy$))
      .subscribe(model => {
        this.currentModel = model;
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
    if (!this.inputMessage.trim() || !this.isModelLoaded) return;
    
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

  clearChat(): void {
    if (confirm('Clear all messages?')) {
      this.aiService.clearMessages();
    }
  }

  toggleModelManager(): void {
    if (this.compactMode) {
      // In compact mode, navigate to the model manager in the main router outlet
      this.router.navigate(['/main/navigation/ai-model-manager']);
    } else {
      // In full mode, toggle the embedded model manager
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

  formatMessageContent(content: string): string {
    // Basic markdown-like formatting
    return content
      .replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>')
      .replace(/`([^`]+)`/g, '<code>$1</code>')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      .replace(/\n/g, '<br>');
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
}