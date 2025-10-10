using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
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

        /// <summary>
        /// List of compatibility modes this command supports.
        /// If null or empty, command is assumed to support all modes.
        /// </summary>
        List<CompatibilityMode> SupportedModes { get; }

        string TransformInNewMDFromMD(string markdown,RequestInfo requestInfo);
        string TransformAfterConversion(string html, RequestInfo requestInfo);
        string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo);

        MatchCollection GetMatches(string markdown);
        string Name { get; set; }
    }
}
