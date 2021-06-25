using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Models
{
    public class MdExplorerAppSettings
    {
        public const string MdExplorer = "MdExplorer";
        public string PlantumlServer { get; set; }
        public string JiraServer { get; set; }
        public bool CreateStartupLog { get; set; }

    }
}
