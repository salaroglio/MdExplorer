using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Class dedicated to prepare markdown/html behind the scenes.
    /// Maybe is obvious or maybe not, this is only a part of all code
    /// you need to know to implement "Command-Calendar" feature.
    /// In fact this is only a part of all you need to see in order to understand
    /// the complete feature. 
    /// Here you find out only the raw part html creation.
    /// The calendar feature is composed of javascript part and bootstrap-calendar part
    /// So, you have to integrate your knoledge with MdExplorerService javascript
    /// </summary>
    public class FromEmojiCalendarToDatepicker : ICommand, IDisposable
    {
        protected readonly ILogger<FromEmojiCalendarToDatepicker> _logger;
        protected readonly IServerCache _serverCache;

        public int Priority { get; set; } = 20;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromEmojiCalendarToDatepicker";

        public FromEmojiCalendarToDatepicker(ILogger<FromEmojiCalendarToDatepicker> logger, IServerCache serverCache)
        {
            _logger = logger;
            _serverCache = serverCache;
        }

        public void Dispose()
        {
            //nothing to do
        }

        /// <summary>
        /// Im looking for calendar and, eventually, a date expressed in yyyy-mm-dd format
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex("(:calendar:) ?([0-9]{4}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1]))?",
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

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
            var currentIncrement = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                var item = matches[i];
                var text = item.Groups[1].Value;                                
                // gestione :calendar:
                if (text == ":calendar:")
                {
                    var target = $"dateForCalendar{i}";
                    var source = $"sourceEmoji{i}";
                    var currentDate = item.Groups[2].Value;
                    var dateFormat = "yyyy-mm-dd";
                    var raplaceWith = $@"<span id=""{source}"" style=""cursor: pointer"" onclick=""activateCalendar(this,{i},'{target}','{dateFormat}','{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}')""> {text}</span><span id=""{target}"">{currentDate}</span> ";
                    (stringToReturn, currentIncrement) = ManageReplaceOnMD(stringToReturn, currentIncrement, item, raplaceWith);
                }
            }

            return stringToReturn;
        }

        private (string, int) ManageReplaceOnMD(string stringToReturn, int currentIncrement, Match item, string raplaceWith)
        {
            var currentIndex = item.Index + currentIncrement;
            stringToReturn = stringToReturn.Remove(currentIndex, item.Groups[0].Value.Length).Insert(currentIndex, raplaceWith);
            currentIncrement += raplaceWith.Length - item.Groups[0].Value.Length;
            return (stringToReturn, currentIncrement);
        }
    }
}
