import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { Clipboard } from '@angular/cdk/clipboard';
import { Subject } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import { P2PService, TransferInfo, P2PStatus, P2PProject } from '../../../services/p2p.service';

interface MdProjectBasic {
  id: string;
  name: string;
  path: string;
}

export interface P2PManagerDialogData {
  projectPath?: string;
}

@Component({
  selector: 'app-p2p-manager',
  templateUrl: './p2p-manager.component.html',
  styleUrls: ['./p2p-manager.component.scss']
})
export class P2PManagerComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  status: P2PStatus | null = null;
  transfers: TransferInfo[] = [];
  projects: P2PProject[] = [];
  isLoading = true;
  isLoadingProjects = false;
  magnetInput = '';
  selectedTab = 0;
  expandedProjects: Set<string> = new Set();

  constructor(
    public dialogRef: MatDialogRef<P2PManagerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: P2PManagerDialogData,
    private http: HttpClient,
    public p2pService: P2PService,
    private snackBar: MatSnackBar,
    private clipboard: Clipboard
  ) {}

  ngOnInit(): void {
    // Subscribe to status updates
    this.p2pService.status$
      .pipe(takeUntil(this.destroy$))
      .subscribe(status => {
        this.status = status;
        this.isLoading = false;
      });

    // Subscribe to transfers
    this.p2pService.transfers$
      .pipe(takeUntil(this.destroy$))
      .subscribe(transfers => {
        this.transfers = transfers;
      });

    // Subscribe to transfer events
    this.p2pService.transferComplete$
      .pipe(takeUntil(this.destroy$))
      .subscribe(transfer => {
        this.snackBar.open(`Download complete: ${transfer.name}`, 'OK', { duration: 3000 });
      });

    this.p2pService.transferError$
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ error }) => {
        this.snackBar.open(`Transfer error: ${error}`, 'OK', { duration: 5000 });
      });

    // Check availability and load data
    this.p2pService.checkAvailability();

    // Load projects with P2P
    this.loadProjects();
  }

  loadProjects(): void {
    this.isLoadingProjects = true;

    // First fetch all projects, then filter for those with P2P metadata
    this.http.get<MdProjectBasic[]>('../api/MdProjects/GetProjects')
      .pipe(
        takeUntil(this.destroy$),
        switchMap(allProjects => {
          // Pass all projects to the P2P service to check which have metadata.json
          const projectsToCheck = allProjects.map(p => ({
            id: p.id,
            name: p.name,
            path: p.path
          }));
          return this.p2pService.getProjectsWithP2P(projectsToCheck);
        })
      )
      .subscribe({
        next: (projects) => {
          this.projects = projects;
          this.isLoadingProjects = false;
        },
        error: (err) => {
          console.error('[P2PManager] Error loading projects:', err);
          this.isLoadingProjects = false;
        }
      });
  }

  toggleProject(projectId: string): void {
    if (this.expandedProjects.has(projectId)) {
      this.expandedProjects.delete(projectId);
    } else {
      this.expandedProjects.add(projectId);
    }
  }

  isProjectExpanded(projectId: string): boolean {
    return this.expandedProjects.has(projectId);
  }

  getProjectFiles(project: P2PProject): { filename: string; info: any }[] {
    if (!project.files || typeof project.files !== 'object') {
      return [];
    }
    return Object.entries(project.files).map(([filename, info]) => ({
      filename,
      info
    }));
  }

  restoreSeeding(project: P2PProject): void {
    this.snackBar.open(`Restoring seeding for ${project.name}...`, '', { duration: 0 });
    this.p2pService.restoreSeeding(project.path).subscribe({
      next: (result) => {
        this.snackBar.open(result.message, 'OK', { duration: 5000 });
        this.refreshTransfers();
      },
      error: (err) => {
        this.snackBar.open('Error: ' + err.message, 'OK', { duration: 5000 });
      }
    });
  }

  copyFileMagnet(info: any): void {
    if (info?.magnetUri) {
      this.clipboard.copy(info.magnetUri);
      this.snackBar.open('Magnet link copied to clipboard', 'OK', { duration: 2000 });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get activeTransfers(): TransferInfo[] {
    return this.transfers.filter(t => t.isDownloading);
  }

  get seedingTransfers(): TransferInfo[] {
    return this.transfers.filter(t => t.isSeeding);
  }

  get completedTransfers(): TransferInfo[] {
    return this.transfers.filter(t => t.progress >= 100 && !t.isSeeding);
  }

  refreshTransfers(): void {
    this.p2pService.refreshTransfers();
  }

  copyMagnetLink(transfer: TransferInfo): void {
    if (transfer.magnetUri) {
      this.clipboard.copy(transfer.magnetUri);
      this.snackBar.open('Magnet link copied to clipboard', 'OK', { duration: 2000 });
    }
  }

  pauseTransfer(transfer: TransferInfo): void {
    this.p2pService.pauseTransfer(transfer.infoHash).subscribe({
      next: () => {
        this.snackBar.open('Transfer paused', 'OK', { duration: 2000 });
        this.refreshTransfers();
      },
      error: (err) => {
        this.snackBar.open('Error pausing transfer: ' + err.message, 'OK', { duration: 3000 });
      }
    });
  }

  resumeTransfer(transfer: TransferInfo): void {
    this.p2pService.resumeTransfer(transfer.infoHash).subscribe({
      next: () => {
        this.snackBar.open('Transfer resumed', 'OK', { duration: 2000 });
        this.refreshTransfers();
      },
      error: (err) => {
        this.snackBar.open('Error resuming transfer: ' + err.message, 'OK', { duration: 3000 });
      }
    });
  }

  stopTransfer(transfer: TransferInfo, deleteFiles: boolean = false): void {
    const action = deleteFiles ? 'remove and delete files' : 'stop';
    if (confirm(`Are you sure you want to ${action} "${transfer.name}"?`)) {
      this.p2pService.stopTransfer(transfer.infoHash, deleteFiles).subscribe({
        next: () => {
          this.snackBar.open('Transfer stopped', 'OK', { duration: 2000 });
          this.refreshTransfers();
        },
        error: (err) => {
          this.snackBar.open('Error stopping transfer: ' + err.message, 'OK', { duration: 3000 });
        }
      });
    }
  }

  startDownload(): void {
    if (!this.magnetInput.trim()) {
      this.snackBar.open('Please enter a magnet link', 'OK', { duration: 2000 });
      return;
    }

    this.p2pService.download(this.magnetInput.trim()).subscribe({
      next: (result) => {
        if (result.success) {
          this.snackBar.open(`Download started: ${result.name}`, 'OK', { duration: 3000 });
          this.magnetInput = '';
          this.selectedTab = 0; // Switch to Transfers tab
          this.refreshTransfers();
        } else {
          this.snackBar.open('Error starting download: ' + result.error, 'OK', { duration: 3000 });
        }
      },
      error: (err) => {
        this.snackBar.open('Error starting download: ' + err.message, 'OK', { duration: 3000 });
      }
    });
  }

  formatBytes(bytes: number): string {
    return this.p2pService.formatBytes(bytes);
  }

  formatSpeed(bytesPerSecond: number): string {
    return this.p2pService.formatSpeed(bytesPerSecond);
  }

  formatEta(seconds: number): string {
    return this.p2pService.formatEta(seconds);
  }

  getStatusIcon(transfer: TransferInfo): string {
    if (transfer.status === 'paused') return 'pause';
    if (transfer.isSeeding) return 'cloud_upload';
    if (transfer.isDownloading) return 'cloud_download';
    if (transfer.progress >= 100) return 'check_circle';
    return 'hourglass_empty';
  }

  getStatusColor(transfer: TransferInfo): string {
    if (transfer.status === 'paused') return 'warn';
    if (transfer.isSeeding) return 'accent';
    if (transfer.isDownloading) return 'primary';
    if (transfer.progress >= 100) return 'primary';
    return '';
  }

  close(): void {
    this.dialogRef.close();
  }
}
