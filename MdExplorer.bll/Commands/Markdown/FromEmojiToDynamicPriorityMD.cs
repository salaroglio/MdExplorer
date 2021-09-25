using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.FunctionParameters;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.Markdown
{
    public class FromEmojiToDynamicPriorityMD : FromEmojiToDynamicPriority, ICommandMD
    {
        private Dictionary<string, string> EmojiContextDictionary = new Dictionary<string, string>() {
            // Priority
            {"❔",":grey_question:" },
            {"❗",":exclamation:" },
            {"❕",":grey_exclamation:" },
            {"❓",":question:" },
            {"⛔",":no_entry:" },
            {"❌",":x:" },
            {"❎",":negative_squared_cross_mark:" },
        };

        public FromEmojiToDynamicPriorityMD(ILogger<FromEmojiToDynamicPriorityMD> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }

        /// <summary>
        /// Feature replace the emoji marker in the sequantial One 
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="requestinfo"></param>
        /// <param name="toReplace"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string ReplaceSingleItem(string markdown, RequestInfo requestinfo, string toReplace, int index)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
            var currentIncrement = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                if (i == index)
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

        /// <summary>
        /// Feature replace the new sorting order 
        /// Delete the in the previous place, write in the new one
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="requestinfo"></param>
        /// <param name="emojiPriorityOrderInfo"></param>
        /// <returns></returns>
        public (string, EmojiPriorityOrderInfo) ReplaceSingleItem(string markdown, RequestInfo requestinfo, EmojiPriorityOrderInfo emojiPriorityOrderInfo)
        {
            var modifiedEmojiPriorityOrderInfo = new EmojiPriorityOrderInfo();

            var direction = GetDirection(emojiPriorityOrderInfo);
            if (direction == Direction.Stay) // non è cambiato nulla
            {
                return (markdown, emojiPriorityOrderInfo);
            }
            // finalmente pronti a fare la replace
            var stringToReturn = markdown;
            var currentIncrement = 0;

            Regex rxTableGames = new Regex("\r\n- :(question|exclamation|grey_exclamation|grey_question|no_entry|x|negative_squared_cross_mark):(.+?(?=\r\n\r\n))\r\n\r\n",
                              RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var tableGameMatches = rxTableGames.Matches(markdown);

            for (var k = 0; k < tableGameMatches.Count; k++)
            {
                var tableGameMatch = tableGameMatches[k];
                if (k == emojiPriorityOrderInfo.TableGameIndex)
                {
                    //https://stackoverflow.com/questions/7124778/how-to-match-anything-up-until-this-sequence-of-characters-in-a-regular-expres
                    Regex rx = new Regex("\r\n- :(question|exclamation|grey_exclamation|grey_question|no_entry|x|negative_squared_cross_mark):(.+?(?=\r\n-)|.+?(?=\r\n\r\n))",
                                       RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var cardMatches = rx.Matches(tableGameMatch.Groups[0].Value);

                    // cancello il testo 
                    var textToMove = string.Empty;
                    for (int i = 0; i < cardMatches.Count; i++)
                    {
                        // currentNodeIndex can't be null
                        if (i == emojiPriorityOrderInfo.CurrentNodeIndex)
                        {
                            var cardMatch = cardMatches[i];
                            var currentIndex = tableGameMatch.Index + cardMatch.Index + currentIncrement;
                            stringToReturn = stringToReturn.Remove(currentIndex, cardMatch.Groups[0].Value.Length);
                            currentIncrement += -cardMatch.Groups[0].Value.Length; // negative while i'm subtracting text (removing the old)
                            textToMove = cardMatch.Groups[0].Value;
                            break; // no need to contnue
                        }
                    }

                    if (direction == Direction.MovingDown)
                    {
                        // manage the null NextNodeIndex: the meaning is the moved item is at the bottom of stack
                        if (emojiPriorityOrderInfo.NextNodeIndex == null)
                        {
                            modifiedEmojiPriorityOrderInfo.CurrentNodeIndex = emojiPriorityOrderInfo.PreviousNodeIndex.Value;
                            modifiedEmojiPriorityOrderInfo.NextNodeIndex = null;
                            modifiedEmojiPriorityOrderInfo.PreviousNodeIndex = emojiPriorityOrderInfo.PreviousNodeIndex - 1;
                            var cardMatch = cardMatches[emojiPriorityOrderInfo.PreviousNodeIndex.Value];
                            var currentIndex = tableGameMatch.Index + cardMatch.Index + currentIncrement;
                            stringToReturn = stringToReturn.Insert(currentIndex + cardMatch.Groups[0].Value.Length, textToMove);
                        }
                        else
                        {
                            for (int i = 0; i < cardMatches.Count; i++)
                            {
                                if (i == emojiPriorityOrderInfo.NextNodeIndex)
                                {
                                    modifiedEmojiPriorityOrderInfo.CurrentNodeIndex = i - 1;
                                    modifiedEmojiPriorityOrderInfo.NextNodeIndex = i;
                                    modifiedEmojiPriorityOrderInfo.PreviousNodeIndex = i - 2;

                                    var cardMatch = cardMatches[i];
                                    var currentIndex = tableGameMatch.Index + cardMatch.Index + currentIncrement; // the text is allready changed and so the old index of match have to be renewed
                                    stringToReturn = stringToReturn.Insert(currentIndex, textToMove);
                                }
                            }
                        }
                    }

                    if (direction == Direction.MovingUp)
                    {
                        // manage the null NextNodeIndex: the meaning is the moved item is at the bottom of stack
                        if (emojiPriorityOrderInfo.PreviousNodeIndex == null)
                        {
                            modifiedEmojiPriorityOrderInfo.CurrentNodeIndex = emojiPriorityOrderInfo.NextNodeIndex.Value; // remember index is 0 based, count is 1 based
                            modifiedEmojiPriorityOrderInfo.NextNodeIndex = emojiPriorityOrderInfo.NextNodeIndex + 1;
                            modifiedEmojiPriorityOrderInfo.PreviousNodeIndex = null;
                            var cardMatch = cardMatches[0];
                            var currentIndex = tableGameMatch.Index + cardMatch.Index;
                            stringToReturn = stringToReturn.Insert(currentIndex, textToMove);
                        }
                        else
                        {
                            for (int i = 0; i < cardMatches.Count; i++)
                            {
                                if (i == emojiPriorityOrderInfo.PreviousNodeIndex)
                                {
                                    modifiedEmojiPriorityOrderInfo.CurrentNodeIndex = i + 1;
                                    modifiedEmojiPriorityOrderInfo.NextNodeIndex = i + 2;
                                    modifiedEmojiPriorityOrderInfo.PreviousNodeIndex = i;
                                    var cardMatch = cardMatches[i];
                                    var currentIndex = tableGameMatch.Index + cardMatch.Index; // the text is changed after this position, so this index is still good
                                    stringToReturn = stringToReturn.Insert(currentIndex + cardMatch.Groups[0].Value.Length, textToMove);
                                }
                            }
                        }
                    }
                    // aggiungo il nuovo testo
                    break;
                }
            }

            return (stringToReturn, modifiedEmojiPriorityOrderInfo);
        }

        private Direction GetDirection(EmojiPriorityOrderInfo emojiPriorityOrderInfo)
        {
            var toReturn = Direction.Stay;
            if (emojiPriorityOrderInfo.CurrentNodeIndex > emojiPriorityOrderInfo.PreviousNodeIndex &&
                emojiPriorityOrderInfo.CurrentNodeIndex < emojiPriorityOrderInfo.NextNodeIndex)
            {
                return Direction.Stay;
            }
            if (emojiPriorityOrderInfo.CurrentNodeIndex < emojiPriorityOrderInfo.PreviousNodeIndex)
            {
                return Direction.MovingDown;
            }
            if (emojiPriorityOrderInfo.CurrentNodeIndex > emojiPriorityOrderInfo.NextNodeIndex)
            {
                return Direction.MovingUp;
            }
            return toReturn;
        }
    }

    enum Direction
    {
        MovingUp,
        MovingDown,
        Stay
    }
}
