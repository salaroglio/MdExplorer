import { Component, OnInit } from '@angular/core';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
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
    private snackBar: MatSnackBar,
    private translate: TranslateService
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
        this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.LOAD_FAILED'), 'OK', { duration: 3000 });
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
      this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.PICKER_DESKTOP_ONLY'), 'OK', { duration: 3000 });
      return;
    }
    const result = await (window as any).electronAPI.showOpenDialog({
      title: this.translate.instant('EXTERNAL_APPS_SETTINGS.SELECT_EXECUTABLE'),
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
      this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.APP_ID_REQUIRED'), 'OK', { duration: 2000 });
      return;
    }
    if (!this.form.executable?.trim()) {
      this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.EXECUTABLE_REQUIRED'), 'OK', { duration: 2000 });
      return;
    }
    if (!this.form.name?.trim()) {
      this.form.name = this.form.id;
    }

    // Normalize id: lowercase, no spaces
    this.form.id = this.form.id.trim().toLowerCase().replace(/\s+/g, '-');

    this.externalAppsService.addApp(this.form).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.APP_SAVED', { name: this.form.name }), 'OK', { duration: 2000 });
        this.showForm = false;
        this.loadApps();
      },
      error: () => {
        this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.SAVE_FAILED'), 'OK', { duration: 3000 });
      }
    });
  }

  deleteApp(app: MdeAppDefinition): void {
    if (!confirm(this.translate.instant('EXTERNAL_APPS_SETTINGS.REMOVE_CONFIRM', { name: app.name }))) return;

    this.externalAppsService.deleteApp(app.id).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.APP_REMOVED', { name: app.name }), 'OK', { duration: 2000 });
        this.loadApps();
      },
      error: () => {
        this.snackBar.open(this.translate.instant('EXTERNAL_APPS_SETTINGS.REMOVE_FAILED'), 'OK', { duration: 3000 });
      }
    });
  }
}
