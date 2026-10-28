# mde-e2e — Analizzare un rigioco fallito

Parte della skill `mde-e2e`: le schede di pagina sono descritte in `mappa.md`, le regole degli script e delle attese in `esecuzione.md`.

## Analizzare un rigioco fallito

Quando uno script rigiocato fallisce, l'utente te lo chiede in chat (dalla finestra dei test, «Analizza con
MarkAgent»: il messaggio dice il file di test, i test falliti con script e messaggio, la cartella del rigioco).
Lo scopo è capire **perché**, insieme all'utente, e lasciare scritto ciò che si è capito.

1. **Leggi**: il `.e2e.md`, lo script, il **registro** del rigioco (`registro.T<n>.md` nella cartella del
   rigioco: l'esito dello script con il messaggio d'errore completo, ogni chiamata con inizio, durata e
   dimensione, e la console, con i tempi dall'inizio del test), le
   schede delle pagine che il test attraversa, e se serve il `report.md` dell'ultima esecuzione con te.
2. **Trova la causa** confrontando ciò che lo script aspetta con ciò che il registro mostra. Le cause tipiche:
   - l'**attesa** manca o aspetta la cosa sbagliata: lo script prosegue prima che la pagina abbia finito (nel
     registro la chiamata o il `[mde] ready` arrivano dopo il passo che fallisce);
   - il **tempo massimo** è troppo basso per la durata che il registro mostra;
   - il **localizzatore** trova più elementi (strict mode) o nessuno;
   - il **sito è cambiato** o **sbaglia**: una chiamata fallita, un errore in console, un testo diverso. Questo è
     un **difetto del sito**, cioè proprio ciò che il test deve scoprire: dillo, e non correggere lo script per
     farlo passare.
3. **Spiega** all'utente in poche righe, citando le righe del registro e dello script che lo mostrano.
4. **Proponi** la correzione, e applicala **solo quando l'utente è d'accordo**:
   - la **scheda** della pagina: cosa aspettare, tempo massimo, trappole; e una riga in «Storia dei problemi» con
     la data, cosa è successo e cosa è cambiato;
   - lo **script**: l'attesa, il tempo massimo o il localizzatore, con il commento `// attesa: …` aggiornato. Se la
     correzione richiede di guardare di nuovo il sito (un localizzatore nuovo, una chiamata che non conosci),
     proponi invece di rieseguire il test con MarkAgent (tasto destro → «Test e2e…»): lo script si rigenera
     seguendo la scheda corretta.
5. Se l'attesa era **dedotta** (chiamata o fine del lavoro) e il codice sorgente del sito è nel progetto, suggerisci
   i segnali con la skill `mde-e2e-signals`.
