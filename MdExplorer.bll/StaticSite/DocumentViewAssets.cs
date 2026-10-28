using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.StaticSite
{
    /// <summary>
    /// What a document page loads, for the static export. MdExplorer's document page loads its scripts
    /// through <c>wwwroot/common.js</c> (<c>loadScriptOnce</c>, <c>document.write</c> of the styles); that
    /// file stays the one list: here it is read in its order, and the files that only talk to the service
    /// (knowledge graph, corrections, AI, running code…) or that the export leaves out (mermaid) are
    /// dropped. An exported page lists the rest in its <c>&lt;head&gt;</c>, so the export sees and copies
    /// each one.
    /// </summary>
    public static class DocumentViewAssets
    {
        /// <summary>The files of <c>common.js</c> an exported document does not load, and why.</summary>
        public static readonly IReadOnlyDictionary<string, string> NotInExport = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["/mermaid/mermaid.min.js"] = "mermaid is not in the export (the project draws with PlantUML)",
            ["/javascripts/jqueryForFirstPage/diagrams/mermaid-rendering.js"] = "mermaid is not in the export",
            ["/javascripts/jqueryForFirstPage/emojis/emoji-interactions.js"] = "writes the emoji's state in the file",
            ["/javascripts/jqueryForFirstPage/emojis/emoji-sortable.js"] = "writes the emoji's order in the file",
            ["/javascripts/jqueryForFirstPage/ai-selection/ai-selection.css"] = "AI on the selection: the service",
            ["/javascripts/jqueryForFirstPage/ai-selection/ai-selection.js"] = "AI on the selection: the service",
            ["/javascripts/jqueryForFirstPage/clipboard/clipboard-paste.css"] = "pastes images into the file",
            ["/javascripts/jqueryForFirstPage/clipboard/clipboard-paste.js"] = "pastes images into the file",
            ["/javascripts/jqueryForFirstPage/inline-edit/inline-edit.css"] = "corrects the file",
            ["/javascripts/jqueryForFirstPage/inline-edit/inline-edit.js"] = "corrects the file",
            ["/javascripts/jqueryForFirstPage/panels/kg-manager.css"] = "knowledge graph: the service",
            ["/javascripts/jqueryForFirstPage/panels/kg-manager.js"] = "knowledge graph: the service",
            ["/javascripts/lib/force-graph.min.js"] = "knowledge graph only",
            ["/javascripts/jqueryForFirstPage/diagrams/plantuml-integration.js"] = "presentation and clipboard of a diagram: the service",
            ["/javascripts/jqueryForFirstPage/execution/mde-exec-blocks.css"] = "runs code blocks on this computer",
            ["/javascripts/jqueryForFirstPage/execution/mde-exec-blocks.js"] = "runs code blocks on this computer",
            ["/ansi_up/ansi_up.js"] = "output of running code only",
            ["/javascripts/jqueryForFirstPage/interactive-svg/mark-diagram-context.css"] = "Ask MarkAgent: the service",
            ["/javascripts/jqueryForFirstPage/interactive-svg/mark-diagram-context.js"] = "Ask MarkAgent: the service",
            ["/javascripts/jqueryForFirstPage.js"] = "the old single file, not shipped (common.js loads the modules)",
        };

        /// <summary>
        /// Files the document page's scripts set by themselves (<c>mdeAsset('…')</c> in core/globals.js): no
        /// address on the page names them, so the export copies them with every document.
        /// </summary>
        public static readonly IReadOnlyList<string> ScriptAssets = new[]
        {
            "assets/drawAnimated.gif",
            "assets/drawStatic.png",
            "assets/magnifier.svg",
        };

        private static readonly Regex Asset = new(
            @"loadScriptOnce\(\s*'(?<js>[^']+)'\s*\)|href='(?<css>[^']+\.css)'", RegexOptions.Compiled);

        /// <summary>The files <c>common.js</c> loads, in its order, each once (<c>.css</c> and <c>.js</c>, from the site's root).</summary>
        public static IReadOnlyList<string> ReadCommonJs(string commonJs)
        {
            var files = new List<string>();
            foreach (Match match in Asset.Matches(commonJs ?? string.Empty))
            {
                var file = match.Groups["js"].Success ? match.Groups["js"].Value : match.Groups["css"].Value;
                if (!files.Contains(file, StringComparer.OrdinalIgnoreCase))
                {
                    files.Add(file);
                }
            }
            if (files.Count == 0)
            {
                throw new StaticSiteException("common.js lists no scripts: the document page of the export cannot be written.");
            }
            return files;
        }

        /// <summary>The files an exported document loads: <see cref="ReadCommonJs"/> without <see cref="NotInExport"/>.</summary>
        public static IReadOnlyList<string> ForExport(string commonJs)
            => ReadCommonJs(commonJs).Where(f => !NotInExport.ContainsKey(f)).ToList();

        /// <summary>The tags for the <c>&lt;head&gt;</c> of an exported document (addresses from the site's root: the export rewrites them).</summary>
        public static string HeadTags(string commonJs)
        {
            var tags = new StringBuilder();
            foreach (var file in ForExport(commonJs))
            {
                var address = WebUtility.HtmlEncode(file);
                tags.Append(file.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                    ? $"<link rel=\"stylesheet\" href=\"{address}\" />\n"
                    : $"<script src=\"{address}\"></script>\n");
            }
            return tags.ToString();
        }
    }
}
