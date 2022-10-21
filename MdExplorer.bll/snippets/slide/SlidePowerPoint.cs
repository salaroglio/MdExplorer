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

            var text = Helper.ExtractResFileString("MdExplorer.Features.snippets.slide.SlidePowerPoint.reveal");
            text = text.Replace("__title__", (string)parameters[ParameterName.StringDocumentTitle]);
            return text;
        }

        public void SetAssets(string assetsPath)
        {
            // Nothing to do
        }
    }
}
