using Markdig;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Features.Interfaces.ICommandsSpecificContext
{
    public interface IEditorH1Context
    {
        string SaveNewMarkdown(string markdown, int indexStart, int indexEnd, string replace, string oldMd);
        string GetDataBy(string editorH1CurrentIndex, string absolutePathFile);
    }
}
