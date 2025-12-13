// File per l'integrazione come WebComponent
// BUILD VERSION: 2025-12-12T21:00:00 - Image upload integration
const BUILD_VERSION = "2025-12-12T21:00:00-image-upload";

import { Crepe, CrepeFeature } from "@milkdown/crepe";
import { languages } from "@codemirror/language-data";
import { LanguageDescription } from "@codemirror/language";
import { replaceAll } from "@milkdown/utils";

/**
 * Normalizza i line endings da CRLF (Windows) a LF (Unix).
 * CodeMirror/Milkdown ha un bug con CRLF che causa il salto del cursore nei code blocks.
 */
function normalizeCRLF(content: string): string {
  return content.replace(/\r\n/g, '\n');
}

// Definisce PlantUML come linguaggio riconosciuto per i code blocks
const plantumlLanguage = LanguageDescription.of({
  name: "PlantUML",
  alias: ["plantuml", "puml"],
  extensions: ["puml", "plantuml", "pu"],
  load() {
    // Usa Java come base per syntax highlighting (PlantUML ha sintassi simile)
    return import('@codemirror/lang-java').then(m => m.java());
  }
});


// Esporta la classe DocsPilotElement
export class DocsPilotElement extends HTMLElement {
  editor: any = null;
  editorContainer: HTMLDivElement | null = null;
  shadowRoot: ShadowRoot | null = null;
  defaultMarkdown = '<calc>';

  // Osserva gli attributi per reagire ai cambiamenti
  static get observedAttributes() {
    return ['markdown'];
  }

  // Useremmo questa proprietà in futuro se avessimo bisogno di una gestione più complessa degli eventi
  // private editorEvents: CustomEvent[] = [];

  // Flag per indicare che l'inizializzazione è in corso
  private isInitializing = false;

  // Flag per evitare doppie elaborazioni quando setMarkdown() e attributeChangedCallback() vengono chiamati insieme
  private skipNextAttributeChange = false;

  // Flag per tracciare se il CSS è stato caricato (usato internamente)
  // @ts-ignore - used in onload callback
  private cssLoaded = false;

  // Contesto per l'upload delle immagini (path del documento e connectionId per le API)
  private currentDocumentPath: string = '';
  private connectionId: string = '';

  // Quando un attributo cambia (chiamato dal host Angular quando carica un nuovo file)
  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (name === 'markdown' && oldValue !== newValue) {
      // Salta se il cambiamento è stato già gestito da setMarkdown()
      // (previene race condition con binding Angular)
      if (this.skipNextAttributeChange) {
        this.skipNextAttributeChange = false;
        return;
      }

      // Non fare nulla durante la fase di inizializzazione o se il valore è undefined o non una stringa
      if (this.isInitializing || typeof newValue !== 'string') {
        return;
      }

      // Se l'editor non è ancora inizializzato, non fare nulla
      // Verrà usato il valore dell'attributo durante l'inizializzazione
      if (!this.editor) {
        return;
      }

      // Usa replaceAll per aggiornare il contenuto senza distruggere l'editor
      // Questo preserva la posizione del cursore e lo stato dell'editor
      try {
        // Normalizza CRLF -> LF per evitare bug cursore in CodeMirror
        const normalizedValue = normalizeCRLF(newValue);
        console.log('Aggiornamento contenuto via replaceAll:', normalizedValue.substring(0, 20) + '...');
        this.editor.editor.action(replaceAll(normalizedValue));
      } catch (error) {
        console.error('Errore in attributeChangedCallback:', error);
      }
    }
  }
  
  // Getter per il markdown attuale
  get markdown() {
    // Se l'editor è inizializzato, ottieni il valore direttamente dall'editor
    if (this.editor && typeof this.editor.getMarkdown === 'function') {
      let md = this.editor.getMarkdown();
      // Fix: rimuovi escape dagli shortcode emoji per compatibilità con Markdig
      // Milkdown/remark-stringify escapa underscore e colon che rompono gli emoji
      // Esempio: :heavy\_check\_mark: → :heavy_check_mark:
      // Pattern: trova :parola: dove parola può contenere \_ (escaped underscore)
      md = md.replace(/:([a-zA-Z0-9_+\\-]+):/g, (match: string) => {
        // Rimuovi i backslash prima degli underscore dentro lo shortcode
        return match.replace(/\\_/g, '_');
      });
      return md;
    }
    // Altrimenti, restituisci il valore dell'attributo
    return this.getAttribute('markdown') || this.defaultMarkdown;
  }
  
  // Setter per il markdown
  set markdown(value) {
    this.setAttribute('markdown', value);
  }

  // Metodo per aggiornare il contenuto senza distruggere l'editor
  // Questo preserva la posizione del cursore e lo stato dell'editor
  setMarkdown(content: string) {
    console.log('[DocsPilot] setMarkdown called, content length:', content?.length, '| BUILD:', BUILD_VERSION);
    if (!this.editor || typeof content !== 'string') return;

    try {
      // Normalizza CRLF -> LF per evitare bug cursore in CodeMirror
      const normalizedContent = normalizeCRLF(content);

      // Usa replaceAll per aggiornare il contenuto
      this.editor.editor.action(replaceAll(normalizedContent));

      // Aggiorna anche il defaultMarkdown per consistenza
      this.defaultMarkdown = normalizedContent;

    } catch (error) {
      console.error('Errore in setMarkdown:', error);
    }
  }

  /**
   * Imposta il contesto necessario per l'upload delle immagini.
   * Deve essere chiamato dall'host Angular quando viene caricato un documento.
   * @param documentPath - Path completo del file markdown corrente
   * @param connectionId - ID della connessione per identificare il progetto nelle API
   */
  setContext(documentPath: string, connectionId: string) {
    console.log('[DocsPilot] setContext called:', { documentPath, connectionId, BUILD: BUILD_VERSION });
    this.currentDocumentPath = documentPath;
    this.connectionId = connectionId;
  }

  /**
   * Carica un'immagine sul server MdExplorer.
   * Chiamato dall'handler onUpload di Milkdown ImageBlock.
   * @param file - Il file immagine da caricare
   * @returns Il path relativo dell'immagine salvata (es: "./assets/image.png")
   */
  private async uploadImageToServer(file: File): Promise<string> {
    console.log('[DocsPilot] uploadImageToServer called:', { fileName: file.name, size: file.size });

    if (!this.connectionId) {
      console.error('[DocsPilot] ConnectionId not set. Call setContext() first.');
      throw new Error('ConnectionId not set. Cannot upload image.');
    }

    if (!this.currentDocumentPath) {
      console.error('[DocsPilot] DocumentPath not set. Call setContext() first.');
      throw new Error('DocumentPath not set. Cannot upload image.');
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentPath', this.currentDocumentPath);

    // Aggiungi ConnectionId come query parameter (come fa l'interceptor Angular)
    const apiUrl = `/api/mdfiles/uploadImage?ConnectionId=${encodeURIComponent(this.connectionId)}`;

    try {
      const response = await fetch(apiUrl, {
        method: 'POST',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        console.error('[DocsPilot] Upload failed:', response.status, errorData);
        throw new Error(errorData.message || `Upload failed with status ${response.status}`);
      }

      const result = await response.json();
      console.log('[DocsPilot] Upload successful:', result);
      return result.relativePath; // es: "./assets/image_20231212.png"
    } catch (error) {
      console.error('[DocsPilot] Upload error:', error);
      throw error;
    }
  }
  
  // Quando l'elemento viene aggiunto al DOM
  connectedCallback() {
    // Imposta il flag di inizializzazione
    this.isInitializing = true;

    // Crea Shadow DOM per incapsulamento stili
    this.shadowRoot = this.attachShadow({ mode: 'open' });

    // Aggiungi stili CSS in modo che il percorso sia relativo allo script stesso
    const linkElem = document.createElement('link');
    linkElem.setAttribute('rel', 'stylesheet');

    let cssPath: string;

    // Rileva se siamo in dev mode (Vite dev server)
    const port = window.location.port;
    const isDevMode = (port >= '5173' && port <= '5180') || (import.meta as any).env?.DEV;

    if (isDevMode) {
      // In dev mode, usa il CSS bundled dalla cartella public
      cssPath = '/css/milkdown-bundled.css';
    } else {
      // In produzione, cerca il CSS relativo allo script o usa il fallback
      const currentScript = document.currentScript as HTMLScriptElement;

      if (currentScript && currentScript.src) {
        const scriptFolderURL = new URL('.', currentScript.src).href;
        cssPath = new URL('assets/css/milkdown-all.css', scriptFolderURL).href;
      } else {
        const fixedRelativePath = '../../../milk_react/assets/css/milkdown-all.css';
        cssPath = new URL(fixedRelativePath, window.location.href).href;
      }
    }

    linkElem.setAttribute('href', cssPath);

    // Traccia il caricamento del CSS
    linkElem.onload = () => {
      this.cssLoaded = true;
    };

    // Aggiungi al Shadow DOM
    this.shadowRoot.appendChild(linkElem);

    // Crea container per l'editor Milkdown
    this.editorContainer = document.createElement('div');
    this.editorContainer.className = 'docs-pilot-editor';
    this.shadowRoot.appendChild(this.editorContainer);

    // Inizializza l'editor con un piccolo ritardo per assicurarsi che gli stili siano caricati
    setTimeout(() => {
      this.initEditor();
    }, 50);
  }
  
  // Configura gli eventi dell'editor
  setupEditorListeners() {
    // Placeholder per eventuali listener futuri
  }
  
  // Inizializza l'editor Milkdown
  async initEditor() {
    if (!this.editorContainer) return;

    try {
      this.isInitializing = true;

      // Normalizza CRLF -> LF per evitare bug cursore in CodeMirror
      const normalizedMarkdown = normalizeCRLF(this.markdown);

      // Crea l'istanza dell'editor con handler per upload immagini
      this.editor = new Crepe({
        root: this.editorContainer,
        defaultValue: normalizedMarkdown,
        featureConfigs: {
          [CrepeFeature.ImageBlock]: {
            onUpload: async (file: File): Promise<string> => {
              // Se il contesto è impostato, carica l'immagine sul server
              if (this.connectionId && this.currentDocumentPath) {
                return await this.uploadImageToServer(file);
              }
              // Fallback: usa blob URL temporaneo (comportamento originale)
              console.warn('[DocsPilot] Context not set, using temporary blob URL');
              return URL.createObjectURL(file);
            }
          },
          [CrepeFeature.CodeMirror]: {
            languages: [...languages, plantumlLanguage]
          }
        }
      });

      // Configura gli eventi dell'editor
      this.setupEditorListeners();

      // Inizializza l'editor
      await this.editor.create();

      // Resetta il flag di inizializzazione dopo un piccolo ritardo
      setTimeout(() => {
        this.isInitializing = false;
      }, 50);

    } catch (error) {
      console.error('Errore nell\'inizializzazione dell\'editor Milkdown:', error);
      this.isInitializing = false;
    }
  }
  
  // Quando l'elemento viene rimosso dal DOM
  disconnectedCallback() {
    // Rimuovi l'editor quando l'elemento viene distrutto
    if (this.editor) {
      this.editor.destroy();
      this.editor = null;
    }
  }
}

// Registra automaticamente il web component se siamo in un browser
if (typeof window !== 'undefined' && typeof customElements !== 'undefined') {
  if (!customElements.get('docs-pilot')) {
    customElements.define('docs-pilot', DocsPilotElement);
  }
}

// Funzione di inizializzazione per chi non utilizza i WebComponents
export function initDocsPilot(element: HTMLElement) {
  const docsPilot = document.createElement('docs-pilot');
  element.appendChild(docsPilot);
  return docsPilot;
}

// Istruzioni di utilizzo
export const usage = {
  // Utilizzo come WebComponent HTML
  html: `
    <!-- Inserisci direttamente il tag nel tuo HTML -->
    <docs-pilot></docs-pilot>
  `,
  
  // Utilizzo come script
  script: `
    <script type="module">
      import { initDocsPilot } from 'path/to/integration.js';
      
      // Inizializza DocsPilot in un elemento specifico
      const container = document.getElementById('container');
      initDocsPilot(container);
    </script>
  `,
  
  // Utilizzo con React
  react: `
    import DocsPilotWebComponent from 'path/to/DocsPilotWebComponent';
    
    function MyComponent() {
      return <DocsPilotWebComponent />;
    }
  `
}

export default {
  DocsPilotElement,
  initDocsPilot,
  usage
};
