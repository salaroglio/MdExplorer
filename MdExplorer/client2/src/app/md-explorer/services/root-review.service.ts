import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { AiChatService } from '../../services/ai-chat.service';
import { ProjectsService } from './projects.service';
import { ChatHistoryService } from '../../services/chat-history.service';
import { RootReviewGuardDialogComponent, RootReviewGuardChoice, RootReviewGuardReason } from '../components/root-review/root-review-guard-dialog.component';

/** Un file delle radici cambiato da MarkAgent e non ancora rivisto (api/coderoots/review). */
export interface RootChangeSummary {
  alias: string;
  relative: string;
  logicalPath: string;
  status: 'modified' | 'created' | 'deleted';
  added: number;
  removed: number;
  hunks: number;
  /** Il file non è quello dell'ultima scrittura di MarkAgent: qualcuno l'ha cambiato dopo. */
  changedOutside: boolean;
  updatedAt: string;
}

export interface ChangeHunk {
  index: number;
  originalStart: number;
  originalLines: string[];
  currentStart: number;
  currentLines: string[];
  hash: string;
}

export interface RootChangeDetail {
  summary: RootChangeSummary;
  originalText: string | null;
  currentText: string | null;
  encoding: string | null;
  hunks: ChangeHunk[];
}

/**
 * La revisione dei file delle radici di codice cambiati da MarkAgent (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, F4):
 * MarkAgent scrive davvero e MDE tiene l'originale; qui l'elenco in attesa (il contatore del tab), il pannello nel riquadro
 * grande, il riquadro a fine turno nella chat (D9) e l'avviso dove il contesto si spezza (D11): nuova chat, ripresa di
 * un'altra, cambio di progetto. «Chiudi tutto e esci» della tray no: è della shell Electron, e tutto resta in Angular e .NET
 * (scelta dell'utente 27/11) — le modifiche sono già nei file, gli originali nel registro, il contatore le ricorda alla
 * riapertura, come VS Code.
 */
@Injectable({ providedIn: 'root' })
export class RootReviewService {
  private readonly api = '../api/coderoots/review';

  /** I file in attesa di revisione nel progetto aperto. */
  readonly pending$ = new BehaviorSubject<RootChangeSummary[]>([]);
  /** Il pannello della revisione nel riquadro grande. */
  readonly open$ = new BehaviorSubject<boolean>(false);
  /** Il file da mostrare all'apertura del pannello (dal riquadro di un turno). */
  readonly focus$ = new BehaviorSubject<string | null>(null);

  private turnStartedAt: number | null = null;

  constructor(private http: HttpClient, private dialog: MatDialog, private projects: ProjectsService,
              private aiChat: AiChatService, private chatHistory: ChatHistoryService) {
    this.projects.currentProjects$.subscribe(() => { this.close(); this.refresh(); });
    // Il turno comincia quando la chat si mette a rispondere; a fine turno i file toccati DA QUEL turno vanno nel riquadro.
    this.aiChat.isStreaming$.subscribe(streaming => { if (streaming && this.turnStartedAt == null) this.turnStartedAt = Date.now(); });
    this.aiChat.turnCompleted$.subscribe(messageId => this.afterTurn(messageId));
  }

  private get projectPath(): string | null {
    return this.projects.currentProjects$.value?.path ?? null;
  }

  async refresh(): Promise<RootChangeSummary[]> {
    const projectPath = this.projectPath;
    if (!projectPath) { this.pending$.next([]); return []; }
    try {
      const list = await firstValueFrom(this.http.get<RootChangeSummary[]>(this.api, { params: new HttpParams().set('projectPath', projectPath) }));
      this.pending$.next(list || []);
      return list || [];
    } catch (e) {
      // un progetto senza radici o un servizio che non risponde: niente da rivedere da mostrare, il motivo nella console
      console.warn('[RootReview] elenco non letto:', e);
      this.pending$.next([]);
      return [];
    }
  }

  private async afterTurn(messageId: string | null): Promise<void> {
    const since = this.turnStartedAt;
    this.turnStartedAt = null;
    const list = await this.refresh();
    if (since == null) return;
    // margine di un secondo: l'orologio del servizio e quello della pagina sono lo stesso computer, ma i millisecondi no
    const touched = list.filter(f => new Date(f.updatedAt).getTime() >= since - 1000);
    if (touched.length > 0)
      this.aiChat.attachRootChanges(messageId, touched.map(f => ({ logicalPath: f.logicalPath, status: f.status, added: f.added, removed: f.removed })));
  }

  openPanel(logicalPath?: string): void {
    // un riquadro alla volta: la cronologia delle chat sta nello stesso posto
    this.chatHistory.close();
    this.focus$.next(logicalPath ?? null);
    this.open$.next(true);
    this.refresh();
  }

  close(): void {
    if (this.open$.value) this.open$.next(false);
  }

  detail(alias: string, path: string): Promise<RootChangeDetail> {
    return firstValueFrom(this.http.get<RootChangeDetail>(`${this.api}/file`, {
      params: new HttpParams().set('projectPath', this.projectPath ?? '').set('alias', alias).set('path', path),
    }));
  }

  async keep(alias: string, path: string, hunk?: ChangeHunk): Promise<void> {
    await firstValueFrom(this.http.post(`${this.api}/keep`, { projectPath: this.projectPath, alias, path, hunk: hunk?.index ?? null, hash: hunk?.hash ?? null }));
    await this.refresh();
  }

  async undo(alias: string, path: string, hunk?: ChangeHunk): Promise<void> {
    await firstValueFrom(this.http.post(`${this.api}/undo`, { projectPath: this.projectPath, alias, path, hunk: hunk?.index ?? null, hash: hunk?.hash ?? null }));
    await this.refresh();
  }

  /** Tieni tutto o Annulla tutto: i file che non riescono tornano con il motivo. */
  async all(keep: boolean): Promise<string[]> {
    const result = await firstValueFrom(this.http.post<{ problems: string[] }>(`${this.api}/${keep ? 'keep-all' : 'undo-all'}`, { projectPath: this.projectPath }));
    await this.refresh();
    return result?.problems ?? [];
  }

  /**
   * L'avviso dove il contesto si spezza (D11). Vero = si può andare avanti (nessun file in attesa, Tieni tutto, Annulla tutto
   * riuscito o «Più tardi»); falso = ci si ferma (Rivedi adesso, o un Annulla tutto con file che non sono riusciti).
   */
  async guard(reason: RootReviewGuardReason): Promise<boolean> {
    const pending = await this.refresh();
    if (pending.length === 0) return true;
    const choice: RootReviewGuardChoice | undefined = await firstValueFrom(this.dialog.open(RootReviewGuardDialogComponent, {
      data: { reason, files: pending }, disableClose: false, width: '520px',
    }).afterClosed());
    switch (choice) {
      case 'review': this.openPanel(); return false;
      case 'keep': return (await this.all(true)).length === 0 || (this.openPanel(), false);
      case 'undo': return (await this.all(false)).length === 0 || (this.openPanel(), false);
      default: return true; // «Più tardi» o chiuso senza scegliere: restano da rivedere, ricordati alla riapertura
    }
  }
}
