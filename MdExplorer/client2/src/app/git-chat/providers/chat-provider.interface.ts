import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { ChatMessage, ChatUser, PresenceInfo } from '../models/chat.models';

/**
 * Abstract interface for chat providers
 * Allows switching between Firebase, Supabase, or custom backend
 */
export interface IChatProvider {
  /**
   * Connect to a chat room
   * @param roomId The unique room identifier (hash of repository URL)
   * @param user The current user information
   */
  connect(roomId: string, user: ChatUser): Promise<void>;

  /**
   * Disconnect from the current chat room
   */
  disconnect(): Promise<void>;

  /**
   * Send a message to the current room
   * @param message The message to send
   */
  sendMessage(message: Omit<ChatMessage, 'id'>): Promise<void>;

  /**
   * Send a system message (join/leave notifications)
   * @param content The system message content
   */
  sendSystemMessage(content: string): Promise<void>;

  /**
   * Observable stream of messages in the current room
   * Returns the most recent messages (limited)
   */
  messages$: Observable<ChatMessage[]>;

  /**
   * Observable stream of presence information
   * Shows who is currently online in the room
   */
  presence$: Observable<PresenceInfo>;

  /**
   * Observable for connection state
   */
  isConnected$: Observable<boolean>;

  /**
   * Observable for project users count (users who have the project open)
   */
  projectUsersCount$: Observable<number>;

  /**
   * Get the current room ID (null if not connected)
   */
  getCurrentRoomId(): string | null;

  /**
   * Get the current user (null if not connected)
   */
  getCurrentUser(): ChatUser | null;
}

/**
 * Injection token for the chat provider
 * Use this to inject the provider in services/components
 *
 * @example
 * constructor(@Inject(CHAT_PROVIDER) private chatProvider: IChatProvider) {}
 */
export const CHAT_PROVIDER = new InjectionToken<IChatProvider>('ChatProvider');
