import { Injectable, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { distinctUntilChanged, skip } from 'rxjs/operators';
import { CHAT_PROVIDER, IChatProvider } from '../providers/chat-provider.interface';
import {
  ChatMessage,
  ChatUser,
  PresenceInfo,
  RoomInfo,
  ChatSettings,
  DEFAULT_CHAT_SETTINGS,
  ChatConnectionState
} from '../models/chat.models';

const CHAT_SETTINGS_KEY = 'mdexplorer_chat_settings';

@Injectable()
export class GitChatService implements OnDestroy {
  private subscriptions: Subscription[] = [];
  private currentRepositoryPath: string | null = null;
  private previousMessageCount = 0;

  // Electron detection
  private isElectron = !!(window && (window as any).electronAPI);

  // Chat tab active state (set by component)
  private _isChatTabActive$ = new BehaviorSubject<boolean>(false);
  public isChatTabActive$ = this._isChatTabActive$.asObservable();

  // Connection state
  private _connectionState$ = new BehaviorSubject<ChatConnectionState>('disconnected');
  public connectionState$ = this._connectionState$.asObservable();

  // Room info
  private _roomInfo$ = new BehaviorSubject<RoomInfo | null>(null);
  public roomInfo$ = this._roomInfo$.asObservable();

  // Settings
  private _settings$ = new BehaviorSubject<ChatSettings>(this.loadSettings());
  public settings$ = this._settings$.asObservable();

  // Expose provider observables
  public messages$: Observable<ChatMessage[]>;
  public presence$: Observable<PresenceInfo>;
  public isConnected$: Observable<boolean>;
  public projectUsersCount$: Observable<number>;

  constructor(
    @Inject(CHAT_PROVIDER) private provider: IChatProvider,
    private http: HttpClient
  ) {
    this.messages$ = this.provider.messages$;
    this.presence$ = this.provider.presence$;
    this.isConnected$ = this.provider.isConnected$;
    this.projectUsersCount$ = this.provider.projectUsersCount$;

    // Subscribe to messages for notifications
    this.setupNotifications();
  }

  /**
   * Set up notification listener for new messages
   */
  private setupNotifications(): void {
    const sub = this.provider.messages$.pipe(
      skip(1), // Skip initial empty array
      distinctUntilChanged((prev: ChatMessage[], curr: ChatMessage[]) => prev.length === curr.length)
    ).subscribe((messages: ChatMessage[]) => {
      if (messages.length > this.previousMessageCount && this.previousMessageCount > 0) {
        const newMessage = messages[messages.length - 1];
        if (newMessage && newMessage.type === 'message') {
          this.notifyNewMessage(newMessage);
        }
      }
      this.previousMessageCount = messages.length;
    });

    this.subscriptions.push(sub);
  }

  /**
   * Show notification for new message (Electron only)
   */
  private async notifyNewMessage(message: ChatMessage): Promise<void> {
    const settings = this._settings$.value;
    const currentUser = this.provider.getCurrentUser();

    // Don't notify if:
    // - Not in Electron
    // - Notifications disabled
    // - Chat tab is active
    // - Message is from current user
    if (!this.isElectron) return;
    if (!settings.notificationsEnabled) return;
    if (this._isChatTabActive$.value) return;
    if (currentUser && message.senderEmail === currentUser.userEmail) return;

    // Truncate content for notification
    const truncatedContent = message.content.length > 50
      ? message.content.substring(0, 50) + '...'
      : message.content;

    const title = 'Team Chat - MdExplorer';
    const body = `${message.senderName}: ${truncatedContent}`;

    // Try native Electron notification first (works in agent mode / background)
    const api = (window as any).electronAPI;
    if (api?.showNotification && api?.isWindowFocused) {
      try {
        const isFocused = await api.isWindowFocused();
        // Use native notification when window is not focused (agent mode / minimized)
        if (!isFocused) {
          api.showNotification(title, body, { silent: !settings.soundEnabled });
          console.log('[GitChatService] Sent native Electron notification');
          return;
        }
      } catch (e) {
        console.warn('[GitChatService] Failed to check window focus:', e);
      }
    }

    // Fallback to Web Notification API (when window is focused)
    // Request notification permission if needed
    if (Notification.permission === 'default') {
      Notification.requestPermission();
      return;
    }

    if (Notification.permission !== 'granted') return;

    try {
      const notification = new Notification(title, {
        body,
        icon: '/assets/icons/chat-notification.png',
        silent: !settings.soundEnabled,
        tag: 'mdexplorer-chat' // Replaces previous notifications
      });

      // Handle notification errors
      notification.onerror = (e) => {
        console.warn('[GitChatService] Notification error:', e);
      };

      // Focus app when notification clicked
      notification.onclick = () => {
        window.focus();
        notification.close();
      };

      // Auto-close after 5 seconds
      setTimeout(() => notification.close(), 5000);
    } catch (e) {
      console.warn('[GitChatService] Failed to create notification:', e);
    }
  }

  /**
   * Join a chat room for a repository
   */
  async joinRoom(repositoryPath: string): Promise<void> {
    if (this.currentRepositoryPath === repositoryPath) {
      return; // Already in this room
    }

    this._connectionState$.next('connecting');
    this.previousMessageCount = 0;

    try {
      // Get room info from backend
      const roomInfo = await this.http.get<RoomInfo>(
        `/api/GitChat/room-info?repositoryPath=${encodeURIComponent(repositoryPath)}`
      ).toPromise();

      if (!roomInfo || !roomInfo.hasRemote) {
        this._connectionState$.next('disconnected');
        this._roomInfo$.next(roomInfo || null);
        return;
      }

      this._roomInfo$.next(roomInfo);

      // Create user object
      const user: ChatUser = {
        oderId: this.generateSessionId(),
        userId: roomInfo.user!.userId,
        userName: roomInfo.user!.userName,
        userEmail: roomInfo.user!.userEmail,
        online: true,
        lastSeen: Date.now()
      };

      // Connect to room via provider
      await this.provider.connect(roomInfo.roomId!, user);

      this.currentRepositoryPath = repositoryPath;
      this._connectionState$.next('connected');

      console.log(`[GitChatService] Joined room for repository: ${repositoryPath}`);
    } catch (error) {
      console.error('[GitChatService] Failed to join room:', error);
      this._connectionState$.next('error');
      throw error;
    }
  }

  /**
   * Leave the current chat room
   */
  async leaveRoom(): Promise<void> {
    if (!this.currentRepositoryPath) {
      return;
    }

    try {
      await this.provider.disconnect();
      this.currentRepositoryPath = null;
      this._connectionState$.next('disconnected');
      this._roomInfo$.next(null);
      this.previousMessageCount = 0;

      console.log('[GitChatService] Left room');
    } catch (error) {
      console.error('[GitChatService] Failed to leave room:', error);
    }
  }

  /**
   * Send a message to the current room
   */
  async sendMessage(content: string): Promise<void> {
    const trimmedContent = content.trim();
    if (!trimmedContent) return;

    const currentUser = this.provider.getCurrentUser();
    if (!currentUser) {
      throw new Error('Not connected to a room');
    }

    await this.provider.sendMessage({
      content: trimmedContent,
      senderName: currentUser.userName,
      senderEmail: currentUser.userEmail,
      timestamp: Date.now(),
      type: 'message'
    });
  }

  /**
   * Set whether the chat tab is currently active
   */
  setChatTabActive(active: boolean): void {
    this._isChatTabActive$.next(active);
  }

  /**
   * Update chat settings
   */
  updateSettings(settings: Partial<ChatSettings>): void {
    const current = this._settings$.value;
    const updated = { ...current, ...settings };
    this._settings$.next(updated);
    this.saveSettings(updated);
  }

  /**
   * Load settings from localStorage
   */
  private loadSettings(): ChatSettings {
    try {
      const stored = localStorage.getItem(CHAT_SETTINGS_KEY);
      if (stored) {
        return { ...DEFAULT_CHAT_SETTINGS, ...JSON.parse(stored) };
      }
    } catch (e) {
      console.warn('[GitChatService] Failed to load settings:', e);
    }
    return DEFAULT_CHAT_SETTINGS;
  }

  /**
   * Save settings to localStorage
   */
  private saveSettings(settings: ChatSettings): void {
    try {
      localStorage.setItem(CHAT_SETTINGS_KEY, JSON.stringify(settings));
    } catch (e) {
      console.warn('[GitChatService] Failed to save settings:', e);
    }
  }

  /**
   * Generate a unique session ID
   */
  private generateSessionId(): string {
    return `${Date.now()}-${Math.random().toString(36).substring(2, 11)}`;
  }

  /**
   * Check if currently connected
   */
  get isConnected(): boolean {
    return this._connectionState$.value === 'connected';
  }

  /**
   * Get current room ID
   */
  get currentRoomId(): string | null {
    return this.provider.getCurrentRoomId();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.leaveRoom();
  }
}
