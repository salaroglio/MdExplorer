using ExCSS;
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

namespace MdExplorer.Features.Commands.html
{
    public class CSSSavedOnPageHtml : CSSSavedOnPage, ICommandHtml
    {
        public CSSSavedOnPageHtml(ILogger<CSSSavedOnPageHtml> logger,IHelper helper):base(logger,helper)
        {

        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var imgMatches = GetLinkWithCurlyBracketsMatches(markdown);
            var matches = GetMatches(markdown);
            // i should remove the CSS extra            
            foreach (Match itemImg in imgMatches)
            {
                var curlyBrackets = itemImg.Groups[3].Value;
                var metadataMatch = GetMetaDataMatches(curlyBrackets);

                var classes = metadataMatch.Where(_ => _.Groups[1].Value.StartsWith(".")).FirstOrDefault();
                var linkHash = _helper.GetHashString(classes.Value, Encoding.UTF8);
                var metadataString = curlyBrackets;
                var dataMdHash = string.Empty;
                var cssHash = "empty";
                var divWidth = "100%";
                var divHeight = "100%";
                foreach (Match itemCSS in matches)
                {
                    var cssToAnalyze = itemCSS.Groups[1].Value;
                    cssHash = _helper.GetHashString(itemCSS.Groups[0].Value, Encoding.UTF8);
                    var parser = new StylesheetParser();
                    var stylesheet = parser.Parse(cssToAnalyze);
                    foreach (var rule in stylesheet.StyleRules)
                    {
                        var selector = rule.SelectorText; // Yields .someClass
                        //linkHash = _helper.GetHashString(selector, Encoding.UTF8);
                        if (selector == classes.Value)
                        {
                            divWidth = rule.Style.Width;
                            divHeight = rule.Style.Height;
                            dataMdHash = $" data-md-hash=\"{cssHash}\"";
                            break; // exit from foreach serching for CSS classes
                        }


                    }
                    markdown = markdown.Replace(itemCSS.Groups[0].Value, string.Empty);
                }
                if (dataMdHash == string.Empty) // nothing is done
                {
                    dataMdHash = $" data-md-hash=\"empty\"";
                }

                var metadataToReplaceString = metadataString.Substring(0, metadataString.Length) + dataMdHash;
                var linkToReplace = itemImg.Groups[0].Value.Replace(metadataString, metadataToReplaceString);
                linkToReplace = $"<div md-css-hash=\"{cssHash}\" style=\"width:{divWidth}; height:{divHeight};\" class=\"resizable defaultImg \" onmouseup=\"resizeImage(this,'{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}','{linkHash}')\">{System.Environment.NewLine}{System.Environment.NewLine}{linkToReplace}{System.Environment.NewLine}{System.Environment.NewLine}</div>";
                var newDivForMove = $"<div>";
                var newDivForMoveClose = $"</div>";
                markdown = markdown.Replace(itemImg.Groups[0].Value, linkToReplace);

            }

            //foreach (Match itemCSS in matches)
            //{
            //    var cssToSave = itemCSS.Groups[1].Value;
            //    var textHash = _helper.GetHashString(cssToSave, Encoding.UTF8);
            //    var newText = $@"<style id=""{textHash}"">{cssToSave}</style>";
            //    markdown = markdown.Replace(itemCSS.Groups[0].Value, newText);
            //}
            //if (matches.Count==0)
            //{
            //    var textHash = _helper.GetHashString(CSSToInsert, Encoding.UTF8);
            //    var newText = $@"<style id=""{textHash}"">{CSSToInsert}</style>";
            //    markdown = string.Concat(markdown, System.Environment.NewLine, CSSToInsert);
            //}
            return markdown;
        }
       
    }
}
