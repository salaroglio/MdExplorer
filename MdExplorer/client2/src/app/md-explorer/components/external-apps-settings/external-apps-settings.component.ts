import { Component, OnInit } from '@angular/core';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { ExternalAppsService, MdeAppDefinition } from '../../services/external-apps.service';
import { MdFileService } from '../../services/md-file.service';

@Component({
  selector: 'app-external-apps-settings',
  templateUrl: './external-apps-settings.component.html',
  styleUrls: ['./external-apps-settings.component.scss']
})
export class ExternalAppsSettingsComponent implements OnInit {

  apps: MdeAppDefinition[] = [];
  isLoading = false;
  showForm = false;
  isElectron = !!(window as any).electronAPI;

  form: MdeAppDefinition = this.emptyForm();

  constructor(
    private externalAppsService: ExternalAppsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadApps();
  }

  private emptyForm(): MdeAppDefinition {
    return {
      id: '',
      name: '',
      description: '',
      icon: 'launch',
      executable: '',
      args: [],
      treePosition: 'bottom',
      singleton: true
    };
  }

  loadApps(): void {
    this.isLoading = true;
    this.externalAppsService.getApps().subscribe({
      next: apps => {
        this.apps = apps ?? [];
        this.isLoading = false;
      },
      error: () => {
        this.snackBar.open('Failed to load external apps', 'OK', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  openAddForm(): void {
    this.form = this.emptyForm();
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
  }

  async browseExecutable(): Promise<void> {
    if (!(window as any).electronAPI?.showOpenDialog) {
      this.snackBar.open('File picker is only available in the desktop version.', 'OK', { duration: 3000 });
      return;
    }
    const result = await (window as any).electronAPI.showOpenDialog({
      title: 'Select Executable',
      properties: ['openFile'],
      filters: [
        { name: 'Executables', extensions: ['exe', 'cmd', 'bat', 'sh', ''] },
        { name: 'All Files', extensions: ['*'] }
      ]
    });
    if (!result.canceled && result.filePaths?.length > 0) {
      this.form.executable = result.filePaths[0];
    }
  }

  saveApp(): void {
    if (!this.form.id?.trim()) {
      this.snackBar.open('App ID is required.', 'OK', { duration: 2000 });
      return;
    }
    if (!this.form.executable?.trim()) {
      this.snackBar.open('Executable path is required.', 'OK', { duration: 2000 });
      return;
    }
    if (!this.form.name?.trim()) {
      this.form.name = this.form.id;
    }

    // Normalize id: lowercase, no spaces
    this.form.id = this.form.id.trim().toLowerCase().replace(/\s+/g, '-');

    this.externalAppsService.addApp(this.form).subscribe({
      next: () => {
        this.snackBar.open(`App "${this.form.name}" saved.`, 'OK', { duration: 2000 });
        this.showForm = false;
        this.loadApps();
      },
      error: () => {
        this.snackBar.open('Failed to save app.', 'OK', { duration: 3000 });
      }
    });
  }

  deleteApp(app: MdeAppDefinition): void {
    if (!confirm(`Remove "${app.name}" from this project?`)) return;

    this.externalAppsService.deleteApp(app.id).subscribe({
      next: () => {
        this.snackBar.open(`App "${app.name}" removed.`, 'OK', { duration: 2000 });
        this.loadApps();
      },
      error: () => {
        this.snackBar.open('Failed to remove app.', 'OK', { duration: 3000 });
      }
    });
  }
}
