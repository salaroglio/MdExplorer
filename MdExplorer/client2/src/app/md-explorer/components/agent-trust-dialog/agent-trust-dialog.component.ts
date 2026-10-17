import { Component, Inject } from '@angular/core';
import {
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { AgentEffect, AgentRegistryEntry } from '../../services/a2a-agents.service';

export interface AgentTrustDialogData {
  agent: AgentRegistryEntry;
  projectPath: string;
}

/**
 * La conferma di fiducia di un agente: cosa fa e cosa può fare sul tuo computer.
 * <p>
 * Due blocchi, volutamente diversi: il <b>riassunto</b> lo scrive l'autore della scheda (una dichiarazione, non
 * verificata) mentre le <b>azioni</b> le calcola l'app dagli strumenti dichiarati, gli unici che fa rispettare.
 * Chi si fida deve poter distinguere ciò che l'app garantisce da ciò che qualcuno ha scritto.
 */
@Component({
  selector: 'app-agent-trust-dialog',
  templateUrl: './agent-trust-dialog.component.html',
  styleUrls: ['./agent-trust-dialog.component.scss'],
})
export class AgentTrustDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<AgentTrustDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) public data: AgentTrustDialogData,
  ) {}

  get agent(): AgentRegistryEntry { return this.data.agent; }

  get effects(): AgentEffect[] { return this.agent.effects || []; }

  /** Almeno un'azione che esce dalla sola lettura è concessa: serve l'avviso. */
  get hasDanger(): boolean { return this.effects.some(e => e.granted && e.danger); }

  /** Chiave di traduzione della frase: una per azione, una per «sì» e una per «no». */
  effectKey(e: AgentEffect): string {
    return `AGENT_TRUST.EFFECTS.${e.id}.${e.granted ? 'YES' : 'NO'}`;
  }

  iconOf(e: AgentEffect): string {
    if (!e.granted) return 'block';
    return e.danger ? 'warning' : 'check_circle';
  }

  confirm(): void { this.dialogRef.close(true); }
  cancel(): void { this.dialogRef.close(false); }
}
