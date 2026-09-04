import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import {
  ConversationMessage, ConversationSummary, MailboxMessage, MailboxService, MemFact,
} from '../../services/mailbox.service';
import { FederationRequest, FederationService } from '../../services/federation.service';

export interface MailboxDialogData {
  projectPath: string;
  /** Tab iniziale: 0 inbox, 1 conversazioni, 2 richieste federate. */
  initialTab?: number;
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

  // ---- 7f: consolidamento (promozione + decadimento) ----
  consolidateOpenId: string | null = null;
  consolidateFacts: (MemFact & { selected: boolean })[] = [];
  consolidateLoading = false;
  consolidateBusy = false;

  // ---- 6d: richieste federate (gate umano §12.6) ----
  federationRequests: FederationRequest[] = [];
  fedLoading = false;
  fedError: string | null = null;
  /** Override dell'agente proposto, per requestId. */
  approveAgent: { [id: string]: string } = {};
  deciding: { [id: string]: boolean } = {};
  selectedTab = 0;

  constructor(
    public dialogRef: MatDialogRef<MailboxDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MailboxDialogData,
    private mailbox: MailboxService,
    private federation: FederationService,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.reload();
    this.selectedTab = this.data?.initialTab ?? 0;
    if (this.selectedTab === 2) this.loadFederationRequests();
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

  /** Primo ingresso in un tab → carica pigramente la sua lista. */
  onTabChange(index: number): void {
    this.selectedTab = index;
    if (index === 1 && !this.conversations.length && !this.conversationsLoading) {
      this.loadConversations();
    } else if (index === 2 && !this.federationRequests.length && !this.fedLoading) {
      this.loadFederationRequests();
    }
  }

  // ---- 6d: gate delle richieste federate ----

  loadFederationRequests(): void {
    this.fedLoading = true;
    this.fedError = null;
    this.federation.requests(this.data?.projectPath || '').subscribe({
      next: (res) => { this.federationRequests = res.requests || []; this.fedLoading = false; },
      error: (err) => {
        this.fedError = err?.error?.error || err?.message || 'Errore nel caricamento delle richieste federate.';
        this.fedLoading = false;
      },
    });
  }

  approveFederation(req: FederationRequest): void {
    this.deciding[req.id] = true;
    const override = this.approveAgent[req.id]?.trim() || undefined;
    this.federation.approve(req.id, override).subscribe({
      next: (res) => {
        this.deciding[req.id] = false;
        this.snackBar.open(this.translate.instant('FEDERATION.APPROVED', { agent: res.targetAgent }), 'OK', { duration: 4000 });
        this.loadFederationRequests();
      },
      error: (err) => { this.deciding[req.id] = false; this.showError(err); },
    });
  }

  rejectFederation(req: FederationRequest): void {
    this.deciding[req.id] = true;
    this.federation.reject(req.id).subscribe({
      next: () => { this.deciding[req.id] = false; this.loadFederationRequests(); },
      error: (err) => { this.deciding[req.id] = false; this.showError(err); },
    });
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

  // ---- 7f: consolidamento ----

  /** Apre il pannello di consolidamento: carica i fatti dei partecipanti (per la scelta di promozione). */
  startConsolidate(conv: ConversationSummary): void {
    if (this.consolidateOpenId === conv.id) { this.consolidateOpenId = null; return; }
    this.consolidateOpenId = conv.id;
    this.consolidateFacts = [];
    this.consolidateLoading = true;
    const parts = (conv.participants || []).map(p => (p || '').toLowerCase());
    this.mailbox.memoryFacts(this.data?.projectPath || '').subscribe({
      next: (res) => {
        this.consolidateFacts = (res.facts || [])
          .filter(f => !f.shared && parts.includes((f.agent || '').toLowerCase()))
          .map(f => ({ ...f, selected: false }));
        this.consolidateLoading = false;
      },
      error: (err) => { this.consolidateLoading = false; this.showError(err); },
    });
  }

  cancelConsolidate(): void {
    this.consolidateOpenId = null;
    this.consolidateFacts = [];
  }

  /** Promuove i fatti selezionati nel .agent.md e decade il resto. */
  confirmConsolidate(conv: ConversationSummary): void {
    const promote = this.consolidateFacts
      .filter(f => f.selected)
      .map(f => ({ factUri: f.factUri, graph: f.graph, statement: f.statement }));
    this.consolidateBusy = true;
    this.mailbox.consolidate(conv.id, this.data?.projectPath || '', promote).subscribe({
      next: (res) => {
        this.consolidateBusy = false;
        this.consolidateOpenId = null;
        this.consolidateFacts = [];
        const msg = res.memoryDisabled
          ? this.translate.instant('CONSOLIDATE.DISABLED')
          : this.translate.instant('CONSOLIDATE.DONE', { promoted: res.promoted || 0, decayed: res.decayed || 0, deleted: res.deleted || 0 });
        this.snackBar.open(msg, 'OK', { duration: 8000 });
      },
      error: (err) => { this.consolidateBusy = false; this.showError(err); },
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
