using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.FunctionParameters;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.Markdown
{
    public class FromEmojiFloppyDiskToSaveFileMD : FromEmojiFloppyDiskToSaveFile, ICommandMD, ICommandSaveMD<string, string>
    {
        public FromEmojiFloppyDiskToSaveFileMD(ILogger<FromEmojiFloppyDiskToSaveFile> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }

        public (string, string) GetMDAndFileNameToSave(string markdown, string additionalInfo)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);

            if (matches.Count == 0)
                return (stringToReturn, default);

            var currentIncrement = 0;
            var item = matches[0];
            var fileName = item.Groups[2].Value.Replace("\r", string.Empty);
            var text = item.Groups[2].Value;
            var source = $"floppyDiskEmoji";
            var raplaceWith = $@"# {text}";
            (stringToReturn, currentIncrement) = ManageReplaceOnMD(stringToReturn, currentIncrement, item, raplaceWith);
            fileName = fileName.Trim();
            return (stringToReturn, $"{fileName}.md");
        }

        public string ReplaceSingleItem(string markdown, RequestInfo requestinfo, string toReplace, int index)
        {
            throw new NotImplementedException();
        }

    }
}
