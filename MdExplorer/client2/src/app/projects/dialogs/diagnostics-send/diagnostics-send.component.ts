import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { versionInfo } from '../../../../environments/version';

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
  /** Quante rilevazioni di MarkAgent ci sono da allegare (0 = la casella non si mostra). */
  findingsAvailable: number;
  redactions: { [kind: string]: number };
  files: PackageFile[];
}

/**
 * «Invia diagnostica» (sprint 2026-11-06-Invia-Diagnostica, D1): l'anteprima del pacchetto di log, già oscurato, prima di
 * qualsiasi invio. Chi invia vede quali file partono, da dove vengono, cosa è stato tolto, e la coda di ciascuno.
 * D3: «Invia» spedisce lo stesso pacchetto al relay (errantia) e mostra il codice da dire a chi aiuta.
 * 11/11/2026: a richiesta partono anche le rilevazioni di MarkAgent (ciò che ha trovato sul codice, in forme e numeri):
 * con la casella spuntata l'anteprima si rifà e le mostra, come tutto il resto.
 */
@Component({
  selector: 'app-diagnostics-send',
  templateUrl: './diagnostics-send.component.html',
  styleUrls: ['./diagnostics-send.component.scss']
})
export class DiagnosticsSendDialogComponent implements OnInit {
  preview: PackagePreview | null = null;
  loading = true;
  /** Allegare le rilevazioni di MarkAgent: lo sceglie chi invia, mai da solo. */
  withFindings = false;
  saving = false;
  sending = false;
  sent: { code: string; expiresAt: string } | null = null;
  copied = false;
  error = '';

  constructor(
    public dialogRef: MatDialogRef<DiagnosticsSendDialogComponent>,
    private http: HttpClient,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  /** L'anteprima di ciò che parte: si rifà quando cambia la casella delle rilevazioni. */
  load(): void {
    this.loading = true;
    this.error = '';
    this.http.get<PackagePreview>('../api/Diagnostics/Package', { params: { findings: String(this.withFindings) } }).subscribe({
      next: p => { this.preview = p; this.loading = false; },
      error: err => {
        this.error = this.translate.instant('DIAGNOSTICS.LOAD_FAILED', { error: err?.error?.error || err?.message || err });
        this.loading = false;
      }
    });
  }

  isFinding(f: PackageFile): boolean {
    return f.name.startsWith('rilevazioni/');
  }

  kb(bytes: number): string {
    return bytes >= 1024 * 1024 ? `${(bytes / 1024 / 1024).toFixed(1)} MB` : `${Math.max(1, Math.round(bytes / 1024))} KB`;
  }

  count(r: { [kind: string]: number } | undefined, kind: string): number {
    return (r && r[kind]) || 0;
  }

  send(): void {
    this.sending = true;
    this.error = '';
    this.http.post<{ code: string; expiresAt: string }>('../api/Diagnostics/Package/send', { version: versionInfo.version, findings: this.withFindings }).subscribe({
      next: r => { this.sent = r; this.sending = false; },
      error: err => {
        this.error = this.translate.instant('DIAGNOSTICS.SEND_FAILED', { error: err?.error?.error || err?.message || err });
        this.sending = false;
      }
    });
  }

  expiry(iso: string): string {
    const d = new Date(iso);
    return isNaN(d.getTime()) ? iso : d.toLocaleString();
  }

  copyCode(): void {
    if (!this.sent?.code) { return; }
    navigator.clipboard.writeText(this.sent.code).then(() => { this.copied = true; setTimeout(() => this.copied = false, 2000); });
  }

  saveZip(): void {
    this.saving = true;
    this.http.get('../api/Diagnostics/Package/zip', { params: { findings: String(this.withFindings) }, responseType: 'blob', observe: 'response' }).subscribe({
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
