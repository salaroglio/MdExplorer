import { Injectable, NgZone } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { Subscription } from 'rxjs';
import { MdFileService } from './md-file.service';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { ScreenshotAnnotationWizardDialogComponent, WizardDialogData } from '../components/dialogs/screenshot-annotation-wizard/screenshot-annotation-wizard-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ClipboardPasteService {
  private isInitialized = false;
  private pasteHandler?: (event: ClipboardEvent) => void;
  private signalRSubscription?: Subscription;

  constructor(
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private mdFileService: MdFileService,
    private serverMessages: MdServerMessagesService,
    private ngZone: NgZone
  ) {}

  /**
   * Initialize the global Ctrl+V listener and SignalR subscription.
   * Should be called once from the main/sidenav component.
   */
  initialize(): void {
    if (this.isInitialized) {
      console.log('[ClipboardPasteService] Already initialized');
      return;
    }

    // Setup paste event listener for Angular context
    this.pasteHandler = (event: ClipboardEvent) => {
      this.handlePaste(event);
    };
    document.addEventListener('paste', this.pasteHandler);

    // Subscribe to SignalR events from iframe Ctrl+V
    this.signalRSubscription = this.serverMessages.screenshotAnnotationRequest$.subscribe(data => {
      console.log('[ClipboardPasteService] Received SignalR event:', data);
      this.handleSignalRPaste(data);
    });

    this.isInitialized = true;
    console.log('[ClipboardPasteService] Initialized global paste listener and SignalR subscription');
  }

  /**
   * Cleanup the listener and subscription when the service is destroyed
   */
  destroy(): void {
    if (this.pasteHandler) {
      document.removeEventListener('paste', this.pasteHandler);
      this.pasteHandler = undefined;
    }
    if (this.signalRSubscription) {
      this.signalRSubscription.unsubscribe();
      this.signalRSubscription = undefined;
    }
    this.isInitialized = false;
    console.log('[ClipboardPasteService] Destroyed global paste listener and SignalR subscription');
  }

  /**
   * Handle paste event
   */
  private handlePaste(event: ClipboardEvent): void {
    // Skip if we're in an input/textarea element (allow normal paste)
    const activeElement = document.activeElement;
    if (activeElement instanceof HTMLInputElement ||
        activeElement instanceof HTMLTextAreaElement ||
        activeElement?.getAttribute('contenteditable') === 'true') {
      console.log('[ClipboardPasteService] Skipping - focus is on editable element');
      return;
    }

    // Check if clipboard has image data
    const clipboardData = event.clipboardData;
    if (!clipboardData) {
      console.log('[ClipboardPasteService] No clipboard data');
      return;
    }

    // Look for image in clipboard items
    const imageItem = Array.from(clipboardData.items).find(
      item => item.type.startsWith('image/')
    );

    if (!imageItem) {
      console.log('[ClipboardPasteService] No image in clipboard');
      return;
    }

    // Get the currently selected document
    const selectedFile = this.mdFileService.currentSelectedMdFile;
    if (!selectedFile || !selectedFile.fullPath) {
      console.log('[ClipboardPasteService] No document selected');
      this.ngZone.run(() => {
        this.snackBar.open('Select a markdown document before pasting', 'OK', {
          duration: 3000,
          verticalPosition: 'top'
        });
      });
      return;
    }

    // Prevent default paste behavior
    event.preventDefault();

    // Get image as blob
    const imageBlob = imageItem.getAsFile();
    if (!imageBlob) {
      console.error('[ClipboardPasteService] Failed to get image blob');
      return;
    }

    console.log('[ClipboardPasteService] Image found in clipboard, opening wizard');
    console.log('[ClipboardPasteService] Selected document:', selectedFile.fullPath);

    // Get SignalR connection ID
    const connectionId = this.serverMessages.connectionId || '';

    // Open wizard dialog
    this.ngZone.run(() => {
      const dialogData: WizardDialogData = {
        imageBlob: imageBlob,
        documentPath: selectedFile.fullPath,
        connectionId: connectionId
      };

      this.dialog.open(ScreenshotAnnotationWizardDialogComponent, {
        width: '1200px',
        maxWidth: '95vw',
        maxHeight: '95vh',
        data: dialogData,
        disableClose: true
      });
    });
  }

  /**
   * Check if clipboard API is available and has permission
   */
  async checkClipboardPermission(): Promise<boolean> {
    try {
      // Modern Clipboard API check
      const clipboard = navigator.clipboard as any;
      if (!clipboard || !clipboard.read) {
        console.warn('[ClipboardPasteService] Clipboard API not available');
        return false;
      }

      // Try to read clipboard (will prompt for permission)
      const permissionStatus = await navigator.permissions.query({
        name: 'clipboard-read' as PermissionName
      });

      return permissionStatus.state === 'granted' || permissionStatus.state === 'prompt';
    } catch (err) {
      console.warn('[ClipboardPasteService] Error checking clipboard permission:', err);
      // Fall back to paste event listener which doesn't need explicit permission
      return true;
    }
  }

  /**
   * Handle paste request from SignalR (triggered by iframe Ctrl+V)
   */
  private handleSignalRPaste(data: {
    success: boolean,
    imageBase64?: string,
    mimeType?: string,
    documentPath?: string,
    errorMessage?: string,
    platformHint?: string
  }): void {
    // Check if there was an error
    if (!data.success) {
      console.log('[ClipboardPasteService] SignalR paste failed:', data.errorMessage);
      this.ngZone.run(() => {
        this.snackBar.open(
          data.errorMessage || 'No image in clipboard',
          'OK',
          {
            duration: 4000,
            verticalPosition: 'top'
          }
        );
      });
      return;
    }

    // Check for image data
    if (!data.imageBase64) {
      console.error('[ClipboardPasteService] No image data in SignalR response');
      return;
    }

    // Get document path - use the one from SignalR or fall back to current selection
    let documentPath = data.documentPath;
    if (!documentPath) {
      const selectedFile = this.mdFileService.currentSelectedMdFile;
      if (!selectedFile || !selectedFile.fullPath) {
        console.log('[ClipboardPasteService] No document selected');
        this.ngZone.run(() => {
          this.snackBar.open('Select a markdown document before pasting', 'OK', {
            duration: 3000,
            verticalPosition: 'top'
          });
        });
        return;
      }
      documentPath = selectedFile.fullPath;
    }

    // Convert base64 to Blob
    const mimeType = data.mimeType || 'image/png';
    const imageBlob = this.base64ToBlob(data.imageBase64, mimeType);

    console.log('[ClipboardPasteService] Opening wizard from SignalR event');
    console.log('[ClipboardPasteService] Document path:', documentPath);
    console.log('[ClipboardPasteService] Image size:', imageBlob.size, 'bytes');

    // Get SignalR connection ID
    const connectionId = this.serverMessages.connectionId || '';

    // Open wizard dialog
    this.ngZone.run(() => {
      const dialogData: WizardDialogData = {
        imageBlob: imageBlob,
        documentPath: documentPath,
        connectionId: connectionId
      };

      this.dialog.open(ScreenshotAnnotationWizardDialogComponent, {
        width: '1200px',
        maxWidth: '95vw',
        maxHeight: '95vh',
        data: dialogData,
        disableClose: true
      });
    });
  }

  /**
   * Convert base64 string to Blob
   */
  private base64ToBlob(base64: string, mimeType: string): Blob {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
  }
}
