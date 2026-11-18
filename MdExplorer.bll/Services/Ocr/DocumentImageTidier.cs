using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Diagrams;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Sistema un'immagine alla volta di un documento convertito: chiede al modello che cos'è, verifica il
    /// PlantUML che propone, e scrive nel documento. Il modello risponde soltanto; qui si decide e si scrive.
    /// <para>
    /// Uno schema ridisegnato che passa la verifica prende il posto dell'immagine. Se dall'immagine non esce
    /// (il modello non lo scrive, o non passa la verifica nemmeno dopo una correzione) c'è un secondo tentativo
    /// senza immagine: il modello corregge la descrizione con il testo del documento e disegna da lì
    /// (<see cref="RedrawVerdict"/>, utente 18/11/2026). Quello schema resta nel documento comunque, accanto alla
    /// descrizione: al posto dell'immagine se si disegna (con un avviso se i confronti di
    /// <see cref="DiagramFidelity"/> non tornano), ripiegato come testo se non si disegna.
    /// Fra la domanda e la scrittura il documento viene riletto: l'utente può averlo cambiato nel frattempo.
    /// </para>
    /// </summary>
    public sealed class DocumentImageTidier
    {
        public const string Redrawn = "plantuml";

        /// <summary>Ridisegnata dalla descrizione, ma i confronti con descrizione e documento non tornano tutti: chi legge deve controllare.</summary>
        public const string RedrawnToCheck = "plantuml-to-check";
        public const string Described = "described";
        public const string Transcribed = "transcribed";
        public const string Captioned = "captioned";
        public const string Skipped = "skipped";
        public const string Failed = "failed";

        private const int ContextChars = 600;

        // Quello che i tre motori sanno guardare con il loro strumento di lettura.
        private static readonly HashSet<string> ReadableImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".webp" };

        private readonly string _documentFullPath;
        private readonly string _projectPath;
        private readonly string _instructions;
        private readonly string _redrawInstructions;
        private readonly Func<string, Task<string>> _ask;
        private readonly PlantumlBlockVerifier _verifier;

        /// <summary>
        /// A che punto è il lavoro su un'immagine, una riga per tappa: chi guarda (Mark, nel fumetto del pensiero) sa
        /// che si sta lavorando anche quando il modello non manda il suo ragionamento.
        /// </summary>
        public Action<string> OnStage { get; set; }

        private void Stage(string italian, string english) => OnStage?.Invoke(UiText.Say(italian, english));

        /// <param name="instructions">Il prompt <c>mde-doc-tidy</c>.</param>
        /// <param name="redrawInstructions">Il prompt <c>mde-doc-tidy-redraw</c>: lo schema dalla descrizione.</param>
        /// <param name="ask">Una domanda al motore del progetto, in sola lettura: torna il testo della risposta.</param>
        public DocumentImageTidier(string documentFullPath, string projectPath, string instructions, string redrawInstructions, Func<string, Task<string>> ask, PlantumlBlockVerifier verifier)
        {
            _documentFullPath = documentFullPath;
            _projectPath = projectPath;
            _instructions = instructions;
            _redrawInstructions = redrawInstructions;
            _ask = ask;
            _verifier = verifier;
        }

        /// <summary>I percorsi delle immagini ancora da sistemare, nell'ordine del documento.</summary>
        public static List<string> PendingImages(string markdown)
        {
            return DocumentImageTidy.Find(markdown).Where(image => !image.Treated).Select(image => image.Path).Distinct().ToList();
        }

        /// <summary>Guarda un'immagine, decide e scrive. Torna che cosa ha fatto e come lo racconta Mark.</summary>
        public async Task<(string Action, string Comment)> TidyAsync(string imagePath, CancellationToken ct = default)
        {
            var imageFullPath = ResolveImage(_documentFullPath, _projectPath, imagePath);
            if (imageFullPath == null || !File.Exists(imageFullPath))
                return (Failed, UiText.Say($"il file {imagePath} non c'è.", $"the file {imagePath} does not exist."));

            var extension = Path.GetExtension(imageFullPath);
            if (!ReadableImages.Contains(extension))
                return (Skipped, UiText.Say($"è in un formato che non so guardare ({extension}): la lascio com'è.", $"it is in a format I cannot look at ({extension}): I leave it as it is."));

            var markdown = await File.ReadAllTextAsync(_documentFullPath, ct);
            var image = Pending(markdown, imagePath)
                ?? throw new InvalidOperationException(UiText.Say("nel frattempo è sparita dal documento.", "meanwhile it disappeared from the document."));

            var prompt = _instructions + "\n\n---- IMAGE ----\nFile: " + imageFullPath
                + "\nAlternative text in the source document: " + (image.Alt.Trim().Length > 0 ? image.Alt : "(none)")
                + "\n\n---- TEXT AROUND THE IMAGE ----\n" + Around(markdown, image);

            Stage("Guardo l'immagine…", "Looking at the image…");
            var (verdict, answer) = await AskAsync(prompt, _ask, ImageVerdict.Parse);
            if (verdict.Kind == ImageVerdict.Decorative)
                return (Skipped, UiText.Say("è decorativa (un logo o simile): la lascio com'è.", "it is decorative (a logo or similar): I leave it as it is."));

            if (!image.Standalone)
            {
                await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithAlternativeText(text, current, verdict), ct);
                return (Captioned, UiText.Say($"sta in mezzo al testo, le ho dato il testo alternativo «{verdict.Title}».", $"it sits inside the text, I gave it the alternative text \"{verdict.Title}\"."));
            }

            if (verdict.Kind == ImageVerdict.Text)
            {
                await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithTranscription(text, current, verdict), ct);
                return (Transcribed, UiText.Say($"«{verdict.Title}» è un'immagine di testo: l'ho trascritta sotto l'immagine, ripiegata.", $"\"{verdict.Title}\" is an image of text: I transcribed it under the image, folded."));
            }

            if (verdict.Kind == ImageVerdict.Diagram)
            {
                PlantumlVerification check = null;
                if (verdict.PlantUml != null)
                {
                    Stage("È uno schema: verifico che il PlantUML si disegni…", "It is a diagram: checking that the PlantUML draws…");
                    check = await VerifyAsync(_verifier, verdict.PlantUml, ct);
                    if (check.ToolUnavailable == null && check.HasProblems)
                    {
                        Stage("Non si disegna: chiedo una correzione…", "It does not draw: asking for a correction…");
                        // Una sola correzione: il modello rilegge che cosa non va e riscrive.
                        var (second, _) = await AskAsync(prompt + "\n\n---- YOUR PREVIOUS ANSWER ----\n" + answer + "\n\n---- CHECK ----\n" + check.ForModel()
                            + "\n\nAnswer again, in the same format, with the diagram corrected.", _ask, ImageVerdict.Parse);
                        if (second.Kind == ImageVerdict.Diagram && second.PlantUml != null)
                        {
                            verdict = second;
                            check = await VerifyAsync(_verifier, verdict.PlantUml, ct);
                        }
                    }
                    if (check.ToolUnavailable == null && !check.HasProblems)
                    {
                        await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDiagram(text, current, verdict), ct);
                        return (Redrawn, UiText.Say($"«{verdict.Title}» è uno schema: l'ho ridisegnato in PlantUML. L'originale resta sotto, ripiegata.", $"\"{verdict.Title}\" is a diagram: I redrew it in PlantUML. The original stays below, folded."));
                    }
                }

                if (check?.ToolUnavailable != null)
                {
                    // Senza verifica non si sa se uno schema si disegna: un secondo tentativo non direbbe di più.
                    await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDescription(text, current, verdict), ct);
                    return (Described, UiText.Say($"è uno schema, ma non ho potuto verificare il PlantUML ({check.ToolUnavailable})", $"it is a diagram, but I could not check the PlantUML ({check.ToolUnavailable})") + DescriptionBelow());
                }
                return await RedrawFromDescriptionAsync(imagePath, markdown, image, verdict, ct);
            }

            await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDescription(text, current, verdict), ct);
            return (Described, (verdict.Kind == ImageVerdict.Screenshot ? UiText.Say($"«{verdict.Title}» è una schermata", $"\"{verdict.Title}\" is a screenshot")
                : UiText.Say($"«{verdict.Title}» è una foto o un disegno", $"\"{verdict.Title}\" is a photo or a drawing")) + DescriptionBelow());
        }

        private static string DescriptionBelow() => UiText.Say(": ho messo la descrizione sotto l'immagine, ripiegata.", ": I put the description under the image, folded.");

        /// <summary>
        /// Il secondo tentativo: senza immagine, dalla descrizione corretta con il documento. Scrive sempre qualcosa:
        /// lo schema (con o senza avviso), o la descrizione con il sorgente che non si disegna, o la descrizione sola.
        /// </summary>
        private async Task<(string Action, string Comment)> RedrawFromDescriptionAsync(string imagePath, string markdown, DocumentImage image, ImageVerdict seen, CancellationToken ct)
        {
            Stage("Dall'immagine lo schema non esce: lo ridisegno dalla descrizione, leggendo il capitolo del documento…",
                "The diagram does not come out of the image: redrawing it from the description, reading the section of the document…");
            var (outline, section) = DocumentContext.Of(markdown, image);
            var prompt = _redrawInstructions + "\n\n---- DESCRIPTION OF THE IMAGE ----\nTitle: " + seen.Title + "\n" + seen.Description
                + "\n\n---- DOCUMENT OUTLINE ----\n" + outline
                + "\n\n---- SECTION OF THE DOCUMENT ----\n" + section;

            RedrawVerdict redraw;
            string answer;
            try
            {
                (redraw, answer) = await AskAsync(prompt, _ask, RedrawVerdict.Parse);
            }
            catch (InvalidOperationException ex)
            {
                // Il secondo tentativo non ha dato una risposta leggibile: resta la descrizione fatta guardando l'immagine.
                await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDescription(text, current, seen), ct);
                return (Described, UiText.Say($"«{seen.Title}» è uno schema che non sono riuscito a ridisegnare, nemmeno dalla descrizione ({ex.Message})", $"\"{seen.Title}\" is a diagram I could not redraw, not even from the description ({ex.Message})") + DescriptionBelow());
            }

            ImageVerdict Result(RedrawVerdict from) => new ImageVerdict { Kind = ImageVerdict.Diagram, Title = seen.Title, Description = from.Description, PlantUml = from.PlantUml };
            if (redraw.PlantUml == null)
            {
                var described = Result(redraw);
                await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDescription(text, current, described), ct);
                return (Described, UiText.Say($"«{seen.Title}» è uno schema che in PlantUML non si riproduce, nemmeno dalla descrizione: ho messo sotto l'immagine la descrizione, corretta con il testo del documento, ripiegata.",
                    $"\"{seen.Title}\" is a diagram that PlantUML cannot reproduce, not even from the description: I put the description, corrected with the text of the document, under the image, folded."));
            }

            Stage("Verifico che lo schema si disegni…", "Checking that the diagram draws…");
            var check = await VerifyAsync(_verifier, redraw.PlantUml, ct);
            if (check.ToolUnavailable == null && check.HasProblems)
            {
                Stage("Non si disegna: chiedo una correzione…", "It does not draw: asking for a correction…");
                var (second, _) = await AskAsync(prompt + "\n\n---- YOUR PREVIOUS ANSWER ----\n" + answer + "\n\n---- CHECK ----\n" + check.ForModel()
                    + "\n\nAnswer again, in the same format, with the diagram corrected.", _ask, RedrawVerdict.Parse);
                if (second.PlantUml != null)
                {
                    redraw = second;
                    check = await VerifyAsync(_verifier, redraw.PlantUml, ct);
                }
            }

            var result = Result(redraw);
            if (check.ToolUnavailable != null || check.HasProblems)
            {
                await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDescriptionAndBrokenDiagram(text, current, result), ct);
                var why = check.ToolUnavailable != null
                    ? UiText.Say($"non ho potuto verificare che si disegni ({check.ToolUnavailable})", $"I could not check that it draws ({check.ToolUnavailable})")
                    : UiText.Say("non si disegna", "it does not draw");
                return (Described, UiText.Say($"«{seen.Title}» è uno schema: l'ho riscritto in PlantUML dalla descrizione, ma {why}. Sotto l'immagine, ripiegati, trovi la descrizione e il sorgente da correggere a mano.",
                    $"\"{seen.Title}\" is a diagram: I rewrote it in PlantUML from the description, but {why}. Under the image, folded, you find the description and the source to fix by hand."));
            }

            // Si disegna. Che dica le stesse cose lo si può controllare solo per nomi; il resto lo dichiara il modello.
            Stage("Confronto i nomi dello schema con la descrizione e il documento…", "Comparing the names in the diagram with the description and the document…");
            var toCheck = DiagramFidelity.Check(redraw.PlantUml, redraw.Elements, redraw.Relations, redraw.Description, markdown);
            toCheck.AddRange(redraw.Doubts);
            await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDiagramFromDescription(text, current, result, toCheck), ct);
            return toCheck.Count == 0
                ? (Redrawn, UiText.Say($"«{seen.Title}» è uno schema: dall'immagine non ci sono riuscito, l'ho ridisegnato dalla descrizione, aiutandomi con il testo del documento. Immagine e descrizione restano sotto, ripiegate.",
                    $"\"{seen.Title}\" is a diagram: I could not do it from the image, I redrew it from the description, with the help of the text of the document. Image and description stay below, folded."))
                : (RedrawnToCheck, UiText.Say($"«{seen.Title}» è uno schema: l'ho ridisegnato dalla descrizione, ma va controllato: {string.Join("; ", toCheck)}. Immagine e descrizione restano sotto, ripiegate.",
                    $"\"{seen.Title}\" is a diagram: I redrew it from the description, but it needs checking: {string.Join("; ", toCheck)}. Image and description stay below, folded."));
        }

        // La risposta nel formato chiesto; se non lo rispetta, una seconda domanda dice che cosa manca.
        private static async Task<(T Verdict, string Answer)> AskAsync<T>(string prompt, Func<string, Task<string>> ask, Func<string, T> parse)
        {
            var answer = await ask(prompt);
            try
            {
                return (parse(answer), answer);
            }
            catch (FormatException first)
            {
                answer = await ask(prompt + "\n\n---- YOUR PREVIOUS ANSWER ----\n" + answer
                    + "\n\n---- CHECK ----\nThe answer is not in the required format: " + first.Message + ". Answer again, in the required format only.");
                try
                {
                    return (parse(answer), answer);
                }
                catch (FormatException second)
                {
                    throw new InvalidOperationException(UiText.Say(
                        $"il motore ha risposto due volte fuori dal formato chiesto ({second.Message}).",
                        $"the engine answered twice out of the required format ({second.Message})."));
                }
            }
        }

        private static Task<PlantumlVerification> VerifyAsync(PlantumlBlockVerifier verifier, string plantUml, CancellationToken ct)
        {
            return verifier.VerifyAsync(new[] { ("la tua risposta", "```plantuml\n" + plantUml + "\n```\n") }, ct);
        }

        /// <summary>
        /// Rilegge il documento, ritrova l'immagine e scrive: fra la domanda al motore e la scrittura passano secondi, e
        /// l'utente (o un'immagine precedente) può aver cambiato il testo.
        /// </summary>
        private static async Task WriteAsync(string documentFullPath, string imagePath, Func<string, DocumentImage, string> change, CancellationToken ct)
        {
            var markdown = await File.ReadAllTextAsync(documentFullPath, ct);
            var image = Pending(markdown, imagePath)
                ?? throw new InvalidOperationException(UiText.Say("nel frattempo è sparita dal documento.", "meanwhile it disappeared from the document."));
            var changed = change(markdown, image);

            var temp = Path.Combine(Path.GetDirectoryName(documentFullPath), $".{Path.GetFileName(documentFullPath)}.{Guid.NewGuid():N}.tmp");
            try
            {
                await File.WriteAllTextAsync(temp, changed, ct);
                File.Move(temp, documentFullPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    try { File.Delete(temp); } catch { /* residuo innocuo */ }
                }
            }
        }

        private static DocumentImage Pending(string markdown, string imagePath)
        {
            return DocumentImageTidy.Find(markdown).FirstOrDefault(image => !image.Treated && image.Path == imagePath);
        }

        private static string Around(string markdown, DocumentImage image)
        {
            var start = Math.Max(0, image.Start - ContextChars);
            var end = Math.Min(markdown.Length, image.Start + image.Length + ContextChars);
            return markdown.Substring(start, image.Start - start) + "[THE IMAGE IS HERE]" + markdown.Substring(image.Start + image.Length, end - image.Start - image.Length);
        }

        // «/assets/x_img/img1.png» parte dalla radice del progetto, il resto dalla cartella del documento.
        private static string ResolveImage(string documentFullPath, string projectPath, string imagePath)
        {
            var decoded = Uri.UnescapeDataString(imagePath);
            var full = decoded.StartsWith("/", StringComparison.Ordinal)
                ? Path.GetFullPath(Path.Combine(projectPath, decoded.TrimStart('/')))
                : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(documentFullPath), decoded));
            return PathContainment.RelativeOrNull(projectPath, full) == null ? null : full;
        }
    }
}
