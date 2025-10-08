import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { GITService } from '../../services/gitservice.service';
import { IBranch } from '../../models/branch';

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
  isLoading = true;
  error: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<GitBranchDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitBranchDialogData,
    private gitService: GITService
  ) {}

  ngOnInit(): void {
    this.loadBranchInfo();
  }

  loadBranchInfo(): void {
    this.isLoading = true;
    this.error = null;

    // Subscribe to current branch data
    this.gitService.currentBranch$.subscribe(branch => {
      if (branch && branch.name) {
        this.currentBranch = branch;
        this.isLoading = false;
      }
    });

    // Trigger branch status update
    this.gitService.modernGetBranchStatus(this.data.projectPath).subscribe({
      next: (branch) => {
        this.currentBranch = branch;
        this.isLoading = false;
        // Update the BehaviorSubject
        this.gitService.currentBranch$.next(branch);
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Errore nel caricamento delle informazioni del branch';
        console.error('Error loading branch info:', err);
      }
    });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}