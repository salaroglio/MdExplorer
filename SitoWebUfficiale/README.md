# Sito Web Ufficiale mdExplorer

Questo è il sito web ufficiale per mdExplorer, un software di gestione documenti Markdown.

## Struttura

```
SitoWebUfficiale/
├── index.html          # Homepage
├── privacy.html        # Privacy Policy (GDPR compliant)
├── terms.html          # Termini di Servizio
├── cookies.html        # Cookie Policy
├── license.html        # Licenza MIT
├── css/
│   └── style.css      # Stili CSS
└── js/
    └── main.js        # JavaScript (gestione cookie, navigazione)
```

## Caratteristiche Legali

Il sito include tutte le protezioni legali necessarie:

1. **Privacy Policy** - Conforme GDPR con tutti i diritti degli utenti
2. **Cookie Policy** - Trasparenza sull'uso dei cookie tecnici
3. **Termini di Servizio** - Limitazione responsabilità e uso consentito
4. **Licenza MIT** - Chiarezza sulla licenza open source
5. **Cookie Banner** - Consenso cookie con localStorage

## Note Importanti

### Dati da Personalizzare

Prima di pubblicare il sito, aggiorna:

- Email di contatto (attualmente placeholder: support@mdexplorer.com)
- Link GitHub al repository reale
- Link download per le varie piattaforme
- Informazioni di contatto legali

### Conformità GDPR

Il sito è progettato per essere conforme GDPR:
- Cookie solo tecnici essenziali
- Privacy by design
- Consenso esplicito per cookie
- Tutti i diritti utente elencati

### Hosting

Per l'hosting considera:
- HTTPS obbligatorio per sicurezza
- CDN per performance globali
- Backup regolari
- Monitoraggio uptime

## Sviluppo Locale

Per testare il sito localmente:

1. Apri index.html in un browser
2. O usa un server locale: `python -m http.server 8000`
3. Visita http://localhost:8000

## Prossimi Passi

1. Sostituire placeholder con dati reali
2. Aggiungere favicon.ico
3. Implementare form contatti (con backend)
4. Aggiungere analytics (con consenso utente)
5. Ottimizzare immagini e performance
6. Aggiungere sitemap.xml per SEO