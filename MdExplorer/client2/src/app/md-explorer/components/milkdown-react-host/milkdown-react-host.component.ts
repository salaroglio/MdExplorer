import { Component, ElementRef, OnInit, ViewChild, AfterViewInit, OnDestroy, HostListener } from '@angular/core';
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
    <div class="editor-container">
      <div class="editor-content">
        <docs-pilot #docsPilotElement [attr.markdown]="markdownContent"></docs-pilot>
      </div>
    </div>
    <button class="save-button" (click)="saveMarkdown()">
      Salva
    </button>
  `,
  styles: [`
    :host {
      display: block;
      position: relative;
      height: 100%;
    }
    .editor-container {
      display: flex;
      flex-direction: column;
      height: 100%;
      position: relative;
    }
    .editor-content {
      flex: 1;
      overflow-y: auto;
      border: 1px solid #ccc;
    }
    .save-button {
      position: fixed;  /* Torniamo a fixed per mantenerlo fisso durante lo scroll */
      bottom: 40px;     /* Aumentiamo la distanza dal fondo */
      right: 40px;      /* Posizioniamo a destra invece che a sinistra */
      z-index: 1000;    /* Aumentiamo lo z-index per assicurarci che sia sopra tutto */
      background-color: #3f51b5;
      color: white;
      padding: 12px 28px;  /* Padding leggermente più grande per migliore visibilità */
      border: none;
      border-radius: 24px;  /* Bordi più arrotondati per un aspetto moderno */
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      box-shadow: 0 4px 8px -2px rgba(0,0,0,.3), 0 8px 16px 0 rgba(0,0,0,.2);
      text-transform: uppercase;
      transition: all 0.2s ease;  /* Transizione smooth per hover */
    }
    .save-button:hover {
      background-color: #303f9f;
      transform: translateY(-2px);  /* Solleva leggermente il pulsante */
      box-shadow: 0 6px 12px -2px rgba(0,0,0,.4), 0 12px 24px 0 rgba(0,0,0,.3);
    }
    .save-button:active {
      transform: translateY(0);  /* Ritorna alla posizione originale quando premuto */
    }
  `]
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
    console.log('MilkdownReactHostComponent initialized');
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

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    // Intercetta Ctrl+S o Cmd+S (per Mac)
    if ((event.ctrlKey || event.metaKey) && event.key === 's') {
      event.preventDefault(); // Previene il comportamento di default del browser
      this.saveMarkdown();
    }
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
      // Dopo il salvataggio, torna indietro
      this.location.back();
    } catch (error) {
      console.error('React Host: Errore durante il salvataggio del markdown:', error);
      // Aggiungere un feedback di errore per l'utente
    }
  }

}
