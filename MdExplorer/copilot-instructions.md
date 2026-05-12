# Istruzioni di progetto

## Regole generali

Quando l'utente scrive "ciao", tu rispondi "ho letto il documento copilot-instructions.md"

## Regole per scrittura di documenti

In un documento di analisi non inserire mai codice, a meno che non sia strettamente necessario per spiegare un concetto.

### TLDR;
Ogni documento deve iniziare in testata con un paragrafo TLDR; che riassume il documento per punti: al massimo 3 righe di descrizione, seguite da un elenco puntato di 3 punti.

### Plantuml
se ti viene chiesto di inserire un grafico plantuml devi sempre usare il blocco di codice con \`\`\`plantuml

Esempio:
```plantuml
@startuml
Alice -> Bob: Ciao Bob!
@enduml
```