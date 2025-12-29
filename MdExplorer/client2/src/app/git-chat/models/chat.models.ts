/**
 * Chat message model
 */
export interface ChatMessage {
  id?: string;
  content: string;
  senderName: string;
  senderEmail: string;
  timestamp: number;
  type: 'message' | 'system';
}

/**
 * Chat user model for presence tracking
 */
export interface ChatUser {
  oderId: string;      // Unique session ID
  userId: string;       // Hash of email
  userName: string;     // git config user.name
  userEmail: string;    // git config user.email
  lastSeen: number;
  online: boolean;
}

/**
 * Presence information for a chat room
 */
export interface PresenceInfo {
  users: ChatUser[];
  totalOnline: number;
}

/**
 * Room information returned from backend
 */
export interface RoomInfo {
  hasRemote: boolean;
  roomId?: string;
  repositoryName?: string;
  remoteUrl?: string;
  user?: {
    userId: string;
    userName: string;
    userEmail: string;
  };
  error?: string;
}

/**
 * Git user information returned from backend
 */
export interface GitUserInfo {
  found: boolean;
  userId?: string;
  userName?: string;
  userEmail?: string;
  error?: string;
}

/**
 * Firebase configuration for chat
 */
export interface FirebaseConfig {
  apiKey: string;
  authDomain: string;
  databaseURL: string;
  projectId: string;
  storageBucket: string;
  messagingSenderId: string;
  appId: string;
  measurementId?: string;
}

/**
 * Chat connection state
 */
export type ChatConnectionState = 'disconnected' | 'connecting' | 'connected' | 'error';

/**
 * Chat settings stored in localStorage
 */
export interface ChatSettings {
  notificationsEnabled: boolean;
  soundEnabled: boolean;
}

/**
 * Default chat settings
 */
export const DEFAULT_CHAT_SETTINGS: ChatSettings = {
  notificationsEnabled: true,
  soundEnabled: true
};
