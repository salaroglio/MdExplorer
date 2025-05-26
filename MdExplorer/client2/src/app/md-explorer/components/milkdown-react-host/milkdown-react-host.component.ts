import { Component, ElementRef, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router'; // Per ricevere dati, se necessario
import { HttpClient } from '@angular/common/http';
import { MdFileService } from '../../services/md-file.service';
import { MdFile } from '../../models/md-file';
import { Subscription } from 'rxjs';
import { filter, switchMap, tap } from 'rxjs/operators';

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
export class MilkdownReactHostComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('docsPilotElement') docsPilotElementRef: ElementRef<HTMLElement & { markdown: string }>;

  markdownContent: string = '# Benvenuto nell\\\'Editor React (Milkdown)!';
  private fileSubscription: Subscription;

  constructor(
    private location: Location,
    private route: ActivatedRoute, // Lo manteniamo se serve per altro, ma non per filePath
    private http: HttpClient,
    private mdFileService: MdFileService
  ) {}

  ngOnInit(): void {
    this.fileSubscription = this.mdFileService.selectedMdFileFromSideNav.pipe(
      filter((file): file is MdFile => file !== null && file !== undefined), // Assicura che file non sia null/undefined
      tap(file => console.log('React Host: File selezionato:', file.fullPath)), // Log per debug
      switchMap(file => {
        if (file && file.fullPath) {
          // Pulisce il percorso da eventuali caratteri problematici o normalizza i separatori se necessario
          // Per ora, assumiamo che fullPath sia già corretto per l'URL.
          // L'API controller usa {*url}, quindi dovrebbe catturare il percorso correttamente.
          const encodedPath = encodeURIComponent(file.fullPath);
          // NOTA: Il controller /api/MdExplorerEditorReact/{*url} si aspetta che il path sia parte dell'URL,
          // non un parametro query. encodeURIComponent potrebbe non essere necessario se il path non contiene
          // caratteri speciali che romperebbero l'URL. Tuttavia, il controller C# usa GetRelativePathFileSystem
          // che potrebbe fare delle assunzioni sul formato del path.
          // Per sicurezza, passiamo il fullPath direttamente, dato che {*url} lo catturerà.
          // Se ci fossero problemi, potremmo dover aggiustare come il path viene inviato o processato.
          // La route del controller è [Route("/api/MdExplorerEditorReact/{*url}")]
          // Quindi l'URL sarà /api/MdExplorerEditorReact/C:/path/to/file.md (o simile)
          // Il controller usa GetRelativePathFileSystem("mdexplorer") che prende l'URL dalla request.
          // Quindi, il path deve essere relativo alla stringa "mdexplorer" nell'URL.
          // Questo implica che il path inviato deve essere solo la parte dopo /api/MdExplorerEditorReact/
          // Es: se fullPath è "C:\projects\MdExplorer\Notes\file.md"
          // e il controller si aspetta "Notes/file.md" (relativo a _fileSystemWatcher.Path)
          // Dovremo inviare il path relativo.
          // Tuttavia, il controller sembra già gestire fullPath tramite _fileSystemWatcher.Path + relativePathFile
          // e GetRelativePathFileSystem usa HttpUtility.UrlDecode(Request.Path.Value).Replace("/api/MdExplorerEditorReact/", string.Empty);
          // Quindi, passare il fullPath direttamente dovrebbe funzionare.
          return this.http.get(`/api/MdExplorerEditorReact/${file.fullPath}`, { responseType: 'text' });
        }
        return ''; // O un Observable vuoto, es: EMPTY
      })
    ).subscribe({
      next: (markdown) => {
        if (typeof markdown === 'string') {
          this.markdownContent = markdown;
          // Forziamo l'aggiornamento dell'attributo nel web component se necessario
          if (this.docsPilotElementRef?.nativeElement) {
            this.docsPilotElementRef.nativeElement.setAttribute('markdown', markdown);
          }
          console.log('React Host: Markdown caricato.');
        }
      },
      error: (err) => {
        console.error('React Host: Errore nel caricare il markdown:', err);
        this.markdownContent = '# Errore nel caricamento del documento';
        if (this.docsPilotElementRef?.nativeElement) {
          this.docsPilotElementRef.nativeElement.setAttribute('markdown', this.markdownContent);
        }
      }
    });
  }

  ngOnDestroy(): void {
    if (this.fileSubscription) {
      this.fileSubscription.unsubscribe();
    }
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
