using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MdExplorer.Features.StaticSite
{
    /// <summary>Why something a page points at is not in the export as it is in MdExplorer.</summary>
    public enum StaticSiteIssueKind
    {
        /// <summary>The file does not exist in the project.</summary>
        Missing,
        /// <summary>The address leaves the project's folder (<c>../../</c>): not copied.</summary>
        OutsideProject,
        /// <summary>A resource on another site (CDN, image): the page needs the network for it.</summary>
        NeedsNetwork,
        /// <summary>A PlantUML diagram without its SVG in <c>.md/</c>: the document was not opened since it changed.</summary>
        DiagramNotGenerated,
        /// <summary>A mermaid block: mermaid is not in the export, the block stays text.</summary>
        Mermaid,
        /// <summary>The address asks MdExplorer's service, which the export does not have.</summary>
        NeedsService,
        /// <summary>The markdown file could not be turned into a page.</summary>
        RenderFailed,
        /// <summary>The start page could not be <c>index.html</c>: the project has one in the export.</summary>
        StartPageRenamed,
        /// <summary>The page still names a folder of this computer (the project's path).</summary>
        LocalPath,
    }

    public sealed class StaticSiteIssue
    {
        public StaticSiteIssueKind Kind { get; init; }

        /// <summary>The exported page (zip path) where it was found.</summary>
        public string Page { get; init; }

        /// <summary>The address as written on the page, or what the issue is about.</summary>
        public string Address { get; init; }

        public string Detail { get; init; }
    }

    /// <summary>What went into the export and what did not, and why: nothing is left out silently.</summary>
    public sealed class StaticSiteReport
    {
        private readonly List<StaticSiteIssue> _issues = new();

        public string StartPage { get; set; }

        /// <summary>The page the start page opens: the deck the export was asked from.</summary>
        public string Entry { get; set; }

        public int Pages { get; set; }

        public int Files { get; set; }

        public long Bytes { get; set; }

        public IReadOnlyList<StaticSiteIssue> Issues => _issues;

        public void Add(StaticSiteIssueKind kind, string page, string address, string detail = null)
        {
            // The same address on the same page counts once (a logo on every slide).
            if (_issues.Any(i => i.Kind == kind && i.Page == page && i.Address == address)) return;
            _issues.Add(new StaticSiteIssue { Kind = kind, Page = page, Address = address, Detail = detail });
        }

        private static string Explain(StaticSiteIssueKind kind) => kind switch
        {
            StaticSiteIssueKind.Missing => "Il file non esiste nel progetto: il link o l'immagine non funzionerà.",
            StaticSiteIssueKind.OutsideProject => "Il file è fuori dalla cartella del progetto: non è stato copiato.",
            StaticSiteIssueKind.NeedsNetwork => "Risorsa su un altro sito: la pagina la carica solo con internet.",
            StaticSiteIssueKind.DiagramNotGenerated => "Diagramma PlantUML non ancora generato: apri il documento in MdExplorer e rifai l'esportazione.",
            StaticSiteIssueKind.Mermaid => "Blocco mermaid: mermaid non è incluso nell'esportazione, il blocco resta testo.",
            StaticSiteIssueKind.NeedsService => "Indirizzo del servizio di MdExplorer: fuori da MdExplorer non risponde.",
            StaticSiteIssueKind.RenderFailed => "Il documento non è stato trasformato in pagina.",
            StaticSiteIssueKind.StartPageRenamed => "Il progetto ha già un index.html: la pagina di avvio ha un altro nome.",
            StaticSiteIssueKind.LocalPath => "La pagina contiene un percorso di questo computer: chi la riceve lo vede.",
            _ => kind.ToString(),
        };

        /// <summary>The report as a page of the export (<c>_mde/resoconto.html</c>).</summary>
        public string ToHtml(string title)
        {
            static string E(string s) => WebUtility.HtmlEncode(s ?? string.Empty);
            var html = new StringBuilder();
            html.Append("<!DOCTYPE html>\n<html lang=\"it\"><head><meta charset=\"utf-8\"><title>Resoconto dell'esportazione</title>\n<style>")
                .Append("body{font-family:system-ui,sans-serif;margin:2em;line-height:1.5;color:#222;background:#fff}")
                .Append("table{border-collapse:collapse;width:100%}th,td{border:1px solid #ccc;padding:.4em .6em;text-align:left;vertical-align:top}")
                .Append("th{background:#f3f3f3}code{word-break:break-all}.ok{color:#1a7f37}")
                .Append("@media (prefers-color-scheme: dark){body{background:#1e1e1e;color:#ddd}th{background:#2a2a2a}th,td{border-color:#444}}")
                .Append("</style></head><body>\n")
                .Append($"<h1>Resoconto dell'esportazione — {E(title)}</h1>\n")
                .Append($"<p>Si parte da <a href=\"../{E(Entry)}\">{E(Entry)}</a> (aprendo <code>{E(StartPage)}</code>) · {Pages} pagine · {Files} file · {Bytes / 1024.0 / 1024.0:0.0} MB</p>\n");
            if (_issues.Count == 0)
            {
                html.Append("<p class=\"ok\">Tutto quello che le pagine raggiungono è nell'esportazione.</p>\n");
            }
            else
            {
                html.Append("<table><tr><th>Cosa</th><th>Dove</th><th>Indirizzo</th></tr>\n");
                foreach (var issue in _issues.OrderBy(i => i.Kind).ThenBy(i => i.Page))
                {
                    var detail = string.IsNullOrEmpty(issue.Detail) ? string.Empty : $"<br><small>{E(issue.Detail)}</small>";
                    html.Append($"<tr><td>{E(Explain(issue.Kind))}{detail}</td><td>{E(issue.Page)}</td><td><code>{E(issue.Address)}</code></td></tr>\n");
                }
                html.Append("</table>\n");
            }
            html.Append("</body></html>\n");
            return html.ToString();
        }
    }
}
