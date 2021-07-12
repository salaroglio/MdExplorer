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
using PlantUml.Net;
using System.Security.Cryptography;
using MdExplorer.Features.Utilities;

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
        private readonly PlantumlServer _plantumlServer;
        private readonly Helper _helper;

        public int Priority { get; set; } = 20;
        public string Name { get; set; } = "FromPlantumlToPng";
        public FromPlantumlToPng(string ServerAddress, 
                ILogger<FromPlantumlToPng> logger, 
                ISession session,
                PlantumlServer plantumlServer,
                Helper helper)
        {
            _serverAddress = ServerAddress;
            _logger = logger;
            _session = session;
            _plantumlServer = plantumlServer;
            _helper = helper;
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"```plantuml([^```]*)```",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }


        public MatchCollection GetMatchesAfterConversion(string html)
        {
            Regex rx = new Regex(@"<img src=\""[^md]*\.md/([^\.png]*).png",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(html);
            return matches;
        }

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {            
            var matches = GetMatchesAfterConversion(html);
            string backPath = _helper.GetBackPath(requestInfo);

            foreach (Match item in matches)
            {
                var stringMatched0 = item.Groups[1].Value;
                var referenceUrl = $"<img src=\"{backPath.Replace("\\","/")}/.md/{stringMatched0}.png";                
                html = html.Replace(item.Groups[0].Value, referenceUrl);
            }
            
            return html;
        }

        /// <summary>
        /// La funzione chiama il server plantuml
        /// che gli restituisce una immagine png, 
        /// fa l'hash, lo salva come png dentro una directory .Md, 
        /// e mette riferimento in markdown al png
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + "\\.md");
            string backPath = _helper.GetBackPath(requestInfo);

            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                var textHash = HashString(text, Encoding.UTF8);
                var filePath = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{textHash}.png"; //text.GetHashCode()
                if (!File.Exists(filePath))
                {
                    var taskSvg = _plantumlServer.GetSVGFromJar(text);
                    taskSvg.Wait();
                    var res = taskSvg.Result;
                    File.WriteAllBytes(filePath, res);
                    _logger.LogInformation($"write file: {filePath}");
                }

                var markdownFilePath = $"{backPath}\\.md\\{textHash}.png";
                var referenceUrl = $@"![]({markdownFilePath.Replace("\\", "/")})";
                _logger.LogInformation(referenceUrl);
                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
                //File.WriteAllText(filePath + "test.md", markdownTest);
            }
            return markdown;
        }

        

            


        //private async Task<byte[]> GetSVG(string plantumlCode)
        //{
        //    var comment = plantumlCode;

        //    var formContent = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("text", comment),
        //    });

        //    var myHttpClient = new HttpClient();

        //    var settingDal = _session.GetDal<Setting>();
        //    var plantumlServer = settingDal.GetList().Where(_ => _.Name == "PlantumlServer").FirstOrDefault()?.ValueString;

        //    var response = await myHttpClient.PostAsync($"http://{plantumlServer}:8080/form", formContent);//_options.Value.PlantumlServer
        //    var content = await response.Content.ReadAsStringAsync();
        //    HtmlDocument mydoc = new HtmlDocument();
        //    mydoc.LoadHtml(content);
        //    var url = mydoc.DocumentNode.SelectSingleNode(@"//input[@name='url']").Attributes["value"].Value;
        //    //var urls = mydoc.DocumentNode.SelectNodes(@"//a");
        //    //url = urls[0].Attributes["href"].Value;

        //    response = await myHttpClient.GetAsync(url);
        //    content = await response.Content.ReadAsStringAsync();

        //    return await response.Content.ReadAsByteArrayAsync();

        //}

        public string HashString(string value, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            byte[] bytes = encoding.GetBytes(value);
            byte[] hash = Hash(bytes);
            string result = String(hash);
            return result;
        }

        public string String(byte[] hash)
        {
            /*https://stackoverflow.com/questions/1300890/
              md5-hash-with-salt-for-keeping-password-in-db-in-c-sharp*/
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));     /*do not make it X2*/
            }
            var result = sb.ToString();
            return result;
        }

        public byte[] Hash(byte[] value)
        {
            /*https://support.microsoft.com/en-za/help/307020/
              how-to-compute-and-compare-hash-values-by-using-visual-cs*/
            /*https://andrewlock.net/why-is-string-gethashcode-
              different-each-time-i-run-my-program-in-net-core*/
            byte[] result = MD5.Create().ComputeHash(value);
            return result;
        }
    }
}
