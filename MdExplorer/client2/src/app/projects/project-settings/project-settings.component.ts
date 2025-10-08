import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ProjectSettingsService } from '../services/project-settings.service';

@Component({
  selector: 'app-project-settings',
  templateUrl: './project-settings.component.html',
  styleUrls: ['./project-settings.component.scss']
})
export class ProjectSettingsComponent implements OnInit {
  rule1Enabled: boolean = false;
  projectId: string;
  projectName: string;
  loading: boolean = false;
  saving: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<ProjectSettingsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private projectSettingsService: ProjectSettingsService
  ) {
    this.projectId = data.projectId;
    this.projectName = data.projectName;
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading = true;
    this.projectSettingsService.getRule1Setting().subscribe({
      next: (response) => {
        this.rule1Enabled = response.enabled;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading settings:', error);
        this.loading = false;
      }
    });
  }

  onRule1Change(): void {
    this.saving = true;
    this.projectSettingsService.setRule1Setting(this.rule1Enabled).subscribe({
      next: () => {
        console.log('Rule 1 setting saved successfully');
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving Rule 1 setting:', error);
        this.saving = false;
        // Revert the change on error
        this.rule1Enabled = !this.rule1Enabled;
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}