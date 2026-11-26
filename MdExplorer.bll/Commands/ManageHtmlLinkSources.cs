using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Refactoring;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Il gemello di <see cref="ManageLinkAbsolutePath"/> per l'HTML scritto nel documento (<c>&lt;img src&gt;</c>,
    /// <c>&lt;a href&gt;</c>): la pagina sta su <c>/api/mdexplorer/docs/a.md?connectionId=…</c> e un file del progetto si
    /// raggiunge solo con il <c>connectionId</c>, che dice di quale progetto è. Senza, un'immagine HTML non si vedeva mai.
    /// Stessa regola dei link markdown: <c>/x</c> dalla radice del progetto diventa <c>/api/mdexplorer/x</c>, un percorso
    /// relativo resta (lo risolve il browser dall'indirizzo della pagina), e si aggiunge <c>connectionId</c>.
    /// Gira presto, sul markdown dell'autore: dopo, i comandi aggiungono HTML loro (<c>/assets/resize.png</c> dei bottoni)
    /// che non va toccato. I link scritti come esempio nel codice restano come sono (<see cref="MarkdownLinks.ReadAll"/>).
    /// </summary>
    internal class ManageHtmlLinkSources : ICommand
    {
        private readonly ILogger<ManageHtmlLinkSources> _logger;

        public ManageHtmlLinkSources(ILogger<ManageHtmlLinkSources> logger)
        {
            _logger = logger;
        }

        public int Priority { get; set; } = 4;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "ManageHtmlLinkSources";
        public List<Configuration.Models.CompatibilityMode> SupportedModes => null;

        public MatchCollection GetMatches(string markdown) => Regex.Matches(string.Empty, "x");

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo) => markdown;

        public string TransformAfterConversion(string html, RequestInfo requestInfo) => html;

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            if (string.IsNullOrEmpty(markdown) || string.IsNullOrEmpty(requestInfo?.ConnectionId) || requestInfo.StaticExport)
                return markdown;
            var links = MarkdownLinks.ReadAll(markdown)
                .Where(l => l.Kind == MarkdownLinkKind.Html && !MarkdownLinks.IsExternal(l.Target) && l.Target.Length > 0
                            && !l.Raw.Contains("connectionId=", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(l => l.Start).ToList();
            if (links.Count == 0) return markdown;

            var sb = new StringBuilder(markdown);
            foreach (var link in links)
            {
                var raw = link.Raw;
                var hash = raw.IndexOf('#');
                var path = hash >= 0 ? raw[..hash] : raw;
                var anchor = hash >= 0 ? raw[hash..] : string.Empty;
                if (path.StartsWith('/')) path = "/api/mdexplorer" + path;
                var written = path + (path.Contains('?') ? "&" : "?") + "connectionId=" + requestInfo.ConnectionId + anchor;
                sb.Remove(link.Start, link.Length).Insert(link.Start, written);
            }
            return sb.ToString();
        }
    }
}
