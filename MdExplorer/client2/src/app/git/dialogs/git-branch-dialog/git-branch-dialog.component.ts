import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { GITService } from '../../services/gitservice.service';
import { IBranch, BranchInfo, CheckoutResult } from '../../models/branch';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';

export interface GitBranchDialogData {
  projectPath: string;
  projectName?: string;
}

@Component({
  selector: 'app-git-branch-dialog',
  templateUrl: './git-branch-dialog.component.html',
  styleUrls: ['./git-branch-dialog.component.scss']
})
export class GitBranchDialogComponent implements OnInit {
  currentBranch: IBranch | null = null;
  branches: BranchInfo[] = [];
  filteredBranches: BranchInfo[] = [];
  searchTerm: string = '';
  isLoading = true;
  isSwitching = false;
  error: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<GitBranchDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitBranchDialogData,
    private gitService: GITService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private serverMessages: MdServerMessagesService
  ) {}

  ngOnInit(): void {
    this.loadBranchInfo();
    this.loadBranches();
  }

  loadBranchInfo(): void {
    // Subscribe to current branch data
    this.gitService.currentBranch$.subscribe(branch => {
      if (branch && branch.name) {
        this.currentBranch = branch;
      }
    });

    // Trigger branch status update
    this.gitService.modernGetBranchStatus(this.data.projectPath).subscribe({
      next: (branch) => {
        this.currentBranch = branch;
        // Update the BehaviorSubject
        this.gitService.currentBranch$.next(branch);
      },
      error: (err) => {
        this.error = 'Errore nel caricamento delle informazioni del branch';
        console.error('Error loading branch info:', err);
      }
    });
  }

  loadBranches(): void {
    this.isLoading = true;
    this.error = null;

    this.gitService.getBranches(this.data.projectPath, true).subscribe({
      next: (branches) => {
        this.branches = branches;
        this.filteredBranches = this.sortBranches(branches);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Errore nel caricamento della lista branch';
        console.error('Error loading branches:', err);
      }
    });
  }

  sortBranches(branches: BranchInfo[]): BranchInfo[] {
    return branches.sort((a, b) => {
      // Current branch first
      if (a.isCurrentBranch) return -1;
      if (b.isCurrentBranch) return 1;

      // Then local branches
      if (!a.isRemote && b.isRemote) return -1;
      if (a.isRemote && !b.isRemote) return 1;

      // Alphabetically
      return a.name.localeCompare(b.name);
    });
  }

  filterBranches(): void {
    const term = this.searchTerm.toLowerCase();
    if (!term) {
      this.filteredBranches = this.sortBranches(this.branches);
    } else {
      const filtered = this.branches.filter(b =>
        b.name.toLowerCase().includes(term)
      );
      this.filteredBranches = this.sortBranches(filtered);
    }
  }

  async switchToBranch(branch: BranchInfo): Promise<void> {
    if (branch.isCurrentBranch) {
      this.snackBar.open('Sei già su questo branch', 'OK', { duration: 2000 });
      return;
    }

    if (this.isSwitching) {
      return; // Prevent multiple simultaneous switches
    }

    // Check for uncommitted changes
    this.gitService.getRepositoryStatus(this.data.projectPath).subscribe({
      next: (status) => {
        const hasChanges = status.hasChanges ||
                          (this.currentBranch?.somethingIsChangedInTheBranch);

        if (hasChanges) {
          this.showUncommittedChangesDialog(branch);
        } else {
          this.performCheckout(branch);
        }
      },
      error: (err) => {
        console.error('Error checking status:', err);
        // Proceed anyway
        this.performCheckout(branch);
      }
    });
  }

  private showUncommittedChangesDialog(branch: BranchInfo): void {
    const changesCount = this.currentBranch?.howManyFilesAreChanged || 0;
    const message = `Hai ${changesCount} file${changesCount !== 1 ? 's' : ''} modificati non committati. Cambiando branch potresti perdere le modifiche.`;

    const confirmed = confirm(`${message}\n\nVuoi comunque cambiare branch?`);
    if (confirmed) {
      this.performCheckout(branch, true);
    }
  }

  private performCheckout(branch: BranchInfo, force: boolean = false): void {
    this.isSwitching = true;
    this.error = null;

    // Extract branch name (remove remote prefix if present)
    let branchName = branch.name;
    if (branch.isRemote) {
      // For remote branches, remove the remote prefix (e.g., "origin/test" -> "test")
      // Handle both cases: when remoteName is set or when we need to detect it
      if (branch.remoteName) {
        branchName = branch.name.replace(`${branch.remoteName}/`, '');
      } else {
        // Fallback: remove everything before the last slash
        const slashIndex = branch.name.indexOf('/');
        if (slashIndex !== -1) {
          branchName = branch.name.substring(slashIndex + 1);
        }
      }
    }

    this.gitService.checkoutBranch(this.data.projectPath, branchName, this.serverMessages.connectionId).subscribe({
      next: (result: CheckoutResult) => {
        this.isSwitching = false;

        if (result.success) {
          const actualBranchName = result.branchName || branchName;

          this.snackBar.open(
            `Switchato a branch: ${actualBranchName}`,
            'OK',
            { duration: 3000, panelClass: ['success-snackbar'] }
          );

          // If backend returned branch name, use it directly to update current branch
          if (result.branchName) {
            this.gitService.currentBranch$.next({
              id: '',
              name: result.branchName,
              somethingIsChangedInTheBranch: false,
              howManyFilesAreChanged: 0,
              howManyCommitAreToPush: 0,
              fullPath: this.data.projectPath
            });
          }

          // Refresh branch list
          this.loadBranches();
        } else {
          this.error = result.error || 'Errore durante il cambio branch';
          this.snackBar.open(
            this.error,
            'OK',
            { duration: 5000, panelClass: ['error-snackbar'] }
          );
        }
      },
      error: (err) => {
        this.isSwitching = false;
        this.error = 'Errore di rete durante il cambio branch';
        console.error('Checkout error:', err);
        this.snackBar.open(
          this.error,
          'OK',
          { duration: 5000, panelClass: ['error-snackbar'] }
        );
      }
    });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
