import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { ProjectCreateConfigOptions } from './project-create-config.model';

@Component({
  selector: 'app-project-create-config-dialog',
  templateUrl: './project-create-config-dialog.component.html',
  styleUrls: ['./project-create-config-dialog.component.scss']
})
export class ProjectCreateConfigDialogComponent implements OnInit {

  config: ProjectCreateConfigOptions;

  constructor(
    public dialogRef: MatDialogRef<ProjectCreateConfigDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { projectPath: string }
  ) {
    // Initialize with default values
    this.config = {
      projectPath: data.projectPath,
      initializeGit: false, // Git not initialized by default
      addCopilotInstructions: true // Copilot instructions enabled by default
    };
  }

  ngOnInit(): void {
  }

  onCreateProject(): void {
    this.dialogRef.close(this.config);
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}