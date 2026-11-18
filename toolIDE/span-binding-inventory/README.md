# span-binding-inventory

Trova i punti del codice che C# 14 lega in modo diverso da prima: `array.Contains(x)` e gli altri metodi che esistono
sia in `Enumerable` sia in `MemoryExtensions`, le conversioni implicite a span verso metodi di altri tipi, e la parola
`field` dentro le proprietà. Nato nello sprint .NET 10 (F2, 18/11/2026); resta come controllo prima di ogni rilascio
(decisione D2).

```bash
toolIDE/span-binding-inventory/run.sh --lang default --out /tmp/inventario.txt
```

- **Uscita 0**: nessuna riga «in query: sì» e nessuna riga «field». Si può rilasciare.
- **Uscita 1**: c'è almeno una riga da correggere. Una riga «in query: sì» è una query LINQ (un albero di espressioni)
  in cui un array si lega a `MemoryExtensions`: il fornitore del database può non capirla (NHibernate 5.3.9 perdeva
  l'intera indicizzazione). Si corregge trasformando l'array in lista prima della query (`var ids = arr.ToList();`).
  Le righe «in query: no» sono corrette e più veloci, tranne i metodi che cambiano risultato, come `Reverse`, che su un
  array ora rovescia l'array sul posto e restituisce `void`.
- **Uscita 2**: l'analisi è incompleta (un progetto non si carica o non compila). L'elenco non vale come «zero».

`--lang 14` forza C# 14 su tutti i progetti, anche su quelli che oggi compilano con un C# più vecchio: è il controllo
prudente. I progetti esclusi, e il motivo, sono scritti in `run.sh`. I casi di controllo stanno in `controls/`: con
`--project controls/SpanControls.csproj --lang 14` devono dare 5 righe «in query: sì», 1 «in query: no» e 1 «field»;
con `--lang 13` zero righe. Se non è così, lo strumento è rotto.
