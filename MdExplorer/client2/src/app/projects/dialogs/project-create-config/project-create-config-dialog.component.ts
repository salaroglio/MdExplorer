import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { HarnessTarget, ProjectCreateConfigOptions, TemplateCatalogEntry } from './project-create-config.model';

@Component({
  selector: 'app-project-create-config-dialog',
  templateUrl: './project-create-config-dialog.component.html',
  styleUrls: ['./project-create-config-dialog.component.scss']
})
export class ProjectCreateConfigDialogComponent implements OnInit {

  config: ProjectCreateConfigOptions;

  // ---- da template (P4) ----
  /** Il progetto nasce da template invece che dalla cartella scelta così com'è. */
  fromTemplate = false;
  /** L'elenco che viaggia con MdExplorer; null finché non arriva. */
  catalog: TemplateCatalogEntry[] | null = null;
  catalogError = '';
  /** L'indirizzo della base scelta ('' = nessuna base). */
  baseUrl = '';
  /** I metodi dell'elenco scelti, per indirizzo. */
  chosenMethods: { [url: string]: boolean } = {};
  /** Altri metodi, per indirizzo: uno per riga. */
  otherMethods = '';
  templateLanguage = 'it';
  templateExample = false;
  folderName = '';
  originUrl = '';
  templateError = '';

  get bases(): TemplateCatalogEntry[] { return (this.catalog || []).filter(t => t.kind === 'project'); }
  get methods(): TemplateCatalogEntry[] { return (this.catalog || []).filter(t => t.kind === 'method'); }

  constructor(
    public dialogRef: MatDialogRef<ProjectCreateConfigDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { projectPath: string },
    private http: HttpClient,
    private translate: TranslateService,
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
    { value: 'claude', icon: 'psychology', labelKey: 'PROJECT_CONFIG.HARNESS_CLAUDE', descKey: 'PROJECT_CONFIG.HARNESS_CLAUDE_DESC' },
    { value: 'none', icon: 'block', labelKey: 'PROJECT_CONFIG.HARNESS_NONE', descKey: 'PROJECT_CONFIG.HARNESS_NONE_DESC' }
  ];

  ngOnInit(): void {
    this.templateLanguage = this.translate.currentLang === 'en' ? 'en' : 'it';
    this.http.get<{ templates: TemplateCatalogEntry[] }>('../api/MdProjects/TemplateCatalog').subscribe({
      next: res => this.catalog = res?.templates || [],
      error: err => { this.catalog = []; this.catalogError = err?.error?.error || err?.message || ''; },
    });
  }

  /** La voce ha la lingua scelta per il progetto: un metodo solo italiano non si unisce a un progetto inglese. */
  speaks(t: TemplateCatalogEntry): boolean { return (t.languages || []).includes(this.templateLanguage); }

  languageNames(t: TemplateCatalogEntry): string {
    const names: { [code: string]: string } = { it: 'italiano', en: 'English' };
    return (t.languages || []).map(l => names[l] || l).join(', ');
  }

  /** Il testo di una voce nella lingua dell'interfaccia, o nella prima che ha. */
  text(texts: { [language: string]: string }): string {
    return texts?.[this.translate.currentLang] || texts?.[Object.keys(texts || {})[0]] || '';
  }

  onCreateProject(): void {
    this.templateError = '';
    if (!this.fromTemplate) { this.dialogRef.close(this.config); return; }

    const chosen = (this.catalog || []).filter(t => t.kind === 'method' && this.chosenMethods[t.url]);
    const mute = chosen.filter(t => !this.speaks(t));
    if (mute.length) {
      this.templateError = this.translate.instant('PROJECT_CONFIG.TEMPLATE_LANGUAGE_MISSING', { methods: mute.map(t => this.text(t.title)).join(', ') });
      return;
    }
    const listed = chosen.map(t => t.url);
    const typed = this.otherMethods.split('\n').map(l => l.trim()).filter(l => l.length > 0);
    const methodUrls = [...listed, ...typed];
    const folderName = this.folderName.trim();
    if (!this.baseUrl && methodUrls.length === 0) { this.templateError = this.translate.instant('PROJECT_CONFIG.TEMPLATE_NONE_CHOSEN'); return; }
    if (!folderName || /[\\/]/.test(folderName)) { this.templateError = this.translate.instant('PROJECT_CONFIG.TEMPLATE_FOLDER_NEEDED'); return; }
    this.dialogRef.close({
      ...this.config,
      template: {
        baseUrl: this.baseUrl || null, methodUrls, language: this.templateLanguage, example: this.templateExample,
        folderName, originUrl: this.originUrl.trim(),
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}