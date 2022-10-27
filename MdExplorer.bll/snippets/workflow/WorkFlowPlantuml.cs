using MdExplorer.Features.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.workflow
{
    public class WorkFlowPlantuml : ISnippet<DictionarySnippetParam>
    {
        public int Id => 3;

        public string Name => "Workflow";

        public string Group => "Workflow made with plantuml";
       

        public string GetSnippet(DictionarySnippetParam parameters)
        {

            var text = Helper.ExtractResFileString("MdExplorer.Features.snippets.workflow.workflow.plantuml");
            text = text.Replace("__title__", (string)parameters[ParameterName.StringDocumentTitle]);
            return text;
        }

        public void SetAssets(string assetsPath)
        {
            // nothing to (at the moment)
        }
    }
}
