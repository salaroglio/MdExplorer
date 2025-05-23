# DocsPilot WebComponent

Questo è un progetto minimalista che contiene solo il WebComponent per l'editor Markdown utilizzato in DocsPilot.

## Struttura dei file

- `very-simple-test.html` - Pagina HTML di test per il WebComponent
- `milkdown-all.css` - Stili CSS principali per l'editor Markdown
- `src/integration.ts` - Implementazione del WebComponent
- `vite.config.ts` - Configurazione per la build con Vite
- `package.json` - Dipendenze del progetto

## Dipendenze principali

- `@milkdown/crepe` - Editor Markdown utilizzato dal WebComponent
- `styled-components` - Per lo styling dei componenti React

## Utilizzo

### Installazione
```bash
npm install
```

### Sviluppo
```bash
npm run dev
```

### Build
```bash
npm run build
```

### Test
Apri il file `very-simple-test.html` nel browser dopo aver eseguito una build.

## Importazione nel tuo progetto

Puoi importare il WebComponent nel tuo progetto in diversi modi:

### HTML puro
```html
<script type="module" src="path/to/docspilot.es.js"></script>
<docs-pilot></docs-pilot>
```

### JavaScript
```javascript
import { initDocsPilot } from 'path/to/docspilot.es.js';

const container = document.getElementById('container');
const editor = initDocsPilot(container);
```

### React
```jsx
import DocsPilotWebComponent from 'path/to/DocsPilotWebComponent';

function MyComponent() {
  return <DocsPilotWebComponent />;
}
```