import { Injectable, OnDestroy } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface TocProgressData {
  directory: string;
  processed: number;
  total: number;
  status: string;
  percentComplete: number;
}

export interface TocCompleteData {
  directory: string;
  timestamp: Date;
}

@Injectable()
export class TocProgressListener implements OnDestroy {
  private destroy$ = new Subject<void>();
  private currentSnackBarRef: any;

  constructor(
    private mdServerMessages: MdServerMessagesService,
    private snackBar: MatSnackBar
  ) {
    this.initializeListeners();
  }

  private initializeListeners(): void {
    // Listen for TOC generation progress
    this.mdServerMessages.hubConnection.on('TocGenerationProgress', (data: TocProgressData) => {
      this.onTocProgress(data);
    });

    // Listen for TOC generation completion
    this.mdServerMessages.hubConnection.on('TocGenerationComplete', (data: TocCompleteData) => {
      this.onTocComplete(data);
    });
  }

  private onTocProgress(data: TocProgressData): void {
    const message = `TOC Generation: ${data.status} (${data.processed}/${data.total} files - ${data.percentComplete}%)`;
    
    // Dismiss previous snackbar if exists
    if (this.currentSnackBarRef) {
      this.currentSnackBarRef.dismiss();
    }
    
    // Show new progress
    this.currentSnackBarRef = this.snackBar.open(message, 'Cancel', {
      duration: 0, // Keep open until dismissed
      horizontalPosition: 'right',
      verticalPosition: 'bottom'
    });
  }

  private onTocComplete(data: TocCompleteData): void {
    // Dismiss progress snackbar
    if (this.currentSnackBarRef) {
      this.currentSnackBarRef.dismiss();
      this.currentSnackBarRef = null;
    }
    
    // Show completion message
    this.snackBar.open(`TOC generation completed for ${data.directory}`, 'OK', {
      duration: 5000,
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
      panelClass: ['success-snackbar']
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    
    // Remove listeners
    this.mdServerMessages.hubConnection.off('TocGenerationProgress');
    this.mdServerMessages.hubConnection.off('TocGenerationComplete');
    
    // Dismiss any open snackbar
    if (this.currentSnackBarRef) {
      this.currentSnackBarRef.dismiss();
    }
  }
}