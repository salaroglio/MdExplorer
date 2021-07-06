using MdExplorer.Features.Interfaces;
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
        public MDShowMDPdf(string ServerAddress, ILogger<MDShowMDPdf> logger) : base(ServerAddress, logger)
        {
        }

        public override string TransformInNewMDFromMD(string markdown)
        {
            var matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
                var fileName = item.Groups[1].Value;
                var allElementToReplace = item.Groups[0].Value;
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var queryEncoded = HttpUtility.UrlEncode(fileName);

                    var uriUrl = new Uri($@"{_serverAddress}/api/mdexplorer/{queryEncoded}");
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
