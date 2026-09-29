import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { forkJoin, Subscription } from 'rxjs';
import { AiChatService } from '../../../services/ai-chat.service';
import {
  E2eOwnSettings, E2ePlan, E2ePrerequisites, E2eReplayResult, E2eRequirement, E2eService, E2eSetting, E2eSettingsState
} from '../../services/e2e.service';

export interface E2eDialogData {
  /** Full path of the .e2e.md or of the folder. */
  path: string;
  name: string;
  projectPath: string;
}

type Choice = 'inherit' | 'yes' | 'no';
type SettingKey = 'commitAfterRun' | 'headless';
type EngineChoice = 'inherit' | 'claude' | 'copilot' | 'opencode';

interface LogLine {
  kind: 'tool' | 'step' | 'error';
  text: string;
}

/**
 * The window of the e2e tests of a .e2e.md or of a folder (sprint 2026-09-26-Test-E2E-Da-Markdown, F4e):
 * the run settings (inherit / yes / no, with the effective value and where it comes from — the particular
 * wins over the general), the checks MdExplorer makes before a launch, the installation of what is
 * missing (the secondary wizard, D7), then the launch with its progress. The summary goes to the
 * MarkAgent tab.
 */
@Component({
  selector: 'app-e2e-dialog',
  templateUrl: './e2e-dialog.component.html',
  styleUrls: ['./e2e-dialog.component.scss']
})
export class E2eDialogComponent implements OnInit, OnDestroy {
  /** Fixed list: the template iterates it, nothing allocates per change detection. */
  readonly settingRows: { key: SettingKey; label: string; hint: string }[] = [
    { key: 'commitAfterRun', label: 'E2E.COMMIT_AFTER_RUN', hint: 'E2E.COMMIT_AFTER_RUN_HINT' },
    { key: 'headless', label: 'E2E.HEADLESS', hint: 'E2E.HEADLESS_HINT' },
  ];
  readonly requirementRows: { key: 'electron' | 'browser' | 'playwrightMcp'; label: string }[] = [
    { key: 'electron', label: 'E2E.REQ_ELECTRON' },
    { key: 'browser', label: 'E2E.REQ_BROWSER' },
    { key: 'playwrightMcp', label: 'E2E.REQ_PLAYWRIGHT' },
  ];

  /**
   * The engines that can run the tests (D10 of sprint 2026-09-29-Motore-LLM-Unico): the tests have an engine of
   * their own, always in a session of their own; «inherit» ends up at the project's. Never the MarkAgent tab's.
   */
  readonly engines: { id: EngineChoice; label: string }[] = [
    { id: 'claude', label: 'Claude Code' },
    { id: 'copilot', label: 'Copilot' },
    { id: 'opencode', label: 'opencode' },
  ];
  engineChoice: EngineChoice = 'inherit';
  modelText = '';
  /** «= Claude Code (sonnet), dal progetto»: computed when the settings arrive, not in a getter. */
  engineEffective = '';

  settings: E2eSettingsState | null = null;
  choices: Record<SettingKey, Choice> = { commitAfterRun: 'inherit', headless: 'inherit' };
  plan: E2ePlan | null = null;
  prerequisites: E2ePrerequisites | null = null;

  loading = true;
  saving = false;
  installing: string | null = null;
  error: string | null = null;

  running = false;
  finished = false;
  runErrors: string[] = [];
  currentFile: string | null = null;
  progress = '';
  log: LogLine[] = [];
  results: { file: string; runFolder: string; answer: string; tab: boolean }[] = [];
  private currentInTab = false;
  private lostSub: Subscription | null = null;
  replaying = false;
  restoring = false;
  replayResults: E2eReplayResult[] | null = null;
  private answer = '';
  /** What the agent wrote after its last tool: its conclusion, without the running commentary. */
  private conclusion = '';
  private channelId = '';
  private channelSub: Subscription | null = null;

  constructor(
    public dialogRef: MatDialogRef<E2eDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: E2eDialogData,
    private e2e: E2eService,
    private aiChat: AiChatService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.refresh();
    // A dropped connection never reports the end of a run started on it: stop waiting and say so.
    this.lostSub = this.aiChat.connectionLost$.subscribe(() => {
      if (!this.running) return;
      this.runErrors = [...this.runErrors, this.translate.instant('E2E.CONNECTION_LOST')];
      this.running = false;
      this.finished = true;
      this.channelSub?.unsubscribe();
      this.channelSub = null;
    });
  }

  ngOnDestroy(): void {
    this.channelSub?.unsubscribe();
    this.lostSub?.unsubscribe();
  }

  /** The credentials file must be excluded from git before a run (D12): MdExplorer adds the line. */
  get needsGitIgnore(): boolean {
    return (this.plan?.errors || []).some(e => e.includes('non è escluso da git'));
  }

  addToGitIgnore(): void {
    this.e2e.addToGitIgnore(this.data.path, this.data.projectPath).subscribe({
      next: () => this.refresh(),
      error: err => this.error = err?.error?.error || err?.message || String(err),
    });
  }

  /** Scripts that can be replayed: at least one script still matching its test, a .NET SDK and the packages downloaded. */
  get canReplay(): boolean {
    const browser = this.prerequisites?.browserArgument;
    return !!this.plan?.dotnet?.ok && (browser === 'chrome' || browser === 'msedge') && !this.running && !this.replaying && !this.loading
      && !this.restoring && !this.replayPackagesMissing
      && (this.plan?.items || []).some(i => (i.scripts || []).some(s => !s.stale));
  }

  /** A replayed file with a failed test or a problem: its analysis can go to MarkAgent (D6). */
  canAnalyze(r: E2eReplayResult): boolean {
    return !!r.problem || (r.outcomes || []).some(o => !o.passed);
  }

  /**
   * «Analizza con MarkAgent»: the analysis is a conversation with the user (D4), so the message goes to the chat of
   * the MarkAgent tab, same session. The message only says where things are; how to analyse is in the mde-e2e skill.
   * Never sent in silence: a busy tab or a missing one is said here.
   */
  analyze(r: E2eReplayResult): void {
    if (this.aiChat.isStreaming) {
      this.runErrors = [...this.runErrors, this.translate.instant('E2E.ANALYZE_BUSY')];
      return;
    }
    if (!this.aiChat.showTab()) {
      this.runErrors = [...this.runErrors, this.translate.instant('E2E.ANALYZE_NO_TAB')];
      return;
    }
    this.dialogRef.close();
    this.aiChat.sendMessage(this.analysisPrompt(r));
  }

  private analysisPrompt(r: E2eReplayResult): string {
    const firstLine = (text: string | null) => {
      const line = (text || '').trim().split('\n')[0] || '';
      return line.length > 300 ? line.slice(0, 300) + '…' : line;
    };
    // The first line of a Playwright failure («Locator expected to be visible») does not say which check failed:
    // the lines naming the expectation do (seen in the app on 29/09/2026). The whole message is in the log.
    const essential = (text: string | null) => {
      const lines = (text || '').trim().split('\n').map(l => l.trim()).filter(l => l.length > 0);
      const picked = [lines[0] || '', ...lines.slice(1).filter(l => /^(- )?(Expect|waiting for|Error:)/.test(l)).slice(0, 2)];
      const joined = picked.join(' — ');
      return joined.length > 500 ? joined.slice(0, 500) + '…' : joined;
    };
    const lines = [`Analizza il rigioco fallito degli script di \`${r.file}\` (skill mde-e2e, «Analizzare un rigioco fallito»).`];
    for (const o of (r.outcomes || []).filter(x => !x.passed)) {
      lines.push(`- T${o.test} (\`${o.script}\`): ${essential(o.message)}`);
    }
    if (r.problem) lines.push(`- Problema del rigioco: ${firstLine(r.problem)}`);
    if (r.runFolder) {
      const logs = (r.logs || []).length > 0 ? ` — registri: ${r.logs.join(', ')}` : ' — nessun registro';
      lines.push(`Cartella del rigioco: \`${r.runFolder}\`${logs}`);
    }
    return lines.join('\n');
  }

  /** A tests project whose packages are not downloaded: the replay waits for the user's consent (D3, D7). */
  get replayPackagesMissing(): boolean {
    return (this.plan?.items || []).some(i => !!i.replayPackages && !i.replayPackages.restored);
  }

  restoreReplayPackages(): void {
    this.restoring = true;
    this.error = null;
    this.e2e.restoreReplayPackages(this.data.path, this.data.projectPath).subscribe({
      next: () => {
        this.restoring = false;
        this.e2e.getPlan(this.data.path, this.data.projectPath).subscribe(plan => this.plan = plan);
      },
      error: err => {
        this.error = err?.error?.error || err?.message || String(err);
        this.restoring = false;
      }
    });
  }

  replay(): void {
    if (!this.canReplay) return;
    this.replaying = true;
    this.replayResults = null;
    this.error = null;
    this.e2e.replay(this.data.path, this.data.projectPath).subscribe({
      next: results => {
        this.replayResults = results;
        this.replaying = false;
        this.e2e.getPlan(this.data.path, this.data.projectPath).subscribe(plan => this.plan = plan);
      },
      error: err => {
        this.error = err?.error?.error || err?.message || String(err);
        this.replaying = false;
      }
    });
  }

  /** Where the summary went: only a run in the tab session is known to MarkAgent's conversation. */
  get summaryText(): string {
    return this.translate.instant(this.results.some(r => r.tab) ? 'E2E.SUMMARY_SENT' : 'E2E.SUMMARY_SENT_DEDICATED');
  }

  get canRun(): boolean {
    return !!this.plan?.canRun && !!this.prerequisites?.readyToRun && !this.running && !this.replaying && !this.loading && !this.saving;
  }

  refresh(): void {
    this.loading = true;
    this.error = null;
    forkJoin({
      settings: this.e2e.getSettings(this.data.path, this.data.projectPath),
      plan: this.e2e.getPlan(this.data.path, this.data.projectPath),
      prerequisites: this.e2e.getPrerequisites(),
    }).subscribe({
      next: ({ settings, plan, prerequisites }) => {
        this.applySettings(settings);
        this.plan = plan;
        this.prerequisites = prerequisites;
        this.loading = false;
      },
      error: err => {
        this.error = err?.error?.error || err?.message || String(err);
        this.loading = false;
      }
    });
  }

  effective(key: SettingKey): E2eSetting | null {
    return this.settings?.effective?.[key] ?? null;
  }

  /** Where the effective value comes from, in words. */
  sourceText(key: SettingKey): string {
    const setting = this.effective(key);
    if (!setting) return '';
    if (!setting.source) return this.translate.instant('E2E.SOURCE_DEFAULT');
    if (this.settings && setting.source === this.settings.settingsFile) return this.translate.instant('E2E.SOURCE_HERE');
    return this.translate.instant('E2E.SOURCE_FROM', { source: setting.source });
  }

  choose(key: SettingKey, choice: Choice): void {
    if (this.choices[key] === choice || this.running) return;
    this.choices = { ...this.choices, [key]: choice };
    this.save();
  }

  /** The engine of the tests (D10): «inherit» removes it, and its model with it. */
  chooseEngine(choice: EngineChoice): void {
    if (this.engineChoice === choice || this.running) return;
    this.engineChoice = choice;
    this.modelText = '';
    this.save();
  }

  /** The model, saved when the field is left or Enter is pressed; empty = the engine's. */
  saveModel(): void {
    if (this.running || this.engineChoice === 'inherit') return;
    const wanted = this.modelText.trim() || null;
    if (wanted === (this.settings?.own.model ?? null)) return;
    this.save();
  }

  private save(): void {
    const own: E2eOwnSettings = {
      engine: this.engineChoice === 'inherit' ? null : this.engineChoice,
      model: this.engineChoice === 'inherit' ? null : (this.modelText.trim() || null),
      commitAfterRun: this.toValue(this.choices.commitAfterRun),
      headless: this.toValue(this.choices.headless),
    };
    this.saving = true;
    this.e2e.putSettings(this.data.path, this.data.projectPath, own).subscribe({
      next: settings => {
        this.applySettings(settings);
        this.saving = false;
        this.e2e.getPlan(this.data.path, this.data.projectPath).subscribe(plan => this.plan = plan);
      },
      error: err => {
        const message = err?.error?.error || err?.message || String(err);
        this.saving = false;
        // Read the real state again, then say why the choice was not saved (refresh() clears the error).
        this.refresh();
        setTimeout(() => this.error = message);
      }
    });
  }

  requirement(key: 'electron' | 'browser' | 'playwrightMcp'): E2eRequirement | null {
    return this.prerequisites ? this.prerequisites[key] : null;
  }

  install(key: 'browser' | 'playwrightMcp'): void {
    this.installing = key;
    this.error = null;
    const call = key === 'playwrightMcp' ? this.e2e.installPlaywrightMcp() : this.e2e.installChromium();
    call.subscribe({
      next: res => {
        this.prerequisites = res.report;
        this.installing = null;
      },
      error: err => {
        this.error = err?.error?.error || err?.message || String(err);
        this.installing = null;
      }
    });
  }

  run(): void {
    if (!this.canRun) return;
    this.running = true;
    this.finished = false;
    this.runErrors = [];
    this.results = [];
    this.log = [];
    this.answer = '';
    this.currentFile = null;
    this.progress = '';
    this.channelId = 'e2e-' + Date.now();

    this.channelSub?.unsubscribe();
    this.channelSub = this.aiChat.getChannelStream$(this.channelId).subscribe(evt => {
      switch (evt.type) {
        case 'e2e':
          this.onEvent(evt.data);
          break;
        case 'tool':
          this.addLog('tool', String(evt.data ?? ''));
          this.conclusion = '';
          break;
        case 'chunk':
          this.answer += evt.data ?? '';
          this.conclusion += evt.data ?? '';
          break;
        case 'error':
          this.runErrors = [...this.runErrors, String(evt.data)];
          break;
        case 'complete':
          this.onComplete();
          break;
      }
    });

    this.aiChat.runE2eTests(this.data.path, this.channelId).catch(err => {
      this.runErrors = [...this.runErrors, err?.message || String(err)];
      this.onComplete();
    });
  }

  stop(): void {
    this.aiChat.cancelPrompt();
  }

  close(): void {
    this.dialogRef.close(this.finished);
  }

  private onEvent(event: any): void {
    switch (event?.type) {
      case 'refused':
        this.runErrors = [...this.runErrors, ...(event.errors || [])];
        break;
      case 'prerequisites':
        this.prerequisites = event.report;
        this.runErrors = [...this.runErrors, this.translate.instant('E2E.MISSING_PREREQUISITES')];
        break;
      case 'skipped':
        this.addLog('step', this.translate.instant('E2E.SKIPPED', { file: event.file }));
        this.runErrors = [...this.runErrors, ...(event.errors || [])];
        break;
      case 'test-failed':
        this.runErrors = [...this.runErrors, this.translate.instant('E2E.TEST_FAILED', { file: event.file, error: event.error })];
        break;
      case 'test-start':
        this.currentInTab = event.session === 'tab';
        this.currentFile = event.file;
        this.progress = `${event.index}/${event.total}`;
        this.answer = '';
        this.conclusion = '';
        this.addLog('step', this.translate.instant(event.session === 'tab' ? 'E2E.STARTED_TAB' : 'E2E.STARTED_DEDICATED', { file: event.file }));
        break;
      case 'test-end':
        this.results = [...this.results, {
          file: event.file,
          runFolder: event.runFolder,
          answer: (this.conclusion.trim() || event.answer || this.answer || '').trim(),
          tab: this.currentInTab,
        }];
        this.addLog('step', this.translate.instant('E2E.ENDED', { file: event.file }));
        break;
      case 'support': {
        const written = [...(event.created || []), ...(event.updated || [])];
        if (written.length > 0) {
          this.addLog('step', this.translate.instant('E2E.SUPPORT_WRITTEN', { files: written.join(', ') }));
        }
        if ((event.customized || []).length > 0) {
          this.runErrors = [...this.runErrors, this.translate.instant('E2E.SUPPORT_CUSTOMIZED', { files: event.customized.join(', ') })];
        }
        break;
      }
      case 'post-run':
        if (event.fingerprinted > 0) {
          this.addLog('step', this.translate.instant('E2E.FINGERPRINTED', { count: event.fingerprinted }));
        }
        for (const leak of event.leaks || []) {
          this.runErrors = [...this.runErrors, this.translate.instant(leak.replaced ? 'E2E.LEAK' : 'E2E.LEAK_REPORTED', { key: leak.key, file: leak.file })];
        }
        this.runErrors = [...this.runErrors, ...(event.problems || [])];
        break;
      case 'commit':
        if (event.committed) {
          this.addLog('step', this.translate.instant('E2E.COMMITTED', { sha: String(event.sha || '').slice(0, 8), message: event.message }));
        } else {
          this.runErrors = [...this.runErrors, this.translate.instant('E2E.NOT_COMMITTED', { file: event.file, reason: event.reason })];
        }
        break;
      case 'cancelled':
        this.runErrors = [...this.runErrors, this.translate.instant('E2E.CANCELLED')];
        break;
    }
  }

  private onComplete(): void {
    if (!this.running) return;
    this.running = false;
    this.finished = true;
    this.currentFile = null;
    this.channelSub?.unsubscribe();
    this.channelSub = null;

    // The summary of the launch goes to the MarkAgent tab (D25): there the user can ask about it.
    if (this.results.length > 0) {
      const parts = this.results.map(r =>
        `**${this.translate.instant('E2E.SUMMARY_TITLE')}** — \`${r.file}\`\n\n${this.lastParagraphs(r.answer)}\n\n` +
        `${this.translate.instant('E2E.SUMMARY_RUN_FOLDER')}: \`${r.runFolder}\``);
      this.aiChat.postToChat('assistant', parts.join('\n\n---\n\n'));
    }
    // The launch wrote results in the test files: read them again.
    this.e2e.getPlan(this.data.path, this.data.projectPath).subscribe(plan => this.plan = plan);
  }

  /** The agent narrates while it works: its conclusion is at the end. */
  private lastParagraphs(text: string): string {
    const trimmed = (text || '').trim();
    return trimmed.length <= 2500 ? trimmed : '…' + trimmed.slice(trimmed.length - 2500);
  }

  private addLog(kind: LogLine['kind'], text: string): void {
    const next = [...this.log, { kind, text }];
    this.log = next.length > 200 ? next.slice(next.length - 200) : next;
  }

  private applySettings(settings: E2eSettingsState): void {
    this.settings = settings;
    this.choices = {
      commitAfterRun: this.toChoice(settings.own.commitAfterRun),
      headless: this.toChoice(settings.own.headless),
    };
    this.engineChoice = (settings.own.engine as EngineChoice) || 'inherit';
    this.modelText = settings.own.model || '';
    this.engineEffective = this.describeEngine(settings);
  }

  /** The engine the tests will use, with its model, and where it comes from. */
  private describeEngine(settings: E2eSettingsState): string {
    const label = (id: string | null) => this.engines.find(e => e.id === id)?.label ?? null;
    const engine = settings.effective.engine;
    if (engine.value) {
      const model = settings.effective.model.value;
      const where = engine.source === settings.settingsFile
        ? this.translate.instant('E2E.SOURCE_HERE')
        : this.translate.instant('E2E.SOURCE_FROM', { source: engine.source });
      return `${label(engine.value)}${model ? ` (${model})` : ''}, ${where}`;
    }
    const project = label(settings.project?.engine ?? null);
    if (!project) return this.translate.instant('E2E.ENGINE_NONE');
    const model = settings.project.model;
    return `${project}${model ? ` (${model})` : ''}, ${this.translate.instant('E2E.SOURCE_PROJECT')}`;
  }

  private toChoice(value: boolean | null): Choice {
    return value === true ? 'yes' : value === false ? 'no' : 'inherit';
  }

  private toValue(choice: Choice): boolean | null {
    return choice === 'yes' ? true : choice === 'no' ? false : null;
  }
}
