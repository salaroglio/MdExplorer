using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
                // here you should compose the path adding missing part
                // the missing part is the distance from the root folder and the current file
                // you can build this using requestInfo.currentqueryrequest
                var listOfItem = requestInfo.CurrentQueryRequest.Split("\\", options: StringSplitOptions.RemoveEmptyEntries).ToList();
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
                        currentWebFolder += "\\" + item1;
                    }


                }

                currentWebFolder = string.Join("\\", listOfItem.ToArray());
                var fileName = currentWebFolder + "\\" + item.Groups[1].Value;
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

                    var uriUrl = new Uri($@"{_serverAddress}/api/mdcreatemd/{queryEncoded}");
                    _logger.LogInformation($"looking for: {uriUrl.AbsoluteUri}");
                    var response = httpClient.GetAsync(uriUrl);
                    response.Wait();
                    if (response.IsCompletedSuccessfully)
                    {
                        var result = response.Result;
                        var taskRead = result.Content.ReadAsStringAsync();
                        taskRead.Wait();
                        markdown = markdown.Replace(allElementToReplace, taskRead.Result);                        
                    }
                }
            }
            return markdown;
        }
    }
}
