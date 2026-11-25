import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { TranslateService } from '@ngx-translate/core';
import { marked } from 'marked';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ChatHistoryService, ChatThreadSummary } from '../../../services/chat-history.service';
import { AiChatService } from '../../../services/ai-chat.service';
import { ProjectsService } from '../../services/projects.service';

/** A row of the list, with its detail already turned into HTML (once, not at every change detection). */
interface ChatRow extends ChatThreadSummary {
  detailHtml: SafeHtml | null;
}

/**
 * The MarkAgent chat history (sprint 2026-11-24-Chat-Memorizzate, F6), in the big pane where the document is: the
 * project's chats, most recent first, each with its title and, opened, the detail the model keeps up to date. From a
 * row: «Riprendi» puts the chat back into the tab (and the engine goes on from its conversation when it can),
 * «Elimina» removes it with its file, after a confirmation in the row itself.
 */
@Component({
  selector: 'app-chat-history-panel',
  templateUrl: './chat-history-panel.component.html',
  styleUrls: ['./chat-history-panel.component.scss'],
})
export class ChatHistoryPanelComponent implements OnInit, OnDestroy {
  chats: ChatRow[] = [];
  loading = false;
  error = '';
  /** The row waiting for the second click on «Elimina». */
  confirmDeleteId: string | null = null;
  busyId: string | null = null;
  rowProblem: { id: string; text: string } | null = null;
  /** The chat shown in the tab right now, marked in the list. */
  currentId: string | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private history: ChatHistoryService,
    private aiChat: AiChatService,
    private projects: ProjectsService,
    private sanitizer: DomSanitizer,
    private translate: TranslateService,
    private ref: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.projects.currentProjects$.pipe(takeUntil(this.destroy$)).subscribe(() => this.reload());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get projectPath(): string | null {
    return this.projects.currentProjects$.value?.path || null;
  }

  reload(): void {
    const projectPath = this.projectPath;
    this.currentId = this.aiChat.chatThreadId;
    this.confirmDeleteId = null;
    this.rowProblem = null;
    if (!projectPath) {
      this.chats = [];
      this.error = this.translate.instant('CHAT_HISTORY.NO_PROJECT');
      return;
    }
    this.loading = true;
    this.error = '';
    this.history.list(projectPath).subscribe({
      next: rows => {
        this.chats = rows.map(r => ({ ...r, detailHtml: this.toHtml(r.detail) }));
        this.loading = false;
        this.ref.detectChanges();
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message || err?.message || String(err);
        this.ref.detectChanges();
      },
    });
  }

  close(): void {
    this.history.close();
  }

  resume(chat: ChatRow): void {
    this.busyId = chat.id;
    this.rowProblem = null;
    this.history.content(chat.id).subscribe({
      next: async content => {
        try {
          const outcome = await this.aiChat.resumeFromHistory(content);
          if (!outcome.resumed) {
            this.rowProblem = { id: chat.id, text: outcome.reason || this.translate.instant('CHAT_HISTORY.NOT_RESUMED') };
            return;
          }
          if (content.problems?.length) {
            this.rowProblem = { id: chat.id, text: this.translate.instant('CHAT_HISTORY.UNREADABLE_LINES', { count: content.problems.length }) };
          }
          this.currentId = chat.id;
          this.aiChat.showTab();
          this.history.close();
        } catch (err: any) {
          this.rowProblem = { id: chat.id, text: err?.message || String(err) };
        } finally {
          this.busyId = null;
          this.ref.detectChanges();
        }
      },
      error: err => {
        this.busyId = null;
        this.rowProblem = { id: chat.id, text: err?.error?.message || err?.message || String(err) };
        this.ref.detectChanges();
      },
    });
  }

  askDelete(chat: ChatRow): void {
    this.confirmDeleteId = chat.id;
  }

  delete(chat: ChatRow): void {
    this.busyId = chat.id;
    this.history.delete(chat.id).subscribe({
      next: () => {
        this.busyId = null;
        this.confirmDeleteId = null;
        this.chats = this.chats.filter(c => c.id !== chat.id);
        this.ref.detectChanges();
      },
      error: err => {
        this.busyId = null;
        this.rowProblem = { id: chat.id, text: err?.error?.message || err?.message || String(err) };
        this.ref.detectChanges();
      },
    });
  }

  byId(_: number, chat: ChatRow): string {
    return chat.id;
  }

  private toHtml(markdown: string | null): SafeHtml | null {
    if (!markdown) return null;
    return this.sanitizer.bypassSecurityTrustHtml(marked.parse(markdown, { breaks: false }) as string);
  }
}
