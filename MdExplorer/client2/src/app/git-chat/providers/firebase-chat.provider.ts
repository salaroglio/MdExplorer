import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { initializeApp, FirebaseApp } from 'firebase/app';
import {
  getDatabase,
  Database,
  ref,
  push,
  set,
  onValue,
  off,
  query,
  orderByChild,
  limitToLast,
  onDisconnect,
  remove,
  DatabaseReference,
  Unsubscribe
} from 'firebase/database';
import { IChatProvider } from './chat-provider.interface';
import { ChatMessage, ChatUser, PresenceInfo, FirebaseConfig } from '../models/chat.models';
import { environment } from '../../../../../environments/environment';

/**
 * Firebase Realtime Database implementation of IChatProvider
 */
@Injectable()
export class FirebaseChatProvider implements IChatProvider {
  private app: FirebaseApp | null = null;
  private db: Database | null = null;

  private currentRoomId: string | null = null;
  private currentUser: ChatUser | null = null;

  private messagesRef: DatabaseReference | null = null;
  private presenceRef: DatabaseReference | null = null;
  private userPresenceRef: DatabaseReference | null = null;

  private messagesUnsubscribe: Unsubscribe | null = null;
  private presenceUnsubscribe: Unsubscribe | null = null;

  private _messages$ = new BehaviorSubject<ChatMessage[]>([]);
  private _presence$ = new BehaviorSubject<PresenceInfo>({ users: [], totalOnline: 0 });
  private _isConnected$ = new BehaviorSubject<boolean>(false);
  private _projectUsersCount$ = new BehaviorSubject<number>(0);

  public messages$: Observable<ChatMessage[]> = this._messages$.asObservable();
  public presence$: Observable<PresenceInfo> = this._presence$.asObservable();
  public isConnected$: Observable<boolean> = this._isConnected$.asObservable();
  public projectUsersCount$: Observable<number> = this._projectUsersCount$.asObservable();

  constructor() {
    this.initializeFirebase();
  }

  /**
   * Initialize Firebase app with configuration from environment
   */
  private initializeFirebase(): void {
    try {
      const config: FirebaseConfig = environment.firebase;
      if (!config || !config.apiKey) {
        console.warn('[FirebaseChatProvider] Firebase config not found in environment');
        return;
      }

      this.app = initializeApp(config, 'mdexplorer-chat');
      this.db = getDatabase(this.app);
      console.log('[FirebaseChatProvider] Firebase initialized successfully');
    } catch (error) {
      console.error('[FirebaseChatProvider] Failed to initialize Firebase:', error);
    }
  }

  /**
   * Connect to a chat room
   */
  async connect(roomId: string, user: ChatUser): Promise<void> {
    if (!this.db) {
      throw new Error('Firebase not initialized');
    }

    // Disconnect from previous room if any
    if (this.currentRoomId) {
      await this.disconnect();
    }

    this.currentRoomId = roomId;
    this.currentUser = user;

    console.log(`[FirebaseChatProvider] Connecting to room: ${roomId}`);

    // Set up references
    this.messagesRef = ref(this.db, `chatRooms/${roomId}/messages`);
    this.presenceRef = ref(this.db, `chatRooms/${roomId}/presence`);
    this.userPresenceRef = ref(this.db, `chatRooms/${roomId}/presence/${user.oderId}`);

    // Set user presence
    await set(this.userPresenceRef, {
      oderId: user.oderId,
      userId: user.userId,
      userName: user.userName,
      userEmail: user.userEmail,
      online: true,
      lastSeen: Date.now()
    });

    // Set up disconnect cleanup
    onDisconnect(this.userPresenceRef).remove();

    // Subscribe to messages (last 50)
    const messagesQuery = query(
      this.messagesRef,
      orderByChild('timestamp'),
      limitToLast(50)
    );

    this.messagesUnsubscribe = onValue(messagesQuery, (snapshot) => {
      const messages: ChatMessage[] = [];
      snapshot.forEach((child) => {
        messages.push({
          id: child.key!,
          ...child.val()
        });
      });
      // Sort by timestamp ascending
      messages.sort((a, b) => a.timestamp - b.timestamp);
      this._messages$.next(messages);
    }, (error) => {
      console.error('[FirebaseChatProvider] Messages subscription error:', error);
    });

    // Subscribe to presence
    this.presenceUnsubscribe = onValue(this.presenceRef, (snapshot) => {
      const users: ChatUser[] = [];
      snapshot.forEach((child) => {
        const userData = child.val();
        if (userData.online) {
          users.push(userData);
        }
      });
      this._presence$.next({
        users,
        totalOnline: users.length
      });
    }, (error) => {
      console.error('[FirebaseChatProvider] Presence subscription error:', error);
    });

    this._isConnected$.next(true);

    // Send join message
    await this.sendSystemMessage(`${user.userName} joined the chat`);

    console.log(`[FirebaseChatProvider] Connected to room: ${roomId}`);
  }

  /**
   * Disconnect from current room
   */
  async disconnect(): Promise<void> {
    if (!this.currentRoomId) {
      return;
    }

    console.log(`[FirebaseChatProvider] Disconnecting from room: ${this.currentRoomId}`);

    // Send leave message before disconnecting
    if (this.currentUser) {
      try {
        await this.sendSystemMessage(`${this.currentUser.userName} left the chat`);
      } catch (e) {
        // Ignore errors when sending leave message
      }
    }

    // Unsubscribe from listeners
    if (this.messagesUnsubscribe) {
      this.messagesUnsubscribe();
      this.messagesUnsubscribe = null;
    }

    if (this.presenceUnsubscribe) {
      this.presenceUnsubscribe();
      this.presenceUnsubscribe = null;
    }

    // Remove user presence
    if (this.userPresenceRef) {
      try {
        await remove(this.userPresenceRef);
      } catch (e) {
        // Ignore errors when removing presence
      }
    }

    // Reset state
    this.currentRoomId = null;
    this.currentUser = null;
    this.messagesRef = null;
    this.presenceRef = null;
    this.userPresenceRef = null;

    this._messages$.next([]);
    this._presence$.next({ users: [], totalOnline: 0 });
    this._isConnected$.next(false);

    console.log('[FirebaseChatProvider] Disconnected');
  }

  /**
   * Send a message to the current room
   */
  async sendMessage(message: Omit<ChatMessage, 'id'>): Promise<void> {
    if (!this.messagesRef) {
      throw new Error('Not connected to a room');
    }

    await push(this.messagesRef, {
      content: message.content,
      senderName: message.senderName,
      senderEmail: message.senderEmail,
      timestamp: Date.now(),
      type: 'message'
    });

    // Update last activity
    if (this.db && this.currentRoomId) {
      const activityRef = ref(this.db, `chatRooms/${this.currentRoomId}/metadata/lastActivity`);
      await set(activityRef, Date.now());
    }
  }

  /**
   * Send a system message
   */
  async sendSystemMessage(content: string): Promise<void> {
    if (!this.messagesRef) {
      return;
    }

    await push(this.messagesRef, {
      content,
      senderName: 'System',
      senderEmail: '',
      timestamp: Date.now(),
      type: 'system'
    });
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
