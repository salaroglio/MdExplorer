import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

export interface TelegramStatus {
  state: 'not-connected' | 'off' | 'standby' | 'listening';
  installationName: string;
  botUsername: string | null;
  ownerName: string | null;
  lastError: string | null;
}

export interface TelegramPairing {
  state: 'none' | 'waiting-start' | 'waiting-bot' | 'ready' | 'failed' | 'expired';
  link?: string;
  expiresAt?: string;
  error?: string | null;
  telegramUserName?: string | null;
  botUsername?: string | null;
  recovered?: boolean;
}

/**
 * Sezione Telegram delle impostazioni (docs-internal/Sprints/2026-10-31-Telecomando-Telegram.md, F3 = D4): collega o
 * recupera il bot personale col QR, conferma di chi si è abbinato, interruttore, «Prendi il controllo», nome, rigenera,
 * scollega. Lo stato cambia anche da solo (un'altra installazione prende il bot), quindi si rilegge mentre è aperta.
 */
@Component({
  selector: 'app-telegram-settings',
  templateUrl: './telegram-settings.component.html',
  styleUrls: ['./telegram-settings.component.scss']
})
export class TelegramSettingsComponent implements OnInit, OnDestroy {
  private readonly api = '../api/telegram';

  status: TelegramStatus | null = null;
  pairing: TelegramPairing | null = null;
  /** Cambia a ogni nuovo abbinamento: il QR si ricarica. */
  qrStamp = 0;
  installationName = '';
  busy = false;
  error: string | null = null;

  private statusTimer: any = null;
  private pairingTimer: any = null;

  constructor(private http: HttpClient, private translate: TranslateService) {}

  ngOnInit(): void {
    this.loadStatus(true);
    this.statusTimer = setInterval(() => this.loadStatus(false), 3000);
    // Un abbinamento lasciato a metà (dialogo chiuso e riaperto) si ritrova.
    this.http.get<TelegramPairing>(`${this.api}/pairing`).subscribe({
      next: p => { if (p.state !== 'none') this.showPairing(p); },
      error: () => {}
    });
  }

  get connected(): boolean { return !!this.status && this.status.state !== 'not-connected'; }
  get pairingActive(): boolean { return !!this.pairing && this.pairing.state !== 'none'; }
  get waitingPhone(): boolean { return this.pairing?.state === 'waiting-start' || this.pairing?.state === 'waiting-bot'; }
  get qrUrl(): string { return `${this.api}/pairing/qr?t=${this.qrStamp}`; }

  loadStatus(resetName: boolean): void {
    this.http.get<TelegramStatus>(`${this.api}/status`).subscribe({
      next: s => {
        this.status = s;
        if (resetName) this.installationName = s.installationName;
      },
      error: e => this.error = this.message(e)
    });
  }

  startPairing(action: 'connect' | 'regenerate'): void {
    if (action === 'regenerate' && !window.confirm(this.translate.instant('TELEGRAM_SETTINGS.REGENERATE_CONFIRM'))) return;
    this.run(this.http.post<TelegramPairing>(`${this.api}/pairing`, { action }), p => {
      this.qrStamp = Date.now();
      this.showPairing(p);
    });
  }

  confirmPairing(): void {
    this.run(this.http.post<TelegramStatus>(`${this.api}/pairing/confirm`, {}), s => {
      this.stopPairingPolling();
      this.pairing = null;
      this.status = s;
      this.installationName = s.installationName;
    });
  }

  cancelPairing(): void {
    this.run(this.http.delete(`${this.api}/pairing`), () => {
      this.stopPairingPolling();
      this.pairing = null;
    });
  }

  setEnabled(enabled: boolean): void {
    this.run(this.http.post<TelegramStatus>(`${this.api}/enabled`, { enabled }), s => this.status = s);
  }

  takeControl(): void {
    this.run(this.http.post<TelegramStatus>(`${this.api}/take-control`, {}), s => this.status = s);
  }

  disconnect(): void {
    if (!window.confirm(this.translate.instant('TELEGRAM_SETTINGS.DISCONNECT_CONFIRM'))) return;
    this.run(this.http.post<TelegramStatus>(`${this.api}/disconnect`, {}), s => this.status = s);
  }

  saveInstallationName(): void {
    this.run(this.http.put<TelegramStatus>(`${this.api}/installation-name`, { name: this.installationName }), s => {
      this.status = s;
      this.installationName = s.installationName;
    });
  }

  openLink(url: string): void {
    const electron = (window as any).electronAPI;
    if (electron?.openExternal) electron.openExternal(url);
    else window.open(url, '_blank');
  }

  ngOnDestroy(): void {
    clearInterval(this.statusTimer);
    this.stopPairingPolling();
  }

  private showPairing(p: TelegramPairing): void {
    this.pairing = p;
    if (this.waitingPhone && !this.pairingTimer) {
      this.pairingTimer = setInterval(() => this.pollPairing(), 2000);
    }
  }

  private pollPairing(): void {
    this.http.get<TelegramPairing>(`${this.api}/pairing`).subscribe({
      next: p => {
        this.pairing = p.state === 'none' ? null : p;
        if (!this.waitingPhone) this.stopPairingPolling();
      },
      error: e => { this.error = this.message(e); this.stopPairingPolling(); }
    });
  }

  private stopPairingPolling(): void {
    clearInterval(this.pairingTimer);
    this.pairingTimer = null;
  }

  private run<T>(request: Observable<T>, done: (result: T) => void): void {
    this.busy = true;
    this.error = null;
    request.subscribe({
      next: r => { this.busy = false; done(r); },
      error: e => { this.busy = false; this.error = this.message(e); }
    });
  }

  /** Il testo dell'errore così come lo scrive il servizio (già nella lingua dell'interfaccia). */
  private message(e: any): string {
    return e?.error?.error || e?.message || String(e);
  }
}
