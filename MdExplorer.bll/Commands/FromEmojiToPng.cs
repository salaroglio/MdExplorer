using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class FromEmojiToPng : ICommand, IDisposable
    {
        private readonly ILogger<FromEmojiToPng> _logger;
        private readonly IServerCache _serverCache;

        public int Priority { get; set; } = 20;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromEmojiToPng";

        public FromEmojiToPng(ILogger<FromEmojiToPng> logger, IServerCache serverCache)
        {
            _logger = logger;
            _serverCache = serverCache;
        }

        public void Dispose()
        {
            //nothing to do
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@":([^:^ ]*):",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            //Nothing to do
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            //Nothing to do
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
                                    
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                if (_serverCache.Emojies.Select(_=>_.Replace(".png", string.Empty)).Contains(text))
                {
                    var raplaceWith = $@"![](.md\EmojiForPandoc\{text}.png)";
                    stringToReturn = stringToReturn.Replace(item.Groups[0].Value, raplaceWith);
                }
            }
            return stringToReturn;
        }
    }
}
