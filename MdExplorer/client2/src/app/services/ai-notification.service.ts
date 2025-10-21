import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { MdFileService } from '../md-explorer/services/md-file.service';

export interface AiFileOperationEvent {
  operationType: string;
  filePath: string;
  success: boolean;
  message: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiNotificationService {

  constructor(
    private snackBar: MatSnackBar,
    private serverMessages: MdServerMessagesService,
    private mdFileService: MdFileService
  ) {
    // Register listener for AI file operations
    this.registerAiFileOperationListener();
  }

  private registerAiFileOperationListener(): void {
    this.serverMessages.addAiFileOperationListener(
      (data: AiFileOperationEvent, objectThis: AiNotificationService) => {
        objectThis.handleAiFileOperation(data);
      },
      this
    );
  }

  private handleAiFileOperation(data: AiFileOperationEvent): void {
    console.log('[AI Notification] Received event:', data);

    const icon = this.getOperationIcon(data.operationType);
    const action = data.success ? 'Open' : 'OK';

    const snackBarRef = this.snackBar.open(
      `${icon} ${data.message}`,
      action,
      {
        duration: data.success ? 5000 : 7000,
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        panelClass: data.success ? ['success-snackbar'] : ['error-snackbar']
      }
    );

    // If successful and user clicks "Open", navigate to the file
    if (data.success) {
      snackBarRef.onAction().subscribe(() => {
        this.openFile(data.filePath);
      });
    }
  }

  private getOperationIcon(operationType: string): string {
    switch (operationType) {
      case 'create':
        return '📄';
      case 'read':
        return '📖';
      case 'update':
        return '✏️';
      default:
        return '🤖';
    }
  }

  private openFile(filePath: string): void {
    console.log('[AI Notification] Opening file:', filePath);

    // TODO: Implement file navigation
    // For now, just log the request. In the future, we can:
    // 1. Find the MdFile object by path
    // 2. Call mdFileService.setSelectedMdFileFromSideNav(mdFile)
    // 3. Navigate to the file in the editor

    // Temporary: show info that feature is coming
    this.snackBar.open(`File path: ${filePath}`, 'OK', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }
}
