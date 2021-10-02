using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// This commands add a function to floppi_disk emoji
    /// This function save a copy of current file with
    /// the name of the title of the document.
    /// the function activates only if the first line of document is
    /// H1 type element
    /// </summary>
    public class FromEmojiFloppyDiskToSaveFile :CommandBase, ICommand, IDisposable
    {
        //:floppy_disk:
        protected readonly ILogger<FromEmojiFloppyDiskToSaveFile> _logger;
        protected readonly IServerCache _serverCache;

        public int Priority { get; set; } = 15;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromEmojiFloppyDiskToSaveFile";

        public FromEmojiFloppyDiskToSaveFile(ILogger<FromEmojiFloppyDiskToSaveFile> logger, IServerCache serverCache)
        {
            _logger = logger;
            _serverCache = serverCache;
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"^# :floppy_disk: (:[^:^ ]*:)?(.*)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // Nothing to do
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
                //Nothing to do
                return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var stringToReturn = markdown;
            var matches = GetMatches(markdown);
            if (matches.Count==0)
                return stringToReturn;
            
            var currentIncrement = 0;
            var item = matches[0];
            var fileName = item.Groups[2].Value.Replace("\r",string.Empty);
            var text = item.Groups[1].Value + item.Groups[2].Value;
            var source = $"floppyDiskEmoji";
            var raplaceWith = $@"# <span id=""{source}"" style=""cursor: pointer"" onclick=""activateSaveCopy(this,'{requestInfo.AbsolutePathFile.Replace(Path.DirectorySeparatorChar, '/')}')""> :floppy_disk: </span> {text}"; //)""> :floppy_disk: </span> {text} ";
            (stringToReturn, currentIncrement) = ManageReplaceOnMD(stringToReturn, currentIncrement, item, raplaceWith);

            return stringToReturn;
        }

        public void Dispose()
        {
            //nothing to do
        }
    }
}
