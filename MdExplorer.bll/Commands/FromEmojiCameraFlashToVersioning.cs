using ExCSS;
using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Class dedicated to prepare markdown/html behind the scenes.
    /// It is a part of the code you need to know to implement "Command-Camera" feature.
    /// The camera feature is composed of javascript part and bootstrap message box part
    /// that allow to create a snapshot of the link in the same row of the emoji.
    /// 
    /// </summary>
    public class FromEmojiCameraFlashToVersioning : CommandBase, ICommand, IDisposable
    {
        private readonly ILogger<FromEmojiCameraFlashToVersioning> _logger;
        protected readonly IServerCache _serverCache;

        public int Priority { get; set; } = 10;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromEmojiCameraFlashToVersioning";

        protected ILogger<FromEmojiCameraFlashToVersioning> Logger => _logger;

        public FromEmojiCameraFlashToVersioning(ILogger<FromEmojiCameraFlashToVersioning> logger, IServerCache serverCache)
        {
            _logger = logger;
            _serverCache = serverCache;
        }

        public void Dispose()
        {
            //nothing to do
        }

        /// <summary>
        ///  Im looking for camera and, eventually, a link
        /// </summary>
        ///  <param name="markdown"></param>
        ///  <returns></returns>
        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"\[(.*)\]\((.*)\).*:camera_flash:",
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
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);
            var regions = MarkdownCodeRegions.Of(markdown);
            var increment = 0;
            foreach (Match item in matches)
            {
                // A :camera_flash: in code stays text.
                if (regions.IsCode(item.Index))
                {
                    continue;
                }
                var link = item.Groups[2].Value;
                if (string.IsNullOrEmpty(link))
                {
                    return markdown;
                }
                var documentRelativePath = Path.GetDirectoryName(requestInfo.RootQueryRequest);
                var fullPathFile = Path.Combine(documentRelativePath, link.Replace('/',Path.DirectorySeparatorChar));
                var newHtml = $@"<span style=""cursor: pointer"" md-fullpath=""{fullPathFile}"" md-camera_flash_id=""{item.Index}"" onclick=""createSnapshot('{item.Index}')""> :camera_flash: </span>";
                var newToReplace = item.Groups[0].Value.Replace(":camera_flash:", newHtml);
                //var markdown1 = markdown.Remove(indexStart, countEnd);
                //var count = replace.Length;
                //var markdown2 = markdown1.Insert(indexStart, replace.Replace("\n", "\r\n"));
                // By position: a Replace would also rewrite the same line written in code.
                var at = item.Index + increment;
                markdown = markdown.Remove(at, item.Length).Insert(at, newToReplace);
                increment += newToReplace.Length - item.Length;
            }
           return markdown;
        }
    }
    

    
}

