import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { forkJoin, Subscription } from 'rxjs';
import { AiChatService } from '../../../services/ai-chat.service';
import {
  E2eOwnSettings, E2ePlan, E2ePrerequisites, E2eRequirement, E2eService, E2eSetting, E2eSettingsState
} from '../../services/e2e.service';

export interface E2eDialogData {
  /** Full path of the .e2e.md or of the folder. */
  path: string;
  name: string;
  projectPath: string;
}

type Choice = 'inherit' | 'yes' | 'no';
type SettingKey = keyof E2eOwnSettings;

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
    { key: 'dedicatedSession', label: 'E2E.DEDICATED_SESSION', hint: 'E2E.DEDICATED_SESSION_HINT' },
    { key: 'commitAfterRun', label: 'E2E.COMMIT_AFTER_RUN', hint: 'E2E.COMMIT_AFTER_RUN_HINT' },
    { key: 'headless', label: 'E2E.HEADLESS', hint: 'E2E.HEADLESS_HINT' },
  ];
  readonly requirementRows: { key: 'electron' | 'browser' | 'playwrightMcp'; label: string }[] = [
    { key: 'electron', label: 'E2E.REQ_ELECTRON' },
    { key: 'browser', label: 'E2E.REQ_BROWSER' },
    { key: 'playwrightMcp', label: 'E2E.REQ_PLAYWRIGHT' },
  ];

  settings: E2eSettingsState | null = null;
  choices: Record<SettingKey, Choice> = { dedicatedSession: 'inherit', commitAfterRun: 'inherit', headless: 'inherit' };
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
  results: { file: string; runFolder: string; answer: string }[] = [];
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
  }

  ngOnDestroy(): void {
    this.channelSub?.unsubscribe();
  }

  get canRun(): boolean {
    return !!this.plan?.canRun && !!this.prerequisites?.readyToRun && !this.running && !this.loading && !this.saving;
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
    const own: E2eOwnSettings = {
      dedicatedSession: this.toValue(this.choices.dedicatedSession),
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
        this.error = err?.error?.error || err?.message || String(err);
        this.saving = false;
        this.refresh();
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
      case 'test-start':
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
        }];
        this.addLog('step', this.translate.instant('E2E.ENDED', { file: event.file }));
        break;
      case 'post-run':
        if (event.fingerprinted > 0) {
          this.addLog('step', this.translate.instant('E2E.FINGERPRINTED', { count: event.fingerprinted }));
        }
        for (const leak of event.leaks || []) {
          this.runErrors = [...this.runErrors, this.translate.instant('E2E.LEAK', { key: leak.key, file: leak.file })];
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
      dedicatedSession: this.toChoice(settings.own.dedicatedSession),
      commitAfterRun: this.toChoice(settings.own.commitAfterRun),
      headless: this.toChoice(settings.own.headless),
    };
  }

  private toChoice(value: boolean | null): Choice {
    return value === true ? 'yes' : value === false ? 'no' : 'inherit';
  }

  private toValue(choice: Choice): boolean | null {
    return choice === 'yes' ? true : choice === 'no' ? false : null;
  }
}
