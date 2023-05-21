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

namespace MdExplorer.Features.Commands.pdf
{
    public class MDShowMDPdf : MDShowMD, ICommandPdf
    {
        private readonly IHelper _helper;

        public MDShowMDPdf(string ServerAddress, ILogger<MDShowMDPdf> logger, IHelper helper) : base(ServerAddress, logger)
        {
            _helper = helper;
        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
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
                    
                }
                else if (item.Groups[1].Value.StartsWith("/"))
                {
                    fileName = item.Groups[1].Value.Remove(0, 1);
                }

                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    fileName = _helper.NormalizePath(fileName);
                    var queryEncoded = HttpUtility.UrlEncode(fileName);

                    var uriUrl = new Uri($@"{_serverAddress}/api/mdcreatemd/{queryEncoded}");
                    _logger.LogInformation($"looking for: {uriUrl.AbsoluteUri}");
                    var response = httpClient.GetAsync(uriUrl);
                    response.Wait();
                    if (response.IsCompletedSuccessfully)
                    {
                        var result = response.Result;
                        var taskRead = result.Content.ReadAsStringAsync();
                        taskRead.Wait();
                        var allElementToReplace = item.Groups[0].Value;
                        markdown = markdown.Replace(allElementToReplace, taskRead.Result);
                    }
                }
            }
            return markdown;
        }
    }
}
