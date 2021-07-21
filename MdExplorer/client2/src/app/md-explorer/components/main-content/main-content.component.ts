import { Component, EventEmitter, NgZone, OnInit, Output } from '@angular/core';
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
export class MainContentComponent implements OnInit {

  mdFile: MdFile;
  html: string;
  htmlSource: string = '../welcome.html';

  helloWorld: IWorkWithElement = (msg) => {
    alert('this is the callback');
  };

  private _this: any;

  constructor(
    private route: ActivatedRoute,    
    private service: MdFileService,
    private sanitizer: DomSanitizer,
    private monitorMDService: MonitorMDService,
    private zone: NgZone,
    private sideNavDataService: SideNavDataService
  ) {
    
    //this.monitorMDService.startConnection();
    this.monitorMDService.addMarkdownFileListener(this.updateModifiedMarkDown, this);
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {      
      const path = this.sideNavDataService.currentPath;
      if (path != undefined) {        
        //this.service.GetHtml(path).subscribe(data => {          
        //  this.html = data;
        //});
        let dateTime = new Date().getTime()/1000;
        this.htmlSource = '../api/mdexplorer' + path  + '?time=' + dateTime;
          
      }
       
    });

   

  }

  private updateModifiedMarkDown(data: any, objectThis: any) {
    console.log(data);
    let dateTime = new Date();
    objectThis.htmlSource = '../api/mdexplorer/' + data.path + '?time=' + dateTime;
    
    //if (data.path != undefined) {
    //  objectThis.service.GetHtml(data.path).subscribe(data => {
    //    objectThis.html = data;

    //  });
    //}
    

  }


}
