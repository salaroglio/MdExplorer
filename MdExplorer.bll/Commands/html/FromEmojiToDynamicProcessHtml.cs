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
    /// <summary>
    /// Manage emoji for process and priority, inherits FromEmojiToPng match
    /// </summary>
    public class FromEmojiToDynamicProcessHtml : FromEmojiToDynamicProcess, ICommandHtml
    {
        private Dictionary<string, string> EmojiContextDictionary = new Dictionary<string, string>() {           
            // Process
            {":information_source:","dynamicEmojiForProcess" },
            {":heavy_check_mark:","dynamicEmojiForProcess" },
            {":ok:","dynamicEmojiForProcess" },
            {":warning:","dynamicEmojiForProcess" },
            {":construction:","dynamicEmojiForProcess" },
        };

        public FromEmojiToDynamicProcessHtml(ILogger<FromEmojiToDynamicProcessHtml> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }
        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
            var currentIncrement = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                var item = matches[i];
                var text = item.Groups[0].Value;
                // Gestione degli emoji per processo e priorità
                EmojiContextDictionary.TryGetValue(text, out var found);
                if (found != null)
                {
                    var raplaceWith = $@"<span id=""emoji{i}"" style=""cursor: pointer"" onclick=""{found}(this,{i},'{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}')""> {text}</span> ";
                    (stringToReturn,currentIncrement) = ManageReplaceOnMD( stringToReturn,  currentIncrement, item, raplaceWith);
                }               
            }
            
            return stringToReturn;
        }

        private  (string,int) ManageReplaceOnMD(string stringToReturn, int currentIncrement, Match item, string raplaceWith)
        {
            var currentIndex = item.Index + currentIncrement;
            stringToReturn = stringToReturn.Remove(currentIndex, item.Groups[0].Value.Length).Insert(currentIndex, raplaceWith);
            currentIncrement += raplaceWith.Length - item.Groups[0].Value.Length;
            return (stringToReturn, currentIncrement);
        }



    }
}
