using System;
using System.Linq;
using System.Text;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// Turns the box context collected from the SVG into the two prompts sent to
    /// the LLM.
    ///
    /// The whole feature exists because analysis documents explain their diagrams
    /// badly: the diagram compresses, the explanation is scattered in the prose,
    /// and nobody wants to go hunting for it. So the prompts are built around three
    /// rules, each of which is a product decision, not a stylistic preference:
    ///
    ///   1. TEN SENTENCES, NEVER MORE. A user buried in text stops reading, and the
    ///      feature fails exactly like the document it was meant to repair.
    ///   2. NEVER INVENT. When the document does not explain the box, say so and
    ///      fall back to what the diagram itself states. One made-up explanation
    ///      costs the trust earned by all the correct ones.
    ///   3. THE STRUCTURE IS GIVEN, NOT GUESSED. Relations arrive already typed
    ///      from the SVG, so the model does not spend effort re-deriving what is
    ///      deterministic — it spends it on re-expanding the compressed meaning.
    /// </summary>
    public static class MarkDiagramPromptBuilder
    {
        public const int MaxSentences = 10;

        /// <summary>
        /// Soft cap on the document text. A document past this size gets truncated
        /// and the prompt says so explicitly, so the model can admit the gap
        /// instead of filling it.
        /// </summary>
        public const int MaxDocChars = 60000;

        public static string BuildSystemPrompt()
        {
            return string.Join("\n", new[]
            {
                "Sei MarkAgent, l'assistente di MdExplorer. L'utente sta guardando un diagramma PlantUML",
                "dentro un documento di analisi e ha chiesto spiegazioni su UN box del diagramma.",
                "",
                "Il diagramma è una sintesi: ogni box comprime un concetto la cui spiegazione è sparsa",
                "nel documento. Il tuo lavoro è riespandere QUELLA sintesi, per QUEL box.",
                "",
                "REGOLE, in ordine di importanza:",
                "",
                $"1. MAI più di {MaxSentences} frasi. È un limite rigido, non un'indicazione di massima.",
                "   L'utente ha attenzione limitata: se lo sommergi smette di leggere. Preferisci dire",
                "   poco e lasciare che sia lui a chiedere il resto.",
                "",
                "2. MAI inventare. Se il documento non spiega quel box, DILLO apertamente e riporta",
                "   solo ciò che il diagramma dichiara. Una risposta che ammette il buco è utile;",
                "   una che lo riempie di plausibilità fa perdere fiducia in tutte le altre.",
                "",
                "3. Le relazioni te le do già tipizzate: non dedurle e non metterle in dubbio.",
                "   Usale per collocare il box, non per descrivere il diagramma riga per riga.",
                "",
                "4. Niente preamboli, niente riepiloghi, niente 'come richiesto'. Parti dal contenuto.",
                "",
                "5. USA GLI ELENCHI PUNTATI quando aiutano davvero, e spesso aiutano: un elenco",
                "   si scorre con l'occhio, un paragrafo va letto tutto. Sono il modo giusto per",
                "   le responsabilita' di un box, le entita' che lo circondano, i vincoli.",
                "   Un punto = una riga breve, e ogni riga conta nel limite delle dieci frasi.",
                "   Resta in prosa quando devi spiegare UN concetto o legare due cose fra loro:",
                "   spezzare un ragionamento in punti lo fa sembrare una lista della spesa.",
                "",
                "6. Scrivi in italiano, con lo stesso lessico del documento.",
            });
        }

        public static string BuildUserPrompt(MarkDiagramContextDto ctx, string documentText, bool documentTruncated)
        {
            var sb = new StringBuilder();
            var box = ctx.Box;

            sb.AppendLine($"BOX SELEZIONATO: {box?.Name}");
            if (!string.IsNullOrWhiteSpace(box?.Kind))
                sb.AppendLine($"tipo di elemento: {box.Kind}");
            if (!string.IsNullOrWhiteSpace(ctx.DiagramTitle))
                sb.AppendLine($"titolo del diagramma: {ctx.DiagramTitle}");
            if (!string.IsNullOrWhiteSpace(ctx.DiagramType))
                sb.AppendLine($"tipo di diagramma: {ctx.DiagramType}");
            sb.AppendLine();

            AppendRelations(sb, ctx);
            AppendSource(sb, ctx);
            AppendDocument(sb, documentText, documentTruncated);

            sb.AppendLine();
            sb.AppendLine($"Spiega il box \"{box?.Name}\" in non più di {MaxSentences} frasi:");
            sb.AppendLine("cosa rappresenta, come si colloca fra gli elementi che lo circondano, e");
            sb.AppendLine("cosa aggiunge il documento che il diagramma da solo non poteva dire.");

            return sb.ToString();
        }

        private static void AppendRelations(StringBuilder sb, MarkDiagramContextDto ctx)
        {
            var relations = ctx.Relations;
            if (relations == null || relations.Count == 0)
            {
                sb.AppendLine("RELAZIONI: nessuna. Il box non è collegato ad altri elementi del diagramma.");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("RELAZIONI dichiarate dal diagramma:");
            foreach (var r in relations)
            {
                var verb = DescribeRelation(r, ctx.Box?.Name);
                var label = string.IsNullOrWhiteSpace(r.Label) ? string.Empty : $" (etichetta: \"{r.Label}\")";
                sb.AppendLine($"  - {verb}{label}");
            }

            // Legacy diagrams carry no data-link-type. Say it, so the model does not
            // read the absence of a type as "plain association".
            if (string.Equals(ctx.SvgFormat, "legacy", StringComparison.OrdinalIgnoreCase) ||
                relations.Any(r => string.IsNullOrWhiteSpace(r.Type)))
            {
                sb.AppendLine();
                sb.AppendLine("NOTA: per alcune relazioni il tipo UML non è dichiarato dal diagramma");
                sb.AppendLine("(formato PlantUML precedente). Non dedurlo: trattale come collegamenti generici.");
            }

            sb.AppendLine();
        }

        /// <summary>
        /// Renders one relation in words. The direction matters: "A eredita da B" and
        /// "B è ereditato da A" are the same arrow but a different sentence, and the
        /// model must not have to work it out.
        ///
        /// Careful with the asymmetry PlantUML bakes into the SVG: for `A &lt;|-- B`
        /// it writes data-entity-1=A and data-entity-2=B, so on an <c>extension</c>
        /// **entity-1 is the base class and entity-2 the derived one** — the opposite
        /// of the reading that feels natural for composition and dependency, where
        /// entity-1 is the owner/consumer. Getting this backwards makes MarkAgent
        /// state the inheritance upside down, which is exactly the kind of confident
        /// wrong answer the feature must never produce.
        /// </summary>
        private static string DescribeRelation(MarkDiagramRelation r, string? boxName)
        {
            var other = r.Other ?? "?";
            var outgoing = string.Equals(r.Direction, "outgoing", StringComparison.OrdinalIgnoreCase);

            return (r.Type?.ToLowerInvariant()) switch
            {
                "extension" => outgoing
                    ? $"{other} eredita da {boxName} (o ne implementa l'interfaccia): {boxName} è la base"
                    : $"{boxName} eredita da {other} (o ne implementa l'interfaccia)",
                "composition" => outgoing
                    ? $"{boxName} è composto da {other}: {other} non esiste senza {boxName}"
                    : $"{other} è composto da {boxName}: {boxName} non esiste senza {other}",
                "aggregation" => outgoing
                    ? $"{boxName} aggrega {other}, che però ha vita propria"
                    : $"{other} aggrega {boxName}, che però ha vita propria",
                "association" => $"{boxName} è associato a {other}",
                "dependency" => outgoing
                    ? $"{boxName} dipende da {other} / lo usa"
                    : $"{other} dipende da {boxName} / lo usa",
                _ => outgoing
                    ? $"{boxName} è collegato a {other} (tipo di relazione non dichiarato)"
                    : $"{other} è collegato a {boxName} (tipo di relazione non dichiarato)",
            };
        }

        private static void AppendSource(StringBuilder sb, MarkDiagramContextDto ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.PlantumlSource)) return;
            sb.AppendLine("SORGENTE PLANTUML del diagramma:");
            sb.AppendLine("---");
            sb.AppendLine(ctx.PlantumlSource.Trim());
            sb.AppendLine("---");
            sb.AppendLine();
        }

        private static void AppendDocument(StringBuilder sb, string documentText, bool truncated)
        {
            if (string.IsNullOrWhiteSpace(documentText))
            {
                sb.AppendLine("DOCUMENTO: non disponibile. Basati solo sul diagramma, e dillo.");
                return;
            }

            sb.AppendLine("DOCUMENTO che contiene il diagramma:");
            sb.AppendLine("---");
            sb.AppendLine(documentText);
            sb.AppendLine("---");
            if (truncated)
            {
                sb.AppendLine();
                sb.AppendLine("ATTENZIONE: il documento è stato troncato perché troppo lungo. Se la");
                sb.AppendLine("spiegazione del box non compare nella parte che vedi, dillo invece di supporla.");
            }
        }

        /// <summary>
        /// Marcatore del blocco con cui il modello propone una modifica. Deliberatamente
        /// improbabile in un testo normale: se comparisse per caso, verrebbe interpretato
        /// come una proposta di modifica al documento dell'utente.
        /// </summary>
        public const string EditFence = "mde-edit";

        /// <summary>
        /// Istruzioni aggiunte al prompt delle domande di seguito: dicono al modello come
        /// proporre una modifica quando l'utente ne chiede una.
        ///
        /// <para>
        /// La divisione del lavoro è quella di sempre: il modello decide <b>cosa</b>
        /// cambiare, il backend applica <b>esattamente</b> e verifica. Per questo si chiede
        /// un blocco strutturato e non una descrizione a parole — una descrizione andrebbe
        /// reinterpretata al momento di scrivere, ed è lì che si sbaglia file.
        /// </para>
        /// </summary>
        public static string BuildEditInstructions()
        {
            return string.Join("\n", new[]
            {
                "SE l'utente chiede di CAMBIARE il diagramma o il documento, non limitarti a",
                "descrivere la modifica: proponila in un blocco come questo, e nient'altro dopo.",
                "",
                "```" + EditFence,
                "{",
                "  \"summary\": \"cosa cambia e perché, in non più di dieci frasi\",",
                "  \"newPlantuml\": \"il sorgente PlantUML NUOVO, integrale, senza i backtick\",",
                "  \"textEdits\": [",
                "    { \"find\": \"testo esatto da sostituire\", \"replace\": \"nuovo testo\", \"why\": \"perché consegue\" }",
                "  ]",
                "}",
                "```",
                "",
                "REGOLE DEL BLOCCO, e sono vincolanti:",
                "",
                "- \"find\" deve essere copiato ALLA LETTERA dal documento e comparire UNA SOLA",
                "  VOLTA in tutto il testo. Se un frammento è ambiguo allungalo finché non è",
                "  unico. Una sostituzione ambigua viene rifiutata in blocco, non indovinata.",
                "- Includi in \"textEdits\" TUTTE le conseguenze del cambiamento nel documento:",
                "  se sposti una dipendenza, il paragrafo che la descriveva ora è falso.",
                "  Le conseguenze dimenticate sono il vero danno di una modifica al diagramma.",
                "- \"newPlantuml\" va omesso se il diagramma non cambia; \"textEdits\" va omesso",
                "  se cambia solo il diagramma.",
                "- Non toccare altri documenti: se sospetti che ne siano coinvolti, dillo in",
                "  \"summary\" e fermati lì.",
                "",
                "Se invece l'utente sta solo facendo una domanda, rispondi normalmente e NON",
                "usare il blocco.",
            });
        }

        /// <summary>
        /// Counts sentences in a reply. Used to notice when the model ignores the
        /// ten-sentence rule. Deliberately NOT used to truncate: a reply cut mid-thought
        /// is worse than a long one, and the fix belongs in the prompt.
        /// </summary>
        public static int CountSentences(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return text.Split(new[] { '.', '!', '?', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Count(part => part.Trim().Length > 2);
        }
    }
}
