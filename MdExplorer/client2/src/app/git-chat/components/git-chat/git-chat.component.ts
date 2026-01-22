import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { Subscription } from 'rxjs';
import { GitChatService } from '../../services/git-chat.service';
import { ChatMessage, PresenceInfo, ChatSettings, ChatConnectionState } from '../../models/chat.models';
import { ProjectsService } from '../../../md-explorer/services/projects.service';

@Component({
  selector: 'app-git-chat',
  templateUrl: './git-chat.component.html',
  styleUrls: ['./git-chat.component.scss']
})
export class GitChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('scrollContainer', { static: false }) private scrollContainer!: ElementRef;
  @ViewChild('messageInput', { static: false }) private messageInput!: ElementRef;

  messages: ChatMessage[] = [];
  presence: PresenceInfo = { users: [], totalOnline: 0 };
  settings: ChatSettings = { notificationsEnabled: true, soundEnabled: true };
  connectionState: ChatConnectionState = 'disconnected';

  inputMessage = '';
  showSettings = false;
  showOnlineUsers = false;
  private shouldScrollToBottom = true;

  private subscriptions: Subscription[] = [];

  // Electron detection
  isElectron = !!(window && (window as any).electronAPI);

  constructor(
    private chatService: GitChatService,
    private projectsService: ProjectsService
  ) {}

  ngOnInit(): void {
    // Subscribe to messages
    this.subscriptions.push(
      this.chatService.messages$.subscribe(messages => {
        this.messages = messages;
        this.shouldScrollToBottom = true;
      })
    );

    // Subscribe to presence
    this.subscriptions.push(
      this.chatService.presence$.subscribe(presence => {
        this.presence = presence;
      })
    );

    // Subscribe to settings
    this.subscriptions.push(
      this.chatService.settings$.subscribe(settings => {
        this.settings = settings;
      })
    );

    // Subscribe to connection state
    this.subscriptions.push(
      this.chatService.connectionState$.subscribe(state => {
        this.connectionState = state;
      })
    );

    // Subscribe to project changes
    this.subscriptions.push(
      this.projectsService.currentProjects$.subscribe(project => {
        if (project?.path) {
          this.initializeChatForProject(project.path);
        }
      })
    );

    // Mark chat tab as active
    this.chatService.setChatTabActive(true);
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.chatService.setChatTabActive(false);
  }

  private async initializeChatForProject(projectPath: string): Promise<void> {
    try {
      await this.chatService.joinRoom(projectPath);
    } catch (error) {
      console.error('[GitChatComponent] Failed to initialize chat:', error);
    }
  }

  async sendMessage(): Promise<void> {
    const content = this.inputMessage.trim();
    if (!content) return;

    try {
      await this.chatService.sendMessage(content);
      this.inputMessage = '';

      // Focus back on input
      if (this.messageInput) {
        this.messageInput.nativeElement.focus();
      }
    } catch (error) {
      console.error('[GitChatComponent] Failed to send message:', error);
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
  }

  toggleOnlineUsers(): void {
    this.showOnlineUsers = !this.showOnlineUsers;
  }

  updateNotifications(enabled: boolean): void {
    this.chatService.updateSettings({ notificationsEnabled: enabled });
  }

  updateSound(enabled: boolean): void {
    this.chatService.updateSettings({ soundEnabled: enabled });
  }

  private scrollToBottom(): void {
    try {
      if (this.scrollContainer) {
        const element = this.scrollContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    } catch (err) {
      // Ignore scroll errors
    }
  }

  getMessageClass(message: ChatMessage): string {
    if (message.type === 'system') {
      return 'message message-system';
    }

    const currentUser = this.chatService['provider']?.getCurrentUser();
    const isOwnMessage = currentUser && message.senderEmail === currentUser.userEmail;

    return isOwnMessage ? 'message message-own' : 'message message-other';
  }

  getAvatarInitial(name: string): string {
    return name ? name.charAt(0).toUpperCase() : '?';
  }

  formatTimestamp(timestamp: number): string {
    const date = new Date(timestamp);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  trackByMessageId(index: number, message: ChatMessage): string {
    return message.id || index.toString();
  }
}
