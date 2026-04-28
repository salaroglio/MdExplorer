import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { ProjectsService } from '../../../md-explorer/services/projects.service';
import { Participant } from '../../../md-explorer/models/participant';
import { IconEditorComponent } from './icon-editor/icon-editor.component';

export interface ProjectEditDialogData {
  id: string;
  name: string;
  description?: string;
  path: string;
  hasCustomIcon?: boolean;
  iconUpdatedAt?: string | null;
}

export type IconChangeAction = 'none' | 'set' | 'remove';

export interface ProjectEditDialogResult {
  id: string;
  name: string;
  description: string;
  path: string;
  participants: Participant[];
  iconAction: IconChangeAction;
  iconPngBase64?: string;
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

@Component({
  selector: 'app-project-edit-dialog',
  templateUrl: './project-edit-dialog.component.html',
  styleUrls: ['./project-edit-dialog.component.scss']
})
export class ProjectEditDialogComponent implements OnInit {

  readonly descriptionMaxLength = 200;
  readonly nameMaxLength = 255;

  form: FormGroup;
  participants: Participant[] = [];
  loadingParticipants = false;
  participantsError: string | null = null;

  // Reference to the embedded editor — used in onSave() to auto-apply any
  // pending paste/drag/zoom that the user did NOT explicitly confirm with the
  // "Applica" button. This is the safety net that fixes the "non memorizza più
  // l'icona" issue: the user pastes, tweaks, then clicks dialog Save without
  // realizing they have to click Applica first.
  @ViewChild(IconEditorComponent) iconEditor?: IconEditorComponent;

  // Icon editor state — collapsed by default; the editor is non-trivial in
  // height so we only mount it when the user actually wants to customize.
  iconEditorOpen = false;
  // Pending icon change tracked locally so it participates in the dialog's
  // Save/Cancel flow alongside name/description/participants.
  pendingIconAction: IconChangeAction = 'none';
  pendingIconPngBase64: string | null = null;
  // data URL used for the in-dialog preview (latest applied or current backend icon).
  iconPreviewUrl: string | null = null;
  // Tracks whether the project currently has a custom icon, accounting for
  // pending changes (so "Rimuovi" inside the editor only renders when sensible).
  get effectiveHasCustomIcon(): boolean {
    if (this.pendingIconAction === 'remove') return false;
    if (this.pendingIconAction === 'set') return true;
    return !!this.data.hasCustomIcon;
  }

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ProjectEditDialogComponent, ProjectEditDialogResult>,
    @Inject(MAT_DIALOG_DATA) public data: ProjectEditDialogData,
    private projectsService: ProjectsService
  ) {
    this.form = this.fb.group({
      name: [data.name ?? '', [Validators.required, Validators.maxLength(this.nameMaxLength)]],
      description: [data.description ?? '', [Validators.maxLength(this.descriptionMaxLength)]]
    });

    if (this.data.hasCustomIcon) {
      this.iconPreviewUrl = this.projectsService.getProjectIconUrl(this.data.id, this.data.iconUpdatedAt);
    }
  }

  ngOnInit(): void {
    this.loadParticipants();
  }

  get descriptionLength(): number {
    return (this.form.value.description ?? '').length;
  }

  private loadParticipants(): void {
    this.loadingParticipants = true;
    this.participantsError = null;
    this.projectsService.getParticipants(this.data.path).subscribe({
      next: list => {
        // Deep clone so we can mutate without affecting other cached lists.
        this.participants = (list || []).map(p => ({ ...p }));
        this.loadingParticipants = false;
      },
      error: err => {
        console.error('[ProjectEditDialog] Failed to load participants:', err);
        this.participantsError = 'load';
        this.loadingParticipants = false;
      }
    });
  }

  refreshFromGit(): void {
    // Merge fresh git authors with whatever the user has currently edited
    // (so in-flight edits aren't lost when the user clicks "Refresh").
    this.projectsService.getGitAuthors(this.data.path).subscribe({
      next: authors => {
        const known = new Map(this.participants.map(p => [p.gitEmail, p]));
        (authors || []).forEach(a => {
          const key = (a.email || '').trim().toLowerCase();
          if (!key) return;
          const existing = known.get(key);
          if (existing) {
            existing.gitName = a.name ?? existing.gitName;
            existing.manual = false;
          } else {
            this.participants.push({
              gitEmail: key,
              gitName: a.name,
              displayName: a.name,
              chatEmail: key,
              manual: false
            });
          }
        });
      },
      error: err => console.error('[ProjectEditDialog] Refresh from git failed:', err)
    });
  }

  addManual(): void {
    this.participants.push({
      gitEmail: '',
      gitName: '',
      displayName: '',
      chatEmail: '',
      manual: true
    });
  }

  removeManual(p: Participant): void {
    this.participants = this.participants.filter(x => x !== p);
  }

  isChatEmailInvalid(p: Participant): boolean {
    const v = (p.chatEmail || '').trim();
    if (!v) return false;  // empty is allowed (button just won't render)
    return !EMAIL_REGEX.test(v);
  }

  isManualInvalid(p: Participant): boolean {
    if (!p.manual) return false;
    const v = (p.chatEmail || '').trim();
    // Manual entries need a valid chat email — otherwise there is nothing to store.
    return !v || !EMAIL_REGEX.test(v);
  }

  get hasInvalidParticipants(): boolean {
    return this.participants.some(p => this.isChatEmailInvalid(p) || this.isManualInvalid(p));
  }

  toggleIconEditor(): void {
    this.iconEditorOpen = !this.iconEditorOpen;
  }

  onIconChanged(event: { pngBase64: string | null }): void {
    if (event.pngBase64 === null) {
      this.pendingIconAction = 'remove';
      this.pendingIconPngBase64 = null;
      this.iconPreviewUrl = null;
    } else {
      this.pendingIconAction = 'set';
      this.pendingIconPngBase64 = event.pngBase64;
      this.iconPreviewUrl = event.pngBase64; // data URL is renderable as-is
    }
    console.debug('[ProjectEditDialog] icon changed, action=', this.pendingIconAction,
      'pngLen=', event.pngBase64?.length ?? 0);
    // Auto-collapse so the user immediately sees the new preview chip;
    // they can re-open if they want another iteration.
    this.iconEditorOpen = false;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (this.hasInvalidParticipants) {
      return;
    }

    // Safety net: if the editor is still open with an image the user didn't
    // explicitly Apply, auto-apply now so the icon is not silently dropped.
    if (this.iconEditorOpen && this.iconEditor && this.iconEditor.hasImage
        && this.pendingIconAction === 'none') {
      this.iconEditor.applyAndEmit();
    }

    console.debug('[ProjectEditDialog] save, iconAction=', this.pendingIconAction,
      'pngLen=', this.pendingIconPngBase64?.length ?? 0);
    const value = this.form.value;
    const cleaned: Participant[] = this.participants
      .map(p => ({
        gitEmail: (p.manual ? (p.chatEmail || '') : p.gitEmail).trim().toLowerCase(),
        gitName: (p.gitName || '').trim(),
        displayName: (p.displayName || '').trim(),
        chatEmail: (p.chatEmail || p.gitEmail || '').trim(),
        manual: p.manual
      }))
      .filter(p => !!p.gitEmail);

    this.dialogRef.close({
      id: this.data.id,
      name: (value.name ?? '').trim(),
      description: (value.description ?? '').trim(),
      path: this.data.path,
      participants: cleaned,
      iconAction: this.pendingIconAction,
      iconPngBase64: this.pendingIconPngBase64 ?? undefined
    });
  }

  trackByEmail = (_: number, p: Participant) => p.gitEmail || p.chatEmail || _;
}
