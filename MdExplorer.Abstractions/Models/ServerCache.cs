using MdExplorer.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    public class ServerCache : IServerCache
    {
        public string[] Emojies { get; set; }
    }
}
