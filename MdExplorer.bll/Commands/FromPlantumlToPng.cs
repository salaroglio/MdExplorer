using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Questa clase comando, trasforma il testo contenuto dentro il markdown che si riferisce
    /// al codice plantuml, 
    /// Devo fare la gestione della session, in particolare la dispose, perché altrimenti non mi cancello la sessione
    /// </summary>
    public class FromPlantumlToPng : ICommand, IDisposable
    {
        private readonly string _serverAddress;
        private readonly ILogger<FromPlantumlToPng> _logger;
        private readonly ISession _session;

        public FromPlantumlToPng(string ServerAddress, ILogger<FromPlantumlToPng> logger, ISession session)
        {
            _serverAddress = ServerAddress;
            _logger = logger;
            _session = session;
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"```plantuml([^```]*)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public virtual string TransformAfterConversion(string text)
        {
            // Nothing to do
            return text;
        }

        /// <summary>
        /// La funzione chiama il server plantuml
        /// che gli restituisce una immagine png, 
        /// fa l'hash, lo salva come png dentro una directory .Md, 
        /// e mette riferimento in markdown al png
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public string TransformInNewMDFromMD(string markdown)
        {
            // Crea la directory
            
            var directoryInfo = Directory.CreateDirectory(".Md");

            var matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;

                var taskSvg = GetSVG(text);
                taskSvg.Wait();

                var res = taskSvg.Result;

                File.WriteAllBytes($"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{text.GetHashCode()}.png", res);
            }
           
            return markdown;

        }

        private async Task<byte[]> GetSVG(string plantumlCode)
        {
            var comment = plantumlCode;

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", comment),
            });

            var myHttpClient = new HttpClient();

            var settingDal = _session.GetDal<Setting>();
            var plantumlServer = settingDal.GetList().Where(_ => _.Name == "PlantumlServer").FirstOrDefault()?.ValueString;

            var response = await myHttpClient.PostAsync($"http://{plantumlServer}:8080/form", formContent);//_options.Value.PlantumlServer
            var content = await response.Content.ReadAsStringAsync();
            HtmlDocument mydoc = new HtmlDocument();
            mydoc.LoadHtml(content);
            var url = mydoc.DocumentNode.SelectSingleNode(@"//input[@name='url']").Attributes["value"].Value;
            //var urls = mydoc.DocumentNode.SelectNodes(@"//a");
            //url = urls[0].Attributes["href"].Value;

            response = await myHttpClient.GetAsync(url);
            content = await response.Content.ReadAsStringAsync();            
            
            return await response.Content.ReadAsByteArrayAsync();

        }
    }
}
