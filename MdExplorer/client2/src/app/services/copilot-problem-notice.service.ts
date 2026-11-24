import { Injectable } from '@angular/core';
import { AiChatService } from './ai-chat.service';
import { MarkAssistantService } from '../mark-assistant/mark-assistant.service';

/**
 * Quando Copilot non si lascia leggere (nessun accesso fatto, versione che MdExplorer non parla, nessuna
 * risposta), Mark lo dice con le parole del servizio, che contengono anche cosa fare. Prima restava nel log:
 * l'utente vedeva solo un elenco di modelli vuoto.
 *
 * Lo stesso problema si dice una volta per sessione: la lettura dei modelli riparte a ogni progetto aperto.
 */
@Injectable({ providedIn: 'root' })
export class CopilotProblemNoticeService {
  private told = new Set<string>();
  private started = false;

  constructor(private aiChat: AiChatService, private mark: MarkAssistantService) {}

  start(): void {
    if (this.started) return;
    this.started = true;
    this.aiChat.copilotProblem$.subscribe(problem => {
      if (!problem || this.told.has(problem)) return;
      this.told.add(problem);
      this.mark.say(problem, [
        { labelKey: 'MARK.COPILOT_PROBLEM_OK', icon: '👍', handler: () => this.mark.hide() },
      ]);
    });
  }
}
