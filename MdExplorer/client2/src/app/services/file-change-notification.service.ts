import { Injectable } from '@angular/core';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';

/**
 * Service to handle taskbar flash notifications when files change externally.
 * Works only in Electron environment.
 */
@Injectable({
  providedIn: 'root'
})
export class FileChangeNotificationService {

  private pendingChanges = 0;
  private isInitialized = false;
  private cleanupFocusListener: (() => void) | null = null;

  // Settings key for localStorage
  private readonly SETTINGS_KEY = 'mdexplorer_file_change_notification_enabled';

  constructor(
    private serverMessages: MdServerMessagesService
  ) { }

  /**
   * Initialize the service - should be called once from AppComponent
   */
  initialize(): void {
    if (this.isInitialized) {
      console.log('[FileChangeNotification] Already initialized');
      return;
    }

    // Only initialize if we're running in Electron
    if (!(window as any).electronAPI?.flashTaskbarIcon) {
      console.log('[FileChangeNotification] Not running in Electron, skipping initialization');
      return;
    }

    console.log('[FileChangeNotification] Initializing service');
    this.isInitialized = true;

    // Subscribe to file change events via SignalR
    this.registerSignalRListeners();

    // Listen for window focus events from Electron
    this.registerWindowFocusListener();
  }

  /**
   * Check if notifications are enabled
   */
  isEnabled(): boolean {
    const stored = localStorage.getItem(this.SETTINGS_KEY);
    // Default to enabled if not set
    return stored === null ? true : stored === 'true';
  }

  /**
   * Set notification enabled state
   */
  setEnabled(enabled: boolean): void {
    localStorage.setItem(this.SETTINGS_KEY, enabled.toString());
    console.log('[FileChangeNotification] Notifications', enabled ? 'enabled' : 'disabled');

    // If disabled, clear any pending notifications
    if (!enabled) {
      this.clearPendingChanges();
    }
  }

  /**
   * Get current pending changes count
   */
  getPendingChangesCount(): number {
    return this.pendingChanges;
  }

  private registerSignalRListeners(): void {
    // File created
    this.serverMessages.addMarkdownFileCreatedListener(
      (data: any, objectThis: FileChangeNotificationService) => {
        objectThis.handleFileChange('created', data);
      },
      this
    );

    // File changed
    this.serverMessages.addMarkdownFileListener(
      (data: any, objectThis: FileChangeNotificationService) => {
        objectThis.handleFileChange('changed', data);
      },
      this
    );

    // File deleted
    this.serverMessages.addMarkdownFileDeletedListener(
      (data: any, objectThis: FileChangeNotificationService) => {
        objectThis.handleFileChange('deleted', data);
      },
      this
    );

    console.log('[FileChangeNotification] SignalR listeners registered');
  }

  private registerWindowFocusListener(): void {
    const electronAPI = (window as any).electronAPI;
    if (electronAPI?.onWindowFocused) {
      this.cleanupFocusListener = electronAPI.onWindowFocused(() => {
        console.log('[FileChangeNotification] Window focused, clearing pending changes');
        this.clearPendingChanges();
      });
    }
  }

  private async handleFileChange(type: string, data: any): Promise<void> {
    // Check if notifications are enabled
    if (!this.isEnabled()) {
      return;
    }

    const electronAPI = (window as any).electronAPI;
    if (!electronAPI) {
      return;
    }

    try {
      // Check if window is focused
      const isFocused = await electronAPI.isWindowFocused();

      if (!isFocused) {
        this.pendingChanges++;
        console.log(`[FileChangeNotification] File ${type}: ${data.name || data.fullPath}, pending: ${this.pendingChanges}`);

        // Flash the taskbar
        electronAPI.flashTaskbarIcon();

        // Set badge with count
        electronAPI.setTaskbarBadge(this.pendingChanges);
      }
    } catch (error) {
      console.error('[FileChangeNotification] Error handling file change:', error);
    }
  }

  private clearPendingChanges(): void {
    this.pendingChanges = 0;

    const electronAPI = (window as any).electronAPI;
    if (electronAPI) {
      electronAPI.stopFlashTaskbarIcon();
      electronAPI.clearTaskbarBadge();
    }
  }

  /**
   * Cleanup when service is destroyed
   */
  ngOnDestroy(): void {
    if (this.cleanupFocusListener) {
      this.cleanupFocusListener();
      this.cleanupFocusListener = null;
    }
  }
}
