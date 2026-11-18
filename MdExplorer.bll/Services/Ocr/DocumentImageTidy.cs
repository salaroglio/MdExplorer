using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>Un'immagine di un documento markdown, dove sta e se è già stata sistemata.</summary>
    public sealed class DocumentImage
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string Alt { get; set; }
        public string Path { get; set; }

        /// <summary>Sta da sola sulla sua riga: le si può mettere accanto un blocco (diagramma, descrizione).</summary>
        public bool Standalone { get; set; }

        /// <summary>Ha già il suo diagramma o la sua descrizione (o, in mezzo al testo, il suo testo alternativo).</summary>
        public bool Treated { get; set; }
    }

    /// <summary>Che cosa ha visto il modello in un'immagine. Lo propone lui; che cosa scrivere lo decide MdExplorer.</summary>
    public sealed class ImageVerdict
    {
        public const string Diagram = "diagram";
        public const string Screenshot = "screenshot";
        public const string Photo = "photo";
        public const string Decorative = "decorative";

        /// <summary>Un'immagine che è soprattutto testo (una pagina, una tabella, del codice): la DESCRIPTION è la trascrizione.</summary>
        public const string Text = "text";

        private static readonly Regex KindLine = new Regex(@"^\s*KIND:\s*(\w+)\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex TitleLine = new Regex(@"^\s*TITLE:\s*(.+?)\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex DescriptionPart = new Regex(@"^\s*DESCRIPTION:\s*(.*)\z", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex PlantumlFence = new Regex(@"```+\s*plantuml\s*\n(.*?)\n\s*```+", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public string Kind { get; set; }
        public string Title { get; set; }

        /// <summary>Il diagramma, da <c>@startuml</c> a <c>@enduml</c>; null se il modello non l'ha ridisegnato.</summary>
        public string PlantUml { get; set; }
        public string Description { get; set; }

        /// <summary>Legge la risposta nel formato chiesto dal prompt <c>mde-doc-tidy</c>.</summary>
        /// <exception cref="FormatException">La risposta non ha il formato chiesto: dice che cosa manca.</exception>
        public static ImageVerdict Parse(string answer)
        {
            answer = (answer ?? string.Empty).Replace("\r\n", "\n");
            var kind = KindLine.Match(answer);
            if (!kind.Success)
            {
                throw new FormatException("manca la riga KIND:");
            }
            var verdict = new ImageVerdict { Kind = kind.Groups[1].Value.ToLowerInvariant() };
            if (verdict.Kind != Diagram && verdict.Kind != Screenshot && verdict.Kind != Photo && verdict.Kind != Decorative && verdict.Kind != Text)
            {
                throw new FormatException($"KIND «{verdict.Kind}» non è fra diagram, text, screenshot, photo, decorative");
            }
            if (verdict.Kind == Decorative)
            {
                return verdict;
            }

            var title = TitleLine.Match(answer);
            if (!title.Success)
            {
                throw new FormatException("manca la riga TITLE:");
            }
            verdict.Title = OneLine(title.Groups[1].Value);

            var description = DescriptionPart.Match(answer);
            verdict.Description = description.Success ? description.Groups[1].Value.Trim() : string.Empty;
            if (verdict.Description.Length == 0)
            {
                throw new FormatException("manca la parte DESCRIPTION:");
            }
            if (verdict.Description.IndexOf("</details", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new FormatException("la DESCRIPTION contiene un </details>");
            }
            // Una trascrizione può portare del codice; un blocco lasciato aperto si mangerebbe il resto del documento.
            var fences = Regex.Matches(verdict.Description, @"^\s*```", RegexOptions.Multiline).Count;
            if (verdict.Kind == Text ? fences % 2 != 0 : fences > 0)
            {
                throw new FormatException(verdict.Kind == Text
                    ? "nella trascrizione un blocco di codice resta aperto"
                    : "la DESCRIPTION contiene un blocco di codice: deve essere solo testo");
            }
            if (verdict.Kind == Text)
            {
                return verdict;
            }

            var beforeDescription = description.Success ? answer.Substring(0, description.Index) : answer;
            var fence = PlantumlFence.Match(beforeDescription);
            if (fence.Success)
            {
                var source = fence.Groups[1].Value.Trim();
                if (!source.StartsWith("@start", StringComparison.Ordinal) || !source.Contains("@end"))
                {
                    throw new FormatException("il blocco plantuml deve andare da @startuml a @enduml");
                }
                if (source.Contains("```"))
                {
                    throw new FormatException("il diagramma contiene tre backtick di fila");
                }
                verdict.PlantUml = source;
            }
            return verdict;
        }

        internal static string OneLine(string text)
        {
            return Regex.Replace((text ?? string.Empty).Replace("[", string.Empty).Replace("]", string.Empty), @"\s+", " ").Trim();
        }
    }

    /// <summary>
    /// Le immagini di un documento convertito e come sistemarle: uno schema ridisegnato prende il posto
    /// dell'immagine (che resta, ripiegata, per il confronto); le altre ricevono una descrizione ripiegata.
    /// <para>
    /// Ogni modifica sostituisce solo il tratto dell'immagine, per posizione: il testo del documento
    /// intorno non viene toccato.
    /// </para>
    /// </summary>
    public static class DocumentImageTidy
    {
        // Il testo alternativo può andare a capo: quello che Word genera da solo («Immagine che contiene…», riga
        // vuota esclusa, «Il contenuto generato dall'IA…») sta su due righe, e un'immagine così restava invisibile.
        private static readonly Regex Image = new Regex(@"!\[(?<alt>(?:[^\]\r\n]|\r?\n(?!\s*\r?\n)){0,1000})\]\((?<path>[^)\s]+)(?:\s+""[^""\r\n]*"")?\)", RegexOptions.Compiled);

        /// <summary>Le immagini locali del documento, fuori dal codice, nell'ordine in cui compaiono.</summary>
        public static IReadOnlyList<DocumentImage> Find(string markdown)
        {
            var code = MarkdownCodeRegions.Of(markdown);
            var images = new List<DocumentImage>();
            foreach (Match match in Image.Matches(markdown))
            {
                var path = match.Groups["path"].Value;
                if (code.IsCode(match.Index) || path.Contains("://") || path.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lineStart = markdown.LastIndexOf('\n', Math.Max(0, match.Index - 1)) + 1;
                if (match.Index == 0)
                {
                    lineStart = 0;
                }
                var lineEnd = markdown.IndexOf('\n', match.Index + match.Length);
                if (lineEnd < 0)
                {
                    lineEnd = markdown.Length;
                }
                // «Da sola sulla riga» anche se Word l'aveva messa in grassetto o corsivo («**![…](…)**») o rientrata di
                // poco: intorno ha solo i segni dell'enfasi. Allora il tratto da sostituire è tutta la riga, segni compresi.
                var before = markdown.Substring(lineStart, match.Index - lineStart);
                var after = markdown.Substring(match.Index + match.Length, lineEnd - match.Index - match.Length);
                var standalone = OnlyEmphasis(before) && OnlyEmphasis(after) && before.Length - before.TrimStart(' ').Length <= 3;
                var image = new DocumentImage
                {
                    Start = standalone ? lineStart : match.Index,
                    Length = standalone ? (markdown.Substring(lineStart, lineEnd - lineStart).TrimEnd().Length) : match.Length,
                    Alt = match.Groups["alt"].Value,
                    Path = path,
                    Standalone = standalone,
                };
                image.Treated = image.Standalone
                    ? NextLine(markdown, lineEnd).StartsWith("<details", StringComparison.OrdinalIgnoreCase)
                      || PreviousLine(markdown, lineStart).StartsWith("<summary", StringComparison.OrdinalIgnoreCase)
                    : image.Alt.Trim().Length > 0;
                images.Add(image);
            }
            return images;
        }

        private static bool OnlyEmphasis(string text)
        {
            var marks = text.Trim();
            return marks.Length <= 3 && marks.All(c => c == '*' || c == '_');
        }

        /// <summary>Quante immagini in mezzo al testo hanno già un testo alternativo (di Word o di Mark): quelle non si toccano.</summary>
        public static int InlineWithAlternativeText(string markdown)
        {
            return Find(markdown).Count(image => !image.Standalone && image.Treated);
        }

        /// <summary>Lo schema ridisegnato al posto dell'immagine; l'originale resta sotto, ripiegata.</summary>
        public static string WithDiagram(string markdown, DocumentImage image, ImageVerdict verdict)
        {
            RequireStandalone(image);
            var block = "```plantuml\n" + verdict.PlantUml + "\n```\n\n"
                + "<details>\n<summary>" + UiText.Say("Immagine originale", "Original image") + "</summary>\n\n"
                + ImageText(image, verdict) + "\n\n</details>";
            return Replace(markdown, image, block);
        }

        /// <summary>
        /// Lo schema ridisegnato dalla descrizione al posto dell'immagine; sotto, ripiegate, l'immagine originale e la
        /// descrizione da cui è nato, perché chi legge possa confrontare. Con <paramref name="toCheck"/> una riga sopra
        /// lo schema dice che cosa non torna.
        /// </summary>
        public static string WithDiagramFromDescription(string markdown, DocumentImage image, ImageVerdict verdict, IReadOnlyList<string> toCheck)
        {
            RequireStandalone(image);
            var note = toCheck == null || toCheck.Count == 0 ? string.Empty
                : "> **" + UiText.Say("Schema ricostruito dalla descrizione dell'immagine, da controllare", "Diagram rebuilt from the description of the image, to be checked")
                    + ":** " + string.Join("; ", toCheck.Select(ImageVerdict.OneLine)) + ".\n\n";
            var block = note + "```plantuml\n" + verdict.PlantUml + "\n```\n\n"
                + "<details>\n<summary>" + UiText.Say("Immagine originale e descrizione", "Original image and description") + "</summary>\n\n"
                + ImageText(image, verdict) + "\n\n" + verdict.Description + "\n\n</details>";
            return Replace(markdown, image, block);
        }

        /// <summary>
        /// L'immagine resta; sotto, ripiegati, la descrizione e il sorgente di uno schema che non si disegna: come
        /// testo, perché un blocco plantuml rotto nella pagina è un riquadro d'errore.
        /// </summary>
        public static string WithDescriptionAndBrokenDiagram(string markdown, DocumentImage image, ImageVerdict verdict)
        {
            RequireStandalone(image);
            var block = ImageText(image, verdict) + "\n\n"
                + "<details>\n<summary>" + UiText.Say("Descrizione dell'immagine", "Image description") + "</summary>\n\n"
                + verdict.Description + "\n\n"
                + "**" + UiText.Say("Schema PlantUML che non si disegna, da correggere a mano", "PlantUML diagram that does not draw, to be fixed by hand") + "**\n\n"
                + "```text\n" + verdict.PlantUml + "\n```\n\n</details>";
            return Replace(markdown, image, block);
        }

        /// <summary>L'immagine resta; sotto, ripiegata, la sua descrizione.</summary>
        public static string WithDescription(string markdown, DocumentImage image, ImageVerdict verdict)
        {
            RequireStandalone(image);
            var block = ImageText(image, verdict) + "\n\n"
                + "<details>\n<summary>" + UiText.Say("Descrizione dell'immagine", "Image description") + "</summary>\n\n"
                + verdict.Description + "\n\n</details>";
            return Replace(markdown, image, block);
        }

        /// <summary>L'immagine resta; sotto, ripiegato, il testo che contiene, trascritto.</summary>
        public static string WithTranscription(string markdown, DocumentImage image, ImageVerdict verdict)
        {
            RequireStandalone(image);
            var block = ImageText(image, verdict) + "\n\n"
                + "<details>\n<summary>" + UiText.Say("Testo dell'immagine", "Text of the image") + "</summary>\n\n"
                + verdict.Description + "\n\n</details>";
            return Replace(markdown, image, block);
        }

        /// <summary>Un'immagine in mezzo al testo o in una tabella: riceve solo il testo alternativo.</summary>
        public static string WithAlternativeText(string markdown, DocumentImage image, ImageVerdict verdict)
        {
            return markdown.Substring(0, image.Start) + ImageText(image, verdict) + markdown.Substring(image.Start + image.Length);
        }

        private static string Replace(string markdown, DocumentImage image, string block)
        {
            var end = image.Start + image.Length;
            var lineEnd = markdown.IndexOf('\n', end);
            // Il blocco HTML si chiude alla prima riga vuota: se dopo c'è subito del testo, la riga vuota va aggiunta.
            var needsBlankLine = lineEnd >= 0 && NextLine(markdown, lineEnd, onlyAdjacent: true).Length > 0;
            return markdown.Substring(0, image.Start) + block + (needsBlankLine ? "\n" : string.Empty) + markdown.Substring(end);
        }

        private static string ImageText(DocumentImage image, ImageVerdict verdict)
        {
            var alt = image.Alt.Trim().Length > 0 ? Regex.Replace(image.Alt.Trim(), @"\s+", " ") : ImageVerdict.OneLine(verdict.Title);
            return "![" + alt + "](" + image.Path + ")";
        }

        private static void RequireStandalone(DocumentImage image)
        {
            if (!image.Standalone)
            {
                throw new InvalidOperationException("DocumentImageTidy: a block can only be placed next to an image that stands alone on its line.");
            }
        }

        // La prima riga non vuota dopo «lineEnd» (o, con onlyAdjacent, la riga subito dopo, anche se vuota).
        private static string NextLine(string markdown, int lineEnd, bool onlyAdjacent = false)
        {
            var position = lineEnd;
            while (position < markdown.Length)
            {
                var start = position + 1;
                if (start >= markdown.Length)
                {
                    return string.Empty;
                }
                var end = markdown.IndexOf('\n', start);
                if (end < 0)
                {
                    end = markdown.Length;
                }
                var line = markdown.Substring(start, end - start).Trim();
                if (line.Length > 0 || onlyAdjacent)
                {
                    return line;
                }
                position = end;
            }
            return string.Empty;
        }

        private static string PreviousLine(string markdown, int lineStart)
        {
            var position = lineStart - 1;
            while (position > 0)
            {
                var start = markdown.LastIndexOf('\n', position - 1) + 1;
                var line = markdown.Substring(start, position - start).Trim();
                if (line.Length > 0)
                {
                    return line;
                }
                position = start - 1;
            }
            return string.Empty;
        }
    }
}
