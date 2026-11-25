import { AfterViewInit, Component, ElementRef, Inject, ViewChild } from '@angular/core';
import {
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MdFile } from '../../../models/md-file';
import { MdFileService, RenamedDocument } from '../../../services/md-file.service';

/**
 * «Rinomina» di un documento dall'albero (sprint 2026-11-25-Spostamento-File-E-Link): il nome con la parte prima di
 * `.md` già selezionata, Invio salva, Esc annulla. Il servizio rinomina come uno spostamento nella stessa cartella, quindi
 * i link si ricalcolano; se rifiuta (nome già preso, caratteri non validi) il motivo resta qui sotto e la finestra aperta.
 * Chiude con il risultato, o con niente se annullata.
 */
@Component({
  selector: 'app-rename-document',
  templateUrl: './rename-document.component.html',
  styleUrls: ['./rename-document.component.scss'],
})
export class RenameDocumentComponent implements AfterViewInit {
  @ViewChild('nameInput') nameInput: ElementRef<HTMLInputElement>;
  name: string;
  /** Una cartella: niente .md da lasciare fuori dalla selezione, titolo e spiegazione suoi. */
  readonly isFolder: boolean;
  error: string | null = null;
  saving = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<RenameDocumentComponent, RenamedDocument | null>,
    private mdFileService: MdFileService,
  ) {
    this.name = data.name;
    this.isFolder = data.type === 'folder';
  }

  ngAfterViewInit(): void {
    // Selezionata la parte prima di .md: si scrive subito il nome nuovo, l'estensione resta.
    setTimeout(() => {
      const input = this.nameInput?.nativeElement;
      if (!input) return;
      input.focus();
      const dot = !this.isFolder && this.name.toLowerCase().endsWith('.md') ? this.name.length - 3 : this.name.length;
      input.setSelectionRange(0, dot);
    });
  }

  get canSave(): boolean {
    const n = (this.name || '').trim();
    return !this.saving && n.length > 0 && n !== this.data.name;
  }

  save(): void {
    if (!this.canSave) return;
    this.saving = true;
    this.error = null;
    this.mdFileService.renameMdFile(this.data, this.name.trim()).subscribe({
      next: result => this.dialogRef.close(result),
      error: err => {
        this.saving = false;
        this.error = err?.error?.error || err?.message || 'Error';
      },
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
