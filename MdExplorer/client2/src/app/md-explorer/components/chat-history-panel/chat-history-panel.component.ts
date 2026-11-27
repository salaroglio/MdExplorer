import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { TranslateService } from '@ngx-translate/core';
import { marked } from 'marked';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ChatHistoryService, ChatThreadSummary } from '../../../services/chat-history.service';
import { AiChatService } from '../../../services/ai-chat.service';
import { ProjectsService } from '../../services/projects.service';

/** A point of the chat on its timeline: its text already turned into HTML, when and on which branch. */
interface ChatPoint {
  html: SafeHtml;
  /** When it was written; null for the chats before 27/11/2026, which kept only the text. */
  at: Date | null;
  /** The branch label, only on the first point of a run on the same branch (reading upwards, as time goes). */
  branch: string | null;
}

/** A row of the list, with its points already prepared (once, not at every change detection). */
interface ChatRow extends ChatThreadSummary {
  /** The last {@link RECENT_POINTS} points, most recent first: the top of the timeline. */
  recent: ChatPoint[];
  /** The ones before, most recent first: the bottom of the timeline, closed until asked. */
  older: ChatPoint[];
  /** A detail that is not a list of points (none should be): shown as it is. */
  detailHtml: SafeHtml | null;
  /** {@link lastActivityAt} as a date. */
  lastActivity: Date | null;
}

/** How many points are always on the timeline; the older ones open below them. */
const RECENT_POINTS = 5;

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
  /** The chats whose older points are open. */
  olderOpen = new Set<string>();
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
        this.chats = rows.map(r => this.toRow(r));
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

  toggleOlder(chat: ChatRow): void {
    if (this.olderOpen.has(chat.id)) this.olderOpen.delete(chat.id);
    else this.olderOpen.add(chat.id);
  }

  /**
   * The points on a timeline that goes up, as time does: the oldest at the bottom, the most recent on top. From the rows
   * of the points (with time and branch) when the chat has them; for the older chats from the log in the detail, one
   * «- …» per line from the oldest, as they were.
   */
  private toRow(r: ChatThreadSummary): ChatRow {
    const lines = (r.detail || '').replace(/\r\n/g, '\n').split('\n').map(l => l.trim());
    const texts = lines.filter(l => l.startsWith('- ') || l.startsWith('* ')).map(l => l.substring(2).trim()).filter(l => l.length > 0);
    const oldestFirst: { text: string; at: Date | null; branch: string | null }[] = r.points?.length
      ? r.points.map(p => ({ text: p.point, at: this.when(p.at), branch: p.branch }))
      : texts.map(text => ({ text, at: null, branch: null }));
    const points = oldestFirst.map((p, i) => ({
      html: this.sanitizer.bypassSecurityTrustHtml(marked.parseInline(p.text) as string),
      at: p.at,
      // The label where the branch starts: on the first point, and where it differs from the point before.
      branch: p.branch && (i === 0 || oldestFirst[i - 1].branch !== p.branch) ? p.branch : null,
    } as ChatPoint)).reverse();
    return {
      ...r,
      recent: points.slice(0, RECENT_POINTS),
      older: points.slice(RECENT_POINTS),
      detailHtml: texts.length === 0 && !r.points?.length ? this.toHtml(r.detail) : null,
      lastActivity: this.when(r.lastActivityAt),
    };
  }

  /**
   * The backend gives the local time without an offset (measured 27/11/2026: «09:05» with UTC at 08:05, the DB turns the
   * UTC it is given into local time): read as it is, which for a date without offset is local time.
   */
  private when(at: string): Date | null {
    if (!at) return null;
    const d = new Date(at);
    return isNaN(d.getTime()) ? null : d;
  }

  private toHtml(markdown: string | null): SafeHtml | null {
    if (!markdown) return null;
    return this.sanitizer.bypassSecurityTrustHtml(marked.parse(markdown, { breaks: false }) as string);
  }
}
