import { Component, Inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { HarnessTarget, ProjectCreateConfigOptions, TemplateShowcaseChoice } from './project-create-config.model';

@Component({
  selector: 'app-project-create-config-dialog',
  templateUrl: './project-create-config-dialog.component.html',
  styleUrls: ['./project-create-config-dialog.component.scss']
})
export class ProjectCreateConfigDialogComponent {

  config: ProjectCreateConfigOptions;

  // ---- da template: la scelta arriva dalla vetrina (sprint 2026-10-19-Vetrina-Dei-Template.md) ----
  /** Il progetto nasce da template: la vetrina ha scelto una base o almeno un metodo. */
  readonly fromTemplate: boolean;
  /** «Da: La demo, Gara con ISO 9001». */
  readonly chosenTitles: string;
  templateExample = false;
  folderName = '';
  originUrl = '';
  templateError = '';

  constructor(
    public dialogRef: MatDialogRef<ProjectCreateConfigDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { projectPath: string; choice: TemplateShowcaseChoice },
    private translate: TranslateService,
  ) {
    this.fromTemplate = !!(data.choice?.baseUrl || data.choice?.methodUrls.length);
    this.chosenTitles = (data.choice?.titles || []).join(', ');
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
    { value: 'claude', icon: 'psychology', labelKey: 'PROJECT_CONFIG.HARNESS_CLAUDE', descKey: 'PROJECT_CONFIG.HARNESS_CLAUDE_DESC' },
    { value: 'none', icon: 'block', labelKey: 'PROJECT_CONFIG.HARNESS_NONE', descKey: 'PROJECT_CONFIG.HARNESS_NONE_DESC' }
  ];

  onCreateProject(): void {
    this.templateError = '';
    if (!this.fromTemplate) { this.dialogRef.close(this.config); return; }

    const choice = this.data.choice;
    const folderName = this.folderName.trim();
    if (!folderName || /[\\/]/.test(folderName)) { this.templateError = this.translate.instant('PROJECT_CONFIG.TEMPLATE_FOLDER_NEEDED'); return; }
    this.dialogRef.close({
      ...this.config,
      template: {
        baseUrl: choice.baseUrl, methodUrls: choice.methodUrls, language: choice.language, example: this.templateExample,
        folderName, originUrl: this.originUrl.trim(),
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}