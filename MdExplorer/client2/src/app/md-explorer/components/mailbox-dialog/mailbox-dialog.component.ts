import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import {
  ConversationMessage, ConversationSummary, MailboxMessage, MailboxService,
} from '../../services/mailbox.service';

export interface MailboxDialogData {
  projectPath: string;
}

/**
 * Il centro notifiche dell'umano (§13 Fase 4a): elenca i messaggi che i cittadini
 * hanno indirizzato a `user`, permette di segnarli letti e di <b>rispondere</b> —
 * la risposta risveglia l'agente nella stessa conversazione (hop esente, §9).
 * Speculare a MailboxController (/api/A2A/mailbox). Fail-loud: gli errori del
 * Service sono mostrati, mai inghiottiti.
 */
@Component({
  selector: 'app-mailbox-dialog',
  templateUrl: './mailbox-dialog.component.html',
  styleUrls: ['./mailbox-dialog.component.scss'],
})
export class MailboxDialogComponent implements OnInit {
  messages: MailboxMessage[] = [];
  unread = 0;
  loading = false;
  error: string | null = null;
  includeRead = false;

  /** Testo della risposta in composizione, per conversationId. */
  replyDraft: { [conversationId: string]: string } = {};
  sending: { [conversationId: string]: boolean } = {};

  // ---- 4b: tab conversazioni + governance ----
  conversations: ConversationSummary[] = [];
  conversationsLoading = false;
  conversationsError: string | null = null;
  /** Id del thread espanso e i suoi messaggi (dettaglio on-demand). */
  expandedId: string | null = null;
  threadMessages: ConversationMessage[] = [];
  threadLoading = false;

  constructor(
    public dialogRef: MatDialogRef<MailboxDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MailboxDialogData,
    private mailbox: MailboxService,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading = true;
    this.error = null;
    this.mailbox.inbox(this.data?.projectPath || '', this.includeRead).subscribe({
      next: (res) => {
        this.messages = res.messages || [];
        this.unread = res.unread || 0;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.error || err?.message || 'Errore nel caricamento della inbox.';
        this.loading = false;
      },
    });
  }

  toggleIncludeRead(): void {
    this.includeRead = !this.includeRead;
    this.reload();
  }

  markRead(msg: MailboxMessage): void {
    this.mailbox.markRead(msg.id).subscribe({
      next: () => this.reload(),
      error: (err) => this.showError(err),
    });
  }

  canReply(msg: MailboxMessage): boolean {
    const draft = this.replyDraft[msg.conversationId];
    return !!draft && draft.trim().length > 0 && !this.sending[msg.conversationId];
  }

  reply(msg: MailboxMessage): void {
    const body = (this.replyDraft[msg.conversationId] || '').trim();
    if (!body) return;
    this.sending[msg.conversationId] = true;
    this.mailbox.reply(msg.conversationId, body).subscribe({
      next: (res) => {
        this.sending[msg.conversationId] = false;
        this.replyDraft[msg.conversationId] = '';
        this.snackBar.open(
          this.translate.instant('MAILBOX.REPLY_SENT', { agent: res.toAgent }),
          'OK', { duration: 4000 });
        this.reload();
      },
      error: (err) => {
        this.sending[msg.conversationId] = false;
        this.showError(err);
      },
    });
  }

  projectName(path: string): string {
    return (path || '').split(/[\\\/]/).filter(Boolean).pop() || path || '';
  }

  // ---- 4b: conversazioni + governance ----

  loadConversations(): void {
    this.conversationsLoading = true;
    this.conversationsError = null;
    this.mailbox.conversations(this.data?.projectPath || '').subscribe({
      next: (res) => {
        this.conversations = res.conversations || [];
        this.conversationsLoading = false;
      },
      error: (err) => {
        this.conversationsError = err?.error?.error || err?.message || 'Errore nel caricamento delle conversazioni.';
        this.conversationsLoading = false;
      },
    });
  }

  /** Il primo ingresso nel tab conversazioni carica pigramente la lista. */
  onTabChange(index: number): void {
    if (index === 1 && !this.conversations.length && !this.conversationsLoading) {
      this.loadConversations();
    }
  }

  toggleThread(conv: ConversationSummary): void {
    if (this.expandedId === conv.id) {
      this.expandedId = null;
      this.threadMessages = [];
      return;
    }
    this.expandedId = conv.id;
    this.threadMessages = [];
    this.threadLoading = true;
    this.mailbox.conversationMessages(conv.id).subscribe({
      next: (res) => {
        this.threadMessages = res.messages || [];
        this.threadLoading = false;
      },
      error: (err) => {
        this.threadLoading = false;
        this.showError(err);
      },
    });
  }

  kill(conv: ConversationSummary): void {
    this.mailbox.kill(conv.id).subscribe({
      next: (res) => { conv.status = res.status; },
      error: (err) => this.showError(err),
    });
  }

  reopen(conv: ConversationSummary): void {
    this.mailbox.reopen(conv.id).subscribe({
      next: (res) => { conv.status = res.status; conv.hopCount = res.hopCount; },
      error: (err) => this.showError(err),
    });
  }

  statusClass(status: string): string {
    return 'status-' + (status || 'unknown');
  }

  private showError(err: any): void {
    const msg = err?.error?.error || err?.message || 'Operazione fallita.';
    this.snackBar.open(`⚠️ ${msg}`, 'OK', { duration: 8000, panelClass: ['kg-stale-snack'] });
  }

  close(): void {
    this.dialogRef.close();
  }
}
