import { Injectable } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { MailboxService } from '../md-explorer/services/mailbox.service';
import { MailboxDialogComponent } from '../md-explorer/components/mailbox-dialog/mailbox-dialog.component';

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
  }

  /** Il toolbar comunica il progetto attivo; ricarichiamo il conteggio non-letti. */
  public setProject(projectPath: string): void {
    this.currentProjectPath = projectPath || '';
    this.refresh();
  }

  /** Ricarica il badge non-letti dal Service (fonte autoritativa). */
  public refresh(): void {
    this.mailbox.unreadCount(this.currentProjectPath).subscribe({
      next: (res) => this.unread$.next(res.unread || 0),
      error: () => { /* best-effort: il badge non deve rompere la UI */ },
    });
  }

  /** Apre il centro notifiche (opzionalmente su un tab: 0 inbox, 1 conversazioni, 2 federazione). */
  public open(initialTab: number = 0): void {
    const ref = this.dialog.open(MailboxDialogComponent, {
      width: '640px',
      maxHeight: '82vh',
      data: { projectPath: this.currentProjectPath, initialTab },
    });
    ref.afterClosed().subscribe(() => this.refresh());
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
