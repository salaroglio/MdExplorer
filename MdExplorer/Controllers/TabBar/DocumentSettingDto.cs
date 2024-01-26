using System;

namespace MdExplorer.Service.Controllers.TabBar
{
    public class DocumentSettingDto
    {
        
        public  string DocumentPath { get; set; }
        public  bool ShowTOC { get; set; }
        public  int? TocWidth { get; set; }
        public  int? RefsWidth { get; set; }
        public bool ShowRefs { get; set; }
    }
}