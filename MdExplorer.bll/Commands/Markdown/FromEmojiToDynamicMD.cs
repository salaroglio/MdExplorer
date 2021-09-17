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
    public class FromEmojiToDynamicMD : FromEmojiToPng, ICommandMD
    {
        private Dictionary<string, string> EmojiContextDictionary = new Dictionary<string, string>() {
            // Priority
            {"❔",":question:" },
            {"❗",":exclamation:" },
            {"❕",":grey_exclamation:" },
            {"❓",":grey_question:" },
            {"⛔",":no_entry:" },
            {"❌",":x:" },
            // Process
            {"ℹ️",":information_source:" },
            {"✔️",":heavy_check_mark:" },
            {"🆗",":ok:" },
            {"⚠️",":warning:" },
            {"🚧",":construction:" },
        };

        public FromEmojiToDynamicMD(ILogger<FromEmojiToPng> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }

        public string ReplaceSingleItem(string markdown, RequestInfo requestinfo, string toReplace, int index)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
            var currentIncrement = 0;

            for (int i = 0; i < matches.Count-1; i++)
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

            //foreach (var item in matches.Where(_ => _.Index == index))
            //{

            //    var replaceWith = $@"{EmojiContextDictionary[toReplace]}";
            //    //stringToReturn = stringToReturn.Replace(item.Groups[0].Value, replaceWith);
            //    stringToReturn = stringToReturn.Remove(index,item.Groups[0].Value.Length).Insert(index, replaceWith);
            //    newIndex = index + replaceWith.Length;
            //}
            return stringToReturn;
        }
    }
}
