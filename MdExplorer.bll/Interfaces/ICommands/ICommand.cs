using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public interface ICommand
    {
        //string ServerAddress { get; set; }
        public int Priority { get; set; }
        public bool Enabled { get; set; }
        string  TransformInNewMDFromMD(string markdown,RequestInfo requestInfo);
        string TransformAfterConversion(string html, RequestInfo requestInfo);
        string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo);
        
        MatchCollection GetMatches(string markdown);
        string Name { get; set; }
    }
}
