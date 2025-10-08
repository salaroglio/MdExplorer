import { Component, OnInit, Inject, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { GITService } from '../../services/gitservice.service';
import { GitCommitInfo } from '../../models/modern-git-models';

export interface GitHistoryDialogData {
  projectPath: string;
  projectName?: string;
}

@Component({
  selector: 'app-git-history-dialog',
  templateUrl: './git-history-dialog.component.html',
  styleUrls: ['./git-history-dialog.component.scss']
})
export class GitHistoryDialogComponent implements OnInit, AfterViewInit {
  @ViewChild('graphCanvas', { static: false }) graphCanvas: ElementRef<HTMLCanvasElement>;
  @ViewChild(MatSort) sort: MatSort;

  isLoading = true;
  error: string | null = null;
  commits: GitCommitInfo[] = [];
  dataSource: MatTableDataSource<GitCommitInfo>;
  displayedColumns: string[] = ['hash', 'date', 'author', 'message'];
  selectedView: 'table' | 'graph' = 'table';

  constructor(
    public dialogRef: MatDialogRef<GitHistoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitHistoryDialogData,
    private gitService: GITService
  ) {
    this.dataSource = new MatTableDataSource<GitCommitInfo>([]);
  }

  ngOnInit(): void {
    this.loadCommitHistory();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  loadCommitHistory(): void {
    this.isLoading = true;
    this.error = null;

    this.gitService.getCommitHistory(this.data.projectPath, 100).subscribe({
      next: (commits) => {
        this.isLoading = false;
        this.commits = commits;
        this.dataSource.data = commits;

        // If graph view is selected and we have commits, render the graph
        if (this.selectedView === 'graph' && commits.length > 0) {
          setTimeout(() => this.renderGraph(), 100);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Errore nel caricamento della cronologia dei commit';
        console.error('Error loading commit history:', err);
      }
    });
  }

  switchView(view: 'table' | 'graph'): void {
    this.selectedView = view;
    if (view === 'graph' && this.commits.length > 0) {
      setTimeout(() => this.renderGraph(), 100);
    }
  }

  renderGraph(): void {
    // Basic graph rendering using canvas
    // This is a simplified implementation - you can enhance it with @gitgraph/js
    if (!this.graphCanvas || !this.graphCanvas.nativeElement) {
      return;
    }

    const canvas = this.graphCanvas.nativeElement;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Set canvas size
    canvas.width = canvas.offsetWidth;
    canvas.height = this.commits.length * 60 + 40;

    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Draw commits
    const commitHeight = 60;
    const nodeRadius = 6;
    const leftMargin = 20;

    this.commits.forEach((commit, index) => {
      const y = index * commitHeight + 30;

      // Draw line to next commit
      if (index < this.commits.length - 1) {
        ctx.strokeStyle = '#999';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(leftMargin, y);
        ctx.lineTo(leftMargin, y + commitHeight);
        ctx.stroke();
      }

      // Draw commit node
      ctx.fillStyle = commit.isMerge ? '#ff6b6b' : '#4caf50';
      ctx.beginPath();
      ctx.arc(leftMargin, y, nodeRadius, 0, 2 * Math.PI);
      ctx.fill();

      // Draw commit info
      ctx.fillStyle = '#333';
      ctx.font = '12px monospace';
      ctx.fillText(commit.shortHash || commit.hash.substring(0, 7), leftMargin + 20, y - 5);

      ctx.font = '11px sans-serif';
      ctx.fillStyle = '#666';
      const dateStr = new Date(commit.date).toLocaleDateString();
      ctx.fillText(`${dateStr} - ${commit.author}`, leftMargin + 20, y + 8);

      // Truncate message if too long
      const maxMessageWidth = canvas.width - leftMargin - 40;
      let message = commit.message;
      if (ctx.measureText(message).width > maxMessageWidth) {
        while (ctx.measureText(message + '...').width > maxMessageWidth && message.length > 0) {
          message = message.substring(0, message.length - 1);
        }
        message += '...';
      }
      ctx.fillText(message, leftMargin + 20, y + 20);
    });
  }

  formatDate(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString();
  }

  copyHash(hash: string): void {
    navigator.clipboard.writeText(hash);
  }

  onClose(): void {
    this.dialogRef.close();
  }
}