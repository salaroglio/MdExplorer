import { Component, Inject, OnInit } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { AppStoreService } from '../../../md-explorer/services/app-store.service';
import { StoreCatalogApp } from '../../../md-explorer/models/app-store.models';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-catalog-picker',
  templateUrl: './catalog-picker.component.html',
  styleUrls: ['./catalog-picker.component.scss']
})
export class CatalogPickerDialogComponent implements OnInit {
  catalogApps: StoreCatalogApp[] = [];
  filteredApps: StoreCatalogApp[] = [];
  existingAppIds: Set<string>;
  loading = true;
  errorMessage = '';
  searchText = '';

  constructor(
    public dialogRef: MatDialogRef<CatalogPickerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { existingAppIds: string[] },
    private appStoreService: AppStoreService,
    private translate: TranslateService
  ) {
    this.existingAppIds = new Set(data.existingAppIds || []);
  }

  ngOnInit(): void {
    this.appStoreService.getCatalog().subscribe({
      next: (catalog) => {
        this.catalogApps = catalog.apps || [];
        this.filteredApps = [...this.catalogApps];
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading catalog:', err);
        this.errorMessage = this.translate.instant('CATALOG.FAILED_LOAD');
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    const q = this.searchText.toLowerCase().trim();
    if (!q) {
      this.filteredApps = [...this.catalogApps];
    } else {
      this.filteredApps = this.catalogApps.filter(a =>
        (a.name || '').toLowerCase().includes(q) ||
        (a.description || '').toLowerCase().includes(q)
      );
    }
  }

  isAlreadyAdded(app: StoreCatalogApp): boolean {
    return this.existingAppIds.has(app.id);
  }

  selectApp(app: StoreCatalogApp): void {
    this.dialogRef.close(app);
  }

  close(): void {
    this.dialogRef.close(null);
  }
}
