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
    /// Uno schema ridisegnato che passa la verifica prende il posto dell'immagine; uno che non la passa, dopo
    /// una correzione, ripiega sulla descrizione. Fra la domanda e la scrittura il documento viene riletto:
    /// l'utente può averlo cambiato nel frattempo.
    /// </para>
    /// </summary>
    public sealed class DocumentImageTidier
    {
        public const string Redrawn = "plantuml";
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
        private readonly Func<string, Task<string>> _ask;
        private readonly PlantumlBlockVerifier _verifier;

        /// <param name="instructions">Il prompt <c>mde-doc-tidy</c>.</param>
        /// <param name="ask">Una domanda al motore del progetto, in sola lettura: torna il testo della risposta.</param>
        public DocumentImageTidier(string documentFullPath, string projectPath, string instructions, Func<string, Task<string>> ask, PlantumlBlockVerifier verifier)
        {
            _documentFullPath = documentFullPath;
            _projectPath = projectPath;
            _instructions = instructions;
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

            var (verdict, answer) = await AskVerdictAsync(prompt, _ask);
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

            string whyNotDiagram = null;
            if (verdict.Kind == ImageVerdict.Diagram && verdict.PlantUml != null)
            {
                var check = await VerifyAsync(_verifier, verdict, ct);
                if (check.ToolUnavailable == null && check.HasProblems)
                {
                    // Una sola correzione: il modello rilegge che cosa non va e riscrive.
                    var (second, _) = await AskVerdictAsync(prompt + "\n\n---- YOUR PREVIOUS ANSWER ----\n" + answer + "\n\n---- CHECK ----\n" + check.ForModel()
                        + "\n\nAnswer again, in the same format, with the diagram corrected.", _ask);
                    if (second.Kind == ImageVerdict.Diagram && second.PlantUml != null)
                    {
                        verdict = second;
                        check = await VerifyAsync(_verifier, verdict, ct);
                    }
                }

                if (check.ToolUnavailable != null)
                    whyNotDiagram = UiText.Say($"è uno schema, ma non ho potuto verificare il PlantUML ({check.ToolUnavailable})", $"it is a diagram, but I could not check the PlantUML ({check.ToolUnavailable})");
                else if (check.HasProblems)
                    whyNotDiagram = UiText.Say("è uno schema, ma il PlantUML che ho scritto non passa la verifica", "it is a diagram, but the PlantUML I wrote does not pass the check");
                else
                {
                    await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDiagram(text, current, verdict), ct);
                    return (Redrawn, UiText.Say($"«{verdict.Title}» è uno schema: l'ho ridisegnato in PlantUML. L'originale resta sotto, ripiegata.", $"\"{verdict.Title}\" is a diagram: I redrew it in PlantUML. The original stays below, folded."));
                }
            }

            await WriteAsync(_documentFullPath, imagePath, (text, current) => DocumentImageTidy.WithDescription(text, current, verdict), ct);
            var what = whyNotDiagram ?? (verdict.Kind == ImageVerdict.Diagram ? UiText.Say($"«{verdict.Title}» è uno schema che in PlantUML non si riproduce bene", $"\"{verdict.Title}\" is a diagram that PlantUML cannot reproduce well")
                : verdict.Kind == ImageVerdict.Screenshot ? UiText.Say($"«{verdict.Title}» è una schermata", $"\"{verdict.Title}\" is a screenshot")
                : UiText.Say($"«{verdict.Title}» è una foto o un disegno", $"\"{verdict.Title}\" is a photo or a drawing"));
            return (Described, what + UiText.Say(": ho messo la descrizione sotto l'immagine, ripiegata.", ": I put the description under the image, folded."));
        }

        // La risposta nel formato chiesto; se non lo rispetta, una seconda domanda dice che cosa manca.
        private static async Task<(ImageVerdict Verdict, string Answer)> AskVerdictAsync(string prompt, Func<string, Task<string>> ask)
        {
            var answer = await ask(prompt);
            try
            {
                return (ImageVerdict.Parse(answer), answer);
            }
            catch (FormatException first)
            {
                answer = await ask(prompt + "\n\n---- YOUR PREVIOUS ANSWER ----\n" + answer
                    + "\n\n---- CHECK ----\nThe answer is not in the required format: " + first.Message + ". Answer again, in the required format only.");
                try
                {
                    return (ImageVerdict.Parse(answer), answer);
                }
                catch (FormatException second)
                {
                    throw new InvalidOperationException(UiText.Say(
                        $"il motore ha risposto due volte fuori dal formato chiesto ({second.Message}).",
                        $"the engine answered twice out of the required format ({second.Message})."));
                }
            }
        }

        private static Task<PlantumlVerification> VerifyAsync(PlantumlBlockVerifier verifier, ImageVerdict verdict, CancellationToken ct)
        {
            return verifier.VerifyAsync(new[] { ("la tua risposta", "```plantuml\n" + verdict.PlantUml + "\n```\n") }, ct);
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
