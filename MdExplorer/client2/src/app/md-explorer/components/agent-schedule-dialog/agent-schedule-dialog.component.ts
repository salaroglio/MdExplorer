import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialog as MatDialog,
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import {
  AgentSchedule,
  AgentScheduleRequest,
  AgentScheduleService,
  AgentExecution,
} from '../../services/agent-schedule.service';
import { ConfirmDialogComponent } from '../../../commons/components/confirm-dialog/confirm-dialog.component';

export interface AgentScheduleDialogData {
  projectPath: string;
  agentFilePath: string;
  agentName: string;
  /** Prepared prompt handed over by the launch dialog ("Save as scheduled job"). */
  preparedPrompt?: string;
}

/**
 * Per-user scheduling of a *.agent.md agent: temporal (cron) triggers plus
 * event hooks (commit / project open). Saving an ENABLED schedule requires the
 * trust confirmation (the agent runs unattended with full tool access) —
 * enforced here in the UI and again server-side.
 */
@Component({
  selector: 'app-agent-schedule-dialog',
  templateUrl: './agent-schedule-dialog.component.html',
  styleUrls: ['./agent-schedule-dialog.component.scss'],
})
export class AgentScheduleDialogComponent implements OnInit {
  schedules: AgentSchedule[] = [];
  executions: AgentExecution[] = [];

  // Editor state (new schedule or editing an existing one)
  editingId: string | null = null;
  name = '';
  preparedPrompt = '';
  promptReadonly = true;
  triggerType: 'cron' | 'commit' | 'projectOpen' = 'cron';
  cronExpression = '';
  enabled = true;

  isSaving = false;
  error: string | null = null;
  showExecutions = false;

  constructor(
    public dialogRef: MatDialogRef<AgentScheduleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentScheduleDialogData,
    private scheduleService: AgentScheduleService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.name = this.data.agentName.replace(/\.agent\.md$/i, '');
    this.preparedPrompt = this.data.preparedPrompt || '';
    this.reload();
    // Without a handed-over prompt, prefill from the last draft of the launch dialog.
    if (!this.preparedPrompt) {
      this.scheduleService.getDraft(this.data.projectPath, this.data.agentFilePath).subscribe({
        next: (r) => {
          if (!this.preparedPrompt && r.draft?.prompt) {
            this.preparedPrompt = r.draft.prompt;
          }
        },
        error: () => { /* draft is a nicety, not a requirement */ },
      });
    }
  }

  reload(): void {
    this.scheduleService.list(this.data.projectPath, this.data.agentFilePath).subscribe({
      next: (r) => (this.schedules = r.schedules || []),
      error: (err) => (this.error = err?.error?.error || 'Load failed'),
    });
    this.scheduleService
      .executions({ projectPath: this.data.projectPath, agentFilePath: this.data.agentFilePath, take: 50 })
      .subscribe({
        next: (r) => (this.executions = r.executions || []),
        error: () => { /* history is best-effort */ },
      });
  }

  edit(schedule: AgentSchedule): void {
    this.editingId = schedule.id;
    this.name = schedule.name;
    this.preparedPrompt = schedule.preparedPrompt;
    this.triggerType = schedule.triggerType;
    this.cronExpression = schedule.cronExpression || '';
    this.enabled = schedule.enabled;
    this.error = null;
  }

  newSchedule(): void {
    this.editingId = null;
    this.name = this.data.agentName.replace(/\.agent\.md$/i, '');
    this.triggerType = 'cron';
    this.cronExpression = '';
    this.enabled = true;
    this.error = null;
  }

  async save(): Promise<void> {
    if (!this.canSave()) return;
    this.error = null;

    // Trust confirmation: an enabled schedule runs unattended with --allow-all-tools.
    let trusted = false;
    if (this.enabled) {
      const confirmed = await firstValueFrom(
        this.dialog
          .open(ConfirmDialogComponent, {
            width: '480px',
            data: {
              title: this.translate.instant('AGENT_SCHEDULE.TRUST_TITLE'),
              message: this.translate.instant('AGENT_SCHEDULE.TRUST_MESSAGE', {
                agent: this.data.agentName,
                project: this.data.projectPath,
              }),
              confirmText: this.translate.instant('AGENT_SCHEDULE.TRUST_CONFIRM'),
            },
          })
          .afterClosed(),
      );
      if (!confirmed) return;
      trusted = true;
    }

    const request: AgentScheduleRequest = {
      projectPath: this.data.projectPath,
      agentFilePath: this.data.agentFilePath,
      name: this.name.trim(),
      preparedPrompt: this.preparedPrompt,
      triggerType: this.triggerType,
      cronExpression: this.triggerType === 'cron' ? this.cronExpression.trim() : undefined,
      enabled: this.enabled,
      trusted,
    };

    this.isSaving = true;
    const call = this.editingId
      ? this.scheduleService.update(this.editingId, request)
      : this.scheduleService.create(request);
    call.subscribe({
      next: () => {
        this.isSaving = false;
        this.snackBar.open(this.translate.instant('AGENT_SCHEDULE.SAVED'), undefined, { duration: 3000 });
        this.newSchedule();
        this.reload();
      },
      error: (err) => {
        this.isSaving = false;
        this.error = err?.error?.error || this.translate.instant('AGENT_SCHEDULE.SAVE_ERROR');
      },
    });
  }

  async toggleEnabled(schedule: AgentSchedule): Promise<void> {
    const request: AgentScheduleRequest = {
      projectPath: schedule.projectPath,
      agentFilePath: schedule.agentFilePath,
      name: schedule.name,
      preparedPrompt: schedule.preparedPrompt,
      triggerType: schedule.triggerType,
      cronExpression: schedule.cronExpression,
      enabled: !schedule.enabled,
      trusted: schedule.trusted,
    };

    if (request.enabled) {
      const confirmed = await firstValueFrom(
        this.dialog
          .open(ConfirmDialogComponent, {
            width: '480px',
            data: {
              title: this.translate.instant('AGENT_SCHEDULE.TRUST_TITLE'),
              message: this.translate.instant('AGENT_SCHEDULE.TRUST_MESSAGE', {
                agent: schedule.agentName,
                project: schedule.projectPath,
              }),
              confirmText: this.translate.instant('AGENT_SCHEDULE.TRUST_CONFIRM'),
            },
          })
          .afterClosed(),
      );
      if (!confirmed) return;
      request.trusted = true;
    }

    this.scheduleService.update(schedule.id, request).subscribe({
      next: () => this.reload(),
      error: (err) => (this.error = err?.error?.error || this.translate.instant('AGENT_SCHEDULE.SAVE_ERROR')),
    });
  }

  async remove(schedule: AgentSchedule): Promise<void> {
    const confirmed = await firstValueFrom(
      this.dialog
        .open(ConfirmDialogComponent, {
          width: '420px',
          data: {
            title: this.translate.instant('AGENT_SCHEDULE.DELETE_TITLE'),
            message: this.translate.instant('AGENT_SCHEDULE.DELETE_MESSAGE', { name: schedule.name }),
          },
        })
        .afterClosed(),
    );
    if (!confirmed) return;
    this.scheduleService.delete(schedule.id).subscribe({
      next: () => this.reload(),
      error: (err) => (this.error = err?.error?.error || this.translate.instant('AGENT_SCHEDULE.SAVE_ERROR')),
    });
  }

  canSave(): boolean {
    if (this.isSaving) return false;
    if (!this.name || !this.name.trim()) return false;
    if (!this.preparedPrompt || !this.preparedPrompt.trim()) return false;
    if (this.triggerType === 'cron' && (!this.cronExpression || !this.cronExpression.trim())) return false;
    return true;
  }

  triggerLabel(type: string): string {
    return this.translate.instant('AGENT_SCHEDULE.TRIGGER_' + (type || '').toUpperCase());
  }

  close(): void {
    this.dialogRef.close(null);
  }
}
