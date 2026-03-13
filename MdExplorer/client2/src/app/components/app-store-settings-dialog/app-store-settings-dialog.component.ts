import { Component, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';

import { AppStoreService } from '../../md-explorer/services/app-store.service';
import { AppStoreRepository } from '../../md-explorer/models/app-store.models';

@Component({
  selector: 'app-app-store-settings-dialog',
  templateUrl: './app-store-settings-dialog.component.html',
  styleUrls: ['./app-store-settings-dialog.component.scss']
})
export class AppStoreSettingsDialogComponent implements OnInit {
  appStoreRepos: AppStoreRepository[] = [];
  editingRepo: AppStoreRepository | null = null;
  repoForm: { label: string; url: string; username: string; password: string } = { label: '', url: '', username: '', password: '' };
  isAddingRepo = false;
  isSavingRepo = false;

  constructor(
    private dialogRef: MatDialogRef<AppStoreSettingsDialogComponent>,
    private appStoreService: AppStoreService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadAppStoreRepos();
  }

  loadAppStoreRepos(): void {
    this.appStoreService.getRepositories().subscribe({
      next: repos => this.appStoreRepos = repos,
      error: () => {}
    });
  }

  startAddRepo(): void {
    this.isAddingRepo = true;
    this.editingRepo = null;
    this.repoForm = { label: '', url: '', username: '', password: '' };
  }

  startEditRepo(repo: AppStoreRepository): void {
    this.editingRepo = repo;
    this.isAddingRepo = false;
    this.repoForm = { label: repo.label, url: repo.url, username: repo.username || '', password: '' };
  }

  cancelRepoEdit(): void {
    this.editingRepo = null;
    this.isAddingRepo = false;
  }

  saveRepo(): void {
    this.isSavingRepo = true;
    const data = {
      label: this.repoForm.label,
      url: this.repoForm.url,
      username: this.repoForm.username || null,
      password: this.repoForm.password || null
    };

    const obs = this.editingRepo
      ? this.appStoreService.updateRepository(this.editingRepo.id, data)
      : this.appStoreService.addRepository(data);

    obs.subscribe({
      next: () => {
        this.isSavingRepo = false;
        this.cancelRepoEdit();
        this.loadAppStoreRepos();
        this.appStoreService.notifyReposChanged();
        this.snackBar.open('Repository saved', '', { duration: 2000 });
      },
      error: () => {
        this.isSavingRepo = false;
        this.snackBar.open('Failed to save repository', '', { duration: 3000 });
      }
    });
  }

  deleteRepo(repo: AppStoreRepository): void {
    if (!confirm(`Delete repository "${repo.label}"?`)) return;
    this.appStoreService.deleteRepository(repo.id).subscribe({
      next: () => {
        this.loadAppStoreRepos();
        this.appStoreService.notifyReposChanged();
        this.snackBar.open('Repository deleted', '', { duration: 2000 });
      },
      error: () => this.snackBar.open('Failed to delete repository', '', { duration: 3000 })
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}
