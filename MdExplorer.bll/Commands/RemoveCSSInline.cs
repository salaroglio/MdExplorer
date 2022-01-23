using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// This command is used in common with CSSSavedOnPage and
    /// FromPlantumlToSvg. They use the CSSInline metadata, so thats informatons are very important
    /// for at least two commands.
    /// i have to delete these information last.
    /// </summary>
    public class RemoveCSSInline : CommandBase, ICommand, IDisposable
    {
        private readonly ILogger<RemoveCSSInline> _logger;

        protected readonly IHelper _helper;

        public int Priority { get; set; } = 100;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "RemoveCSSInline";

        public RemoveCSSInline(
              ILogger<RemoveCSSInline> logger,
              IHelper helper)
        {
            _logger = logger;

            _helper = helper;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"```CSSInline([^```]*)```",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);
            foreach (Match itemCSS in matches)
            {
                markdown = markdown.Replace(itemCSS.Groups[0].Value, string.Empty);
            }
            return markdown;
        }
    }
}
