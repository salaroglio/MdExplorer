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
        public string RelativePathFile { get; set; }
        public string CurrentQueryRequest { get; set; }
        public string CurrentRoot { get; set; }
        public int Recursionlevel { get; set; }
        public string RootQueryRequest { get; set; }
        public string ConnectionId {  get; set; }
        public string BaseUrl { get; set; }

        /// <summary>
        /// Render read-only (Fase 7h — review-view worktree): i comandi NON devono scrivere su
        /// disco (cache <c>.md/</c>) né cambiare la cwd di processo, perché la root è il worktree
        /// di un agente (una scrittura verrebbe poi committata dal deliverable, e la cwd
        /// resterebbe fuori dal progetto aperto).
        /// </summary>
        public bool ReadOnly { get; set; }

    }
}
