import { Component, Input, OnInit } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { ShowFileSystemComponent } from '../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../commons/components/show-file-system/show-file-metadata';
import { LanguageLibraryEntry, LanguageLibraryService, ProjectLanguageEntry } from './language-library.service';

/**
 * I linguaggi del grafo (12/11/2026). Senza `projectPath`: la libreria di questo computer, nelle impostazioni generali —
 * si carica un pacchetto (cartella o .zip) una volta sola, si toglie. Con `projectPath`: le impostazioni del progetto —
 * si attiva un pacchetto della libreria (copiato in .mde-languages/, viaggia con Git), si aggiorna, si disattiva; uno
 * arrivato con il progetto si può aggiungere alla libreria.
 */
@Component({
  selector: 'app-language-library',
  templateUrl: './language-library.component.html',
  styleUrls: ['./language-library.component.scss'],
})
export class LanguageLibraryComponent implements OnInit {
  /** Il progetto: se c'è, la vista è quella delle impostazioni del progetto. */
  @Input() projectPath: string | null = null;

  libraryFolder = '';
  library: LanguageLibraryEntry[] = [];
  project: ProjectLanguageEntry[] = [];
  loading = false;
  /** L'id su cui si sta lavorando (null = nessuno). */
  busy: string | null = null;
  message: string | null = null;
  error: string | null = null;
  /** Un import nella libreria che aspetta la conferma per sostituire il pacchetto con lo stesso id. */
  pendingReplace: { sourcePath: string; text: string } | null = null;

  constructor(private service: LanguageLibraryService, private dialog: MatDialog, private translate: TranslateService) { }

  ngOnInit(): void { this.load(); }

  get projectMode(): boolean { return !!this.projectPath; }

  load(): void {
    this.loading = true;
    const done = () => { this.loading = false; };
    if (this.projectMode) {
      this.service.forProject(this.projectPath!).subscribe({
        next: r => { this.libraryFolder = r.folder; this.project = r.packages; done(); },
        error: e => { this.fail(e); done(); },
      });
    } else {
      this.service.list().subscribe({
        next: r => { this.libraryFolder = r.folder; this.library = r.packages; done(); },
        error: e => { this.fail(e); done(); },
      });
    }
  }

  /** Nella libreria: una cartella di un pacchetto o uno .zip, scelti sul file system del servizio. */
  importPackage(from: 'folder' | 'zip'): void {
    const picker = new ShowFileMetadata();
    picker.title = this.translate.instant(from === 'folder' ? 'LANGUAGE_LIBRARY.PICK_FOLDER' : 'LANGUAGE_LIBRARY.PICK_ZIP');
    picker.typeOfSelection = from === 'folder' ? 'Folders' : 'FoldersAndFiles';
    picker.buttonText = this.translate.instant('LANGUAGE_LIBRARY.LOAD');
    picker.start = 'root';
    this.dialog.open(ShowFileSystemComponent, { width: '900px', height: '700px', panelClass: 'resizable-dialog-container', data: picker })
      .afterClosed().subscribe(result => {
        const path = typeof result?.data === 'string' ? result.data : null;
        if (path) { this.runImport(path, false); }
      });
  }

  confirmReplace(): void { if (this.pendingReplace) { this.runImport(this.pendingReplace.sourcePath, true); } }
  cancelReplace(): void { this.pendingReplace = null; }

  private runImport(sourcePath: string, replace: boolean): void {
    this.start('import');
    this.service.import(sourcePath, replace).subscribe({
      next: r => this.ok(this.translate.instant(r.replaced ? 'LANGUAGE_LIBRARY.REPLACED' : 'LANGUAGE_LIBRARY.LOADED', { name: r.displayName, version: r.version })),
      error: e => {
        this.busy = null;
        if (e?.status === 409) { this.pendingReplace = { sourcePath, text: e?.error?.error || '' }; return; }
        this.fail(e);
      },
    });
  }

  remove(entry: LanguageLibraryEntry): void {
    this.start(entry.id);
    this.service.remove(entry.id).subscribe({
      next: () => this.ok(this.translate.instant('LANGUAGE_LIBRARY.REMOVED', { name: entry.displayName })),
      error: e => this.fail(e),
    });
  }

  activate(entry: ProjectLanguageEntry, replace = false): void {
    this.start(entry.id);
    this.service.activate(this.projectPath!, entry.id, replace).subscribe({
      next: r => this.ok(this.translate.instant(replace ? 'LANGUAGE_LIBRARY.UPDATED' : 'LANGUAGE_LIBRARY.ACTIVATED',
        { name: entry.displayName, version: r.version, skills: r.installed.length })),
      error: e => this.fail(e),
    });
  }

  deactivate(entry: ProjectLanguageEntry): void {
    this.start(entry.id);
    this.service.deactivate(this.projectPath!, entry.id).subscribe({
      next: () => this.ok(this.translate.instant('LANGUAGE_LIBRARY.DEACTIVATED', { name: entry.displayName })),
      error: e => this.fail(e),
    });
  }

  adopt(entry: ProjectLanguageEntry): void {
    this.start(entry.id);
    this.service.adopt(this.projectPath!, entry.id, entry.inLibrary).subscribe({
      next: r => this.ok(this.translate.instant('LANGUAGE_LIBRARY.ADOPTED', { name: entry.displayName, version: r.version })),
      error: e => this.fail(e),
    });
  }

  /** Che cosa fa il pacchetto, in poche parole: le estensioni e, se ci sono, database e pagine. */
  kinds(entry: LanguageLibraryEntry): string {
    const parts = [entry.extensions?.join(' ')];
    if (entry.database) { parts.push(this.translate.instant('LANGUAGE_LIBRARY.KIND_DATABASE')); }
    if (entry.pages) { parts.push(this.translate.instant('LANGUAGE_LIBRARY.KIND_PAGES')); }
    return parts.filter(p => !!p).join(' · ');
  }

  private start(id: string): void { this.busy = id; this.message = null; this.error = null; this.pendingReplace = null; }
  private ok(text: string): void { this.busy = null; this.message = text; this.load(); }
  private fail(e: any): void { this.busy = null; this.error = e?.error?.error || e?.message || this.translate.instant('COMMON.ERROR'); }
}
