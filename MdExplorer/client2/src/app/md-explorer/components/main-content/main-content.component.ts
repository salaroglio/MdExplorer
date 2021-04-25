import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';


@Component({
  selector: 'app-main-content',
  templateUrl: './main-content.component.html',
  styleUrls: ['./main-content.component.scss']
})
export class MainContentComponent implements OnInit {

  mdFile: MdFile;
  html: string;


  constructor(
    private route: ActivatedRoute,
    private service: MdFileService) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const path = params['path'];
      if (path != undefined) {
        this.service.GetHtml(path).subscribe(data => {
          debugger;
          this.html = data;
        });
      }      
    });
  }

}
