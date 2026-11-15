using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Gli avvisi di un'analisi, ciascuno con il suo TIPO (14/11/2026, utente: «anziché una lista sarebbe meglio dargli una
    /// sintesi delle informazioni, perché in maniera progressiva sappia come esplorare il grafo»). Il tipo si dice dove
    /// l'avviso nasce — mai dedotto dopo dal testo — e serve a contarli per gruppo: <c>code_inventory</c> dà i conti per
    /// tipo con un esempio, e l'elenco intero di un tipo solo a chi lo chiede. Si usa come la lista di testi di prima
    /// (<c>Add(testo)</c>, LINQ sui testi): un avviso senza tipo è <see cref="Other"/>.
    /// </summary>
    public sealed class CodeWarnings : IReadOnlyList<string>
    {
        // ---- i tipi: nomi stabili, in inglese come gli altri vocabolari del grafo (li legge MarkAgent)
        /// <summary>Un progetto riferito che non è fra i sorgenti analizzati: ciò che ne usa resta un nome non risolto.</summary>
        public const string ProjectMissing = "referenced-project-missing";
        /// <summary>Un progetto riferito per GUID che è in più file di progetto, e nessuna soluzione dice quale.</summary>
        public const string ProjectAmbiguous = "referenced-project-ambiguous";
        /// <summary>Un riferimento risolto in un modo che va detto: la libreria di un altro progetto, il progetto della soluzione.</summary>
        public const string ReferenceResolved = "reference-resolved-otherwise";
        /// <summary>Una libreria (o il framework) che non c'è dove il progetto la indica.</summary>
        public const string LibraryMissing = "library-missing";
        /// <summary>File della cartella che il progetto non compila: restano senza simboli.</summary>
        public const string FilesNotCompiled = "files-not-compiled";
        /// <summary>Un file (sorgente, progetto, soluzione, catalogo) che non si legge o non si analizza.</summary>
        public const string FileUnreadable = "file-unreadable";
        /// <summary>Nomi dichiarati in più file (classi partial, copie): un nodo solo per nome.</summary>
        public const string DuplicateNames = "names-declared-in-several-files";
        /// <summary>Riferimenti che non arrivano a niente: nomi che nessun file dichiara, letture di query che nessun catalogo ha, pagine incluse non trovate.</summary>
        public const string Unresolved = "unresolved-references";
        /// <summary>I ponti fra cartelle program (dipendenze del build): rimandati, non fatti, cambiati.</summary>
        public const string Bridges = "bridges";
        /// <summary>Un linguaggio scelto che non si può estrarre qui (grammatica o estrattore mancante).</summary>
        public const string ExtractorUnavailable = "extractor-unavailable";
        /// <summary>Uno script che il grafo non ha caricato, o un passo dopo l'estrazione (regole, registrazioni, report) fallito.</summary>
        public const string StepFailed = "step-failed";
        /// <summary>Il linguaggio (una regola del pacchetto, un blocco non chiuso, una direttiva sconosciuta) dice che il sorgente non è come atteso.</summary>
        public const string LanguageRules = "language-rules";
        /// <summary>Le regole di arricchimento e le registrazioni: una regola che ora collega meno, righe saltate, non riapplicate.</summary>
        public const string EnrichmentRules = "enrichment-rules";
        /// <summary>Le descrizioni: quelle di file spariti non ritrovate, il controllo della descrizione automatica fallito.</summary>
        public const string Descriptions = "descriptions";
        /// <summary>Una cartella che su questa macchina non si analizza (non collegata, non raggiungibile).</summary>
        public const string NotAnalyzedHere = "not-analysed-here";
        public const string Other = "other";

        private readonly List<string> _texts = new();
        private readonly List<string> _kinds = new();

        public int Count => _texts.Count;
        public string this[int index] => _texts[index];
        public IEnumerator<string> GetEnumerator() => _texts.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Un avviso senza tipo: <see cref="Other"/>. C'è perché il codice di prima continui a valere; chi scrive un avviso nuovo dice il tipo.</summary>
        public void Add(string text) => Add(Other, text);

        public void Add(string kind, string text)
        {
            _texts.Add(text);
            _kinds.Add(string.IsNullOrWhiteSpace(kind) ? Other : kind);
        }

        /// <summary>Testi senza tipo, o un'altra lista di avvisi con i suoi tipi.</summary>
        public void AddRange(IEnumerable<string> texts)
        {
            if (texts is CodeWarnings typed) { AddRange(typed); return; }
            foreach (var t in texts) Add(t);
        }

        /// <summary>Gli avvisi di un'altra lista, con il loro tipo; <paramref name="text"/> cambia il testo (un prefisso) senza toccare il tipo.</summary>
        public void AddRange(CodeWarnings other, Func<string, string> text = null)
        {
            for (var i = 0; i < other.Count; i++) Add(other._kinds[i], text == null ? other._texts[i] : text(other._texts[i]));
        }

        /// <summary>Testi tutti dello stesso tipo.</summary>
        public void AddRange(string kind, IEnumerable<string> texts)
        {
            foreach (var t in texts) Add(kind, t);
        }

        /// <summary>Toglie gli avvisi dopo i primi <paramref name="count"/>: una passata di prova che non deve lasciarne.</summary>
        public void TruncateTo(int count)
        {
            if (count < 0 || count >= _texts.Count) return;
            _texts.RemoveRange(count, _texts.Count - count);
            _kinds.RemoveRange(count, _kinds.Count - count);
        }

        public string KindAt(int index) => _kinds[index];

        public IEnumerable<(string Kind, string Text)> Typed => _texts.Select((t, i) => (_kinds[i], t));
    }
}
