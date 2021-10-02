using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class FromEmojiToDynamicPriority : CommandBase, ICommand, IDisposable
    {
        protected readonly ILogger<FromEmojiToDynamicPriority> _logger;
        protected readonly IServerCache _serverCache;

        public int Priority { get; set; } = 20;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromEmojiToDynamicPriority";

        public FromEmojiToDynamicPriority(ILogger<FromEmojiToDynamicPriority> logger, IServerCache serverCache)
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
            Regex rx = new Regex(@":(question|exclamation|grey_exclamation|grey_question|no_entry|x|negative_squared_cross_mark):",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            //Nothing to do
            return markdown;
        }

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            //Nothing to do
            return html;
        }

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                if (_serverCache.Emojies.Select(_ => _.Replace(".png", string.Empty)).Contains(text))
                {
                    var raplaceWith = $@"![](.md\EmojiForPandoc\{text}.png)";
                    stringToReturn = stringToReturn.Replace(item.Groups[0].Value, raplaceWith);
                }
            }
            return stringToReturn;
        }
    }
}
