using MdExplorer.Features.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.slide
{
    public class SlidePowerPoint : ISnippet<DictionarySnippetParam>
    {
        public int Id => 7;

        public string Name => "Slide power point";

        public string Group => "Presentation";

        public string GetSnippet(DictionarySnippetParam parameters = null)
        {
            // The body of a new slide deck: the front matter (document_type: slides) and the title
            // come from TextDocument, which writes them before this.
            return Helper.ExtractResFileString("MdExplorer.Features.snippets.slide.SlidePowerPoint.md");
        }

        public void SetAssets(string assetsPath)
        {
            // Nothing to do
        }
    }
}
