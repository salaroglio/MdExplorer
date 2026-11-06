import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';

interface PackageFile {
  name: string;
  source: string;
  bytes: number;
  truncated: boolean;
  lines: number;
  redactions: { [kind: string]: number };
  preview: string;
}

interface PackagePreview {
  zipBytes: number;
  redactions: { [kind: string]: number };
  files: PackageFile[];
}

/**
 * «Invia diagnostica» (sprint 2026-11-06-Invia-Diagnostica, D1): l'anteprima del pacchetto di log, già oscurato, prima di
 * qualsiasi invio. Chi invia vede quali file partono, da dove vengono, cosa è stato tolto, e la coda di ciascuno.
 * D1: solo «Salva lo zip». L'invio al relay arriva con la D3.
 */
@Component({
  selector: 'app-diagnostics-send',
  templateUrl: './diagnostics-send.component.html',
  styleUrls: ['./diagnostics-send.component.scss']
})
export class DiagnosticsSendDialogComponent implements OnInit {
  preview: PackagePreview | null = null;
  loading = true;
  saving = false;
  error = '';

  constructor(
    public dialogRef: MatDialogRef<DiagnosticsSendDialogComponent>,
    private http: HttpClient,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.http.get<PackagePreview>('../api/Diagnostics/Package').subscribe({
      next: p => { this.preview = p; this.loading = false; },
      error: err => {
        this.error = this.translate.instant('DIAGNOSTICS.LOAD_FAILED', { error: err?.error?.error || err?.message || err });
        this.loading = false;
      }
    });
  }

  kb(bytes: number): string {
    return bytes >= 1024 * 1024 ? `${(bytes / 1024 / 1024).toFixed(1)} MB` : `${Math.max(1, Math.round(bytes / 1024))} KB`;
  }

  count(r: { [kind: string]: number } | undefined, kind: string): number {
    return (r && r[kind]) || 0;
  }

  saveZip(): void {
    this.saving = true;
    this.http.get('../api/Diagnostics/Package/zip', { responseType: 'blob', observe: 'response' }).subscribe({
      next: res => {
        const disposition = res.headers.get('content-disposition') || '';
        const name = /filename=([^;]+)/.exec(disposition)?.[1]?.trim() || 'mde-diagnostica.zip';
        const link = document.createElement('a');
        link.href = URL.createObjectURL(res.body as Blob);
        link.download = name;
        link.click();
        setTimeout(() => URL.revokeObjectURL(link.href), 10000);
        this.saving = false;
      },
      error: err => {
        this.error = this.translate.instant('DIAGNOSTICS.SAVE_FAILED', { error: err?.message || err });
        this.saving = false;
      }
    });
  }
}
