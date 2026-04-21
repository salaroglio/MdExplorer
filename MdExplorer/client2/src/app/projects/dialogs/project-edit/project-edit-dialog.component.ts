import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';

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
}

@Component({
  selector: 'app-project-edit-dialog',
  templateUrl: './project-edit-dialog.component.html',
  styleUrls: ['./project-edit-dialog.component.scss']
})
export class ProjectEditDialogComponent {

  readonly descriptionMaxLength = 200;
  readonly nameMaxLength = 255;

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ProjectEditDialogComponent, ProjectEditDialogResult>,
    @Inject(MAT_DIALOG_DATA) public data: ProjectEditDialogData
  ) {
    this.form = this.fb.group({
      name: [data.name ?? '', [Validators.required, Validators.maxLength(this.nameMaxLength)]],
      description: [data.description ?? '', [Validators.maxLength(this.descriptionMaxLength)]]
    });
  }

  get descriptionLength(): number {
    return (this.form.value.description ?? '').length;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.value;
    this.dialogRef.close({
      id: this.data.id,
      name: (value.name ?? '').trim(),
      description: (value.description ?? '').trim()
    });
  }
}
