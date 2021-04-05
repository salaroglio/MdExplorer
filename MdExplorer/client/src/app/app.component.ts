import { Component, Pipe, PipeTransform, ViewChild, ElementRef } from '@angular/core';
import { DomSanitizer } from "@angular/platform-browser";




@Pipe({ name: 'Safe' })
export class SafePipe implements PipeTransform {
    constructor(private sanitizer: DomSanitizer) { }
    transform(url: string) {
        return this.sanitizer.bypassSecurityTrustResourceUrl(url);
    }
}

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    @ViewChild('myIframe') myIframe: ElementRef | undefined;
    onCheckUrl() {
        alert(this.myIframe?.nativeElement.contentDocument.location.href);
    }
    iFrameUrl: string = "./MdExplorer/Home";
    title = 'MdExplorer';
}
