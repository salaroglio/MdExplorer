# mde-e2e — La mappa del sito e le schede di pagina

Parte della skill `mde-e2e`: valgono quando prepari, esegui o analizzi i test.

## La mappa del sito

La mappa del sito è la memoria di ciò che si sa del sito: la leggi **prima** di eseguire, la aggiorni **dopo**
(se il file non esiste ancora, crealo tu al primo giro, nel percorso indicato da `siteMap`).
Ci va solo ciò che serve a eseguire e scrivere test futuri, e che resta vero nel tempo:

- per ogni **pagina**: indirizzo, titolo, a cosa serve;
- gli **elementi** con cui si interagisce, con **ruolo e nome accessibile** come li mostra `browser_snapshot`
  (`textbox "Username"`, `button "Login"`, `link "Logout"`): sono i nomi che usano gli script;
- i **messaggi** che il sito mostra e quando (conferme, errori);
- la **navigazione**: dove porta un'azione (redirect, pagine intermedie);
- le **trappole**: nomi accessibili strani (un'icona nel nome del bottone), attese necessarie, errori di console
  che non sono difetti.

Non ci va: valori delle credenziali, dati che cambiano a ogni visita (orari, numeri d'ordine), l'esito dei test.

````markdown esempio=mappa
# Mappa del sito the-internet

Indirizzo: `https://the-internet.herokuapp.com`. Ultimo aggiornamento: 2026-09-27, da `login.e2e.md`.

## Elementi comuni
- Messaggi: un riquadro in cima alla pagina, senza ruolo ARIA; si trova per testo.
- La console riporta errori di risorse esterne a ogni caricamento: non sono difetti del sito.

## `/login` — Login Page
- textbox "Username", textbox "Password"
- button "Login" (il nome accessibile comincia con un'icona: cercarlo per nome parziale)
- Credenziali giuste → `/secure`, testo "You logged into a secure area!"
- Password sbagliata → resta su `/login`, testo "Your password is invalid!"

## `/secure` — Secure Area
- link "Logout" → `/login`, testo "You logged out of the secure area!"
````

Aggiorna la mappa aggiungendo o correggendo, non riscrivendola da capo. Ogni fatto va sotto la **pagina** a cui
appartiene; in «Elementi comuni» solo ciò che vale per tutte le pagine. Se una cosa scritta non è più vera,
correggila e scrivi accanto la data. Per ogni pagina che ha una scheda, la mappa ha il link alla scheda.

## Le schede di pagina

Accanto alla mappa c'è una cartella con lo stesso nome (senza `.md`) e dentro **una scheda per pagina**: dice
**come la pagina carica** e quindi cosa deve aspettare uno script. La leggi prima di eseguire un test che passa
da quella pagina, la aggiorni dopo. In un'applicazione a pagina singola la «pagina» è la vista che l'utente
percepisce come un posto diverso, e quali siano le pagine lo decide l'utente.

````markdown esempio=scheda
# Login

Si apre da: `/login`. Ultimo aggiornamento: 2026-09-28, da `login.e2e.md`.

## Azioni e chiamate

### Premi "Login"
- Attesa: navigazione a `/secure` (il modulo si invia con un POST e la pagina cambia).
- Osservato: `POST /authenticate` → 303 verso `/secure`, 180 ms, 1,2 KB.
- Tempo massimo: 10 s.
- Rumore da ignorare: nessuno.
- Fine del lavoro: compare `link "Logout"`.

## Da tenere d'occhio
- Il bottone "Login" ha un'icona nel nome accessibile: cercarlo per nome parziale.

## Storia dei problemi
- 2026-09-28: nessun problema.
````

Le regole della scheda:

- **Solo ciò che hai osservato**, con il dato che lo prova: durata e dimensione lette da `browser_network_request`,
  l'elemento visto nello snapshot, la riga della console. Un'ipotesi (perché è lenta, cosa fa il server) si
  scrive come tale: «ipotesi: …». Una scheda sbagliata fa sbagliare tutti gli script che verranno.
- Per ogni azione che carica: cosa aspettare (segnale, chiamata, navigazione), a cosa serve, dimensione e durata
  osservate, tempo massimo, rumore da ignorare, fine del lavoro.
- **Storia dei problemi**: quando un rigioco fallisce e se ne capisce il perché, una riga con la data, cosa è
  successo e cosa è cambiato nella scheda. Non cancellare le righe vecchie.
- Aggiorna aggiungendo o correggendo, non riscrivendo da capo; una cosa non più vera si corregge con la data.
