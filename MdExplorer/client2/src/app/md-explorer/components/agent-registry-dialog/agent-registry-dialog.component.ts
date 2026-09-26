import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialog as MatDialog,
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { A2aAgentsService, AgentRegistryEntry } from '../../services/a2a-agents.service';
import { ConfirmDialogComponent } from '../../../commons/components/confirm-dialog/confirm-dialog.component';

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
    this.agentsService.getAgents(this.data.projectPath).subscribe({
      next: (agents) => {
        this.agents = agents || [];
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

  get citizens(): AgentRegistryEntry[] {
    return this.agents.filter((a) => a.isCitizen);
  }

  get excluded(): AgentRegistryEntry[] {
    return this.agents.filter((a) => a.isExcluded);
  }

  async trust(agent: AgentRegistryEntry): Promise<void> {
    const toolsDisplay = agent.tools && agent.tools.length
      ? agent.tools.map((t) => (this.isDangerous(t) ? `${t} ⚠` : t)).join(', ')
      : this.translate.instant('AGENT_REGISTRY.NO_TOOLS');
    const hasDangerous = (agent.tools || []).some((t) => this.isDangerous(t));

    let message = this.translate.instant('AGENT_TRUST.MESSAGE', {
      agent: agent.name,
      project: this.data.projectPath,
      tools: toolsDisplay,
    });
    if (hasDangerous) {
      message += '\n\n' + this.translate.instant('AGENT_TRUST.DANGER_WARNING');
    }

    const confirmed = await firstValueFrom(
      this.dialog
        .open(ConfirmDialogComponent, {
          width: '520px',
          data: {
            title: this.translate.instant('AGENT_TRUST.TITLE'),
            message,
            confirmText: this.translate.instant('AGENT_TRUST.CONFIRM'),
          },
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
