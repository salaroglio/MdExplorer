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
using System.Xml;

namespace MdExplorer.Features.Commands.html
{
    public class FromEmojiToDynamicPriorityHtml : FromEmojiToDynamicPriority, ICommandHtml
    {
        //private Dictionary<string, string> EmojiContextDictionary = new Dictionary<string, string>() {
        //    // Priority
        //    {":question:","dynamicEmojiForPriority" },
        //    {":exclamation:","dynamicEmojiForPriority" },
        //    {":grey_exclamation:","dynamicEmojiForPriority" },
        //    {":grey_question:","dynamicEmojiForPriority" },
        //    {":no_entry:","dynamicEmojiForPriority" },
        //    {":x:","dynamicEmojiForPriority" },
        //    {":negative_squared_cross_mark:","dynamicEmojiForPriority" },            
        //};

        public FromEmojiToDynamicPriorityHtml(ILogger<FromEmojiToDynamicPriorityHtml> logger, IServerCache serverCache) : base(logger, serverCache)
        {

        }

        public class SortableInfo
        {
            public string SortCardIndex { get; set; } = string.Empty;
            public string SortPriorityDataFilePath { get; set; } = string.Empty;
            public string TableGameIndex { get; set; } = string.Empty;
            public int MatchCardIndex { get; set; }
            public int MatchGameIndex { get; set; }
            public int AbsoluteMatchIndex { get; set; }
        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {


            var stringToReturn = markdown;
            var currentPathFile = requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/');
            var priorityMatches = GetMatches(markdown);
            Regex rxGameTable = new Regex("\r\n- :(question|exclamation|grey_exclamation|grey_question|no_entry|x|negative_squared_cross_mark):(.+?(?=\r\n\r\n))\r\n\r\n", //.+?(?=\r\n-)|.
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var gameTableMatches = rxGameTable.Matches(markdown);
            var infos = new List<SortableInfo>();
            
            for (var k = 0; k < gameTableMatches.Count; k++)
            {
                var sortableItem = gameTableMatches[k];

                // adesso preparo le carte
                Regex rxCard = new Regex("\r\n- :(question|exclamation|grey_exclamation|grey_question|no_entry|x|negative_squared_cross_mark):(.+?(?=\r\n-)|.+?(?=\r\n\r\n))", //|.
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var cardMatches = rxCard.Matches(sortableItem.Groups[0].Value);

                for (int z = 0; z < cardMatches.Count; z++)
                {
                    var sortableInfo = new SortableInfo();
                    sortableInfo.SortCardIndex = $@"data-md-card-index=""{z}""";
                    var cardMatch = cardMatches[z];
                    sortableInfo.TableGameIndex = $@"data-md-table-game-index=""{k}""";
                    sortableInfo.SortPriorityDataFilePath = $@"data-md-pathfile=""{currentPathFile}""";
                    sortableInfo.MatchCardIndex = cardMatch.Index;
                    sortableInfo.MatchGameIndex = sortableItem.Index;
                    sortableInfo.AbsoluteMatchIndex = cardMatch.Index + sortableItem.Index + 4; // 4 è il correttore per una diversa ricerca
                    infos.Add(sortableInfo);
                }
            }

            var dictionaryInfo = infos.ToDictionary(_ => _.AbsoluteMatchIndex);
            var currentIncrement = 0;
            for (int i = 0; i < priorityMatches.Count; i++)
            {
                var priorityItem = priorityMatches[i];
                var priorityText = priorityItem.Groups[0].Value;
                
                var idName = $"emojiPriority{i}";
                dictionaryInfo.TryGetValue(priorityItem.Index, out var currentSortableInfo);
                if (currentSortableInfo != null)
                {
                    idName = $"sortableEmojiPriority{i}";
                }
                // Guida il priority match che prende comunque tutti i tag priority
                var raplaceWith = $@"<span id=""{idName}"" {currentSortableInfo?.TableGameIndex} {currentSortableInfo?.SortCardIndex} {currentSortableInfo?.SortPriorityDataFilePath}  style=""cursor: pointer"" onclick=""dynamicEmojiForPriority(this,{i},'{currentPathFile}')""> {priorityText}</span> ";
                (stringToReturn, currentIncrement) = ManageReplaceOnMD(stringToReturn, currentIncrement, priorityItem, raplaceWith);

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

        public override string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            var htmlToReturn = $@"<root>
                                    {base.TransformAfterConversion(html, requestInfo)}
                                </root>";
            var doc = new XmlDocument();
            doc.LoadXml(htmlToReturn);
            //var sortable = doc.SelectNodes(@"//span[contains(@id,'sortableEmojiPriority')]/..");
            var sortables = doc.SelectNodes(@"//span[contains(@id,'sortableEmojiPriority')]/..");
            foreach (XmlNode itemSortable in sortables)
            {
                var classAttr1 = doc.CreateAttribute("class");
                classAttr1.Value = " s1";
                itemSortable.Attributes.Append(classAttr1);
                var parentP = itemSortable.ParentNode;
                if (parentP.Attributes["sortable"] == null)
                {
                    var classAttr = doc.CreateAttribute("class");
                    classAttr.Value = "sortable";
                    parentP?.Attributes.Append(classAttr);
                }
            }


            //var parantLi = parentP.ParentNode;

            htmlToReturn = doc.FirstChild.InnerXml;
            return htmlToReturn;
        }
    }
}
