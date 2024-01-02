using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    public class MDShowH2 : CommandBase, ICommand
    {
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 10;
        public string Name { get; set; } = "MDShowH1";
        public MDShowH2(string ServerAddress, ILogger<MDShowH2> logger)
        {
            _serverAddress = ServerAddress;
            _logger = logger;
        }
        protected readonly string _serverAddress;
        protected readonly ILogger<MDShowH2> _logger;

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"MDShowH2\((.*),(.*),(.*)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public virtual string TransformAfterConversion(string text, RequestInfo requestInfo)
        {
            return text;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            //nothing to do
            return markdown;
        }
    }
}
