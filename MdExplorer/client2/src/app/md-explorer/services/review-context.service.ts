import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';

/**
 * Di chi è il lavoro che stai guardando adesso.
 *
 * `null` = il tuo. Il nome di un agente = sei entrato in revisione sul suo posto di lavoro,
 * e da quel momento il tab delle differenze, gli aggregati e il commit parlano di lui.
 *
 * Vive in un servizio e non dentro un componente perché il contesto è la stessa cosa per
 * superfici diverse — il tab, la finestrella del commit, l'intestazione — e due copie dello
 * stesso stato prima o poi si contraddicono.
 */
@Injectable({ providedIn: 'root' })
export class ReviewContextService {
  readonly agent$ = new BehaviorSubject<string | null>(null);

  /**
   * Qualcuno chiede di far vedere le differenze. Passa di qui perché chi lo chiede — la
   * finestrella del commit, in un altro angolo dell'applicazione — non deve sapere in che
   * posizione sta quel tab: lo sa solo chi i tab li dichiara.
   */
  readonly showChanges$ = new Subject<void>();

  showChanges(): void {
    this.showChanges$.next();
  }

  get agent(): string | null {
    return this.agent$.value;
  }

  enterAgent(agentName: string): void {
    this.agent$.next(agentName);
  }

  backToUser(): void {
    this.agent$.next(null);
  }
}
