# mde-e2e — Preparare i test

Parte della skill `mde-e2e`: le forme del file di test sono in `SKILL.md`, mappa del sito e schede di pagina in `mappa.md`.

## Preparare i test di un sito (prima configurazione)

Quando l'utente ti chiede di preparare i test di un sito («configura i test e2e», «preparami un test del login di
http://localhost:4200»):

1. **Indirizzo e cartella.** `baseUrl` è l'indirizzo del sito, senza percorso finale; la cartella dei test è quella
   che dice l'utente, altrimenti `test-e2e/` nella radice del progetto. Nomi dei file: `mappa-sito-<sito>.md`,
   `credenziali-<sito>.txt`, `<argomento>.e2e.md`, con `<sito>` breve e minuscolo (es. `miosito`).
2. **Testi esatti, mai inventati.** Etichette dei campi, testi dei bottoni, messaggi e percorsi vanno scritti
   **come il sito li mostra**. Se il codice del sito è nel progetto, prendili da lì: route per i percorsi, template
   e componenti per etichette e bottoni, file di traduzione (nella lingua che il sito mostra) per i testi. Se non
   c'è, o un testo non si trova, chiedilo all'utente; se lui preferisce non rispondere, scrivi il testo più
   probabile ed **elencalo** alla fine tra quelli da confermare.
3. **Il file di test**, nelle forme di «Il file di test» (`SKILL.md`): front matter completo, un `## T<n>` per ogni caso
   (almeno il caso che riesce e uno che fallisce, se ha senso), verifiche con **✔** nelle forme della tabella,
   `## Artefatti` e `## Esiti` vuote.
4. **La mappa del sito**: se hai il codice sorgente, puoi crearne una prima versione (pagine dalle route, campi e
   bottoni dai template) con le regole di `mappa.md`; altrimenti la crei al primo giro. Se il sito è
   un'**applicazione a pagina singola** (un solo indirizzo, le «pagine» sono viste, linguette, pannelli), **chiedi
   all'utente** quali sono le pagine, proponendo quelle che vedi: ogni pagina avrà la sua scheda
   (`mappa.md`). Dal sorgente puoi anche anticipare nelle schede quali chiamate fa ogni azione,
   scritte come *«dal sorgente, da confermare»* finché un'esecuzione non le osserva.
5. **Le credenziali, senza valori.** Scegli le chiavi (`<sito>.utente`, `<sito>.password`, …). Prima aggiungi al
   `.gitignore` del progetto la riga `credenziali-*.txt`, se manca. Poi, **solo se il file non esiste**, crealo
   con le chiavi e i valori vuoti:

   ```text
   # credenziali per i test di miosito: compila i valori (solo account di prova)
   miosito.utente=
   miosito.password=
   ```

   Se esiste già non aprirlo: di' all'utente quali chiavi deve contenere. **Non chiedere mai i valori** e, se
   l'utente te ne scrive uno in chat, non ricopiarlo da nessuna parte: digli di scriverlo lui nel file. Finché un
   valore è vuoto MdExplorer non lancia i test e lo dice.
6. **Non creare** `E2eTests.csproj`, `E2eSupport.cs` ed `e2e.runsettings`: nascono alla prima esecuzione.
7. **Alla fine** di' all'utente, in poche righe: i file creati; i valori da scrivere nel file delle credenziali; i
   testi da confermare; come si lancia (tasto destro sul file o sulla cartella → «Test e2e…»).
