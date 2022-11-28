import { Component, OnInit } from '@angular/core';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';
import { IDocumentSettings } from '../../services/Types/IDocumentSettings';

@Component({
  selector: 'app-document-settings',
  templateUrl: './document-settings.component.html',
  styleUrls: ['./document-settings.component.scss']
})
export class DocumentSettingsComponent implements OnInit {

  selectedMdFile: MdFile;
  documentDescriptor: IDocumentSettings = {
    author: 'Carlo',
    date: '2022',
    documentType: 'document',
    email: '@carlo',
    title: 'title',
    wordSection: {
      documentHeader: 'None',
      writeToc:true
      }
  };
;

  constructor(private mdFileService: MdFileService) {


  }

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

}
