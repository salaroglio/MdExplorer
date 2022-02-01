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
    public class ToolbarImagesHtml : ToolbarImages, ICommandHtml
    {
        public ToolbarImagesHtml(ILogger<ToolbarImagesHtml> logger,IHelper helper):base(logger,helper)
        {

        }



        private MatchCollection GetHashFromLink(string html)
        {            
            Regex rx = new Regex(@"/([^.svg]*).svg",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(html);
            return matches;
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

                var plantumlInfo = metadataMatch.Where(_ => _.Value.StartsWith("md-plantuml")).FirstOrDefault()?.Value;
                var isPlantumlDynamic = plantumlInfo?.Contains("dynamic:true")??false;
                var isPlantumlCopy = plantumlInfo?.Contains("copy:true")??false;

                var classes = metadataMatch.Where(_ => _.Groups[1].Value.StartsWith(".")).FirstOrDefault();
                var linkHash = classes !=null ? _helper.GetHashString(classes.Value, Encoding.UTF8):null;
                var metadataString = curlyBrackets;
                var dataMdHash = string.Empty;
                var cssHash = "empty";
                var divWidth = "auto";
                var divHeight = "auto";
                var divLeft = string.Empty;
                var divTop = string.Empty;
                var divPosition = string.Empty;
                var classForDivContainer = string.Empty;
                var styleTopDivContainer = string.Empty;
                if (classes != null) // nolink
                {
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
                        //markdown = markdown.Replace(itemCSS.Groups[0].Value, string.Empty);
                    }
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

                
                var prepareCurrentQueryRequest = requestInfo.CurrentQueryRequest.Replace(@"\",@"\\");
                
                var mathTostring = GetHashFromLink(itemImg.Groups[2].Value);
                var stringMatchedHash = mathTostring.Count >0 ? mathTostring[0].Groups[1].Value:string.Empty;
                var guidToDisplayToolbar = Guid.NewGuid().ToString("D");
                var currentSvg = $".md{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}{stringMatchedHash}.svg";
                var currentPng = $".md{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}{stringMatchedHash}.png";

                var metadataToReplaceString = metadataString.Substring(0, metadataString.Length) + dataMdHash;
                var divContainsImage = itemImg.Groups[0].Value.Replace(metadataString, metadataToReplaceString);
                divContainsImage = $"<div id=\"{stringMatchedHash}\" onmouseenter=\"showImageToolbar('{guidToDisplayToolbar}')\" onmouseleave=\"hideImageToolbar('{guidToDisplayToolbar}')\"  md-path-file=\"{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}\" md-css-hash=\"{cssHash}\" md-CurrentQueryRequest=\"{prepareCurrentQueryRequest}\" md-link-hash=\"{linkHash}\" style=\"width:{divWidth}; height:{divHeight};\" class=\"defaultImg \" onmouseup=\"resizeImage(this)\">" +
                    $"{System.Environment.NewLine}{System.Environment.NewLine}" +
                    $"{divContainsImage}" +
                    $"{System.Environment.NewLine}{System.Environment.NewLine}" +
                    $"</div>";
                var newDivContainer = $"\r\n<div {classForDivContainer} {styleTopDivContainer} >";
                var newDivToolbar = $"\r\n<div id=\"{guidToDisplayToolbar}\" onmouseenter=\"showImageToolbar('{guidToDisplayToolbar}')\" onmouseleave=\"hideImageToolbar('{guidToDisplayToolbar}')\" style=\" display:none;\">"; 
                var newButtonForResize = linkHash != null ?  $"\r\n<button onclick =\"activateResize('{linkHash}')\" class=\"btn btn-md btn-primary-outline\"><img src=\"/assets/resize.png\"/></button>"
                    :string.Empty;
                var newButtonForMove = linkHash !=null? $"\r\n<button onclick=\"activateMove(this,'{linkHash}','{guidToDisplayToolbar}')\"  class=\"btn btn-md btn-primary-outline\"><img src=\"/assets/move.png\"/></button>"
                    :string.Empty;
                var newButtonForDynamicPlantuml = isPlantumlDynamic ? 
                    $@"<button id=""forwardArrow{stringMatchedHash}"" data-step=""1"" onclick=""presentationSVG('{prepareCurrentQueryRequest}','{stringMatchedHash}')"" class=""btn btn-md btn-primary-outline""><img src=""/assets/green_right_arrow.png""/></button>" :string.Empty;
                var newButtonForPlantumlCopy = isPlantumlCopy ? 
                    $@"<button alt=""copy into clipboard"" onclick = ""copyToClipboard('/api/mdexplorer/{currentPng}', '{prepareCurrentQueryRequest}', '{stringMatchedHash}', 0)"" ><img src = ""/assets/clipboard.png""/></button>" : string.Empty;
                var newButtonEyes = $@"<button alt=""see original size"" onclick=""toggleSeeMe('{stringMatchedHash}')""><img src = ""/assets/eyes.png""/></button>";
                var endDivForToolbar = "</div>";
                var endDivContainer = $"</div>";
                divContainsImage = string.Concat(newDivContainer,
                                                    newDivToolbar,
                                                         newButtonForResize, 
                                                         newButtonForMove,
                                                         newButtonForDynamicPlantuml,
                                                         newButtonForPlantumlCopy,
                                                         newButtonEyes,
                                                    endDivForToolbar, 
                                                    divContainsImage, 
                                                endDivContainer);
                markdown = markdown.Replace(itemImg.Groups[0].Value, divContainsImage);

            }

            return markdown;
        }
       
    }
}
