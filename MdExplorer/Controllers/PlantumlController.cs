using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlantumlController
    {
        [HttpGet]
        public async Task<ContentResult> GetAsync()
        {
            var comment = "@startuml\r\nBob -> Alice : hello\r\n@enduml";
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", comment),
            });

            var myHttpClient = new HttpClient();
            var response = await myHttpClient.PostAsync("http://www.plantuml.com/plantuml/form", formContent); //"http://172.25.74.116:8080/form"
            var content = await response.Content.ReadAsStringAsync();
            HtmlDocument mydoc = new HtmlDocument();
            mydoc.LoadHtml(content);
            var url = mydoc.DocumentNode.SelectSingleNode(@"//input[@name='url']").Attributes["value"].Value;
            var urls = mydoc.DocumentNode.SelectNodes(@"//a");
            url = urls[1].Attributes["href"].Value;

            response = await myHttpClient.GetAsync(url);
            content = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                ContentType = "text/html",
                Content = content
            };
        }

        public void Compress1(string toCompress)
        {
            using (var originalFileStream = GenerateStreamFromString(toCompress))
            {
                using (FileStream compressedFileStream = File.Create(@"c:\temp\test" + ".cmp"))
                {
                    using (DeflateStream compressionStream = new DeflateStream(compressedFileStream, CompressionLevel.Fastest))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public string Compress(string toCompress)
        {
            using (var originalStream = GenerateStreamFromString(toCompress))
            {

                using (var compressedStream = new MemoryStream())
                {
                    using (DeflateStream compressionStream = new DeflateStream(compressedStream, CompressionMode.Compress))
                    {
                        originalStream.CopyTo(compressionStream);
                        compressionStream.Flush();
                        var stremReader = new StreamReader(compressedStream);
                        var data = stremReader.ReadToEnd();
                        return data;
                    }
                }
            }
        }

        private string testDeflate(string toDeflate)
        {
            var toReturn = string.Empty;
            using (var stream = GenerateStreamFromString(toDeflate))
            {
                using (DeflateStream compressionStream = new DeflateStream(stream, CompressionMode.Compress))
                {
                    compressionStream.CopyTo(stream);
                    var value = compressionStream.ToString();
                }
            }

            return toReturn;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


       

    }



}
