using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.workflow
{
    public class WorkFlowPlantuml : ISnippet<DictionarySnippetParam>
    {
        public int Id => 2;

        public string Name => "Workflow";

        public string Group => "Workflow made with plantuml";
       

        public string GetSnippet(DictionarySnippetParam parameters)
        {
            throw new NotImplementedException();
        }

        public void SetAssets(string assetsPath)
        {
            throw new NotImplementedException();
        }
    }
}
