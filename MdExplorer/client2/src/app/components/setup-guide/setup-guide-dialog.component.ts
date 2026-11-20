import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { firstValueFrom } from 'rxjs';
import { HarnessEnvironment, SetupGuideStatus } from './setup-guide.models';

export type StepState = 'checking' | 'ok' | 'missing' | 'restart' | 'attention' | 'error';

interface InstallCommand { label?: string; command: string; }

/**
 * «Che cosa installare» (19/11/2026): tre schede, una per ciò che MDE non porta con sé — un motore per MarkAgent, Java 21,
 * Pandoc. Ogni scheda dice a che cosa serve, com'è il computer ADESSO (il controllo vero del servizio) e come installarlo per
 * il sistema in uso. «Ricontrolla» rifà i controlli senza riavviare.
 */
@Component({
  selector: 'app-setup-guide-dialog',
  templateUrl: './setup-guide-dialog.component.html',
  styleUrls: ['./setup-guide-dialog.component.scss'],
})
export class SetupGuideDialogComponent implements OnInit {
  readonly steps = ['harness', 'java', 'pandoc'] as const;
  index = 0;
  status: SetupGuideStatus | null = null;
  checking = false;
  loadError = '';
  copied = '';

  readonly engineNames: Record<string, string> = { copilot: 'GitHub Copilot CLI', claude: 'Claude Code', opencode: 'opencode' };

  constructor(private http: HttpClient, private ref: MatDialogRef<SetupGuideDialogComponent>) {}

  ngOnInit(): void {
    this.recheck();
  }

  get step(): 'harness' | 'java' | 'pandoc' { return this.steps[this.index]; }
  get isLast(): boolean { return this.index === this.steps.length - 1; }
  get os(): 'windows' | 'linux' | 'macos' { return this.status?.os ?? 'windows'; }

  async recheck(): Promise<void> {
    this.checking = true;
    this.loadError = '';
    try {
      this.status = await firstValueFrom(this.http.get<SetupGuideStatus>('../api/SetupGuide/Status'));
    } catch (e: any) {
      this.loadError = e?.error?.error || e?.message || String(e);
    } finally {
      this.checking = false;
    }
  }

  goTo(i: number): void { this.index = i; }
  next(): void { if (this.isLast) this.ref.close(); else this.index++; }
  back(): void { if (this.index > 0) this.index--; }
  close(): void { this.ref.close(); }

  /** Lo stato di una scheda, per il pallino e per il riquadro dello stato. */
  stateOf(step: string): StepState {
    if (this.checking && !this.status) return 'checking';
    if (!this.status) return 'error';
    switch (step) {
      case 'harness': {
        const h = this.status.harness;
        if (h.error) return 'error';
        if ((h.usable?.length ?? 0) > 0) return 'ok';
        return (h.environments ?? []).some(e => e.installed) ? 'attention' : 'missing';
      }
      case 'java': {
        const j = this.status.java;
        if (j.error) return 'error';
        if (j.ok) return 'ok';
        return j.outcome === 'UserPathNotSuitable' ? 'attention' : 'missing';
      }
      case 'pandoc': {
        const p = this.status.pandoc;
        if (p.usable) return 'ok';
        if (p.needsRestart) return 'restart';
        return p.error ? 'error' : 'missing';
      }
    }
    return 'error';
  }

  get allReady(): boolean { return !!this.status && this.steps.every(s => this.stateOf(s) === 'ok'); }

  engines(): HarnessEnvironment[] {
    const order = ['copilot', 'claude', 'opencode'];
    return [...(this.status?.harness.environments ?? [])].sort((a, b) => order.indexOf(a.id) - order.indexOf(b.id));
  }

  engineState(e: HarnessEnvironment): StepState {
    if (e.usable) return 'ok';
    return e.installed ? 'attention' : 'missing';
  }

  /** I comandi da lanciare, per il sistema in uso. La versione della CLI di Copilot è quella per cui è fatto l'SDK di MDE. */
  commands(step: string): InstallCommand[] {
    const os = this.os;
    switch (step) {
      case 'harness': {
        const copilot = this.status?.copilotCliVersion ? `@github/copilot@${this.status.copilotCliVersion}` : '@github/copilot';
        return [
          { label: 'GitHub Copilot CLI', command: `npm install -g ${copilot}` },
          { label: 'Claude Code', command: 'npm install -g @anthropic-ai/claude-code' },
          { label: 'opencode', command: 'npm install -g opencode-ai' },
        ];
      }
      case 'java':
        return os === 'windows' ? [{ command: 'winget install EclipseAdoptium.Temurin.21.JDK' }]
          : os === 'macos' ? [{ command: 'brew install --cask temurin@21' }]
          : [{ command: 'sudo apt install openjdk-21-jre' }];
      case 'pandoc':
        return os === 'windows' ? [{ command: 'winget install --id JohnMacFarlane.Pandoc' }]
          : os === 'macos' ? [{ command: 'brew install pandoc' }]
          : [{ command: 'sudo apt install pandoc' }];
    }
    return [];
  }

  link(step: string): string {
    switch (step) {
      case 'harness': return 'https://nodejs.org/';
      case 'java': return 'https://adoptium.net/temurin/releases/?version=21';
      case 'pandoc': return 'https://pandoc.org/installing.html';
    }
    return '';
  }

  openLink(url: string): void {
    const api = (window as any).electronAPI;
    if (api?.openExternal) api.openExternal(url);
    else window.open(url, '_blank', 'noopener');
  }

  copy(command: string): void {
    navigator.clipboard.writeText(command).then(
      () => { this.copied = command; setTimeout(() => { if (this.copied === command) this.copied = ''; }, 1800); },
      err => console.error('[SetupGuide] copia non riuscita:', err));
  }
}
