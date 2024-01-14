using ExCSS;
using Markdig;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace MdExplorer.Features.Commands.html
{
    public class MDShowH2Html : MDShowH2, ICommandHtml
    {
        private readonly IHelper _helper;

        private string[] colorsArray = new[] { "red", "green", "yellow", "brown", "black" };

        public MDShowH2Html(string ServerAddress, ILogger<MDShowH2Html> logger, IHelper helper) : base(ServerAddress, logger)
        {
            _helper = helper;
        }

        private MatchCollection GetMatchesH2(string markdown)
        {
            Regex rx = new Regex(@"## ([^\n])*((?!\n## )(?!\n# ).)*",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        override public string TransformAfterConversion(string markdown, RequestInfo requestInfo)
        {
            // Devo cercare dentro markdown il comando MDShowH2Html(pathfile)
            // potrei usare le regular expression
            // Devo agganciare la parola "MDShowH2Html(xxx, title of interest)"
            // ed in più devo prendere xxx e il nome del paragrafo

            var matches = GetMatches(markdown);

            var matchCounter = 0;
            var currentIncrement = 0;
            foreach (Match item in matches)
            {
                if (matchCounter < matches.Count) 
                {
                    matchCounter++;
                    var fileName = item.Groups[1].Value;
                    if (item.Groups[1].Value.StartsWith("../") || item.Groups[1].Value.StartsWith("./"))
                    {

                        // here you should compose the path adding missing part
                        // the missing part is the distance from the root folder and the current file
                        // you can build this using requestInfo.currentqueryrequest
                        var listOfItem = requestInfo.CurrentQueryRequest.Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries).ToList();
                        listOfItem.RemoveAt(listOfItem.Count - 1);
                        var currentWebFolder = string.Empty;
                        foreach (var item1 in listOfItem)
                        {
                            if (item1 == listOfItem.First())
                            {
                                currentWebFolder = item1;
                            }
                            else
                            {
                                currentWebFolder += Path.DirectorySeparatorChar + item1;
                            }
                        }

                        currentWebFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                        fileName = currentWebFolder + Path.DirectorySeparatorChar + item.Groups[1].Value.Replace('/', Path.DirectorySeparatorChar);


                        fileName = _helper.NormalizePath(fileName);
                    }
                    else if (item.Groups[1].Value.StartsWith("/"))
                    {
                        fileName = item.Groups[1].Value.Remove(0, 1);
                    }
                    var queryEncoded = fileName.Replace("\\", "/")+item.Groups[3].Value;// HttpUtility.UrlEncode(fileName);
                    var httpClientHandler = new HttpClientHandler();
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                    {
                        return true;
                    };

                    // now we know where to search for the second instance of the command

                    var markdownTxt = string.Empty;
                    var rootPathSystem = requestInfo.CurrentRoot;
                    var filePathSystem = string.Concat(rootPathSystem, Path.DirectorySeparatorChar, fileName.Replace('/', Path.DirectorySeparatorChar));

                    using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        markdownTxt = sr.ReadToEnd();
                    }
                    var matchesH2 = GetMatchesH2(markdownTxt);

                    var fullPathFileMdToRender = string.Empty;

                    foreach (Match matchH1 in matchesH2)
                    {
                        var title = string.Join(string.Empty, matchH1.Groups[1].Captures).Replace("\r", string.Empty);

                        string pattern = @":[^:]*:";
                        var titleFromDoc = Regex.Replace(title, pattern, string.Empty);

                        if (titleFromDoc.Trim() == item.Groups[2].Value.Trim())
                        {
                            var allContent = matchH1.Groups[0].Value;
                            // hash, and save
                            var textHash = _helper.GetHashString(matchH1.Groups[0].Value, Encoding.UTF8);
                            fullPathFileMdToRender = rootPathSystem + Path.DirectorySeparatorChar + ".md" + Path.DirectorySeparatorChar + textHash + ".md";
                            File.WriteAllText(fullPathFileMdToRender, allContent);
                            break;
                        }
                    }
                    fileName = fullPathFileMdToRender.Replace(rootPathSystem,string.Empty);

                    using (var httpClient = new HttpClient(httpClientHandler))
                    {
                        requestInfo.Recursionlevel++;
                        requestInfo.RelativePathFile = fileName;
                        var uriUrl = new Uri($@"{_serverAddress}/api/mdexplorer2");
                        
                        Regex rxSharp = new Regex(@"([^#]*)(?:(#.*))?",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        var matchesSharp = rxSharp.Matches(queryEncoded);

                        var firstPart = matchesSharp.First().Groups[1].Value;
                        var secondPart = matchesSharp.First().Groups[2]?.Value;

                        var stringURI = $@"{_serverAddress}/api/mdexplorer/{firstPart}" + "?connectionId=" + requestInfo.ConnectionId + secondPart;

                        var uriUrlRoot = new Uri(stringURI);

                        _logger.LogInformation($"looking for: {uriUrl.AbsoluteUri}");
                        //var response = httpClient.GetAsync(uriUrl);
                        var payload = Newtonsoft.Json.JsonConvert.SerializeObject(requestInfo);
                        HttpContent c = new StringContent(payload, Encoding.UTF8, "application/json");
                        var response = httpClient.PostAsync(uriUrl, c);
                        response.Wait();

                        if (response.IsCompletedSuccessfully)
                        {
                            var result = response.Result;
                            var taskRead = result.Content.ReadAsStringAsync();
                            taskRead.Wait();
                            var rnd = new Random();
                            var numberedColor = rnd.Next(0, 4);

                            var selectedColor = colorsArray[numberedColor];
                            XmlDocument doc1 = new XmlDocument();
                            var tagStyle = @"<style>
                                #overlay" + numberedColor + @" {
                                  position:relative;
                                  top: 0;
                                  left: 0;
                                  right: 0;
                                  bottom: 0;
                                  border-left:dashed;
                                  border-color:" + selectedColor + @";
                                  /*background-color: lightgreen;  Black background with opacity */
                                  z-index: 2; /* Specify a stack order in case you're using a different order for other elements */
                                  cursor: pointer; /* Add a pointer on hover */
                                }
                                /*Important:*/
                                .link-spanner{
                                  position:absolute; 
                                  width:100%;
                                  height:100%;
                                  top:0;
                                  left: 0;
                                  z-index: 1;

                                  /* edit: fixes overlap error in IE7/8, 
                                     make sure you have an empty gif 
                                  background-image: url('empty.gif');*/
                                }  
                                </style>" + "\r\n";

                            var nodeDiv = doc1.CreateElement("div");
                            nodeDiv.InnerXml = taskRead.Result;

                            var attributeId = doc1.CreateAttribute("id");
                            attributeId.Value = "overlay" + numberedColor;
                            nodeDiv.Attributes.Append(attributeId);

                            var nodeA = doc1.CreateElement("a");
                            var attraHref = doc1.CreateAttribute("href");
                            attraHref.Value = uriUrlRoot.AbsoluteUri.Replace("127.0.0.1", "localhost"); // WorkAraound bug in Avalonia CefGlue

                            nodeA.Attributes.Append(attraHref);
                            nodeDiv.AppendChild(nodeA);

                            var nodeSpan = doc1.CreateElement("span");
                            var attrTitle = doc1.CreateAttribute("title");
                            attrTitle.Value = fileName;
                            nodeSpan.Attributes.Append(attrTitle);
                            var spanClass = doc1.CreateAttribute("class");
                            spanClass.Value = "link-spanner";
                            nodeSpan.Attributes.Append(spanClass);
                            nodeA.AppendChild(nodeSpan);

                            var stringToReplace = nodeDiv.OuterXml;
                            var allElementToReplace = item.Groups[0].Value;

                            (markdown, currentIncrement) = ManageReplaceOnMD(markdown, currentIncrement, item, stringToReplace);

                            markdown =  markdown + tagStyle;
                        }
                    }
                }
                else { break; }
            }


            return markdown;
        }


    }
}
