---
description: "Prompt da passare all'agente mde-skillcreator per creare una skill il cui grafo è consultabile da un'AI che deve generare codice a partire da un documento di analisi/specifica."
mde:
  origin: mdexplorer
  version: 1
  updatePolicy: replace
---

<!--
MdExplorer-managed prompt.
Il blocco `mde:` segnala che questo file è distribuito da MdExplorer e verrà
sovrascritto per restare allineato. Per personalizzarlo mantenendo le tue
modifiche, rimuovi il blocco `mde:` (o cambia `origin`).

USO: aprire una chat NUOVA con l'agente mde-skillcreator e incollare come primo
messaggio tutto il testo sotto la riga di separazione.
-->

---

Voglio creare una skill di estrazione concetti per i documenti di analisi/specifica
di questo progetto, dai quali un'AI dovrà poi generare codice.

Azione futura: un'AI legge il grafo prodotto dalla skill e genera il codice descritto
dal documento, SENZA rileggere ogni volta l'intero documento. Il grafo deve essere
l'unica fonte necessaria per scrivere il codice; il documento si rilegge solo per il
dettaglio fine di una singola unità, tramite il range di righe registrato sul nodo.

Vincoli di progettazione del grafo (tienili fermi durante il tuo dialogo a step):

- Atomicità operativa: ogni nodo è un'unità su cui il codice agisce concretamente —
  mai categorie astratte.
- Copertura totale: ogni unità di output descritta dal documento ha un nodo, comprese
  quelle la cui logica non è ancora definita — marcate esplicitamente come non risolte,
  così il codice generato le segnala invece di saltarle in silenzio.
- Le relazioni codificano l'OPERAZIONE, non un legame generico: ogni freccia deve
  corrispondere a un passo che il generatore di codice può tradurre direttamente.
- Determinismo: ogni unità di output ha una derivazione unica e non ambigua; se il
  documento ne fornisce due in conflitto, il grafo lo rende esplicito.
- Metadati per il codice: i nodi portano i tipi/formati necessari a generare codice
  corretto (tipo, lunghezza, formato, vincoli).
- Deve esistere una query canonica "dammi tutto ciò che serve a generare X" che in una
  sola interrogazione restituisce input, costanti, dipendenze, tipo e range di righe.

Procedi col tuo dialogo a step. Allo Step 1 parti da qui; chiedimi i dettagli di
dominio (tipo di documento, unità atomiche, operazioni di trasformazione) che ti
servono per gli step successivi.
