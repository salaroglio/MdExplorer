using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
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
    public class ManageLinkAsImageHtml : ManageLinkAsImages, ICommandHtml
    {
        /// <summary>Runs on slide decks: image paths resolved against the project, as in a document.</summary>
        public bool WorksInSlides => true;

        public ManageLinkAsImageHtml(ILogger<ManageLinkAsImages> logger, IHelper helper) : base(logger, helper)
        {
        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);
            var regions = MarkdownCodeRegions.Of(markdown);
            var increment = 0;

            foreach (Match item in matches)
            {
                // An image in code is text: its path stays as written.
                if (regions.IsCode(item.Index))
                {
                    continue;
                }

                var originalImagePath = item.Groups[2].Value;

                // Skip absolute paths (ManageLinkAbsolutePath handles them)
                // Skip external URLs
                if (originalImagePath.StartsWith("/") ||
                    originalImagePath.StartsWith("http://") ||
                    originalImagePath.StartsWith("https://"))
                    continue;

                // Get directory of current document relative to project root
                var currentDir = Path.GetDirectoryName(requestInfo.CurrentQueryRequest)
                    ?.Replace(Path.DirectorySeparatorChar, '/') ?? "";

                // Clean up relative path
                var cleanPath = originalImagePath.Replace(Path.DirectorySeparatorChar.ToString(), "/");
                if (cleanPath.StartsWith("./"))
                    cleanPath = cleanPath.Substring(2);

                // Build absolute path from project root
                string absolutePath;
                if (string.IsNullOrEmpty(currentDir))
                    absolutePath = "/" + cleanPath;
                else
                    absolutePath = "/" + currentDir + "/" + cleanPath;

                // Normalize: resolve ../ segments
                absolutePath = NormalizePath(absolutePath);

                // Replace in markdown, by position: a Replace would also rewrite the same image written in code.
                var allElementToReplace = item.Groups[0].Value.Replace(originalImagePath, absolutePath);
                var at = item.Index + increment;
                markdown = markdown.Remove(at, item.Length).Insert(at, allElementToReplace);
                increment += allElementToReplace.Length - item.Length;
            }

            return markdown;
        }

        /// <summary>
        /// Normalize a path: resolve ../ segments and clean up ./ and //
        /// </summary>
        private static string NormalizePath(string path)
        {
            var segments = path.Split('/').ToList();
            var result = new List<string>();

            foreach (var segment in segments)
            {
                if (segment == ".." && result.Count > 0 && result.Last() != "")
                {
                    result.RemoveAt(result.Count - 1);
                }
                else if (segment != "." && segment != "")
                {
                    result.Add(segment);
                }
            }

            return "/" + string.Join("/", result);
        }
    }
}
