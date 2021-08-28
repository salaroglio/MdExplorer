using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Models
{
    public class MonitoredMDModel
    {
        public string Path { get; set; }        
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string ConnectionId { get; set; }
        public string Action { get; set; }
        public DateTime StartExportTime { get; set; }
        public DateTime StopExportTime { get; set; }
        public int ExecutionTimeInSeconds { get {
                var span = StopExportTime - StartExportTime;
                return span.Seconds;
            }}

    }
}
