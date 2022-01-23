using ExCSS;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.FunctionParameters;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.Markdown
{
    public class CSSSavedOnPageMD : CSSSavedOnPage, ICommandMD, IReplaceSingleItemMD<CSSSavedOnPageInfo, CSSSavedOnPageInfo>
    {
        public CSSSavedOnPageMD(Microsoft.Extensions.Logging.ILogger<CSSSavedOnPage> logger, Utilities.IHelper helper) :
            base(logger, helper)
        {
        }

        /// <summary>
        /// It search for CSSInPage using linkHash and CSSHash elements inside markdwon
        /// then it writes the width and heigth of image inside the pseudo CSS
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="requestinfo"></param>
        /// <param name="cSSSavedOnPageInfo"></param>
        /// <returns></returns>
        public (string, CSSSavedOnPageInfo) ReplaceSingleItem(string markdown, RequestInfo requestinfo, CSSSavedOnPageInfo cSSSavedOnPageInfo)
        {
            var stringToReturn = markdown;
            var cssSerialized = string.Empty;
            var cssMatches = GetMatches(markdown);
            var imgMatches = GetLinkWithCurlyBracketsMatches(markdown);

            foreach (Match itemImg in imgMatches)
            {
                
                var curlyBrackets = itemImg.Groups[3].Value;
                var metadataMatch = GetMetaDataMatches(curlyBrackets);
                var classes = metadataMatch.Where(_ => _.Groups[1].Value.StartsWith(".")).FirstOrDefault();
                var linkHash = _helper.GetHashString(classes.Value, Encoding.UTF8);
                if (linkHash == cSSSavedOnPageInfo.LinkHash) // found!
                {
                    var foundCSS = false;
                    foreach (Match itemCSS in cssMatches)
                    {
                        var cssToSave = itemCSS.Groups[1].Value;
                        var cssHash = _helper.GetHashString(itemCSS.Groups[0].Value, Encoding.UTF8);
                        if (cssHash == cSSSavedOnPageInfo.CSSHash) // Youpi! Found!
                        {
                            var parser = new StylesheetParser();
                            var stylesheet = parser.Parse(cssToSave);
                            foreach (var rule in stylesheet.StyleRules)
                            {
                                var selector = rule.SelectorText; // Yields .someClass
                                if (selector == classes.Value)
                                {
                                    rule.Style.Width = $"{cSSSavedOnPageInfo.Width}px";
                                    rule.Style.Height = $"{cSSSavedOnPageInfo.Height}px";
                                    rule.Style.Top = $"{cSSSavedOnPageInfo.ClientY}px";
                                    rule.Style.Left = $"{cSSSavedOnPageInfo.ClientX}px";
                                    rule.Style.Position = cSSSavedOnPageInfo.Position;

                                    var cssToReplace = string.Concat(System.Environment.NewLine, stylesheet.ToCss(new CompressedStyleFormatter()), System.Environment.NewLine);
                                    stringToReturn = markdown.Replace(cssToSave, cssToReplace);
                                    cssSerialized = string.Concat("```CSSInline", cssToReplace, "```");
                                    foundCSS = true;
                                    break; // exit from foreach serching for CSS classes
                                }

                               
                               
                            }
                        }


                    }

                    if (cssMatches.Count == 0) // first time ever writing cssinpage
                    {
                        var CSSToInsert = string.Concat("```CSSInline", Environment.NewLine);
                        CSSToInsert = GetCSS(cSSSavedOnPageInfo, classes, CSSToInsert);
                        CSSToInsert = string.Concat(CSSToInsert, System.Environment.NewLine, "```");
                        cssSerialized = CSSToInsert;
                        stringToReturn = string.Concat(markdown, CSSToInsert);
                    }
                    if (cssMatches.Count>0 && !foundCSS) // cssinpage allready exits 
                    {
                        var CSSToInsert = string.Concat( cssMatches[0].Groups[1].Value, Environment.NewLine);
                        CSSToInsert = GetCSS(cSSSavedOnPageInfo, classes, CSSToInsert);
                        CSSToInsert = string.Concat(CSSToInsert, Environment.NewLine);
                        cssSerialized = string.Concat("```CSSInline", CSSToInsert, "```");
                        stringToReturn = markdown.Replace(cssMatches[0].Groups[1].Value, CSSToInsert);
                    }
                }
            }
            var cssToReturn = new CSSSavedOnPageInfo
            {
                CSSHash = _helper.GetHashString(cssSerialized, Encoding.UTF8)
            };

            return (stringToReturn, cssToReturn);

            static string GetCSS(CSSSavedOnPageInfo cSSSavedOnPageInfo, Match classes, string CSSToInsert)
            {
                CSSToInsert += string.Concat(
                            System.Environment.NewLine,
                            classes.Value, " {", System.Environment.NewLine,
                            $"\twidth:{cSSSavedOnPageInfo.Width};", System.Environment.NewLine,
                            $"\theight:{cSSSavedOnPageInfo.Height};", System.Environment.NewLine,
                            "}", System.Environment.NewLine
                            );
                return CSSToInsert;
            }
        }
    }




}


