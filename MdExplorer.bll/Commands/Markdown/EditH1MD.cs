using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands.html
{
    internal class EditH1MD : EditH1, ICommandMD, IEditorH1Context
    {
        public EditH1MD(ILogger<EditH1> logger) : base(logger)
        {
        }

        public string SaveNewMarkdown(string markdown, int indexStart, int indexEnd, string replace, string oldMd)
        {
            var countEnd = oldMd.Replace("\n","\r\n").Length;
            var markdown1 = markdown.Remove(indexStart, countEnd);
            var count = replace.Length;
            var markdown2 = markdown1.Insert(indexStart, replace.Replace("\n", "\r\n"));
            markdown = markdown2;


            return markdown;
        }
    }
}
