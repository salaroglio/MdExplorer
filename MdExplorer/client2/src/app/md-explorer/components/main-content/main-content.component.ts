import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { MdFile } from '../../models/md-file';
//import { IWorkWithElement } from '../../services/href-interceptor.service';
import { MdFileService } from '../../services/md-file.service';
import { DomSanitizer } from '@angular/platform-browser';
import { ServerMessagesService } from '../../../signalr/services/server-messages.service';
import { MatDialog } from '@angular/material/dialog';
//import { ConnectionLostComponent } from '../../../signalR/dialogs/connection-lost/connection-lost.component';///dialogs/connection-lost/connection-lost.component


@Component({
  selector: 'app-main-content',
  templateUrl: './main-content.component.html',
  styleUrls: ['./main-content.component.scss']
})
export class MainContentComponent implements OnInit, AfterViewInit {
  @ViewChild('myBelovedIframe') el: ElementRef;
  public whatClass: string = "showToolbar";

  ngAfterViewInit() {
    this.el.nativeElement.onload = _ => {
      try {
        _.target.contentWindow.document.myReferenceObject = this;
        _.target.contentWindow.document.addEventListener('wheel',
          this.toolbarHandler);
      } catch (e) { // for some reason the wheel event "injection" failed, so in ordet to Not hide tolbar i set block
        this.service.setWhatDisplayForToolbar('showToolbar');
      }
      
    };
  }



  toolbarHandler(event) {
    event.currentTarget.myReferenceObject.lastCall(event);
  }

  lastCall(event) {
    if (event.deltaY < 0) {
      // visualizzare
      this.service.setWhatDisplayForToolbar('showToolbar');

    } else {
      // nascondere
      this.service.setWhatDisplayForToolbar('hideToolbar');
    }    
  }

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
    private monitorMDService: ServerMessagesService,
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
    this.service.selectedDirectoryFromNewDirectory.subscribe(_ => {

    });
    this.service.whatDisplayForToolbar.subscribe(_ => {

      if ((_ == 'showToolbar' && this.whatClass != _) ||
        (_ == 'hideToolbar' && this.whatClass != _ + ' ' + 'hideToolbarNone')
        //|| (_ == 'hideToolbar' + ' ' + 'hideToolbarNone' && this.whatClass != _ + ' ' + 'hideToolbarNone')
      ) { // check if something is truely changed
        this.whatClass = _;
        this.sleep(300).then(m => {
          if (_ == 'hideToolbar' && _ != undefined) {
            this.whatClass = _ + ' ' + 'hideToolbarNone'
            this.ref.detectChanges();
          }
        });
        this.ref.detectChanges();
      }
    });
  }
  sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
  private callMdExplorerController(node:  MdFile) {    
    if (node != null && node.relativePath != undefined) {
      let dateTime = new Date().getTime() / 1000;
      
      //this.el.nativeElement.contentWindow(null, 'http://127.0.0.1');
      this.htmlSource = '../api/mdexplorer' + node.relativePath + '?time=' + dateTime;
      
    }
  }

  private markdownFileIsChanged(data: any, objectThis: MainContentComponent) {    
    let dateTime = new Date();
    objectThis.service.navigationArray = [];
    objectThis.service.setSelectedMdFileFromServer(data);
    
    //this.el.nativeElement.contentWindow(null, 'http://127.0.0.1');
    objectThis.htmlSource = '../api/mdexplorer/' + data.path + '?time=' + dateTime;

  }
 
  //private ShowConnectionLost(data: any, objectThis: any) {
  //  console.log('lo so che ti apri');
  //  objectThis.dialog.open(ConnectionLostComponent, {
  //    width: '600px',
  //    data: null,
  //  });
  //}


}
