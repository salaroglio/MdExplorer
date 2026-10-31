import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable, Subscription } from 'rxjs';
import { ProjectsService } from '../../md-explorer/services/projects.service';

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
  action?: 'connect' | 'regenerate' | 'project';
}

export interface ProjectBotStatus {
  state: 'none' | 'setup' | 'configured' | 'error';
  botUsername: string | null;
  channelTitle: string | null;
  subscription: 'subscribed' | 'not-subscribed' | 'unknown' | null;
  setupState: 'waiting-channel' | 'failed' | null;
  channelLink: string | null;
  notPublished: boolean;
  error: string | null;
}

/**
 * Sezione Telegram delle impostazioni (docs-internal/Sprints/2026-10-31-Telecomando-Telegram.md, F3 = D4): collega o
 * recupera il bot personale col QR, conferma di chi si è abbinato, interruttore, «Prendi il controllo», nome, rigenera,
 * scollega. Lo stato cambia anche da solo (un'altra installazione prende il bot), quindi si rilegge mentre è aperta.
 * Sotto, il bot del progetto aperto (F4 = D5): crearlo, aggiungerlo al canale privato, iscriversi agli avvisi.
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

  /** Il progetto aperto (null dalla pagina dei progetti) e il suo bot. */
  projectPath: string | null = null;
  projectName: string | null = null;
  project: ProjectBotStatus | null = null;
  /** Il link d'invito al canale appena creato, per una sola persona. */
  inviteLink: string | null = null;
  projectNotice: string | null = null;

  private statusTimer: any = null;
  private pairingTimer: any = null;
  private projectSub: Subscription | null = null;

  constructor(private http: HttpClient, private translate: TranslateService, private projects: ProjectsService) {}

  ngOnInit(): void {
    this.loadStatus(true);
    this.projectSub = this.projects.currentProjects$.subscribe(p => {
      this.projectPath = p?.path || null;
      this.projectName = p?.name || null;
      this.project = null;
      this.inviteLink = null;
      this.loadProject();
    });
    this.statusTimer = setInterval(() => { this.loadStatus(false); this.loadProject(); }, 3000);
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
  qrOf(link: string): string { return `${this.api}/qr?text=${encodeURIComponent(link)}`; }

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
    const forProject = this.pairing?.action === 'project';
    this.run(this.http.post<TelegramStatus>(`${this.api}/pairing/confirm`, {}), s => {
      this.stopPairingPolling();
      this.pairing = null;
      this.status = s;
      if (forProject) this.loadProject();
      else this.installationName = s.installationName;
    });
  }

  // ---------------------------------------------------------------- bot di progetto

  loadProject(): void {
    if (!this.projectPath) return;
    const path = this.projectPath;
    this.http.get<ProjectBotStatus>(`${this.api}/project`, { params: { path } }).subscribe({
      next: p => { if (path === this.projectPath) this.project = p; },
      error: e => { if (path === this.projectPath) this.project = { state: 'error', error: this.message(e) } as ProjectBotStatus; }
    });
  }

  /** «Crea il bot di progetto»: prima i controlli (già un bot qui o sull'origin, repository pubblico), poi l'abbinamento. */
  createProjectBot(): void {
    if (!this.projectPath) return;
    const path = this.projectPath;
    this.run(this.http.get<{ publicRepository: boolean; origin: string | null }>(`${this.api}/project/preflight`, { params: { path } }), pre => {
      const warning = this.translate.instant(pre.publicRepository ? 'TELEGRAM_SETTINGS.PROJECT_PUBLIC_CONFIRM' : 'TELEGRAM_SETTINGS.PROJECT_CREATE_CONFIRM');
      if (!window.confirm(warning)) return;
      this.run(this.http.post<TelegramPairing>(`${this.api}/pairing`, { action: 'project', projectPath: path }), p => {
        this.qrStamp = Date.now();
        this.showPairing(p);
      });
    });
  }

  cancelProjectSetup(): void {
    if (!this.projectPath) return;
    this.run(this.http.delete(`${this.api}/project/setup`, { params: { path: this.projectPath } }), () => this.loadProject());
  }

  inviteToChannel(): void {
    this.run(this.http.post<{ link: string }>(`${this.api}/project/invite`, { projectPath: this.projectPath }), r => this.inviteLink = r.link);
  }

  sendProjectTest(): void {
    this.run(this.http.post(`${this.api}/project/test`, { projectPath: this.projectPath }),
      () => this.projectNotice = this.translate.instant('TELEGRAM_SETTINGS.PROJECT_TEST_SENT'));
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
    this.projectSub?.unsubscribe();
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
