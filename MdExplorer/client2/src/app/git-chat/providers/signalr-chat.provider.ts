import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { IChatProvider } from './chat-provider.interface';
import { ChatMessage, ChatUser, PresenceInfo } from '../models/chat.models';

/**
 * SignalR implementation of IChatProvider
 * Connects to TeamChatHub on the backend which proxies to Firebase
 */
@Injectable()
export class SignalRChatProvider implements IChatProvider {
  private hubConnection: HubConnection | null = null;

  private currentRoomId: string | null = null;
  private currentUser: ChatUser | null = null;

  private _messages$ = new BehaviorSubject<ChatMessage[]>([]);
  private _presence$ = new BehaviorSubject<PresenceInfo>({ users: [], totalOnline: 0 });
  private _isConnected$ = new BehaviorSubject<boolean>(false);
  private _projectUsersCount$ = new BehaviorSubject<number>(0);

  public messages$: Observable<ChatMessage[]> = this._messages$.asObservable();
  public presence$: Observable<PresenceInfo> = this._presence$.asObservable();
  public isConnected$: Observable<boolean> = this._isConnected$.asObservable();
  public projectUsersCount$: Observable<number> = this._projectUsersCount$.asObservable();

  constructor() {
    this.initializeSignalR();
  }

  /**
   * Initialize SignalR connection
   */
  private initializeSignalR(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/signalr/teamchat')
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    // Handle incoming messages
    this.hubConnection.on('ReceiveMessage', (message: ChatMessage) => {
      const currentMessages = this._messages$.value;
      // Avoid duplicates
      if (!currentMessages.find(m => m.id === message.id)) {
        this._messages$.next([...currentMessages, message]);
      }
    });

    // Handle message history (initial load)
    this.hubConnection.on('ReceiveMessageHistory', (messages: ChatMessage[]) => {
      this._messages$.next(messages);
    });

    // Handle presence updates
    this.hubConnection.on('PresenceUpdate', (presence: PresenceInfo) => {
      this._presence$.next(presence);
    });

    // Handle project users count updates
    this.hubConnection.on('ProjectUsersCountUpdate', (count: number) => {
      this._projectUsersCount$.next(count);
      console.log('[SignalRChatProvider] Project users count update:', count);
    });

    // Handle connection state changes
    this.hubConnection.onreconnecting(() => {
      console.log('[SignalRChatProvider] Reconnecting...');
    });

    this.hubConnection.onreconnected(() => {
      console.log('[SignalRChatProvider] Reconnected');
      // Rejoin room if we were in one
      if (this.currentRoomId && this.currentUser) {
        this.joinRoomInternal(this.currentRoomId, this.currentUser);
      }
    });

    this.hubConnection.onclose(() => {
      console.log('[SignalRChatProvider] Connection closed');
      this._isConnected$.next(false);
    });
  }

  /**
   * Connect to a chat room
   */
  async connect(roomId: string, user: ChatUser): Promise<void> {
    if (!this.hubConnection) {
      throw new Error('SignalR not initialized');
    }

    // Start connection if not already started
    if (this.hubConnection.state === 'Disconnected') {
      await this.hubConnection.start();
      console.log('[SignalRChatProvider] SignalR connected');
    }

    // Leave previous room if any
    if (this.currentRoomId) {
      await this.leaveRoomInternal();
    }

    await this.joinRoomInternal(roomId, user);
  }

  private async joinRoomInternal(roomId: string, user: ChatUser): Promise<void> {
    this.currentRoomId = roomId;
    this.currentUser = user;

    // Join the room on the server
    await this.hubConnection!.invoke('JoinRoom', roomId, user);

    this._isConnected$.next(true);
    console.log(`[SignalRChatProvider] Joined room: ${roomId}`);
  }

  private async leaveRoomInternal(): Promise<void> {
    if (this.hubConnection && this.currentRoomId) {
      try {
        await this.hubConnection.invoke('LeaveRoom', this.currentRoomId);
      } catch (e) {
        console.warn('[SignalRChatProvider] Error leaving room:', e);
      }
    }

    this._messages$.next([]);
    this._presence$.next({ users: [], totalOnline: 0 });
  }

  /**
   * Disconnect from current room
   */
  async disconnect(): Promise<void> {
    await this.leaveRoomInternal();

    this.currentRoomId = null;
    this.currentUser = null;
    this._isConnected$.next(false);

    console.log('[SignalRChatProvider] Disconnected from room');
  }

  /**
   * Send a message to the current room
   */
  async sendMessage(message: Omit<ChatMessage, 'id'>): Promise<void> {
    if (!this.hubConnection || !this.currentRoomId) {
      throw new Error('Not connected to a room');
    }

    await this.hubConnection.invoke('SendMessage', this.currentRoomId, message);
  }

  /**
   * Send a system message
   */
  async sendSystemMessage(content: string): Promise<void> {
    if (!this.hubConnection || !this.currentRoomId) {
      return;
    }

    const systemMessage: Omit<ChatMessage, 'id'> = {
      content,
      senderName: 'System',
      senderEmail: '',
      timestamp: Date.now(),
      type: 'system'
    };

    await this.hubConnection.invoke('SendMessage', this.currentRoomId, systemMessage);
  }

  /**
   * Get current room ID
   */
  getCurrentRoomId(): string | null {
    return this.currentRoomId;
  }

  /**
   * Get current user
   */
  getCurrentUser(): ChatUser | null {
    return this.currentUser;
  }
}
