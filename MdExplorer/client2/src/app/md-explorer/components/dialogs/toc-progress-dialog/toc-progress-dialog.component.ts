import { Component, Inject, OnInit } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';

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
    @Inject(MAT_DIALOG_DATA) public data: TocProgressData,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
  }

  updateProgress(progressData: any): void {
    this.data.processed = progressData.processed || 0;
    this.data.total = progressData.total || 0;
    this.data.percentComplete = progressData.percentComplete || 0;
    this.data.status = progressData.status || this.translate.instant('TOC_PROGRESS.PROCESSING');
    
    // Estrai il nome del file corrente dallo status se contiene informazioni sui batch
    if (progressData.status && progressData.status.includes('batch')) {
      this.data.currentFile = progressData.status;
    }
  }
}