import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MailboxMessage, MailboxService } from '../../services/mailbox.service';

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

  private showError(err: any): void {
    const msg = err?.error?.error || err?.message || 'Operazione fallita.';
    this.snackBar.open(`⚠️ ${msg}`, 'OK', { duration: 8000, panelClass: ['kg-stale-snack'] });
  }

  close(): void {
    this.dialogRef.close();
  }
}
