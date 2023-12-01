using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands.html
{
    internal class EditH1MD : EditH1, ICommandMD, IEditorH1Context
    {
        public EditH1MD(ILogger<EditH1> logger) : base(logger)
        {
        }

        public string GetDataBy(string editorH1CurrentIndex, string absolutePathFile)
        {

            var mdText = File.ReadAllText(absolutePathFile);
            var matches = GetMatches(mdText).ToList();
            foreach (Match itemMatch in matches)
            {
                if (itemMatch.Index.ToString() == editorH1CurrentIndex)
                {
                    return itemMatch.Value;
                }
            }
            return "itemindex not found";

        }

        public string SaveNewMarkdown(string markdown, int indexStart, int indexEnd, string replace, string oldMd)
        {
            var countEnd = oldMd.Replace("\n", "\r\n").Length;
            var markdown1 = markdown.Remove(indexStart, countEnd);
            var count = replace.Length;
            var markdown2 = markdown1.Insert(indexStart, replace.Replace("\n", "\r\n"));
            markdown = markdown2;


            return markdown;
        }
    }
}
