import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { TranslateService } from '@ngx-translate/core';
import { marked } from 'marked';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ChatHistoryService, ChatThreadSummary } from '../../../services/chat-history.service';
import { AiChatService } from '../../../services/ai-chat.service';
import { ProjectsService } from '../../services/projects.service';

/** A point of the chat, numbered from the start of the chat: its label and its text already turned into HTML. */
interface ChatPoint {
  /** «7️⃣» up to 🔟, then the plain number (shown bold). */
  label: string;
  emoji: boolean;
  html: SafeHtml;
}

/** A row of the list, with its points already prepared (once, not at every change detection). */
interface ChatRow extends ChatThreadSummary {
  /** The last {@link RECENT_POINTS} points, most recent first, numbered: the number says the order. */
  recent: ChatPoint[];
  /** The ones before, most recent first, as a plain list (closed until asked). */
  older: ChatPoint[];
  /** A detail that is not a list of points (none should be): shown as it is. */
  detailHtml: SafeHtml | null;
}

/** How many points get a number emoji; the older ones become a plain list. */
const RECENT_POINTS = 5;
const KEYCAPS = ['0️⃣', '1️⃣', '2️⃣', '3️⃣', '4️⃣', '5️⃣', '6️⃣', '7️⃣', '8️⃣', '9️⃣', '🔟'];

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
   * The detail is the log of the chat's points, one «- …» per line from the oldest (the model sends only the new
   * point of each turn, MDE appends it). Shown upside down: the most recent on top with the highest number.
   */
  private toRow(r: ChatThreadSummary): ChatRow {
    const lines = (r.detail || '').replace(/\r\n/g, '\n').split('\n').map(l => l.trim());
    const texts = lines.filter(l => l.startsWith('- ') || l.startsWith('* ')).map(l => l.substring(2).trim()).filter(l => l.length > 0);
    const points = texts.map((text, i) => {
      const n = i + 1;
      return {
        label: n < KEYCAPS.length ? KEYCAPS[n] : String(n),
        emoji: n < KEYCAPS.length,
        html: this.sanitizer.bypassSecurityTrustHtml(marked.parseInline(text) as string),
      } as ChatPoint;
    }).reverse();
    return {
      ...r,
      recent: points.slice(0, RECENT_POINTS),
      older: points.slice(RECENT_POINTS),
      detailHtml: texts.length === 0 ? this.toHtml(r.detail) : null,
    };
  }

  private toHtml(markdown: string | null): SafeHtml | null {
    if (!markdown) return null;
    return this.sanitizer.bypassSecurityTrustHtml(marked.parse(markdown, { breaks: false }) as string);
  }
}
