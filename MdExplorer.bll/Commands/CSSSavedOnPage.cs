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
    public class CSSSavedOnPage : CommandBase, ICommand, IDisposable
    {
        private readonly ILogger<CSSSavedOnPage> _logger;

        private readonly IHelper _helper;

        public int Priority { get; set; } = 10;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "CSSSavedOnPage";
        public CSSSavedOnPage(
              ILogger<CSSSavedOnPage> logger,
              IHelper helper)
        {
            _logger = logger;

            _helper = helper;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private MatchCollection GetLinkWithCurlyBracketsMatches(string markDown)
        {
            var reg = @"!\[([^\]]*)\]\((.*)\){(.*)}";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markDown);
            return matches;
        }

        private MatchCollection GetMetaDataMatches(string markDown)
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

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {

            var imgMatches = GetLinkWithCurlyBracketsMatches(markdown);
            var matches = GetMatches(markdown);
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + $"{Path.DirectorySeparatorChar}.md");
            // i should remove the CSS extra
            foreach (Match itemImg in imgMatches)
            {
                var curlyBrackets = itemImg.Groups[3].Value;
                var metadataMatch = GetMetaDataMatches(curlyBrackets);

                var classes = metadataMatch.Where(_ => _.Groups[1].Value.StartsWith(".")).FirstOrDefault();
                var metadataString = curlyBrackets;
                var dataMdHash = string.Empty;
                foreach (Match itemCSS in matches)
                {
                    var cssToSave = itemCSS.Groups[1].Value;
                    var textHash = _helper.GetHashString(cssToSave, Encoding.UTF8);
                    var parser = new StylesheetParser();
                    var stylesheet = parser.Parse(cssToSave);
                    foreach (var rule in stylesheet.StyleRules)
                    {
                        var selector = rule.SelectorText; // Yields .someClass
                        if (selector == classes.Value)
                        {
                            var width = rule.Style.Width;
                            var height = rule.Style.Height;
                        }

                        dataMdHash = $" data-md-hash=\"{textHash}\"";
                        break; // exit from foreach serching for CSS classes
                    }

                }
                var metadataToReplaceString = metadataString.Substring(0, metadataString.Length) + dataMdHash;
                var linkToReplace = itemImg.Groups[0].Value.Replace(metadataString, metadataToReplaceString);
                linkToReplace = $"<div class=\"resizable\">{System.Environment.NewLine}{System.Environment.NewLine}{linkToReplace}{System.Environment.NewLine}{System.Environment.NewLine}</div>";
                markdown = markdown.Replace(itemImg.Groups[0].Value, linkToReplace);
            }

            foreach (Match itemCSS in matches)
            {
                var cssToSave = itemCSS.Groups[1].Value;
                var textHash = _helper.GetHashString(cssToSave, Encoding.UTF8);
                var newText = $@"<style id=""{textHash}"">{cssToSave}</style>";
                markdown = markdown.Replace(itemCSS.Groups[0].Value, newText);
            }



            return markdown;
        }
    }
}
