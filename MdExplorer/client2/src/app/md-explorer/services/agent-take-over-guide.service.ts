import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { AgentReviewService } from './agent-review.service';
import { MarkAssistantService } from '../../mark-assistant/mark-assistant.service';
import { AgentMailboxNotificationService } from '../../services/agent-mailbox-notification.service';

/**
 * Il passo che manca dopo «Ci metto mano»: usciti dalla copia di un agente con una sua consegna ancora da decidere,
 * Mark lo ricorda. Il lavoro corretto è salvato e pubblicato, ma nel progetto entra solo quando lo si autorizza.
 * Sta in un servizio perché dalla copia si esce da più punti (il tab delle differenze, la striscia sopra il documento).
 */
@Injectable({ providedIn: 'root' })
export class AgentTakeOverGuideService {
  constructor(
    private review: AgentReviewService,
    private mark: MarkAssistantService,
    private mailbox: AgentMailboxNotificationService,
    private translate: TranslateService,
  ) {}

  remindToApprove(projectPath: string, agent: string): void {
    if (!projectPath || !agent) return;
    this.review.pending(projectPath).subscribe({
      next: res => {
        const waiting = (res.requests || []).find(r => r.status === 'pending' && r.agentName === agent);
        if (!waiting) return;
        this.mark.say(this.translate.instant('AGENT_REVIEW.TAKE_BACK', { agent }), [
          { labelKey: 'AGENT_REVIEW.TAKE_BACK_OPEN', icon: '📬', handler: () => { this.mark.hide(); this.mailbox.open(0, waiting.id); } },
          { labelKey: 'AGENT_REVIEW.TAKE_BACK_LATER', icon: '⏳', handler: () => this.mark.hide() },
        ]);
      },
      // Senza l'elenco delle consegne resta il numero sul pulsante dei messaggi: niente promemoria, niente errore.
      error: () => {},
    });
  }
}
