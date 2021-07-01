using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class MDShowMD : ICommand
    {
        public string ServerAddress { get; set; }

        public string TransformInNewMDFromMD(string markdown)
        {
            // Devo cercare dentro markdown il comando m↓ShowMd(pathfile)
            // potrei usare le regular expression
            return markdown;
        }
    }
}
