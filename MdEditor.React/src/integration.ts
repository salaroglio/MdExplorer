// File per l'integrazione come WebComponent
import { Crepe } from "@milkdown/crepe";

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

  // Quando un attributo cambia
  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (name === 'markdown' && oldValue !== newValue) {
      // Non fare nulla durante la fase di inizializzazione o se il valore è undefined o non una stringa
      if (this.isInitializing || typeof newValue !== 'string') {
        return;
      }
      
      // Se l'editor non è ancora inizializzato, non fare nulla
      // Verrà usato il valore dell'attributo durante l'inizializzazione
      if (!this.editor) {
        return;
      }
      
      // Poiché non esiste un modo diretto per aggiornare il contenuto dell'editor,
      // dobbiamo ricreare l'editor preservando gli event listener
      if (this.editorContainer) {
        try {
          this.isInitializing = true;
          console.log('Aggiornamento del contenuto markdown:', newValue.substring(0, 20) + '...');
          
          // Salviamo il contenitore originale prima di distruggere l'editor
          const parentNode = this.editorContainer.parentNode;
          
          // Distruggi l'editor esistente
          this.editor.destroy();
          this.editor = null;
          
          // Rimuoviamo il vecchio contenitore (sarà ricostruito da Crepe)
          if (parentNode && this.editorContainer) {
            parentNode.removeChild(this.editorContainer);
          }
          
          // Crea un nuovo contenitore
          this.editorContainer = document.createElement('div');
          this.editorContainer.className = 'docs-pilot-editor';
          this.shadowRoot?.appendChild(this.editorContainer);
          
          // Ricrea l'editor con il nuovo valore
          this.editor = new Crepe({
            root: this.editorContainer,
            defaultValue: newValue
          });
          
          // Configuriamo gli eventi prima di inizializzare l'editor
          this.setupEditorListeners();
          
          // Inizializza il nuovo editor
          this.editor.create().then(() => {
            console.log('Editor ricreato con successo');
            this.isInitializing = false;
          }).catch((error: Error) => {
            console.error('Errore nella ricostruzione dell\'editor:', error);
            this.isInitializing = false;
          });
        } catch (error) {
          console.error('Errore durante l\'aggiornamento dell\'editor:', error);
          this.isInitializing = false;
        }
      }
    }
  }
  
  // Getter per il markdown attuale
  get markdown() {
    // Se l'editor è inizializzato, ottieni il valore direttamente dall'editor
    if (this.editor && typeof this.editor.getMarkdown === 'function') {
      return this.editor.getMarkdown();
    }
    // Altrimenti, restituisci il valore dell'attributo
    return this.getAttribute('markdown') || this.defaultMarkdown;
  }
  
  // Setter per il markdown
  set markdown(value) {
    this.setAttribute('markdown', value);
  }
  
  // Quando l'elemento viene aggiunto al DOM
  connectedCallback() {
    // Imposta il flag di inizializzazione
    this.isInitializing = true;
    
    // Crea shadow DOM
    this.shadowRoot = this.attachShadow({ mode: 'open' });
    
    // Determina il percorso base in base all'URL corrente
    const basePath = new URL('.', window.location.href).href;
    
    // Aggiungi stili CSS con percorso assoluto
    // Utilizziamo direttamente il CSS di Milkdown
    const linkElem = document.createElement('link');
    linkElem.setAttribute('rel', 'stylesheet');
    linkElem.setAttribute('href', `${basePath}assets/css/milkdown-all.css`);
    this.shadowRoot.appendChild(linkElem);
    
    // Crea container per l'editor Milkdown
    this.editorContainer = document.createElement('div');
    this.editorContainer.className = 'docs-pilot-editor';
    this.shadowRoot.appendChild(this.editorContainer);
    
    // Inizializza l'editor
    setTimeout(() => {
      // Inizializza l'editor con un piccolo ritardo per assicurarsi che gli stili siano caricati
      this.initEditor();
    }, 50);
  }
  
  // Configura gli eventi dell'editor
  setupEditorListeners() {
    if (!this.editor) return;
    
    // Configura gli eventi dell'editor usando l'API listener di Crepe
    this.editor.on((listener: any) => {
      listener.markdownUpdated((markdown: any) => {
        // Converti esplicitamente a stringa se non lo è già
        let markdownText: string;
        
        if (typeof markdown === 'string') {
          markdownText = markdown;
        } else if (markdown && typeof markdown === 'object') {
          // Se è un oggetto, prova ad estrarre proprietà rilevanti o a convertirlo in JSON
          try {
            if (markdown.toString && typeof markdown.toString === 'function' && 
                markdown.toString() !== '[object Object]') {
              markdownText = markdown.toString();
            } else if (markdown.content && typeof markdown.content === 'string') {
              markdownText = markdown.content;
            } else if (markdown.text && typeof markdown.text === 'string') {
              markdownText = markdown.text;
            } else {
              // Fallback - controlla se l'oggetto ha proprietà stringify
              if (markdown.stringify && typeof markdown.stringify === 'function') {
                markdownText = markdown.stringify();
              } else if (markdown.constructor && markdown.constructor.name === 'Jh') {
                // Questo sembra essere un oggetto interno di Milkdown, ignoriamolo silenziosamente
                markdownText = ''; // Non disturbare l'utente con questi messaggi interni
                return; // Esci dalla funzione senza emettere eventi
              } else {
                console.warn('Impossibile convertire l\'oggetto markdown in stringa:', markdown);
                markdownText = ''; // Valore vuoto come fallback sicuro
              }
            }
          } catch (e) {
            console.error('Errore durante la conversione dell\'oggetto markdown:', e);
            markdownText = '';
          }
        } else {
          // Se non è né stringa né oggetto, converti a stringa vuota
          console.warn('Markdown non è una stringa né un oggetto:', markdown);
          markdownText = '';
        }
        
        // Evita di aggiornare l'attributo se il valore è uguale
        // per prevenire loop di aggiornamento
        const currentAttr = this.getAttribute('markdown');
        if (currentAttr !== markdownText) {
          // Imposta un flag per evitare cicli
          this.isInitializing = true;
          
          // Aggiorna l'attributo markdown solo quando è diverso
          this.setAttribute('markdown', markdownText);
          
          // Resetta il flag dopo un piccolo ritardo
          setTimeout(() => {
            this.isInitializing = false;
          }, 10);
        }
        
        // Emetti un evento quando il markdown cambia
        const event = new CustomEvent('markdownChange', {
          detail: { markdown: markdownText },
          bubbles: true,
          composed: true
        });
        this.dispatchEvent(event);
      });
    });
  }
  
  // Inizializza l'editor Milkdown
  async initEditor() {
    if (!this.editorContainer) return;
    
    try {
      this.isInitializing = true;
      console.log('Inizializzazione editor con markdown:', 
                 (this.markdown && this.markdown.substring(0, 20) + '...') || 'vuoto');
      
      // Crea l'istanza dell'editor
      this.editor = new Crepe({
        root: this.editorContainer,
        defaultValue: this.markdown
      });
      
      // Configura gli eventi dell'editor
      this.setupEditorListeners();
      
      // Inizializza l'editor
      await this.editor.create();
      console.log('Editor inizializzato con successo');
      
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