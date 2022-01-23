using ExCSS;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class MoveAndResizeForImages : CommandBase, ICommand, IDisposable
    {
        private readonly ILogger<MoveAndResizeForImages> _logger;

        protected readonly IHelper _helper;

        public int Priority { get; set; } = 10;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "CSSSavedOnPage";
        public MoveAndResizeForImages(
              ILogger<MoveAndResizeForImages> logger,
              IHelper helper)
        {
            _logger = logger;

            _helper = helper;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected MatchCollection GetLinkWithCurlyBracketsMatches(string markDown)
        {
            var reg = @"!\[([^\]]*)\]\((.*)\){(.*)}";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markDown);
            return matches;
        }

        protected MatchCollection GetMetaDataMatches(string markDown)
        {
            var reg = @"{?([^\s{}]+)}?";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markDown);
            return matches;
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

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }
    }
}
