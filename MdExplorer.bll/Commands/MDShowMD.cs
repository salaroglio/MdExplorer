using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MdExplorer.Features.Commands
{
    public class MDShowMD : ICommand
    {
        
        public MDShowMD(string ServerAddress)
        {
            _serverAddress = ServerAddress;
        }
        private readonly string _serverAddress;
        

        public string TransformInNewMDFromMD(string markdown)
        {
            // Devo cercare dentro markdown il comando m↓ShowMd(pathfile)
            // potrei usare le regular expression
            // Devo agganciare la parola "m↓ShowMd(xxx)"
            // ed in più devo prendere xxx

            MatchCollection matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
                var fileName = item.Groups[1].Value;
                var allElementToReplace = item.Groups[0].Value;
                using (var httpClient = new HttpClient())
                {
                    var queryEncoded = HttpUtility.UrlEncode(fileName);
                    var uriUrl = new Uri($@"{_serverAddress}/api/mdexplorer/{queryEncoded}");
                    var response = httpClient.GetAsync(uriUrl);
                    response.Wait();

                    if (response.IsCompletedSuccessfully)
                    {
                        var result = response.Result;
                        var taskRead = result.Content.ReadAsStringAsync();
                        taskRead.Wait();
                        var stringToReplace = taskRead.Result;
                        markdown = markdown.Replace(allElementToReplace, stringToReplace);
                    }
                }

            }


            return markdown;
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"m↓ShowMd\((.*)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markdown);
            return matches;
        }



    }
}
