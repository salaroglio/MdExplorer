import { Component, ElementRef, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router'; // Per ricevere dati, se necessario

@Component({
  selector: 'app-milkdown-react-host',
  template: `
    <div style="height: calc(100vh - 100px); border: 1px solid #ccc;">
      <docs-pilot #docsPilotElement [attr.markdown]="markdownContent"></docs-pilot>
    </div>
    <button mat-raised-button color="primary" (click)="goBack()">Torna ad Angular</button>
    <button mat-raised-button (click)="sendSampleMarkdown()">Invia Markdown di Esempio</button>
  `,
  // styleUrls: ['./milkdown-react-host.component.scss'] // Se avessimo stili specifici
})
export class MilkdownReactHostComponent implements OnInit, AfterViewInit {
  @ViewChild('docsPilotElement') docsPilotElementRef: ElementRef<HTMLElement & { markdown: string }>;

  markdownContent: string = '# Benvenuto nell\\\'Editor React (Milkdown)!';

  constructor(private location: Location, private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Esempio: ricevere markdown dalla route (se lo passeremo in futuro)
    // this.route.queryParams.subscribe(params => {
    //   if (params['initialMd']) {
    //     this.markdownContent = decodeURIComponent(params['initialMd']);
    //   }
    // });
  }

  ngAfterViewInit(): void {
    // Il WebComponent potrebbe richiedere un momento per inizializzarsi
    // Potremmo voler interagire con esso qui se necessario,
    // ad esempio per impostare il markdown programmaticamente se l'attributo non basta
    // o per ascoltare i suoi eventi custom.
    // if (this.docsPilotElementRef?.nativeElement) {
    //    this.docsPilotElementRef.nativeElement.markdown = this.markdownContent;
    // }
  }

  goBack(): void {
    this.location.back();
  }

  sendSampleMarkdown(): void {
    const sampleMd = `## Markdown da Angular\n\n- Lista puntata\n- **Grassetto**\n\nTimestamp: ${new Date().toLocaleTimeString()}`;
    if (this.docsPilotElementRef?.nativeElement) {
      // Usare setAttribute è più robusto per i WebComponent che osservano gli attributi.
      this.docsPilotElementRef.nativeElement.setAttribute('markdown', sampleMd);
      // In alternativa, se il setter della proprietà gestisce la logica di re-render:
      // this.docsPilotElementRef.nativeElement.markdown = sampleMd;
    }
  }
}
