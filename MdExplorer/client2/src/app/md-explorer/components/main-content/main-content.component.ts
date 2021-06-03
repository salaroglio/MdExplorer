import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MdFile } from '../../models/md-file';
import { HrefInterceptorService, IWorkWithElement } from '../../services/href-interceptor.service';
import { MdFileService } from '../../services/md-file.service';
import { DomSanitizer } from '@angular/platform-browser';


@Component({
  selector: 'app-main-content',
  templateUrl: './main-content.component.html',
  styleUrls: ['./main-content.component.scss']
})
export class MainContentComponent implements OnInit {

  mdFile: MdFile;
  html: string;
  htmlSource: string = '../test.html';

  helloWorld: IWorkWithElement = (msg) => {
    alert('this is the callback');
  };

  private _this: any;

  constructor(
    private route: ActivatedRoute,
    private service: MdFileService,
  private sanitizer:DomSanitizer  ) {
    
  }


  ngOnInit(): void {
    this.route.params.subscribe(params => {      
      const path = params['path'];
      const name = params['name'];
      if (path != undefined) {        
        //this.service.GetHtml(path).subscribe(data => {          
        //  this.html = data;
        //});
        
        this.htmlSource ='../api/mdexplorer/' + path;
          
      }      
    });

   

  }

  gettAlert() {
    alert('cliccato');
  }


}
