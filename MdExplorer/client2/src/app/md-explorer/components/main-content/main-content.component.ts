import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { MdFile } from '../../models/md-file';
//import { IWorkWithElement } from '../../services/href-interceptor.service';
import { MdFileService } from '../../services/md-file.service';
import { DomSanitizer } from '@angular/platform-browser';

import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { MatDialog } from '@angular/material/dialog';
//import { ConnectionLostComponent } from '../../../signalR/dialogs/connection-lost/connection-lost.component';///dialogs/connection-lost/connection-lost.component


@Component({
  selector: 'app-main-content',
  templateUrl: './main-content.component.html',
  styleUrls: ['./main-content.component.scss']
})
export class MainContentComponent implements OnInit { // Removed AfterViewInit
  @ViewChild('myBelovedIframe') el: ElementRef;
  public classForContent: string = "hundredPercentContent"; // Set to a default, assuming it fills its container

  mdFile: MdFile;
  html: string;
  htmlSource: string = '../welcome.html';

  //helloWorld: IWorkWithElement = (msg) => {
  //  alert('this is the callback');
  //};


  public _HideIFrame = false;

  constructor(
    private service: MdFileService,
    private sanitizer: DomSanitizer,
    private monitorMDService: MdServerMessagesService,
    public dialog: MatDialog,
    private ref: ChangeDetectorRef,
  ) {
    console.log("MainContentComponent constructor");
    this.monitorMDService.addMarkdownFileListener(this.markdownFileIsChanged, this);
  }

  ngOnInit(): void {

    this.service.selectedMdFileFromSideNav.subscribe(_ => {
      this.callMdExplorerController(_);
    });
    this.service.selectedMdFileFromToolbar.subscribe(_ => {
      let current = _[0];
      if (current != undefined) {
        this.callMdExplorerController(current);
      }
    });
    // Removed subscription to this.service.whatDisplayForToolbar
  }
  private callMdExplorerController(node:  MdFile) {
    if (node != null && node.relativePath != undefined) {
      let dateTime = new Date().getTime() / 1000;
      this.htmlSource = '../api/mdexplorer' + node.relativePath + '?time=' + dateTime + "&connectionId=" + this.monitorMDService.connectionId;
    }
  }

  private markdownFileIsChanged(data: any, objectThis: MainContentComponent) {

    let dateTime = new Date().getTime() / 1000;
    objectThis.service.navigationArray = [];
    objectThis.service.setSelectedMdFileFromServer(data);
    objectThis.htmlSource = '../api/mdexplorer' + data.path + '?time=' + dateTime + "&connectionId=" + objectThis.monitorMDService.connectionId; ;
  }



}
