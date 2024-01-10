using MdExplorer.Abstractions.Entities.EngineDB;
using System.Collections.Generic;
using System;

namespace MdExplorer.Service.Controllers.TabBar.Automapper
{
    public class MarkdownFileDto
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Path { get; set; }
        public virtual string FileType { get; set; }        
    }
}
