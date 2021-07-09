using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace MdExplorer.Features.Commands
{
    public class MDShowMD : ICommand
    {

        public MDShowMD(string ServerAddress, ILogger<MDShowMD> logger)
        {
            _serverAddress = ServerAddress;
            _logger = logger;
        }
        protected readonly string _serverAddress;
        protected readonly ILogger<MDShowMD> _logger;

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"m↓ShowMd\((.*)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public virtual string TransformAfterConversion(string text, RequestInfo requestInfo)
        {
            return text;
        }
    }
}
