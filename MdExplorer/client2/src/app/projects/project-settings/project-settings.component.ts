import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ProjectSettingsService } from '../services/project-settings.service';
import { CompatibilityModeService } from '../../services/compatibility-mode.service';
import { IdeConfigurationService } from '../services/ide-configuration.service';

@Component({
  selector: 'app-project-settings',
  templateUrl: './project-settings.component.html',
  styleUrls: ['./project-settings.component.scss']
})
export class ProjectSettingsComponent implements OnInit {
  rule1Enabled: boolean = false;
  githubModeEnabled: boolean = false;
  selectedIde: string = 'vscode';
  vscodePath: string = '';
  intellijPath: string = '';
  projectId: string;
  projectName: string;
  projectPath: string;
  loading: boolean = false;
  saving: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<ProjectSettingsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private projectSettingsService: ProjectSettingsService,
    private compatibilityService: CompatibilityModeService,
    private ideConfigService: IdeConfigurationService
  ) {
    this.projectId = data.projectId;
    this.projectName = data.projectName;
    this.projectPath = data.projectPath;
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading = true;
    let rule1Loaded = false;
    let compatibilityLoaded = false;
    let ideConfigLoaded = false;

    const checkIfDone = () => {
      if (rule1Loaded && compatibilityLoaded && ideConfigLoaded) {
        this.loading = false;
      }
    };

    // Load Rule 1 setting
    this.projectSettingsService.getRule1Setting().subscribe({
      next: (response) => {
        this.rule1Enabled = response.enabled;
        rule1Loaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading Rule 1 setting:', error);
        rule1Loaded = true;
        checkIfDone();
      }
    });

    // Load compatibility mode for this specific project
    this.compatibilityService.getCurrentMode(this.projectPath).subscribe({
      next: (response) => {
        console.log('Compatibility mode loaded for project:', this.projectPath, response);
        this.githubModeEnabled = response.mode === 'github';
        compatibilityLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading compatibility mode:', error);
        compatibilityLoaded = true;
        checkIfDone();
      }
    });

    // Load IDE configuration for this specific project
    this.ideConfigService.getIdeConfiguration(this.projectPath).subscribe({
      next: (response) => {
        console.log('IDE configuration loaded for project:', this.projectPath, response);
        this.selectedIde = response.selectedIde || 'vscode';
        this.vscodePath = response.vscodePath || '';
        this.intellijPath = response.intellijPath || '';
        ideConfigLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading IDE configuration:', error);
        ideConfigLoaded = true;
        checkIfDone();
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

  onGitHubModeChange(): void {
    console.log('onGitHubModeChange called, githubModeEnabled:', this.githubModeEnabled);
    this.saving = true;
    const mode = this.githubModeEnabled ? 'github' : 'mdexplorer';

    console.log('Setting compatibility mode to:', mode, 'for project:', this.projectPath);
    this.compatibilityService.setCompatibilityMode({
      mode,
      githubOptions: {
        embedImages: false,
        stripInteractive: true,
        preserveEmoji: true
      },
      projectPath: this.projectPath
    }).subscribe({
      next: (response) => {
        console.log('Compatibility mode saved successfully:', mode, response);
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving compatibility mode:', error);
        this.saving = false;
        // Revert the change on error
        this.githubModeEnabled = !this.githubModeEnabled;
      }
    });
  }

  onIdeChange(): void {
    console.log('onIdeChange called, selectedIde:', this.selectedIde);
    this.saving = true;

    this.ideConfigService.setIdeConfiguration({
      selectedIde: this.selectedIde,
      projectPath: this.projectPath
    }).subscribe({
      next: (response) => {
        console.log('IDE configuration saved successfully:', this.selectedIde, response);
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving IDE configuration:', error);
        this.saving = false;
        // Revert the change on error
        this.selectedIde = this.selectedIde === 'vscode' ? 'intellij' : 'vscode';
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}