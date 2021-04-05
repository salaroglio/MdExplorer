import { Component,Pipe, OnInit,PipeTransform,ElementRef,ViewChild} from '@angular/core';
import { SignalRService } from './services/signal-r.service';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer } from "@angular/platform-browser";

@Pipe({ name: 'safe' })
export class SafePipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) { }
  transform(url:string) {
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }
}



@Component({
  selector: 'pm-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  @ViewChild('myIframe') myIframe: ElementRef | undefined;
  constructor(public signalRService: SignalRService, private http: HttpClient) { }

  ngOnInit() {
    this.signalRService.startConnection();
    this.signalRService.addTransferChartDataListener(); 
    this.signalRService.addBroadcastChartDataListener();  
    this.startHttpRequest();
  }
  
  title = 'MDExplorer';
  myHtml:string | undefined ;
  iFrameUrl:string ="https://www.ilfattoquotidiano.it";
 

  private startHttpRequest = () => {
    this.http.get('https://localhost:5001/api/chart')
      .subscribe(res => {
        console.log(res);
      })
  }

  onCheckUrl(){
    alert(this.myIframe?.nativeElement.ownerDocument.contentWindow.location.href);
    
  }

  onSend(path:string):void {
    this.iFrameUrl ="https://localhost:5001/MdExplorer/home";
    // this.http.get('https://localhost:5001/MdExplorer'+ path,{responseType: "text"})
    //   .subscribe(res => {
    //     this.myHtml = res;
    //   })
  }
}
