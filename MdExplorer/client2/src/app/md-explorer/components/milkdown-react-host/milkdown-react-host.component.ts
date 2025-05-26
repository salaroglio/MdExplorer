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
  private currentFilePath: string | null = null; // Per memorizzare il percorso del file corrente

  constructor(
    private location: Location,
    private route: ActivatedRoute, // Lo manteniamo se serve per altro, ma non per filePath
    private http: HttpClient,
    private mdFileService: MdFileService
  ) {}

  ngOnInit(): void {
    this.fileSubscription = this.mdFileService.selectedMdFileFromSideNav.pipe(
      filter((file): file is MdFile => file !== null && file !== undefined), // Assicura che file non sia null/undefined
      tap(file => {
        this.currentFilePath = file.fullPath; // Memorizza il percorso del file
        console.log('React Host: File selezionato:', this.currentFilePath);
      }),
      switchMap(file => {
        // Non è necessario controllare file.fullPath qui perché filter e tap lo hanno già fatto
        return this.http.get(`/api/MdExplorerEditorReact/${file.fullPath}`, { responseType: 'text' });
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

  async saveMarkdown(): Promise<void> {
    if (!this.currentFilePath) {
      console.error('React Host: Nessun percorso file corrente per salvare.');
      return;
    }

    let markdownToSave: string | null = null;
    const editorElement = this.docsPilotElementRef?.nativeElement as any; // Cast to any to access potential custom properties

    if (editorElement) {
      // Tentativo 1: Accedere alla proprietà 'markdown' (che dovrebbe invocare il getter del WebComponent)
      // Questo getter, come visto in integration.ts, prova a chiamare this.editor.getMarkdown()
      if (typeof editorElement.markdown !== 'undefined') {
        markdownToSave = editorElement.markdown;
        console.log('React Host: Markdown ottenuto tramite proprietà .markdown del WebComponent.');
      } else {
        // Tentativo 2: Fallback a getAttribute se la proprietà non esiste o è undefined
        markdownToSave = editorElement.getAttribute('markdown');
        console.log('React Host: Markdown ottenuto tramite .getAttribute(\'markdown\') del WebComponent.');
      }
    }

    // Se entrambi i metodi sopra falliscono o restituiscono null (improbabile se l'editor funziona)
    if (markdownToSave === null) {
        markdownToSave = this.markdownContent; // Fallback estremo
        console.warn('React Host: Impossibile ottenere il markdown aggiornato dal WebComponent tramite proprietà o attributo. Utilizzo di this.markdownContent (potenzialmente obsoleto).');
    }

    console.log('React Host: Tentativo di salvataggio per:', this.currentFilePath);
    // console.log('React Host: Contenuto da salvare:', markdownToSave ? markdownToSave.substring(0,100) + "..." : "null");


    const requestBody = {
      FilePath: this.currentFilePath,
      MarkdownContent: markdownToSave
    };

    try {
      await this.http.post('/api/MdExplorerEditorReact/UpdateMarkdown', requestBody).toPromise();
      console.log('React Host: Markdown salvato con successo per:', this.currentFilePath);
      // Aggiungere un feedback per l'utente, es. con MatSnackBar
    } catch (error) {
      console.error('React Host: Errore durante il salvataggio del markdown:', error);
      // Aggiungere un feedback di errore per l'utente
    }
  }

  async goBack(): Promise<void> {
    await this.saveMarkdown(); // Salva prima di tornare indietro
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
