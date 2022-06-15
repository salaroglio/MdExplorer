import { AfterViewInit, Component, ElementRef, EventEmitter, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MdFile } from '../../models/md-file';
import { HrefInterceptorService, IWorkWithElement } from '../../services/href-interceptor.service';
import { MdFileService } from '../../services/md-file.service';
import { DomSanitizer } from '@angular/platform-browser';
import { MonitorMDService } from '../../services/monitor-md.service';
import { SideNavDataService } from '../../services/side-nav-data.service';


@Component({
  selector: 'app-main-content',
  templateUrl: './main-content.component.html',
  styleUrls: ['./main-content.component.scss']
})
export class MainContentComponent implements OnInit, AfterViewInit {
  @ViewChild('myBelovedIframe') el: ElementRef;

  ngAfterViewInit() {
    this.el.nativeElement.onload = _ => {
      try {
        _.target.contentWindow.document.myReferenceObject = this;
        _.target.contentWindow.document.addEventListener('wheel',
          this.toolbarHandler);
      } catch (e) { // for some reason the wheel event "injection" failed, so in ordet to Not hide tolbar i set block
        this.service.setWhatDisplayForToolbar('block');
      }
      
    };

  }

  toolbarHandler(event) {
    event.currentTarget.myReferenceObject.lastCall(event);
  }

  lastCall(event) {
    if (event.deltaY < 0) {
      // visualizzare
      this.service.setWhatDisplayForToolbar('block');

    } else {
      // nascondere
      this.service.setWhatDisplayForToolbar('none');
    }    
  }

  mdFile: MdFile;
  html: string;
  htmlSource: string = '../welcome.html';

  helloWorld: IWorkWithElement = (msg) => {
    alert('this is the callback');
  };

  //private _this: any;
  public _HideImg = true;
  public _HideIFrame = false;

  constructor(
    private route: ActivatedRoute,    
    private service: MdFileService,
    private sanitizer: DomSanitizer,
    private monitorMDService: MonitorMDService,
    private zone: NgZone,
    private sideNavDataService: SideNavDataService
  ) {
    console.log("MainContentComponent constructor");
    this.monitorMDService.addMarkdownFileListener(this.markdownFileIsChanged, this);
    this.monitorMDService.addOnCloseEvent(this.ShowConnectionLost, this);
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

  }

  private callMdExplorerController(node:  MdFile) {    
    if (node != null && node.relativePath != undefined) {
      let dateTime = new Date().getTime() / 1000;
      debugger;
      //this.el.nativeElement.contentWindow(null, 'http://127.0.0.1');
      this.htmlSource = '../api/mdexplorer' + node.relativePath + '?time=' + dateTime;
      
    }
  }

  private markdownFileIsChanged(data: any, objectThis: MainContentComponent) {    
    let dateTime = new Date();
    objectThis.service.navigationArray = [];
    objectThis.service.setSelectedMdFileFromServer(data);
    debugger;
    //this.el.nativeElement.contentWindow(null, 'http://127.0.0.1');
    objectThis.htmlSource = '../api/mdexplorer/' + data.path + '?time=' + dateTime;

  }
 
  private ShowConnectionLost(data: any, objectThis: any) {
    objectThis._HideImg = false;
  }



}
