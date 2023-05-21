import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';
import { Snippet } from './models/snippet';

@Component({
  selector: 'app-new-markdown',
  templateUrl: './new-markdown.component.html',
  styleUrls: ['./new-markdown.component.scss']
})
export class NewMarkdownComponent implements OnInit {
  public markdownTitle: string
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<NewMarkdownComponent>,
    private mdFileService: MdFileService
  ) {
    this.selectedTemplate = this.plantumlTemplates[0]    
  }

  
  selectedTemplate: Snippet;

  plantumlTemplates: Snippet[] =
    [{ id: 0, name: 'Text document', documentType:'document' },
      { id: 1, name: 'Sequence Diagram', documentType: 'document' },
      { id: 2, name: 'State Diagram', documentType: 'document' },
      { id: 3, name: 'Workflow', documentType: 'document' },
      { id: 4, name: 'Gantt current week', documentType: 'document' }];

  slideTemplates: Snippet[] =
    [{
      id: 5, name: 'Flicker document', documentType: 'slides'
    }, {
      id: 6, name: 'Slide video', documentType: 'slides'
    }, {
      id: 7, name: 'Slide power point', documentType: 'slides'
    }];


  ngOnInit(): void {
  }

  dismiss() {
    this.dialogRef.close();
  }
  save() {
    this.mdFileService.CreateNewMd(
      this.data.fullPath,
      this.markdownTitle,
      this.data.level,
      this.selectedTemplate.id,
      this.selectedTemplate.documentType )
      .subscribe(data => {
        this.mdFileService.addNewFile(data);
        this.mdFileService.setSelectedMdFileFromSideNav(data[data.length - 1]);
      });
    this.dialogRef.close();
  }
}
