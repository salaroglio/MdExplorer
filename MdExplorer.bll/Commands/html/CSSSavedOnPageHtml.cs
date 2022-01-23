using ExCSS;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
                var divLeft = string.Empty;
                var divTop = string.Empty;
                var divPosition = string.Empty;
                var classForDivContainer = string.Empty;
                var styleTopDivContainer = string.Empty;
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
                            divLeft = rule.Style.Left;
                            divTop = rule.Style.Top;
                            divPosition = rule.Style.Position;
                            dataMdHash = $" .overrideImgFluid data-md-hash=\"{cssHash}\"";
                            break; // exit from foreach serching for CSS classes
                        }


                    }
                    markdown = markdown.Replace(itemCSS.Groups[0].Value, string.Empty);
                }
                if (dataMdHash == string.Empty) // nothing is done
                {
                    dataMdHash = $" .overrideImgFluid data-md-hash=\"empty\"";
                }

                if (divPosition == "absolute")
                {
                    classForDivContainer = "class=\"movedAndFixed\"";
                    styleTopDivContainer = $"style=\"top:{divTop}; left:{divLeft};\"";
                }

                var guidToDisplayToolbar = Guid.NewGuid().ToString("D");

                var metadataToReplaceString = metadataString.Substring(0, metadataString.Length) + dataMdHash;
                var newDataToInsert = itemImg.Groups[0].Value.Replace(metadataString, metadataToReplaceString);
                newDataToInsert = $"<div onmouseenter=\"showImageToolbar('{guidToDisplayToolbar}')\" onmouseleave=\"hideImageToolbar('{guidToDisplayToolbar}')\"  md-path-file=\"{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}\" md-css-hash=\"{cssHash}\" md-link-hash=\"{linkHash}\" style=\"width:{divWidth}; height:{divHeight};\" class=\"defaultImg \" onmouseup=\"resizeImage(this)\">" +
                    $"{System.Environment.NewLine}{System.Environment.NewLine}" +
                    $"{newDataToInsert}" +
                    $"{System.Environment.NewLine}{System.Environment.NewLine}" +
                    $"</div>";
                var newDivForResize = $"<div {classForDivContainer} {styleTopDivContainer} ><div id=\"{guidToDisplayToolbar}\" onmouseenter=\"showImageToolbar('{guidToDisplayToolbar}')\" onmouseleave=\"hideImageToolbar('{guidToDisplayToolbar}')\" style=\" display:none;\"><button onclick=\"activateResize('{linkHash}','{guidToDisplayToolbar}')\" class=\"btn btn-md btn-primary-outline\"><img src=\"/assets/resize.png\"/></button>";
                var newDivForMove = $"<button onclick=\"activateMove(this,'{linkHash}','{guidToDisplayToolbar}')\"  class=\"btn btn-md btn-primary-outline\"><img src=\"/assets/move.png\"/></button></div>";
                var newDivForMoveClose = $"</div>";
                newDataToInsert = string.Concat(newDivForResize, newDivForMove, newDataToInsert, newDivForMoveClose);
                markdown = markdown.Replace(itemImg.Groups[0].Value, newDataToInsert);

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
