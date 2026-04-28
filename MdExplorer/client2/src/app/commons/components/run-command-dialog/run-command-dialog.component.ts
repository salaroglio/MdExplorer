import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';

export interface RunCommandDialogParameter {
  name: string;
  defaultValue: string;
  isSecret: boolean;
  description?: string;
  kind?: string;
}

export interface RunCommandDialogData {
  language: string;
  code: string;
  params: RunCommandDialogParameter[];
  needsTrust: boolean;
  projectPath: string;
}

export interface RunCommandDialogResult {
  confirmed: boolean;
  trustProject: boolean;
  parameters: { [name: string]: string };
}

@Component({
  selector: 'app-run-command-dialog',
  templateUrl: './run-command-dialog.component.html',
  styleUrls: ['./run-command-dialog.component.scss']
})
export class RunCommandDialogComponent implements OnInit {
  values: { [name: string]: string } = {};
  trustProject = false;
  showSecret: { [name: string]: boolean } = {};

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: RunCommandDialogData,
    private dialogRef: MatDialogRef<RunCommandDialogComponent, RunCommandDialogResult>,
  ) { }

  ngOnInit(): void {
    for (const p of this.data.params || []) {
      this.values[p.name] = p.defaultValue || '';
    }
  }

  cancel(): void {
    this.dialogRef.close({ confirmed: false, trustProject: false, parameters: {} });
  }

  run(): void {
    this.dialogRef.close({
      confirmed: true,
      trustProject: this.data.needsTrust ? this.trustProject : true,
      parameters: { ...this.values },
    });
  }

  canRun(): boolean {
    if (this.data.needsTrust && !this.trustProject) return false;
    return true;
  }

  toggleSecret(name: string): void {
    this.showSecret[name] = !this.showSecret[name];
  }
}
