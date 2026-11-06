import { Component, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { HttpClient } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { AiChatService } from '../../../services/ai-chat.service';

export interface CommitMessageDialogData {
  defaultMessage: string;
  projectPath?: string;
}

@Component({
  selector: 'app-commit-message-dialog',
  templateUrl: './commit-message-dialog.component.html',
  styleUrls: ['./commit-message-dialog.component.scss']
})
export class CommitMessageDialogComponent {
  commitMessage: string;
  isGeneratingMessage = false;
  aiError: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<CommitMessageDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CommitMessageDialogData,
    private http: HttpClient,
    private translate: TranslateService,
    private aiChat: AiChatService
  ) {
    this.commitMessage = data.defaultMessage || this.translate.instant('GIT_COMMIT.DEFAULT_MSG');
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

  onConfirm(): void {
    if (this.commitMessage && this.commitMessage.trim()) {
      this.dialogRef.close(this.commitMessage.trim());
    }
  }

  /**
   * The message is written by MarkAgent in the MarkAgent tab's own session (a channel of the AI
   * chat): the agent that did the work, or talked about it, knows the WHY. The server gives the
   * prompt with the changes and cleans the answer; no other engine, no generic message in its place.
   * Sprint: docs-internal/Sprints/2026-09-25-Commit-AI-Sessione-Del-Tab.md
   */
  async generateWithAi(): Promise<void> {
    if (!this.data.projectPath) {
      this.aiError = this.translate.instant('GIT_COMMIT.NO_PROJECT_PATH');
      return;
    }

    this.isGeneratingMessage = true;
    this.aiError = null;
    try {
      const built = await firstValueFrom(this.http.post<{ prompt?: string; noChanges?: boolean; message?: string }>(
        '/api/GitAi/commit-prompt', {
          projectPath: this.data.projectPath,
          language: this.translate.currentLang || this.translate.defaultLang || 'en'
        }));
      if (built.noChanges || !built.prompt) {
        this.aiError = built.message || this.translate.instant('GIT_COMMIT.GENERATION_ERROR');
        return;
      }
      const raw = await this.aiChat.askOnChannel(`git-commit-${Date.now()}`, built.prompt);
      const cleaned = await firstValueFrom(this.http.post<{ message: string }>(
        '/api/GitAi/clean-commit-message', { raw }));
      this.commitMessage = cleaned.message;
    } catch (err: any) {
      console.error('Error generating commit message:', err);
      this.aiError = err?.error?.error || err?.message || this.translate.instant('GIT_COMMIT.GENERATION_ERROR');
    } finally {
      this.isGeneratingMessage = false;
    }
  }
}
