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
    public static class MarkPointPromptBuilder
    {
        /// <summary>Soft cap on the deck given whole: a deck is short, the budget goes to the project.</summary>
        public const int MaxDeckChars = 20000;

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
                "5. Niente preamboli, niente riepiloghi, niente 'come richiesto'. Parti dal contenuto.",
                "",
                "6. Scrivi in italiano, con lo stesso lessico dei documenti.",
            });
        }

        public static string BuildAnswerPrompt(
            MarkDiagramContextDto ctx,
            IReadOnlyList<string> keywords,
            IReadOnlyList<ProjectSource> sources,
            string deckText,
            bool deckTruncated,
            string? question)
        {
            var sb = new StringBuilder();
            AppendPoint(sb, ctx);
            sb.AppendLine();

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

            if (string.IsNullOrWhiteSpace(question))
            {
                sb.AppendLine($"Spiega il punto in non più di {MarkDiagramPromptBuilder.MaxSentences} frasi: cosa vuol dire, e cosa");
                sb.AppendLine("aggiungono i documenti del progetto che la slide da sola non poteva dire.");
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
