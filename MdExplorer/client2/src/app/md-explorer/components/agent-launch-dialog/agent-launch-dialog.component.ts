import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialog as MatDialog,
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { AgentLaunchService, AgentParam } from '../../services/agent-launch.service';
import { ProjectSettingsService } from '../../../projects/services/project-settings.service';
import { AgentScheduleService } from '../../services/agent-schedule.service';
import { AgentQueue, AgentQueueService } from '../../services/agent-queue.service';
import { AgentScheduleDialogComponent } from '../agent-schedule-dialog/agent-schedule-dialog.component';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../commons/components/show-file-system/show-file-metadata';

export interface AgentLaunchDialogData {
  projectPath: string;
  agentFilePath: string;
  agentName: string;
}

/**
 * Mask to prepare and launch the prompt of a *.agent.md agent.
 * Free text → "Normalize with Copilot" (mde-prompt-for-agents skill) → parameter
 * pickers (ParameterExtractor grammar) → launch headless.
 */
@Component({
  selector: 'app-agent-launch-dialog',
  templateUrl: './agent-launch-dialog.component.html',
  styleUrls: ['./agent-launch-dialog.component.scss'],
})
export class AgentLaunchDialogComponent implements OnInit {
  // Coda di lavoro dell'agente (§12.5/§12.6, Fase 6d): parcheggiati + federate in attesa.
  queue: AgentQueue | null = null;
  queueLoading = false;
  // What the user reads/edits in the textarea: the free text before normalization,
  // and ONLY the `## Task` body afterwards. The machine scaffolding (title + the
  // ```params declaration block) is kept out of sight in `headerPart` — it still
  // drives the pickers and is re-attached on launch/save/template.
  prompt = '';
  private headerPart = '';

  parameters: AgentParam[] = [];
  paramValues: { [name: string]: string } = {};

  isNormalizing = false;
  isLaunching = false;

  /**
   * Dove far lavorare l'agente in QUESTO lancio.
   *
   * Il default lo dà l'impostazione del progetto, ma i due gesti sono diversi: «lancia e guarda
   * cosa fa» su un ritocco vuole il progetto, con il risultato lì sotto gli occhi; un lavoro
   * vero vuole l'isolamento, perché nel progetto finirebbe mescolato al tuo sul tuo ramo — e
   * quando lo committi lo firmi tu.
   */
  useWorktree = false;
  /** Senza git non ci sono rami né posti di lavoro: la spunta non ha nulla da offrire. */
  canIsolate = false;
  aiError: string | null = null;

  // Splits a normalized prompt at its `## Task` heading. Header = everything before it
  // (title + params block); task = the body the user actually edits. When there is no
  // `## Task` (e.g. still free text), the whole thing is the editable body.
  private splitNormalized(full: string): { header: string; task: string } {
    const m = (full || '').match(/^([\s\S]*?)\r?\n#{1,6}[ \t]*Task[ \t]*\r?\n+([\s\S]*)$/i);
    if (m) return { header: m[1].replace(/\s+$/, ''), task: m[2].trim() };
    return { header: '', task: (full || '').trim() };
  }

  // Re-attaches the hidden header to the edited task body → the full normalized prompt
  // used for launching, saving the draft, and writing the shared template.
  private composeFull(): string {
    const task = (this.prompt || '').trim();
    if (!this.headerPart) return task;
    return this.headerPart.replace(/\s+$/, '') + '\n\n## Task\n\n' + task + '\n';
  }

  // Stores a full normalized prompt as (hidden header, visible task body).
  private applyNormalized(full: string): void {
    const { header, task } = this.splitNormalized(full);
    this.headerPart = header;
    this.prompt = task;
  }

  ngOnInit(): void {
    this.loadQueue();
    this.loadIsolationDefault();
  }

  /**
   * Il default della spunta viene dall'impostazione del progetto: la spunta decide QUESTO
   * lancio, l'impostazione dice come si lavora di solito.
   */
  private loadIsolationDefault(): void {
    this.projectSettingsService.getAgentWorktreesSetting(this.data.projectPath).subscribe({
      next: (res) => {
        this.useWorktree = !!res?.enabled;
        // Il default del backend è vero solo su un repo con un origin: se lì è falso e nessuno
        // ha scelto, l'isolamento non è nemmeno possibile e la spunta mentirebbe.
        this.canIsolate = !!res?.defaultValue || !!res?.enabled;
      },
      // Best-effort: se l'impostazione non si legge, si lancia dove si è sempre lanciato.
      error: () => { this.useWorktree = false; this.canIsolate = false; },
    });
  }

  loadQueue(): void {
    this.queueLoading = true;
    this.agentQueueService.queue(this.data.agentName, this.data.projectPath).subscribe({
      next: (q) => { this.queue = q; this.queueLoading = false; },
      error: () => { this.queueLoading = false; /* best-effort: la coda non deve rompere il lancio */ },
    });
  }

  get queueCount(): number {
    return (this.queue?.messages?.length || 0) + (this.queue?.federatedPending?.length || 0);
  }

  forceQueued(id: string): void {
    this.agentQueueService.force(id).subscribe({ next: () => this.loadQueue(), error: () => this.loadQueue() });
  }

  discardQueued(id: string): void {
    this.agentQueueService.discard(id).subscribe({ next: () => this.loadQueue(), error: () => this.loadQueue() });
  }

  constructor(
    public dialogRef: MatDialogRef<AgentLaunchDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentLaunchDialogData,
    private agentLaunchService: AgentLaunchService,
    private projectSettingsService: ProjectSettingsService,
    private agentScheduleService: AgentScheduleService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    private agentQueueService: AgentQueueService,
  ) {
    // Precedence: the per-user local draft (UserDB) wins; if there is none, seed from
    // the shared template stored inside the .agent.md (travels with git).
    this.agentScheduleService.getDraft(data.projectPath, data.agentFilePath).subscribe({
      next: (r) => {
        if (r.draft?.prompt && !this.prompt) {
          this.applyNormalized(r.draft.prompt);
          try {
            this.paramValues = JSON.parse(r.draft.parameterValuesJson || '{}') || {};
          } catch {
            this.paramValues = {};
          }
          this.detectParams();
        } else if (!this.prompt) {
          this.seedFromTemplate();
        }
      },
      error: () => { this.seedFromTemplate(); },
    });
  }

  /** Seeds the dialog from the shared template in the .agent.md when no local draft exists. */
  private seedFromTemplate(): void {
    this.agentScheduleService.getTemplate(this.data.agentFilePath).subscribe({
      next: (r) => {
        if (r.template && !this.prompt) {
          this.applyNormalized(r.template);
          this.detectParams();
        }
      },
      error: () => { /* template is optional */ },
    });
  }

  private saveDraft(): void {
    if (!this.prompt || !this.prompt.trim()) return;
    this.agentScheduleService
      .saveDraft(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues)
      .subscribe({ error: (err) => console.warn('Draft save failed:', err) });
  }

  /**
   * "Save only for me": persists prompt + parameter values as a per-user local draft
   * (UserDB, AppData) and closes. Nothing is written to the shared .agent.md.
   */
  saveLocal(): void {
    if (!this.prompt || !this.prompt.trim()) return;
    this.agentScheduleService
      .saveDraft(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues)
      .subscribe({
        next: () => {
          this.snackBar.open(this.translate.instant('AGENT_LAUNCH.SAVED_LOCAL'), undefined, { duration: 3000 });
          this.dialogRef.close(null);
        },
        error: (err) => {
          this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          console.warn('Draft save failed:', err);
        },
      });
  }

  /**
   * "Save as template (shared)": writes the prompt into the managed section at the end
   * of the .agent.md (goes to git, seen by everyone). Parameter values stay local — they
   * are machine-specific — so they are also saved as a local draft here for convenience.
   */
  saveAsTemplate(): void {
    if (!this.prompt || !this.prompt.trim()) return;
    this.aiError = null;
    this.agentScheduleService.saveTemplate(this.data.agentFilePath, this.composeFull()).subscribe({
      next: () => {
        // Keep the machine-specific parameter values as a local draft.
        this.agentScheduleService
          .saveDraft(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues)
          .subscribe({ error: (err) => console.warn('Draft save failed:', err) });
        this.snackBar.open(this.translate.instant('AGENT_LAUNCH.SAVED_TEMPLATE'), undefined, { duration: 3000 });
        this.dialogRef.close(null);
      },
      error: (err) => {
        this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
        console.warn('Template save failed:', err);
      },
    });
  }

  /** Substitutes the chosen values and opens the scheduling dialog with the ready prompt. */
  saveAsSchedule(): void {
    if (!this.canLaunch()) return;
    this.aiError = null;
    this.saveDraft();
    this.agentLaunchService.prepare(this.composeFull(), this.paramValues).subscribe({
      next: (r) => {
        if (!r.success || !r.preparedPrompt) {
          this.aiError = r.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          return;
        }
        this.dialogRef.close(null);
        this.dialog.open(AgentScheduleDialogComponent, {
          width: '760px',
          data: {
            projectPath: this.data.projectPath,
            agentFilePath: this.data.agentFilePath,
            agentName: this.data.agentName,
            preparedPrompt: r.preparedPrompt,
          },
        });
      },
      error: (err) => {
        this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
      },
    });
  }

  normalizeWithCopilot(): void {
    if (!this.prompt || !this.prompt.trim()) {
      return;
    }
    this.isNormalizing = true;
    this.aiError = null;

    this.agentLaunchService.normalize(this.data.projectPath, this.composeFull()).subscribe({
      next: (response) => {
        this.isNormalizing = false;
        if (response.success && response.normalizedPrompt) {
          this.applyNormalized(response.normalizedPrompt);
          this.setParameters(response.parameters || []);
          this.saveDraft();
        } else {
          this.aiError = response.error || this.translate.instant('AGENT_LAUNCH.NORMALIZE_ERROR');
        }
      },
      error: (err) => {
        this.isNormalizing = false;
        this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.NORMALIZE_ERROR');
        console.error('Error normalizing agent prompt:', err);
      },
    });
  }

  /** Re-detects parameters after manual edits to the prompt. */
  detectParams(): void {
    if (!this.prompt || !this.prompt.trim()) {
      this.setParameters([]);
      return;
    }
    this.agentLaunchService.extractParams(this.composeFull()).subscribe({
      next: (response) => this.setParameters(response.parameters || []),
      error: (err) => console.error('Error extracting agent params:', err),
    });
  }

  async openPicker(param: AgentParam): Promise<void> {
    const picker = param.picker;
    const data = new ShowFileMetadata();
    // 'root' = current project root; HTTP-backed listing, works in browser and Electron.
    data.start = 'root';

    if (picker === 'out-file') {
      data.title = this.translate.instant('AGENT_LAUNCH.PICK_OUT_FILE');
      data.typeOfSelection = 'Folders';
      data.saveAs = true;
      data.buttonText = this.translate.instant('COMMON.SAVE');
      const current = this.paramValues[param.name] || param.defaultValue || '';
      const m = current.match(/[\\/]([^\\/]+)$/);
      data.defaultFileName = m ? m[1] : current;
    } else if (picker === 'dir') {
      data.title = this.translate.instant('AGENT_LAUNCH.PICK_FOLDER');
      data.typeOfSelection = 'Folders';
      data.buttonText = this.translate.instant('COMMON.SELECT');
    } else {
      data.title = this.translate.instant('AGENT_LAUNCH.PICK_FILE');
      data.typeOfSelection = 'FoldersAndFiles';
      data.buttonText = this.translate.instant('COMMON.SELECT');
    }

    const ref = this.dialog.open(ShowFileSystemComponent, {
      width: '900px',
      height: '700px',
      data,
    });
    const result = await firstValueFrom(ref.afterClosed());
    if (result?.data) {
      this.paramValues[param.name] = result.data;
    }
  }

  launchNow(): void {
    if (!this.canLaunch()) {
      return;
    }
    this.isLaunching = true;
    this.aiError = null;

    this.agentLaunchService
      .launch(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues,
              this.useWorktree)
      .subscribe({
        next: (response) => {
          this.isLaunching = false;
          if (response.success) {
            this.saveDraft();
            this.snackBar.open(
              this.translate.instant('AGENT_LAUNCH.STARTED', { agent: this.data.agentName }),
              undefined,
              { duration: 4000 },
            );
            this.dialogRef.close({ launched: true, runId: response.runId });
          } else {
            this.aiError = response.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          }
        },
        error: (err) => {
          this.isLaunching = false;
          // 409 = already running, 400 = validation — both carry {error} in the body.
          this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          console.error('Error launching agent:', err);
        },
      });
  }

  /** Saving (local or template) only needs a prompt — parameter values are optional. */
  canSave(): boolean {
    return !!this.prompt && !!this.prompt.trim() && !this.isNormalizing && !this.isLaunching;
  }

  canLaunch(): boolean {
    if (!this.prompt || !this.prompt.trim() || this.isNormalizing || this.isLaunching) {
      return false;
    }
    return this.parameters.every((p) => (this.paramValues[p.name] || '').trim().length > 0);
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

  private setParameters(params: AgentParam[]): void {
    this.parameters = params;
    const previous = this.paramValues;
    this.paramValues = {};
    for (const p of params) {
      this.paramValues[p.name] = previous[p.name] || p.defaultValue || '';
    }
  }
}
