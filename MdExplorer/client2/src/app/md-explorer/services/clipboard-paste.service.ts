import { Injectable, NgZone } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MdFileService } from './md-file.service';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { ScreenshotAnnotationWizardDialogComponent, WizardDialogData } from '../components/dialogs/screenshot-annotation-wizard/screenshot-annotation-wizard-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ClipboardPasteService {
  private isInitialized = false;
  private pasteHandler?: (event: ClipboardEvent) => void;

  constructor(
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private mdFileService: MdFileService,
    private serverMessages: MdServerMessagesService,
    private ngZone: NgZone
  ) {}

  /**
   * Initialize the global Ctrl+V listener.
   * Should be called once from the main/sidenav component.
   */
  initialize(): void {
    if (this.isInitialized) {
      console.log('[ClipboardPasteService] Already initialized');
      return;
    }

    this.pasteHandler = (event: ClipboardEvent) => {
      this.handlePaste(event);
    };

    document.addEventListener('paste', this.pasteHandler);
    this.isInitialized = true;
    console.log('[ClipboardPasteService] Initialized global paste listener');
  }

  /**
   * Cleanup the listener when the service is destroyed
   */
  destroy(): void {
    if (this.pasteHandler) {
      document.removeEventListener('paste', this.pasteHandler);
      this.pasteHandler = undefined;
      this.isInitialized = false;
      console.log('[ClipboardPasteService] Destroyed global paste listener');
    }
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
        this.snackBar.open('Seleziona un documento markdown prima di incollare', 'OK', {
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
        width: '900px',
        maxWidth: '95vw',
        maxHeight: '90vh',
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
}
