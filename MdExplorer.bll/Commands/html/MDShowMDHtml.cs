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
    public class MDShowMDHtml : MDShowMD, ICommandHtml
    {
        private readonly IHelper _helper;

        public MDShowMDHtml(string ServerAddress, ILogger<MDShowMDHtml> logger, IHelper helper) : base(ServerAddress, logger)
        {
            _helper = helper;
        }

        override public string TransformAfterConversion(string markdown, RequestInfo requestInfo)
        {
            // Devo cercare dentro markdown il comando m↓ShowMd(pathfile)
            // potrei usare le regular expression
            // Devo agganciare la parola "m↓ShowMd(xxx)"
            // ed in più devo prendere xxx

            var matches = GetMatches(markdown);

            foreach (Match item in matches)
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
                var fileName = currentWebFolder + Path.DirectorySeparatorChar + item.Groups[1].Value.Replace('/', Path.DirectorySeparatorChar);
                var allElementToReplace = item.Groups[0].Value;
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    fileName = _helper.NormalizePath(fileName);
                    var queryEncoded = HttpUtility.UrlEncode(fileName);

                    var uriUrl = new Uri($@"{_serverAddress}/api/mdexplorer/{fileName}");
                    _logger.LogInformation($"looking for: {uriUrl.AbsoluteUri}");
                    var response = httpClient.GetAsync(uriUrl);
                    response.Wait();

                    if (response.IsCompletedSuccessfully)
                    {
                        var result = response.Result;
                        var taskRead = result.Content.ReadAsStringAsync();
                        taskRead.Wait();

                        XmlDocument doc1 = new XmlDocument();
                        var tagStyle = @"<style>
                                #overlay {
                                  position:relative;
                                  top: 0;
                                  left: 0;
                                  right: 0;
                                  bottom: 0;
                                  border:solid;
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
                        attributeId.Value = "overlay";
                        nodeDiv.Attributes.Append(attributeId);
                        //nodeDiv.AppendChild(styleNode);

                        var nodeA = doc1.CreateElement("a");
                        var attraHref = doc1.CreateAttribute("href");
                        attraHref.Value = uriUrl.AbsoluteUri;

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
                        markdown = tagStyle + markdown.Replace(allElementToReplace, stringToReplace);
                    }
                }

            }


            return markdown;
        }

    }
}
