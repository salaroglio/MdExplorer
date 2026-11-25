import { ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { AiChatService } from '../../services/ai-chat.service';
import { ChatHistoryService, ChatRecordEvent } from '../../services/chat-history.service';
import { LearnRequest, LearnReview, ReviewedProposal, SkillLearningService } from '../../services/skill-learning.service';

export interface SkillLearningDialogData {
  projectPath: string;
  threadId: string;
}

type Decision = 'pending' | 'approved' | 'rejected';

/** A proposal in the window, with what the user decided and, if corrected, the text to write. */
interface ProposalView {
  proposal: ReviewedProposal;
  decision: Decision;
  editing: boolean;
  content: string;
  /** The evidence event opened under the proposal, read in full from the chat. */
  openedEvidence: { seq: number; text: string } | null;
}

/**
 * «Impara da questa attività» (sprint 2026-11-24-Skill-Che-Migliorano, F3). The service builds the prompt, the tab's own
 * engine answers on the «learn» channel (same session, same engine), the service reads and checks the answer, and here
 * the user sees what MarkAgent learned and decides proposal by proposal: approve, reject, or correct the text. A proposal
 * that does not pass stays, with its reasons, and cannot be approved.
 */
@Component({
  selector: 'app-skill-learning-dialog',
  templateUrl: './skill-learning-dialog.component.html',
  styleUrls: ['./skill-learning-dialog.component.scss'],
})
export class SkillLearningDialogComponent implements OnInit {
  state: 'preparing' | 'asking' | 'review' | 'error' = 'preparing';
  error = '';
  /** The answer as it arrives, while the engine thinks. */
  streamed = '';
  request: LearnRequest | null = null;
  review: LearnReview | null = null;
  views: ProposalView[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SkillLearningDialogData,
    private ref: MatDialogRef<SkillLearningDialogComponent>,
    private learning: SkillLearningService,
    private aiChat: AiChatService,
    private history: ChatHistoryService,
    private translate: TranslateService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.learning.prepare(this.data.projectPath, this.data.threadId).subscribe({
      next: request => this.ask(request),
      error: err => this.fail(err),
    });
  }

  private async ask(request: LearnRequest): Promise<void> {
    this.request = request;
    this.state = 'asking';
    this.cd.detectChanges();
    try {
      const answer = await this.aiChat.askOnChannel(request.channel, request.prompt, chunk => {
        this.streamed += chunk;
        this.cd.detectChanges();
      });
      this.learning.review(this.data.projectPath, request.from, request.to, answer).subscribe({
        next: review => {
          this.review = review;
          this.views = review.proposals.map(p => ({
            proposal: p, decision: 'pending', editing: false, content: p.content || '', openedEvidence: null,
          }));
          this.state = 'review';
          this.cd.detectChanges();
        },
        error: err => this.fail(err),
      });
    } catch (err) {
      this.fail(err);
    }
  }

  private fail(err: any): void {
    this.error = err?.error?.message || err?.message || String(err);
    this.state = 'error';
    this.cd.detectChanges();
  }

  decide(view: ProposalView, decision: Decision): void {
    if (decision === 'approved' && !view.proposal.isValid) return;
    view.decision = view.decision === decision ? 'pending' : decision;
  }

  toggleEdit(view: ProposalView): void {
    view.editing = !view.editing;
  }

  /** An evidence event, read in full from the saved chat: the reason the proposal exists. */
  openEvidence(view: ProposalView, seq: number): void {
    if (view.openedEvidence?.seq === seq) { view.openedEvidence = null; return; }
    this.history.content(this.data.threadId).subscribe({
      next: content => {
        const e = content.events.find(x => x.seq === seq);
        view.openedEvidence = { seq, text: e ? this.describe(e) : this.translate.instant('SKILL_LEARNING.EVIDENCE_MISSING', { seq }) };
        this.cd.detectChanges();
      },
      error: err => {
        view.openedEvidence = { seq, text: err?.error?.message || err?.message || String(err) };
        this.cd.detectChanges();
      },
    });
  }

  private describe(e: ChatRecordEvent): string {
    const head = e.tool ? `${e.kind} · ${e.tool}` : e.kind;
    const body = e.content ?? (e.json ? JSON.stringify(e.json, null, 2) : '');
    return `${head}\n\n${body}`;
  }

  get approvedCount(): number {
    return this.views.filter(v => v.decision === 'approved').length;
  }

  close(): void {
    this.ref.close();
  }

  byIndex(_: number, view: ProposalView): number {
    return view.proposal.index;
  }
}
