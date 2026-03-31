import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';
import { Snippet } from './models/snippet';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-new-markdown',
  templateUrl: './new-markdown.component.html',
  styleUrls: ['./new-markdown.component.scss']
})
export class NewMarkdownComponent implements OnInit {
  public markdownTitle: string;

  documentTypes: Snippet[] = [];

  private initDocumentTypes(): void {
    this.documentTypes = [
      { id: 0, name: this.translate.instant('NEW_MARKDOWN.TEXT_DOC'), documentType: 'document' },
      { id: 8, name: this.translate.instant('NEW_MARKDOWN.PROMPTLAB'), documentType: 'promptlab' },
      { id: 5, name: this.translate.instant('NEW_MARKDOWN.SLIDES'), documentType: 'slides' }
    ];
    this.selectedTemplate = this.documentTypes[0];
  }

  selectedTemplate: Snippet;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<NewMarkdownComponent>,
    private mdFileService: MdFileService,
    private translate: TranslateService
  ) {
    this.initDocumentTypes();
  }

  ngOnInit(): void {
  }

  dismiss() {
    this.dialogRef.close();
  }

  save() {
    if (!this.markdownTitle || !this.markdownTitle.trim()) {
      return;
    }
    this.mdFileService.CreateNewMd(
      this.data.fullPath,
      this.markdownTitle,
      this.data.level,
      this.selectedTemplate.id,
      this.selectedTemplate.documentType)
      .subscribe(data => {
        console.log(JSON.stringify(data, null, 2));
        this.mdFileService.addNewFile(data);
        this.mdFileService.setSelectedMdFileFromSideNav(data[data.length - 1]);
      });
    this.dialogRef.close();
  }
}
