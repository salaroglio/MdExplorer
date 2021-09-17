using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.html
{
    public class FromEmojiToDynamicHtml : FromEmojiToPng, ICommandHtml
    {
        private Dictionary<string, string> EmojiContextDictionary = new Dictionary<string, string>() {
            // Priority
            {":question:","dynamicEmojiForPriority" },
            {":exclamation:","dynamicEmojiForPriority" },
            {":grey_exclamation:","dynamicEmojiForPriority" },
            {":grey_question:","dynamicEmojiForPriority" },
            {":no_entry:","dynamicEmojiForPriority" },
            {":x:","dynamicEmojiForPriority" },
            // Process
            {":information_source:","dynamicEmojiForProcess" },
            {":heavy_check_mark:","dynamicEmojiForProcess" },
            {":ok:","dynamicEmojiForProcess" },
            {":warning:","dynamicEmojiForProcess" },
            {":construction:","dynamicEmojiForProcess" },
        };

        public FromEmojiToDynamicHtml(ILogger<FromEmojiToPng> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }
        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
            var currentIncrement = 0;
            for (int i = 0; i < matches.Count-1; i++)
            {
                var item = matches[i];
                var text = item.Groups[0].Value;
                EmojiContextDictionary.TryGetValue(text, out var found);
                if (found != null)
                {
                    var raplaceWith = $@"<span style=""cursor: pointer"" onclick=""{found}(this,{i},'{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}')""> {text}</span> ";
                    var currentIndex = item.Index + currentIncrement;
                    stringToReturn = stringToReturn.Remove(currentIndex, item.Groups[0].Value.Length).Insert(currentIndex, raplaceWith);
                    currentIncrement += raplaceWith.Length - item.Groups[0].Value.Length;
                }
            }

            //foreach (Match item in matches)
            //{
            //    var text = item.Groups[0].Value;
            //    EmojiContextDictionary.TryGetValue(text, out var found);
            //    if (found != null)
            //    {
            //        var raplaceWith = $@"<span style=""cursor: pointer"" onclick=""{found}(this,{item.Index},'{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar,'/')}')""> {text}</span> ";
            //        stringToReturn = stringToReturn.Replace(item.Groups[0].Value, raplaceWith);
            //    }
            //}
            
            return stringToReturn;
        }

        


    }
}
