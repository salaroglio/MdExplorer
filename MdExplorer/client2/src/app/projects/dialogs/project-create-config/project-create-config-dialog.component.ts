import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { HarnessTarget, ProjectCreateConfigOptions } from './project-create-config.model';

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
      harness: 'copilot' // Copilot resta il default storico
    };
  }

  /**
   * Le tre scelte, con l'icona che le accompagna nella lista. Sono mutuamente esclusive:
   * il progetto ne dichiara UNA, e MdExplorer installa i propri file solo dove quella dice.
   */
  readonly harnessOptions: { value: HarnessTarget; icon: string; labelKey: string; descKey: string }[] = [
    { value: 'copilot', icon: 'smart_toy', labelKey: 'PROJECT_CONFIG.HARNESS_COPILOT', descKey: 'PROJECT_CONFIG.HARNESS_COPILOT_DESC' },
    { value: 'opencode', icon: 'terminal', labelKey: 'PROJECT_CONFIG.HARNESS_OPENCODE', descKey: 'PROJECT_CONFIG.HARNESS_OPENCODE_DESC' },
    { value: 'none', icon: 'block', labelKey: 'PROJECT_CONFIG.HARNESS_NONE', descKey: 'PROJECT_CONFIG.HARNESS_NONE_DESC' }
  ];

  ngOnInit(): void {
  }

  onCreateProject(): void {
    this.dialogRef.close(this.config);
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}