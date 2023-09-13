using DocumentFormat.OpenXml.Bibliography;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace MdExplorer.Features.Commands
{
    public class EditH1 : CommandBase, ICommand
    {
        public int Priority { get; set; } = 10;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "EditH1";



        public EditH1(ILogger<EditH1> logger)
        {

        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"(#{1,}) ([^\n])*((?!\n#).)*",
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
            if (requestInfo.Recursionlevel == 0) // only at root level
            {
                var dom = new XmlDocument();
                html = string.Concat(@"<mdroot>", html, @"</mdroot>");
                dom.LoadXml(html);
                //var h1tags = dom.SelectNodes("//h1");
                var h1tags = dom.SelectNodes("//h1");
                var h2tags = dom.SelectNodes("//h2");
                var h3tags = dom.SelectNodes("//h3");
                var h4tags = dom.SelectNodes("//h4");
                var h5tags = dom.SelectNodes("//h5");
                var h6tags = dom.SelectNodes("//h6");


                var mdText = File.ReadAllText(requestInfo.AbsolutePathFile);
                var matches = GetMatches(mdText).ToList();

                SetHTag(dom, h1tags, matches, 1);
                SetHTag(dom, h2tags, matches, 2);
                SetHTag(dom, h3tags, matches, 3);
                SetHTag(dom, h4tags, matches, 4);


                

                html = Beautify(dom);
                html = html.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
                html = html.Replace("<mdroot>", string.Empty).Replace("</mdroot>", string.Empty);
                
                //SetHTag(dom, h5tags, matches, 5);
                //html = SetHTag(dom, h6tags, matches, 6);
            }



            return html;
        }

        private void SetHTag(XmlDocument dom, XmlNodeList hTags,
            List<Match> matches, int level)
        {
            string html;
            foreach (XmlElement h1Tag in hTags)
            {
                foreach (Match itemMatch in matches.Where(_ => _.Groups[1].Value.Length == level))
                {
                    var titleType = itemMatch.Groups[1].Value;
                    var title = string.Join(string.Empty, itemMatch.Groups[2].Captures).Replace("\r", string.Empty);
                    var content = string.Join(string.Empty, itemMatch.Groups[0].Captures);
                    if (title.Trim() == h1Tag.InnerText)
                    {
                        // create hook
                        // you should create the new <div>
                        // create a copy of the current  and at the same time
                        // delete the current
                        var divEncapsulator = dom.CreateElement("div");
                        var h1Clone = (XmlElement)h1Tag.CloneNode(true);
                        divEncapsulator.SetAttribute("onclick", $"editH1({itemMatch.Index})");
                        divEncapsulator.SetAttribute("style", "border:2px solid blue; cursor:pointer;");
                        divEncapsulator.SetAttribute("class", "editorH1");
                        divEncapsulator.AppendChild(h1Clone);

                        h1Clone.SetAttribute("md-itemMatchIndex", itemMatch.Index.ToString());


                        while (h1Tag.NextSibling != null
                               && !h1Tag.NextSibling.Name.StartsWith("h")
                                && (h1Tag.NextSibling.Name != "div"
                                     || 
                                    !(h1Tag.NextSibling.Attributes.GetNamedItem("class") != null
                                        && h1Tag.NextSibling.Attributes.GetNamedItem("class").InnerText.Contains("editorH1")
                                    )                                                                                      
                                   )
                                && !(h1Tag.NextSibling.Name == "p" 
                                    && h1Tag.NextSibling.InnerText.Contains("MdShowMd"))
                               )
                        {
                            var tagToDestroy = h1Tag.NextSibling;
                            var tagCloneToAdd = tagToDestroy.CloneNode(true);
                            divEncapsulator.AppendChild(tagCloneToAdd);
                            tagToDestroy.ParentNode.RemoveChild(tagToDestroy);
                        }
                        h1Tag.ParentNode.ReplaceChild(divEncapsulator, h1Tag);
                        // Inject hidden text
                        var hiddenInput = dom.CreateElement("div");
                        hiddenInput.SetAttribute("style", "display:none");
                        hiddenInput.SetAttribute("md-itemMatchIndex", itemMatch.Index.ToString());
                        hiddenInput.InnerText = content;
                        dom.DocumentElement.AppendChild(hiddenInput);
                        break;
                    }
                }
            }

            //return dom;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        public string Beautify(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }
    }


}
