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
    this.selectedPlantumlTemplate = this.plantumlTemplates[0]
    this.selectedSlideTemplate = this.slideTemplates[0]
  }

  selectedPlantumlTemplate: Snippet;
  selectedSlideTemplate: Snippet;

  plantumlTemplates: Snippet[] =
    [{ id: 0, name: 'Text document' },
    { id: 1, name: 'Sequence Diagram' },
    { id: 2, name: 'State Diagram' },
    { id: 3, name: 'Workflow' },
    { id: 4, name: 'Gantt' }];

  slideTemplates: Snippet[] =
    [{
      id: 0, name: 'Flicker document'
    }, {
      id: 1, name: 'Slide video'
      }, {
        id:2,name:'Slide power point'
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
      this.selectedPlantumlTemplate.id)
      .subscribe(data => {
        this.mdFileService.addNewFile(data);
        this.mdFileService.setSelectedMdFileFromSideNav(data[data.length - 1]);
      });
    this.dialogRef.close();
  }
}
