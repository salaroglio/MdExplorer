import { Injectable } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { MailboxService } from '../md-explorer/services/mailbox.service';
import { AgentMailComponent } from '../md-explorer/components/agent-mail/agent-mail.component';

/**
 * La metà "la città parla all'umano" della Fase 4a. Ascolta l'evento SignalR
 * `agentMessageReceived` (un cittadino ha scritto a `user`): mostra un toast con
 * azione "Apri" e mantiene il conteggio dei non-letti (`unread$`) che il toolbar
 * lega al badge della campanella. È il gemello di AiNotificationService per la mailbox.
 */
@Injectable({ providedIn: 'root' })
export class AgentMailboxNotificationService {

  /** Non-letti correnti: il toolbar ci lega il badge della campanella. */
  public unread$ = new BehaviorSubject<number>(0);

  /** Progetto corrente su cui contare i non-letti (impostato dal toolbar). */
  private currentProjectPath = '';

  constructor(
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private translate: TranslateService,
    private mailbox: MailboxService,
    private serverMessages: MdServerMessagesService,
  ) {
    this.serverMessages.agentMessageReceived$.subscribe(evt => this.onMessage(evt));
    // Richiesta federata (§12.6): toast prioritario + apertura sul tab del gate.
    this.serverMessages.federationRequestReceived$.subscribe(evt => this.onFederationRequest(evt));
    // Fase 7e — un agente ha toccato il codice (submodule): awareness, solo info (nessun diff).
    this.serverMessages.submoduleTouchedByAgent$.subscribe(evt => this.onSubmoduleTouched(evt));
    // Delega interna: consapevolezza, non permesso — il gate custodisce la fiducia fra umani
    // diversi, e verso sé stessi non ha niente da custodire.
    this.serverMessages.agentDelegationRouted$.subscribe(evt => this.onDelegationRouted(evt));
    // Un incarico aspetta che tu lo avvii (workflow, start: ask-owner): finché non lo fai, il lavoro di qualcuno è fermo.
    this.serverMessages.agentStartRequested$.subscribe(evt => this.onStartRequested(evt));
  }

  private onStartRequested(evt: { fromAgent: string; toAgent: string; step?: string; projectPath: string }): void {
    this.refresh();
    const text = this.translate.instant('MAILBOX.START_TOAST', { from: evt.fromAgent || '?', agent: evt.toAgent || '?', step: evt.step || evt.toAgent || '?' });
    const toast = this.snackBar.open(text, this.translate.instant('MAILBOX.TOAST_OPEN'),
      { duration: 15000, horizontalPosition: 'right', verticalPosition: 'bottom', panelClass: ['kg-stale-snack'] });
    toast.onAction().subscribe(() => this.open());
    // MdExplorer ridotto o dietro altre finestre: la notifica del sistema, perché l'avviso nell'app non lo vede nessuno.
    try {
      if (typeof Notification !== 'undefined' && (document.hidden || !document.hasFocus())) {
        const show = () => new Notification(this.translate.instant('MAILBOX.START_NOTIFICATION_TITLE'), { body: text });
        if (Notification.permission === 'granted') show();
        else if (Notification.permission !== 'denied') Notification.requestPermission().then(p => { if (p === 'granted') show(); });
      }
    } catch { /* la notifica di sistema è in più: badge e avviso nell'app ci sono comunque */ }
  }

  /** Il toolbar comunica il progetto attivo; ricarichiamo il conteggio non-letti. */
  public setProject(projectPath: string): void {
    this.currentProjectPath = projectPath || '';
    this.refresh();
  }

  /** Ricarica il badge non-letti dal Service (fonte autoritativa). */
  public refresh(): void {
    this.mailbox.unreadCount(this.currentProjectPath).subscribe({
      // Il badge conta ciò che aspetta una decisione, non i messaggi da leggere (La posta in ordine, P2).
      next: (res) => this.unread$.next(res.todo ?? res.unread ?? 0),
      error: () => { /* best-effort: il badge non deve rompere la UI */ },
    });
  }

  /**
   * Apre la posta degli agenti: una pagina a tutto schermo, sopra il progetto, con messaggi, lavori da
   * approvare e richieste dei colleghi in un elenco solo. Le conversazioni (tab 1) restano nella finestra
   * di prima, che la pagina sa aprire.
   */
  /** Apre la posta degli agenti a tutto schermo (la finestra di prima non c'è più: «La posta in ordine», F4). */
  public open(_initialTab: number = 0): void {
    const page = this.dialog.open(AgentMailComponent, {
      width: '100vw',
      height: '100vh',
      maxWidth: '100vw',
      maxHeight: '100vh',
      panelClass: 'agent-mail-page',
      position: { top: '0', left: '0' },
      autoFocus: false,
      data: { projectPath: this.currentProjectPath },
    });
    page.afterClosed().subscribe(() => this.refresh());
  }

  private onFederationRequest(evt: { fromOwner: string; scope: string }): void {
    const toast = this.snackBar.open(
      this.translate.instant('FEDERATION.TOAST', { owner: evt.fromOwner || '?', scope: evt.scope || '?' }),
      this.translate.instant('FEDERATION.TOAST_REVIEW'),
      { duration: 12000, horizontalPosition: 'right', verticalPosition: 'bottom', panelClass: ['kg-stale-snack'] });
    toast.onAction().subscribe(() => this.open(2));   // apre sul tab "Richieste federate"
  }

  private onSubmoduleTouched(evt: { agent: string; submodule: string }): void {
    // Awareness only (§6bis): l'agente ha prodotto codice nel submodule; il push è dell'umano.
    this.snackBar.open(
      this.translate.instant('SUBMODULE_GATE.TOAST', { agent: evt.agent || '?', submodule: evt.submodule || '?' }),
      this.translate.instant('COMMON.OK'),
      { duration: 12000, horizontalPosition: 'right', verticalPosition: 'bottom', panelClass: ['kg-stale-snack'] });
  }

  private onDelegationRouted(evt: { fromAgent: string; toAgent: string; scope: string }): void {
    // Informativo e non bloccante, ma con l'azione per aprire il viewer: se la stessa delega
    // ricorre spesso, e' la mappa di ownership che sta chiedendo di essere rivista.
    const toast = this.snackBar.open(
      this.translate.instant('MAILBOX.DELEGATION_TOAST', {
        from: evt.fromAgent || '?', to: evt.toAgent || '?', scope: evt.scope || '?',
      }),
      this.translate.instant('MAILBOX.TOAST_OPEN'),
      { duration: 9000, horizontalPosition: 'right', verticalPosition: 'bottom' });
    toast.onAction().subscribe(() => this.open(1));   // tab "Conversazioni"
  }

  private onMessage(evt: { fromAgent: string; projectPath: string }): void {
    // Aggiorna il badge dalla fonte autoritativa (conta anche eventuali arretrati).
    this.refresh();

    const toast = this.snackBar.open(
      this.translate.instant('MAILBOX.TOAST', { agent: evt.fromAgent }),
      this.translate.instant('MAILBOX.TOAST_OPEN'),
      { duration: 8000, horizontalPosition: 'right', verticalPosition: 'bottom' });
    toast.onAction().subscribe(() => this.open());
  }
}
