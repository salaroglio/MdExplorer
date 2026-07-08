import { Component, Inject } from '@angular/core';
import {
  MatLegacyDialog as MatDialog,
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { AgentLaunchService, AgentParam } from '../../services/agent-launch.service';
import { AgentScheduleService } from '../../services/agent-schedule.service';
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
export class AgentLaunchDialogComponent {
  prompt = '';
  parameters: AgentParam[] = [];
  paramValues: { [name: string]: string } = {};

  isNormalizing = false;
  isLaunching = false;
  aiError: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<AgentLaunchDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentLaunchDialogData,
    private agentLaunchService: AgentLaunchService,
    private agentScheduleService: AgentScheduleService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {
    // Restore the last working prompt for this agent file (per-user draft, UserDB).
    this.agentScheduleService.getDraft(data.projectPath, data.agentFilePath).subscribe({
      next: (r) => {
        if (r.draft?.prompt && !this.prompt) {
          this.prompt = r.draft.prompt;
          try {
            this.paramValues = JSON.parse(r.draft.parameterValuesJson || '{}') || {};
          } catch {
            this.paramValues = {};
          }
          this.detectParams();
        }
      },
      error: () => { /* draft is a nicety, not a requirement */ },
    });
  }

  private saveDraft(): void {
    if (!this.prompt || !this.prompt.trim()) return;
    this.agentScheduleService
      .saveDraft(this.data.projectPath, this.data.agentFilePath, this.prompt, this.paramValues)
      .subscribe({ error: (err) => console.warn('Draft save failed:', err) });
  }

  /** Substitutes the chosen values and opens the scheduling dialog with the ready prompt. */
  saveAsSchedule(): void {
    if (!this.canLaunch()) return;
    this.aiError = null;
    this.saveDraft();
    this.agentLaunchService.prepare(this.prompt, this.paramValues).subscribe({
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

    this.agentLaunchService.normalize(this.data.projectPath, this.prompt).subscribe({
      next: (response) => {
        this.isNormalizing = false;
        if (response.success && response.normalizedPrompt) {
          this.prompt = response.normalizedPrompt;
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
    this.agentLaunchService.extractParams(this.prompt).subscribe({
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
      .launch(this.data.projectPath, this.data.agentFilePath, this.prompt, this.paramValues)
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
