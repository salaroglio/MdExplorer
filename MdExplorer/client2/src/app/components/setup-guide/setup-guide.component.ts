import { Component, EventEmitter, HostBinding, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { HarnessEnvironment, InstallJob, SetupGuideStatus } from './setup-guide.models';

export type StepState = 'checking' | 'ok' | 'missing' | 'restart' | 'attention' | 'error';
export type SetupStep = 'git' | 'harness' | 'java' | 'pandoc';

interface InstallCommand { label?: string; command: string; }

/**
 * «Che cosa installare» (19/11/2026, rifatto il 20/11 con l'aspetto del carosello di welcome.html): tre schede, una per ciò
 * che MDE non porta con sé — git, un motore per MarkAgent, Java 21, Pandoc (git dal 27/11/2026). Ogni scheda dice a che cosa serve, com'è il computer
 * ADESSO (il controllo vero del servizio) e come installarlo per il sistema in uso, accanto a una scena SVG animata.
 *
 * Un solo pezzo grafico in due posti: `mode="page"` nella colonna di sinistra della pagina dei progetti alla prima
 * installazione (la lista è vuota), `mode="window"` dentro la finestra che si apre dal menu «?» della barra del titolo.
 * Chi lo ospita decide che cosa vuol dire «finito» ascoltando `done`.
 */
@Component({
  selector: 'app-setup-guide',
  templateUrl: './setup-guide.component.html',
  styleUrls: ['./setup-guide.component.scss'],
})
export class SetupGuideComponent implements OnInit, OnDestroy {
  @Input() mode: 'page' | 'window' = 'page';
  @Output() done = new EventEmitter<void>();
  @HostBinding('class.page-mode') get pageMode(): boolean { return this.mode === 'page'; }

  readonly steps: readonly SetupStep[] = ['git', 'harness', 'java', 'pandoc'];
  index = 0;
  /** La scheda in vista, in un array che cambia SOLO quando cambia la scheda: l'*ngFor ridisegna la scena e le animazioni ripartono. */
  stepView: SetupStep[] = [this.steps[0]];
  /** Da che parte entra la scheda nuova. */
  dir: 'forward' | 'back' = 'forward';
  status: SetupGuideStatus | null = null;
  checking = false;
  loadError = '';
  copied = '';

  /** Le installazioni con winget lanciate da qui, per programma. */
  installs: Record<string, InstallJob> = {};
  installErrors: Record<string, string> = {};
  private polls: Record<string, any> = {};

  readonly engineNames: Record<string, string> = { copilot: 'GitHub Copilot CLI', claude: 'Claude Code', opencode: 'opencode' };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.recheck();
  }

  ngOnDestroy(): void {
    Object.values(this.polls).forEach(t => clearTimeout(t));
  }

  /** «Installa adesso»: solo Windows, solo per i programmi che winget conosce, e solo se il programma manca. */
  canInstall(step: string): boolean {
    return this.os === 'windows' && ['git', 'java', 'pandoc'].includes(step) && this.stateOf(step) !== 'ok';
  }

  async install(step: string): Promise<void> {
    this.installErrors[step] = '';
    try {
      const job = await firstValueFrom(this.http.post<InstallJob>('../api/SetupGuide/Install', { tool: step }));
      this.installs[step] = job;
      this.poll(step);
    } catch (e: any) {
      this.installErrors[step] = e?.error?.error || e?.message || String(e);
    }
  }

  /** Chiede lo stato finché winget lavora; alla fine ricontrolla il computer. */
  private poll(step: string): void {
    clearTimeout(this.polls[step]);
    if (this.installs[step]?.state !== 'running') { this.recheck(); return; }
    this.polls[step] = setTimeout(async () => {
      try {
        this.installs[step] = await firstValueFrom(this.http.get<InstallJob>('../api/SetupGuide/InstallStatus', { params: { tool: step } }));
      } catch (e: any) {
        this.installErrors[step] = e?.error?.error || e?.message || String(e);
        return;
      }
      this.poll(step);
    }, 1500);
  }

  get step(): SetupStep { return this.steps[this.index]; }
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

  goTo(i: number): void {
    if (i === this.index || i < 0 || i >= this.steps.length) return;
    this.dir = i > this.index ? 'forward' : 'back';
    this.index = i;
    this.stepView = [this.steps[i]];
  }

  next(): void { if (this.isLast) this.finish(); else this.goTo(this.index + 1); }
  back(): void { this.goTo(this.index - 1); }
  finish(): void { this.done.emit(); }

  /** Lo stato di una scheda, per il pallino, l'etichetta e il riquadro dello stato. */
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
      case 'git':
      case 'pandoc': {
        const p = step === 'git' ? this.status.git : this.status.pandoc;
        if (!p) return 'error';
        if (p.usable) return 'ok';
        if (p.needsRestart) return 'restart';
        return p.error ? 'error' : 'missing';
      }
    }
    return 'error';
  }

  stateIcon(state: StepState): string {
    switch (state) {
      case 'ok': return 'check_circle';
      case 'restart': return 'restart_alt';
      case 'error': return 'error';
      case 'checking': return 'hourglass_empty';
      default: return 'error_outline';
    }
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
      case 'git':
        return os === 'windows' ? [{ command: 'winget install --id Git.Git -e' }]
          : os === 'macos' ? [{ command: 'brew install git' }]
          : [{ command: 'sudo apt install git' }];
      case 'pandoc':
        return os === 'windows' ? [{ command: 'winget install --id JohnMacFarlane.Pandoc' }]
          : os === 'macos' ? [{ command: 'brew install pandoc' }]
          : [{ command: 'sudo apt install pandoc' }];
    }
    return [];
  }

  link(step: string): string {
    switch (step) {
      case 'git': return 'https://git-scm.com/downloads';
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
