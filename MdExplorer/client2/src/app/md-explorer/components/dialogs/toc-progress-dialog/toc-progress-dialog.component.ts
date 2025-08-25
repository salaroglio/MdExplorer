import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface TocProgressData {
  directory: string;
  processed: number;
  total: number;
  status: string;
  percentComplete: number;
  currentFile?: string;
}

@Component({
  selector: 'app-toc-progress-dialog',
  templateUrl: './toc-progress-dialog.component.html',
  styleUrls: ['./toc-progress-dialog.component.scss']
})
export class TocProgressDialogComponent implements OnInit {
  
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: TocProgressData
  ) { }

  ngOnInit(): void {
  }

  updateProgress(progressData: any): void {
    this.data.processed = progressData.processed || 0;
    this.data.total = progressData.total || 0;
    this.data.percentComplete = progressData.percentComplete || 0;
    this.data.status = progressData.status || 'Elaborazione...';
    
    // Estrai il nome del file corrente dallo status se contiene informazioni sui batch
    if (progressData.status && progressData.status.includes('batch')) {
      this.data.currentFile = progressData.status;
    }
  }
}