using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.html;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Manage absolute path and and add SignalR connectionId
    /// </summary>
    internal class ManageLinkAbsolutePath : ICommand
    {
        private readonly ILogger<ManageLinkAbsolutePath> _logger;

        public ManageLinkAbsolutePath(ILogger<ManageLinkAbsolutePath> logger)
        {
            _logger = logger;
        }

        public int Priority { get; set; } = 50;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "ManageLinkAbsolutePath";
        public List<Configuration.Models.CompatibilityMode> SupportedModes => null; // Supports all modes

        public MatchCollection GetMatches(string markdown)
        {
            var reg = @"\[([^\]]*)\]\(([^\)]*)\)";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // DO NOTHING
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // DO NOTHING
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var links = GetMatches(markdown);
            var regions = MarkdownCodeRegions.Of(markdown);
            var increment = 0;
            foreach (Match link in links)
            {
                var linkValue = link.Groups[2].Value;

                // A link in code is text: it shows as written, without "?connectionId=…".
                if (regions.IsCode(link.Index))
                {
                    continue;
                }

                // Skip external links, anchors, and non-markdown files
                if (linkValue.StartsWith("http://") ||
                    linkValue.StartsWith("https://") ||
                    linkValue.StartsWith("mailto:") ||
                    linkValue.StartsWith("#"))
                {
                    continue;
                }

                // Process links that need connectionId: absolute paths, relative paths (..), or .md files
                bool isAbsoluteOrRelative = linkValue.StartsWith("/") || linkValue.StartsWith("../");
                // .md alone, with an anchor (.md#/3) or with a query (.md?pages=2,6-9)
                bool isMdFile = Regex.IsMatch(linkValue, @"\.md(?=[?#]|$)");

                if (isAbsoluteOrRelative || isMdFile)
                {
                    string newlink;

                    if (linkValue.StartsWith("/"))
                    {
                        // Project-absolute path: prepend /api/mdexplorer
                        newlink = "/api/mdexplorer" + linkValue;
                    }
                    else
                    {
                        // Relative to the document's folder (../x, x.md): the browser resolves it against
                        // the document's own address; only connectionId is added. Prefixed it became
                        // "/api/mdexplorer../x" (404).
                        newlink = linkValue;
                    }

                    // Handle anchor fragments (#section)
                    Regex rxSharp = new Regex(@"([^#]*)(?:(#.*))?",
                             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var matchesSharp = rxSharp.Matches(newlink);

                    var firstPart = matchesSharp.First().Groups[1].Value;
                    var secondPart = matchesSharp.First().Groups[2]?.Value;

                    // A link that already has a query (?pages=2,6-9) takes connectionId after it.
                    var queryStart = firstPart.Contains('?') ? "&" : "?";
                    newlink = $@"{firstPart}" + queryStart + "connectionId=" + requestInfo.ConnectionId + secondPart;

                    // Build new link preserving original text: [originalText](newUrl)
                    var linkText = link.Groups[1].Value;
                    var newFullLink = $"[{linkText}]({newlink})";
                    // By position: a Replace would also rewrite the same link written in code.
                    var at = link.Index + increment;
                    markdown = markdown.Remove(at, link.Length).Insert(at, newFullLink);
                    increment += newFullLink.Length - link.Length;
                }
            }
            return markdown;
        }
    }
}
