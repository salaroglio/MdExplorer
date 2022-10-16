using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.workflow
{
    public class WorkFlowPlantuml : ISnippet
    {
        public int Id => 2;

        public string Name => "Workflow";

        public string Description => "Workflow made with plantuml";

        public string GetSnippet()
        {
            throw new NotImplementedException();
        }

        public void SetAssets(string assetsPath)
        {
            throw new NotImplementedException();
        }
    }
}
