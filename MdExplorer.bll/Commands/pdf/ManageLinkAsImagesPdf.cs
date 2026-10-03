using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.pdf
{
    /// <summary>
    /// Come <see cref="ManageLinkAsImages"/>, ma per i documenti che vanno a Pandoc (Word e PDF).
    /// <para>
    /// <b>Perché serve una variante.</b> Il calcolo comune produce il percorso dell'immagine
    /// attaccando la cartella del documento: per un documento nella <b>radice</b> del progetto
    /// quella cartella è vuota, e viene fuori <c>"" + "/" + nome</c>, cioè <c>/nome.png</c>.
    /// Nel viewer va bene — lì un'altra trasformazione ci mette davanti
    /// <c>/api/mdexplorer</c> e ne fa un URL giusto — ma Pandoc quel percorso lo prende alla
    /// lettera, come radice del <b>filesystem</b>, e l'immagine non la trova:
    /// <c>[WARNING] Could not fetch resource '/diagramma.png'</c>. Risultato misurato il
    /// 21/09/2026: nel .docx l'immagine non c'era proprio.
    /// </para>
    /// <para>
    /// Qui lo slash iniziale si toglie: Pandoc viene eseguito con la cartella di lavoro sulla
    /// radice del progetto, quindi un percorso relativo a quella è esattamente ciò che sa aprire.
    /// </para>
    /// </summary>
    public class ManageLinkAsImagesPdf : ManageLinkAsImages, ICommandPdf
    {
        /// <summary>Link a immagine: <c>![alt](percorso)</c>.</summary>
        private static readonly Regex ImageLink = new Regex(@"!\[([^\]]*)\]\(([^\)]*)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public ManageLinkAsImagesPdf(ILogger<ManageLinkAsImages> logger, IHelper helper) : base(logger, helper)
        {
        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var transformed = base.TransformInNewMDFromMD(markdown, requestInfo);
            return MakeRelativeToProjectRoot(transformed);
        }

        /// <summary>
        /// Toglie lo slash iniziale ai percorsi delle immagini, lasciando stare gli URL
        /// (<c>http://…</c>, <c>//host/…</c>, <c>data:…</c>): quelli Pandoc li scarica, e
        /// accorciarli li romperebbe.
        /// </summary>
        public static string MakeRelativeToProjectRoot(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return markdown;

            var regions = MarkdownCodeRegions.Of(markdown);
            var increment = 0;

            foreach (Match item in ImageLink.Matches(markdown))
            {
                // Un'immagine dentro un blocco di codice è testo: il suo percorso resta com'è.
                if (regions.IsCode(item.Index)) continue;

                var path = item.Groups[2].Value;
                if (!path.StartsWith("/", StringComparison.Ordinal)) continue;
                if (path.StartsWith("//", StringComparison.Ordinal)) continue;   // URL senza protocollo

                var corrected = item.Groups[0].Value.Replace("(" + path + ")", "(" + path.TrimStart('/') + ")");

                // Per posizione: una Replace riscriverebbe anche la stessa immagine nel codice.
                var at = item.Index + increment;
                markdown = markdown.Remove(at, item.Length).Insert(at, corrected);
                increment += corrected.Length - item.Length;
            }

            return markdown;
        }
    }
}
