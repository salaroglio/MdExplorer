using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.Markdown
{
    public class FromEmojiToDynamicProcessMD : FromEmojiToDynamicProcess, ICommandMD
    {
        private Dictionary<string, string> EmojiContextDictionary = new Dictionary<string, string>() {            
            // Process
            {"ℹ️",":information_source:" },
            {"✔️",":heavy_check_mark:" },
            {"🆗",":ok:" },
            {"⚠️",":warning:" },
            {"🚧",":construction:" },
        };

        public FromEmojiToDynamicProcessMD(ILogger<FromEmojiToPng> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }

        public string ReplaceSingleItem(string markdown, RequestInfo requestinfo, string toReplace, int index)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
            var currentIncrement = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                if (i==index)
                {
                    var item = matches[i];
                    var replaceWith = $@"{EmojiContextDictionary[toReplace]}";
                    var currentIndex = item.Index + currentIncrement;
                    stringToReturn = stringToReturn.Remove(currentIndex, item.Groups[0].Value.Length).Insert(currentIndex, replaceWith);
                    currentIncrement += replaceWith.Length - item.Groups[0].Value.Length;
                    
                }
            }

            return stringToReturn;
        }
    }
}
