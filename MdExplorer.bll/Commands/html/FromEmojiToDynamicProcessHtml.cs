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
        //private Dictionary<string, string> EmojiContextDictionary = new Dictionary<string, string>() {           
        //    // Process
        //    {":information_source:","dynamicEmojiForProcess" },
        //    {":heavy_check_mark:","dynamicEmojiForProcess" },
        //    {":ok:","dynamicEmojiForProcess" },
        //    {":warning:","dynamicEmojiForProcess" },
        //    {":construction:","dynamicEmojiForProcess" },
        //};

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
                //EmojiContextDictionary.TryGetValue(text, out var found);
                //if (found != null)
                //{
                    var currentIndex = $@"data-md-process-index=""{i}""";
                    var raplaceWith = $@"<span id=""emojiProcess{i}"" {currentIndex} style=""cursor: pointer"" onclick=""dynamicEmojiForProcess(this,{i},'{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}')""> {text}</span> ";
                    (stringToReturn,currentIncrement) = ManageReplaceOnMD( stringToReturn,  currentIncrement, item, raplaceWith);
                //}               
            }
            
            return stringToReturn;
        }

        



    }
}
