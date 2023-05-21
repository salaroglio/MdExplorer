import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';
import { IDocumentSettings } from '../../services/Types/IDocumentSettings';

@Component({
  selector: 'app-document-settings',
  templateUrl: './document-settings.component.html',
  styleUrls: ['./document-settings.component.scss']
})
export class DocumentSettingsComponent implements OnInit {

  standardTemplates = [{ name: "project",fullPath:"" }, {name:"minute",fullPath:""}]
  selectedMdFile: MdFile;
  documentDescriptor: IDocumentSettings = {
    author: 'Carlo',
    date: '2022',
    documentType: 'document',
    email: '@carlo',
    title: 'title',
    wordSection: {
      documentHeader: 'None',
      writeToc: true,
      templateSection: {
        customTemplate: "",
        inheritFromTemplate: "minute",
        templateType: "inherit"
        }
      }
  };
;

  //////////////////////////////////
  constructor(
    private mdFileService: MdFileService,
    private _snackBar: MatSnackBar) {        
  }
  /////////////////////////////////

  ngOnInit(): void {
    this.mdFileService.selectedMdFileFromSideNav.subscribe(
      currentMdFile => {
        this.selectedMdFile = currentMdFile;
        //
        this.mdFileService.getDocumentSettings(currentMdFile).subscribe(descriptor => {
          this.documentDescriptor = descriptor;
        });
      }
      
    );
  }

  openSelectedInheritingTemplateWord() {
    this.mdFileService.openInheritingTemplateWord(this.documentDescriptor.wordSection.templateSection.inheritFromTemplate)
      .subscribe(_ => {
        this._snackBar.open("Opening Word Custom template!");
      });
  }

  saveSettings() {
    this.mdFileService.setDocumentSettings(this.documentDescriptor, this.selectedMdFile)
      .subscribe(_ => {
        this._snackBar.open("Saved!");
      });
  }

  openCustomWord() {
    debugger;
    this.mdFileService.opencustomwordtemplate(this.selectedMdFile)
      .subscribe(_=>
        {
          this._snackBar.open("Opening Word Custom template!");
        });
  }
}
