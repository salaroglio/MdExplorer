import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialog as MatDialog,
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom, forkJoin } from 'rxjs';

import { A2aAgentsService, AgentOwner, AgentOwnersView, AgentRegistryEntry } from '../../services/a2a-agents.service';
import { ConfirmDialogComponent } from '../../../commons/components/confirm-dialog/confirm-dialog.component';
import { AgentTrustDialogComponent, AgentTrustDialogData } from '../agent-trust-dialog/agent-trust-dialog.component';

export interface AgentRegistryDialogData {
  projectPath: string;
}

/** Tool considerati pericolosi: scrivono/eseguono, quindi evidenziati nel trust (§10). */
const DANGEROUS_TOOLS = ['write', 'edit', 'shell', 'execute'];

/**
 * La "città degli agenti" del progetto (§6): elenca i cittadini scoperti dal
 * registry con il loro stato di trust, e permette di confermarlo/revocarlo. Le
 * voci escluse mostrano il proprio RegistrationError (fail-loud). Il trust è
 * ancorato al contenuto del blocco a2a:/tools: (R3): se cambia, decade.
 */
@Component({
  selector: 'app-agent-registry-dialog',
  templateUrl: './agent-registry-dialog.component.html',
  styleUrls: ['./agent-registry-dialog.component.scss'],
})
export class AgentRegistryDialogComponent implements OnInit {
  agents: AgentRegistryEntry[] = [];
  /** Chi risponde di ogni agente; `applies` false a città spenta, dove non si mostra niente. */
  owners: AgentOwnersView = { applies: false, agents: [] };
  /** Gli agenti di cittadinanza valida e quelli senza responsabile: ricalcolati a ogni caricamento. */
  citizens: AgentRegistryEntry[] = [];
  excluded: AgentRegistryEntry[] = [];
  unassigned: AgentRegistryEntry[] = [];
  private ownerByName = new Map<string, AgentOwner>();
  loading = false;
  error: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<AgentRegistryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentRegistryDialogData,
    private agentsService: A2aAgentsService,
    private dialog: MatDialog,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    if (!this.data?.projectPath) {
      this.error = this.translate.instant('AGENT_REGISTRY.NO_PROJECT');
      return;
    }
    this.loading = true;
    this.error = null;
    forkJoin({
      agents: this.agentsService.getAgents(this.data.projectPath),
      owners: this.agentsService.getOwners(this.data.projectPath),
    }).subscribe({
      next: ({ agents, owners }) => {
        this.agents = agents || [];
        this.owners = owners || { applies: false, agents: [] };
        this.ownerByName = new Map((this.owners.agents || []).map((o) => [o.agentName.toLowerCase(), o]));
        this.citizens = this.agents.filter((a) => a.isCitizen);
        this.excluded = this.agents.filter((a) => a.isExcluded);
        this.unassigned = this.citizens.filter((a) => this.ownerOf(a)?.kind === 'unassigned');
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error || this.translate.instant('AGENT_REGISTRY.LOAD_ERROR');
        this.loading = false;
      },
    });
  }

  isDangerous(tool: string): boolean {
    return DANGEROUS_TOOLS.includes((tool || '').trim().toLowerCase());
  }

  /** Di chi è l'agente; undefined a città spenta. */
  ownerOf(agent: AgentRegistryEntry): AgentOwner | undefined {
    return this.owners.applies ? this.ownerByName.get((agent.name || '').toLowerCase()) : undefined;
  }

  /** Ognuno abilita i suoi agenti: quello di un altro lavora sul suo computer, quello di nessuno non lavora. */
  canTrust(agent: AgentRegistryEntry): boolean {
    const owner = this.ownerOf(agent);
    return !owner || owner.canWorkHere;
  }

  /** «È mio»: l'agente senza responsabile diventa di chi è a questo computer. */
  assignToMe(agent: AgentRegistryEntry): void {
    this.assignAll([agent]);
  }

  /** «Sono tutti miei»: una richiesta sola, che scrive il documento una volta. */
  assignAll(agents: AgentRegistryEntry[] = this.unassigned): void {
    this.loading = true;
    this.error = null;
    this.agentsService.assignToMe(this.data.projectPath, agents.map((a) => a.name)).subscribe({
      next: () => this.reload(),
      error: (err) => {
        const failed = err?.error?.error || this.translate.instant('AGENT_REGISTRY.OWNER_ASSIGN_ERROR');
        this.reload();
        this.error = failed;
      },
    });
  }

  async trust(agent: AgentRegistryEntry): Promise<void> {
    // Cosa fa l'agente e cosa può fare sul computer, prima del «Mi fido».
    const confirmed = await firstValueFrom(
      this.dialog
        .open(AgentTrustDialogComponent, {
          width: '560px',
          data: { agent, projectPath: this.data.projectPath } as AgentTrustDialogData,
        })
        .afterClosed(),
    );
    if (!confirmed) return;

    this.loading = true;
    this.agentsService.trust(this.data.projectPath, agent.name).subscribe({
      next: () => this.reload(),
      error: (err) => {
        this.error = err?.error || this.translate.instant('AGENT_TRUST.TRUST_ERROR');
        this.loading = false;
      },
    });
  }

  async untrust(agent: AgentRegistryEntry): Promise<void> {
    const confirmed = await firstValueFrom(
      this.dialog
        .open(ConfirmDialogComponent, {
          width: '480px',
          data: {
            title: this.translate.instant('AGENT_TRUST.UNTRUST_TITLE'),
            message: this.translate.instant('AGENT_TRUST.UNTRUST_MESSAGE', { agent: agent.name }),
            confirmText: this.translate.instant('AGENT_TRUST.UNTRUST_CONFIRM'),
          },
        })
        .afterClosed(),
    );
    if (!confirmed) return;

    this.loading = true;
    this.agentsService.untrust(this.data.projectPath, agent.name).subscribe({
      next: () => this.reload(),
      error: (err) => {
        this.error = err?.error || this.translate.instant('AGENT_TRUST.TRUST_ERROR');
        this.loading = false;
      },
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}
