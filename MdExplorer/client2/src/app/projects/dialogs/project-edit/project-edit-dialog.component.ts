import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { ProjectsService } from '../../../md-explorer/services/projects.service';
import { Participant } from '../../../md-explorer/models/participant';

export interface ProjectEditDialogData {
  id: string;
  name: string;
  description?: string;
  path: string;
}

export interface ProjectEditDialogResult {
  id: string;
  name: string;
  description: string;
  path: string;
  participants: Participant[];
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
      participants: cleaned
    });
  }

  trackByEmail = (_: number, p: Participant) => p.gitEmail || p.chatEmail || _;
}
