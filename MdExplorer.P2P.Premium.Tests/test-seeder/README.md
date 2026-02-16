# MdExplorer Test Seeder

Script standalone per testare il download P2P da MdExplorer.

## Setup

1. Copia questa cartella sul computer remoto (es. Ferdi)
2. Installa le dipendenze:
   ```
   npm install
   ```

## Utilizzo

```bash
node test-seeder.js <token>
```

Esempio:
```bash
node test-seeder.js abc123mytoken
```

## Output

Lo script:
1. Crea un file di test (100 KB)
2. Lo mette in seed sul tracker `wss://errantia.net/p2p/`
3. Stampa il **magnet link**

Copia il magnet link e usalo in MdExplorer per scaricare il file.

## Esempio di output

```
==========================================
  SEEDING ACTIVE
==========================================

Torrent name: test-file.txt
Info hash: abc123...

MAGNET LINK (copy this to MdExplorer):

magnet:?xt=urn:btih:abc123...&dn=test-file.txt&tr=wss://errantia.net/p2p/

==========================================

Waiting for peers...
Press Ctrl+C to stop seeding.

[12:34:56] Peer connected: WebRTC
[12:34:57] Uploaded: 50 KB, Speed: 125.3 KB/s, Peers: 1
```

## Note

- Il token è necessario per autenticarsi con il tracker
- Mantieni lo script in esecuzione mentre scarichi da MdExplorer
- Usa Ctrl+C per terminare
