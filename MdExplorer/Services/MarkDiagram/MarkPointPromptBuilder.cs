using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MdExplorer.Features.Services.MarkPoint;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// The two prompts of "Chiedi a MarkAgent" on a point of a slide, in Mark Search's two phases:
    /// in the first MarkAgent only chooses the keywords (the contract of Mark Search's phase 1,
    /// <c>mark-search.component.ts</c>), MDE runs the search; in the second MarkAgent explains the
    /// point from what the search found, with the rules of the diagram explanation
    /// (<see cref="MarkDiagramPromptBuilder"/>): ten sentences, never invent.
    ///
    /// Each prompt carries everything it needs: only Copilot keeps a session, and a prompt that
    /// leans on the previous one would reach Claude Code without it.
    ///
    /// Sprint: docs-internal/Sprints/2026-09-25-Slide-Chiedi-A-MarkAgent.md
    /// </summary>
    /// <summary>A document the point links to, read whole; <see cref="Text"/> null when it cannot be read.</summary>
    public sealed record LinkedDocument(string Path, string? Text, bool Truncated);

    public static class MarkPointPromptBuilder
    {
        /// <summary>Soft cap on the deck given whole: a deck is short, the budget goes to the project.</summary>
        public const int MaxDeckChars = 20000;

        /// <summary>At most this many documents the point links to, each read whole up to <see cref="MaxDeckChars"/>.</summary>
        public const int MaxLinkedDocuments = 3;

        public static string BuildKeywordsPrompt(MarkDiagramContextDto ctx, string? question)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Sei MarkAgent, l'assistente di MdExplorer. L'utente sta guardando una presentazione e chiede");
            sb.AppendLine("spiegazioni su UN punto di una slide, sulla base dei documenti del progetto.");
            sb.AppendLine("Lavori in due fasi. In questa FASE 1 devi SOLO scegliere le parole chiave per la ricerca istantanea");
            sb.AppendLine("del progetto (cerca per nome file, titoli dei link e contenuto full-text).");
            sb.AppendLine();
            sb.AppendLine("Rispondi ESCLUSIVAMENTE con un blocco ```json contenente:");
            sb.AppendLine("{\"keywords\": [\"parola1\", \"parola2\"]}");
            sb.AppendLine();
            sb.AppendLine("Regole:");
            sb.AppendLine($"- da 1 a {MarkSearchKeywords.MaxKeywords} keyword, brevi (1-2 parole), termini che plausibilmente compaiono nei documenti");
            sb.AppendLine("- niente operatori o virgolette: solo parole semplici");
            sb.AppendLine("- la ricerca trova le parole intere come sono scritte: \"freccia\" non trova \"frecce\";");
            sb.AppendLine("  preferisci i termini specifici del punto a quelli che compaiono ovunque");
            sb.AppendLine("- array vuoto [] SOLO se il punto non ha nulla da cercare nei documenti");
            sb.AppendLine("- nessun testo fuori dal blocco JSON");
            sb.AppendLine("- non usare i tuoi tool: la ricerca la eseguo io per te");
            sb.AppendLine();
            AppendPoint(sb, ctx);
            if (ctx.Point?.Links is { Count: > 0 } links)
            {
                sb.AppendLine($"il punto rimanda a: {string.Join(", ", links)} (questi documenti te li darò per intero:");
                sb.AppendLine("cerca piuttosto ciò che li collega al resto del progetto)");
            }
            if (!string.IsNullOrWhiteSpace(question))
            {
                sb.AppendLine();
                sb.AppendLine($"Domanda dell'utente su questo punto: «{question.Trim()}»");
            }
            return sb.ToString();
        }

        public static string BuildSystemPrompt()
        {
            return string.Join("\n", new[]
            {
                "Sei MarkAgent, l'assistente di MdExplorer. L'utente sta guardando una presentazione e ha",
                "chiesto spiegazioni su UN punto di una slide. Una slide è una sintesi: la spiegazione del",
                "punto sta nei documenti del progetto. Il tuo lavoro è riespandere QUEL punto, con quello",
                "che i documenti dicono.",
                "",
                "REGOLE, in ordine di importanza:",
                "",
                $"1. MAI più di {MarkDiagramPromptBuilder.MaxSentences} frasi in tutto. È un limite rigido: al massimo 5 punti",
                "   di UNA frase ciascuno, oppure un paragrafo breve. L'utente sta presentando o ascoltando:",
                "   se lo sommergi smette di leggere. Il resto lo chiederà lui.",
                "",
                "2. MAI inventare. Usa SOLO i passaggi dei documenti e la presentazione che ti do qui.",
                "   Se non spiegano il punto, DILLO apertamente: una risposta che ammette il buco è utile,",
                "   una che lo riempie di plausibilità fa perdere fiducia in tutte le altre.",
                "",
                "3. Di' da dove viene ciò che scrivi: cita il file con il suo percorso relativo alla radice",
                "   del progetto, così come te lo do (es. `docs/analisi.md`). Non citare file che non ti ho dato.",
                "",
                "4. Non usare i tuoi tool e non cercare altro: la ricerca l'ho già fatta io.",
                "",
                "5. Una SINTESI DI COMPRENSIONE, non una descrizione: non elencare cosa contiene un documento",
                "   («la prima slide dice…, poi…»). Di' cosa SIGNIFICA: il messaggio di fondo, perché conta,",
                "   cosa implica o lascia aperto. L'utente deve capire, e da lì poter chiedere di più.",
                "   Chiudi con 2-3 domande brevi da cui potrebbe proseguire (contano nel limite delle frasi).",
                "",
                "6. Niente preamboli, niente riepiloghi, niente 'come richiesto'. Parti dal contenuto.",
                "",
                "7. Scrivi in italiano, con lo stesso lessico dei documenti.",
            });
        }

        public static string BuildAnswerPrompt(
            MarkDiagramContextDto ctx,
            IReadOnlyList<string> keywords,
            IReadOnlyList<ProjectSource> sources,
            string deckText,
            bool deckTruncated,
            string? question,
            IReadOnlyList<LinkedDocument>? linked = null)
        {
            linked ??= Array.Empty<LinkedDocument>();
            var sb = new StringBuilder();
            AppendPoint(sb, ctx);
            sb.AppendLine();

            // A point that links to a document is a pointer: what it means is what that document says.
            foreach (var doc in linked)
            {
                sb.AppendLine($"DOCUMENTO A CUI IL PUNTO RIMANDA: `{doc.Path}`" + (doc.Text == null ? " — non leggibile." : ", per intero:"));
                if (doc.Text == null) continue;
                sb.AppendLine("---");
                sb.AppendLine(doc.Text);
                sb.AppendLine("---");
                if (doc.Truncated)
                    sb.AppendLine("ATTENZIONE: il documento è stato troncato perché troppo lungo.");
                sb.AppendLine();
            }

            if (ctx.Point?.IsBox == true)
            {
                MarkDiagramPromptBuilder.AppendRelations(sb, ctx);
                MarkDiagramPromptBuilder.AppendSource(sb, ctx);
            }

            AppendSearch(sb, keywords, sources);

            if (string.IsNullOrWhiteSpace(deckText))
            {
                sb.AppendLine("PRESENTAZIONE: non disponibile.");
            }
            else
            {
                sb.AppendLine("PRESENTAZIONE che contiene la slide:");
                sb.AppendLine("---");
                sb.AppendLine(deckText);
                sb.AppendLine("---");
                if (deckTruncated)
                    sb.AppendLine("ATTENZIONE: la presentazione è stata troncata perché troppo lunga.");
            }
            sb.AppendLine();

            if (string.IsNullOrWhiteSpace(question) && linked.Any(d => d.Text != null))
            {
                sb.AppendLine($"Il punto rimanda a {string.Join(", ", linked.Where(d => d.Text != null).Select(d => $"`{d.Path}`"))}: in non più");
                sb.AppendLine($"di {MarkDiagramPromptBuilder.MaxSentences} frasi dai una sintesi di comprensione di quel documento — il suo messaggio");
                sb.AppendLine("di fondo, perché conta, cosa implica o lascia aperto — e come si lega alla slide da cui l'utente");
                sb.AppendLine("arriva. Non descriverlo slide per slide.");
            }
            else if (string.IsNullOrWhiteSpace(question))
            {
                sb.AppendLine($"In non più di {MarkDiagramPromptBuilder.MaxSentences} frasi dai una sintesi di comprensione del punto: cosa");
                sb.AppendLine("significa, e cosa aggiungono i documenti del progetto che la slide da sola non poteva dire.");
            }
            else
            {
                sb.AppendLine($"DOMANDA DELL'UTENTE su questo punto: «{question.Trim()}»");
                sb.AppendLine($"Rispondi in non più di {MarkDiagramPromptBuilder.MaxSentences} frasi, senza inventare ciò che i documenti non dicono.");
            }
            return sb.ToString();
        }

        private static void AppendPoint(StringBuilder sb, MarkDiagramContextDto ctx)
        {
            var point = ctx.Point;
            sb.AppendLine(point?.IsBox == true
                ? $"PUNTO: il box \"{point.Text}\" di un diagramma della slide"
                : $"PUNTO: «{point?.Text?.Trim()}»");
            if (!string.IsNullOrWhiteSpace(point?.SlideTitle))
                sb.AppendLine($"titolo della slide: {point.SlideTitle.Trim()}");
            if (!string.IsNullOrWhiteSpace(point?.SlideText))
            {
                sb.AppendLine("testo della slide:");
                sb.AppendLine("---");
                sb.AppendLine(point.SlideText.Trim());
                sb.AppendLine("---");
            }
            var deck = System.IO.Path.GetFileName(ctx.DocumentPath ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(deck))
                sb.AppendLine($"presentazione: {deck}");
        }

        private static void AppendSearch(StringBuilder sb, IReadOnlyList<string> keywords, IReadOnlyList<ProjectSource> sources)
        {
            if (keywords.Count == 0)
            {
                sb.AppendLine("RICERCA NEL PROGETTO: nessuna — nella fase 1 non hai indicato parole da cercare.");
                sb.AppendLine();
                return;
            }

            sb.AppendLine($"RICERCA NEL PROGETTO con le parole: {string.Join(", ", keywords.Select(k => $"\"{k}\""))}");
            if (sources.Count == 0)
            {
                sb.AppendLine("Nessun documento del progetto contiene queste parole (esclusa la presentazione stessa).");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("Passaggi dei documenti trovati, dal più pertinente:");
            foreach (var source in sources)
            {
                sb.AppendLine();
                sb.AppendLine($"FILE `{source.Path}` (trovato con: {string.Join(", ", source.Keywords)})");
                if (source.Passages.Count == 0)
                {
                    sb.AppendLine("  (trovato dall'indice, ma nessun paragrafo contiene le parole: forse nel nome o in un link)");
                    continue;
                }
                foreach (var passage in source.Passages)
                {
                    sb.AppendLine($"riga {passage.Line}:");
                    sb.AppendLine(passage.Text);
                }
            }
            sb.AppendLine();
        }
    }
}
