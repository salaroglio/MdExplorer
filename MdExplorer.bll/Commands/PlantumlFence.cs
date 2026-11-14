namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Come si trova il corpo di un blocco <c>```plantuml</c>, uguale per tutti quelli che lo leggono (pagina SVG, PNG,
    /// esportazioni, GitHub, barra delle immagini, collegamenti dei diagrammi, <c>plantuml(@json, …)</c>). C4 della
    /// valutazione del 07/11: ognuno aveva la sua copia di <c>```plantuml([^```]*)</c>, che si ferma al PRIMO backtick: un
    /// <c>`mvn compile`</c> in un'etichetta e il diagramma non si vedeva, mentre il verificatore (Markdig) lo leggeva intero.
    /// <para>
    /// Ora il corpo finisce ai primi TRE backtick di fila, dove capitano (anche la chiusura sulla stessa riga,
    /// <c>@enduml ```</c>, che c'era già); un backtick singolo o doppio dentro il diagramma non chiude più niente. Il gruppo 1
    /// resta IDENTICO a prima per ogni blocco senza backtick, perché il suo hash dà il nome ai file .svg/.png e la pagina lo
    /// rimanda indietro (presentazione a passi, PNG). Quello che viene dopo la chiusura (le opzioni <c>{…}</c>) lo aggiunge
    /// ognuno come prima. Resta vietato solo ``` dentro il diagramma (lo dice il verificatore).
    /// </para>
    /// </summary>
    public static class PlantumlFence
    {
        /// <summary>
        /// Gruppo 1 = il corpo, dalla fine di <c>```plantuml</c> alla chiusura esclusa. L'apertura conta solo a inizio riga,
        /// come per Markdig: prima un «usa il blocco ```plantuml» scritto in una frase apriva un finto blocco che si
        /// mangiava la chiusura del diagramma vero dopo (che quindi non si vedeva). Il controllo non cambia la posizione.
        /// </summary>
        public const string Body = @"(?<=(?:^|\n)[ \t]*)```plantuml([\s\S]*?)```";

        /// <summary>Lo stesso, per il corpo che segue un'intestazione (<c>plantuml(@json, …)</c>): solo la chiusura.</summary>
        public const string BodyEnd = @"([\s\S]*?)```";
    }
}
