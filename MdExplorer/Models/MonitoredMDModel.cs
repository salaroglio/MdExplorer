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
        public string FullDirectoryPath { get; set; }
        public string Action { get; set; }
        public DateTime StartExportTime { get; set; }
        public DateTime StopExportTime { get; set; }
        // section for Rule # 1 FileName = to Title document
        public string Message { get; set; }
        public string FromFileName { get; set; }
        public string ToFileName { get; set; }
        public string FullPath { get; set; }
        /// <summary>
        /// Origin of the event: "watcher" for FileSystemWatcher, null/empty for user-initiated actions.
        /// Used by the client to distinguish automatic refresh (blockable) from user-triggered refresh.
        /// </summary>
        public string Source { get; set; }
        public int ExecutionTimeInSeconds { get {
                var span = StopExportTime - StartExportTime;
                return span.Seconds;
            }}

    }
}
