using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    /// <summary>
    /// Informazioni che vanno a zonzo tra la sezione web
    /// e la library Features per i Commands
    /// </summary>
    public class RequestInfo
    {        
        public string AbsolutePathFile { get; set; }
        public string CurrentQueryRequest { get; set; }
        public string CurrentRoot { get; set; }

    }
}
