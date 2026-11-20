import { Component, OnInit, Inject, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MatLegacyTableDataSource as MatTableDataSource } from '@angular/material/legacy-table';
import { MatSort } from '@angular/material/sort';
import { GITService } from '../../services/gitservice.service';
import { GitCommitInfo } from '../../models/modern-git-models';
import { TranslateService } from '@ngx-translate/core';
import { GitDialogRepo } from '../../components/git-repo-picker/git-repo-picker.component';
import { CommitMessageParts, splitCommitMessage } from './commit-message';

export interface GitHistoryDialogData {
  projectPath: string;
  projectName?: string;
  /** Il progetto e i suoi submodule: ognuno ha la sua cronologia. Assente = solo il progetto. */
  repos?: GitDialogRepo[];
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
  // Senza la colonna dell'hash (19/11/2026): l'hash sta nel dettaglio del commit, con il tasto per copiarlo.
  displayedColumns: string[] = ['date', 'author', 'message'];
  /** Il commit aperto sotto la sua riga: uno alla volta, come in un accordion. */
  expandedHash: string | null = null;
  copiedHash: string | null = null;
  private parts = new Map<string, CommitMessageParts>();
  selectedView: 'table' | 'graph' = 'table';

  /** I repository fra cui scegliere, e quello di cui si sta guardando la cronologia. */
  repos: GitDialogRepo[] = [];
  activeRepo: GitDialogRepo | null = null;

  constructor(
    public dialogRef: MatDialogRef<GitHistoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitHistoryDialogData,
    private gitService: GITService,
    private translate: TranslateService
  ) {
    this.dataSource = new MatTableDataSource<GitCommitInfo>([]);
  }

  ngOnInit(): void {
    this.repos = this.data.repos || [];
    this.activeRepo = this.repos.find(r => r.isRoot) || this.repos[0] || null;
    this.loadCommitHistory();
  }

  /** Il percorso su cui si legge: il repository scelto, o il progetto se non c'è scelta. */
  private get activePath(): string {
    return this.activeRepo?.path || this.data.projectPath;
  }

  selectRepo(repo: GitDialogRepo): void {
    this.activeRepo = repo;
    this.loadCommitHistory();
  }

  /**
   * Il commit che il PROGETTO registra per il submodule che si sta guardando. Nella cronologia
   * del submodule segna dove «vuole» il progetto: ciò che sta sopra è più nuovo della versione
   * registrata, ciò che sta sotto è già compreso.
   */
  isRecordedByProject(commit: GitCommitInfo): boolean {
    const recorded = this.activeRepo && !this.activeRepo.isRoot ? this.activeRepo.recordedCommit : null;
    return !!recorded && commit.hash === recorded;
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  loadCommitHistory(): void {
    this.isLoading = true;
    this.error = null;

    this.gitService.getCommitHistory(this.activePath, 100).subscribe({
      next: (commits) => {
        this.isLoading = false;
        this.commits = commits;
        this.parts = new Map(commits.map(c => [c.hash, splitCommitMessage(c.message)]));
        this.expandedHash = null;
        this.dataSource.data = commits;

        // If graph view is selected and we have commits, render the graph
        if (this.selectedView === 'graph' && commits.length > 0) {
          setTimeout(() => this.renderGraph(), 100);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.error = this.translate.instant('GIT_HISTORY.LOAD_ERROR');
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
      let message = this.partsOf(commit).title;
      if (ctx.measureText(message).width > maxMessageWidth) {
        while (ctx.measureText(message + '...').width > maxMessageWidth && message.length > 0) {
          message = message.substring(0, message.length - 1);
        }
        message += '...';
      }
      ctx.fillText(message, leftMargin + 20, y + 20);
    });
  }

  /** Testata, paragrafi e punti del messaggio, calcolati una volta per commit. */
  partsOf(commit: GitCommitInfo): CommitMessageParts {
    let p = this.parts.get(commit.hash);
    if (!p) { p = splitCommitMessage(commit.message); this.parts.set(commit.hash, p); }
    return p;
  }

  hasDetail(commit: GitCommitInfo): boolean {
    return this.partsOf(commit).blocks.length > 0;
  }

  toggle(commit: GitCommitInfo): void {
    this.expandedHash = this.expandedHash === commit.hash ? null : commit.hash;
  }

  formatDay(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString();
  }

  formatTime(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  copyHash(hash: string, event?: Event): void {
    event?.stopPropagation();
    navigator.clipboard.writeText(hash).then(() => {
      this.copiedHash = hash;
      setTimeout(() => { if (this.copiedHash === hash) this.copiedHash = null; }, 1500);
    }, err => console.error('[GitHistory] copia dell\'hash non riuscita:', err));
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
